using Lab1.Models;
using Lab1.Tables;
using System.Globalization;
using System.Text.RegularExpressions;
using Lab1.Exceptions;
using System.Runtime.CompilerServices;

namespace Lab1.Passes;

public class FirstPass
{
    private SymbolicName tempNameInfo;
    private string currentSectionName;

    private List<string> sectionNames = new List<string>();

    private List<string> tableOfExtdef = new List<string>();
    private List<string> tableOfExtref = new List<string>();

    private int loadAddress;
    private int countAddress;
    private int endAddress;

    private const int MAX_MEMORY_VOLUME = 16_777_215;
    private const int MAX_BYTE = 255;
    private const int MAX_WORD = 16_777_215;
    

    private readonly List<string> code;
    private readonly List<Operation> operations;
    private readonly AuxiliaryTable auxiliaryTable;
    private readonly SymbolicNames symbolicNamesTable;

    private bool isStarted = false;
    private bool isEnded = false;
    private List<SymbolicName> symbolicNames = new List<SymbolicName>();
    private int currentIndex = 0;

    private List<SectionInfo> sections = new List<SectionInfo>();
    private SectionInfo section = new SectionInfo
    {
        AuxiliaryOperations = new List<AuxiliaryOperation>()
    };

    private Addressing addressing;

    public FirstPass(
        List<string> code, 
        List<Operation> operations, 
        AuxiliaryTable auxiliaryTable, 
        SymbolicNames symbolicNamesTable
        )

    {
        this.code = code;
        this.operations = operations;
        this.auxiliaryTable = auxiliaryTable;
        this.symbolicNamesTable = symbolicNamesTable;

        addressing = Addressing.GetAddressing();
    }

    public FirstPassResult Run()
    {
        var linesCount = code.Count;
        string[] splittedLine;

        for (int i = 0; i < linesCount && !isEnded; i++, currentIndex++)
        {
            splittedLine = Converters.DeleteExtraWhitespace(code[i])
                .Trim()
                .Split(" ",3)
                .Where(el => el.Length > 0)
                .ToArray();

            Line line = Checks.getTypeOfLine(splittedLine, i, operations, out string name);

            switch (line)
            {
                case Line.DIRECTIVE:
                    HandleDirective(splittedLine, i, name);
                    break;
                case Line.COMMAND:
                    HandleCommand(splittedLine, i, name);
                    break;
            }

            section.AuxiliaryOperations[currentIndex].LineType = line;
            
            if (countAddress > MAX_MEMORY_VOLUME)
            {
                throw new Exception("Ошибка. Произошло переполнение памяти");
            }
        }

        if (!isStarted)
        {
            throw new Exception("Ошибка. В программе должна присутствовать директива START");
        }
        
        if (!isEnded)
        {
            throw new Exception("Ошибка. В программе должна присутствовать директива END");
        }

        var undefinedExternalNamesCount = symbolicNames
            .Count(n => n.Type == NameTypes.ExternalName && String.IsNullOrEmpty(n.Address));

        if (undefinedExternalNamesCount > 0)
        {
            throw new Exception("Ошибка. Имеются неопределенные внешние имена");
        }

        for (int i = 0; i < sections.Count; i++)
        {
            var section = sections[i];

            for (int j = 0; j < section.AuxiliaryOperations.Count; j++)
            {
                var op = section.AuxiliaryOperations[j];

                if (i != sections.Count - 1 && j == section.AuxiliaryOperations.Count - 1)
                    break;

                auxiliaryTable.Add(op);
            }
        }

        foreach (var name in symbolicNames)
        {
            symbolicNamesTable.Add(name);
        }



        return new FirstPassResult
        {
            SectionInfos = sections
        };
    }

    // Обработка команд
    private void HandleCommand(string[] line, int index, string commandName)
    {
        var operation = operations.FirstOrDefault(op => op.MnemonicCode == commandName.ToUpper());

        if (operation == null)
        {
            throw new Exception($"Строка {index + 1}: строка должна содержать или команду, или директиву");
        }

        if (line[0].ToUpper() != commandName && line[1].ToUpper() != commandName)
        {
            throw new Exception($"Строка {index + 1}: в строке не может более одной метки");
        }
        
        int binaryCode;

        if (line.Length == 3 && !Checks.IsConstant(line[2]))
        {
            line = line
                .Take(2)
                .Concat(line[2].Split(" "))
                .ToArray();
        }

        var commandLength = operations
            .FirstOrDefault(x => x.MnemonicCode == commandName)
            .CommandLength;
        
        var lineElementsCount = line.Length;

        if (lineElementsCount > 4)
        {
            throw new Exception($"Строка {index + 1}: более 4-x частей быть не может!");
        } 

        if (line[0].ToUpper() != commandName)
        {

            if (!Checks.IsRightLabel(line[0].ToUpper()))
            {
                throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                                    $"и начинаться с буквы или знака \'_\'");
            }

            if (Checks.IsRegister(line[0].ToUpper()))
            {
                throw new Exception($"Строка {index + 1}: метка не может быть регистром");
            }
            
            if (sectionNames.FirstOrDefault(name => String.Equals(name, line[0], StringComparison.OrdinalIgnoreCase)) != null)
            {
                throw new Exception($"Строка {index + 1}: имя метки занято");
            }

            tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper() && n.Section == currentSectionName);

            ProcessLabel(tempNameInfo, index, line[0]);
            
            if (line.Length == 2 || Checks.IsDirectAddressing(line[2]))
            {
                binaryCode = operation.BinaryCode * 4;
            }
            else if (Checks.IsRightRelativeAddressing(line[2]))
            {
                if (addressing.AddressType == AddressingType.DIRECT)
                {
                    throw new WrongAddressException(index + 1);
                }    
                
                binaryCode = operation.BinaryCode * 4 + 2;
            }
            else
            {
                if (!Checks.IsRightLabel(line[2]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат операнда");
                }
                
                if (addressing.AddressType == AddressingType.RELATIVE)
                {
                    if (symbolicNames.FirstOrDefault(n => n.Name == line[2]
                            && n.Type == NameTypes.ExternalReference
                            && n.Section == currentSectionName)
                        == null)
                    {
                        throw new WrongAddressException(index + 1);
                    }
                }
                
                binaryCode = operation.BinaryCode * 4 + 1;
            }

            if (lineElementsCount == 4)
            {
                if (!Checks.IsDirectAddressing(line[3]))
                {
                    if (Checks.IsRightRelativeAddressing(line[3]) && addressing.AddressType == AddressingType.DIRECT)
                    {
                        throw new WrongAddressException(index + 1);    
                    }
                    if (!Checks.IsRightLabel(line[3]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
                    }
                    if (addressing.AddressType == AddressingType.RELATIVE)
                    {
                        if (symbolicNames.FirstOrDefault(n => n.Name == line[3]
                                && n.Type == NameTypes.ExternalReference
                                && n.Section == currentSectionName)
                            == null)
                        {
                            throw new WrongAddressException(index + 1);
                        }
                    }
                }    
            }
        }
        else
        {
            if (lineElementsCount == 4)
            {
                throw new Exception($"Строка {index + 1}: oперандов не может быть более 2-х");
            }
            
            if (lineElementsCount == 1 || Checks.IsDirectAddressing(line[1]))
            {
                binaryCode = operation.BinaryCode * 4;
            }
            else if (Checks.IsRightRelativeAddressing(line[1]))
            {
                if (addressing.AddressType == AddressingType.DIRECT)
                {
                    throw new WrongAddressException(index + 1);
                }
                
                binaryCode = operation.BinaryCode * 4 + 2;
            }
            else
            {
                if (!Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат операнда");
                }
                
                if (addressing.AddressType == AddressingType.RELATIVE)
                {
                    if (symbolicNames.FirstOrDefault(n => n.Name == line[1]
                            && n.Type == NameTypes.ExternalReference
                            && n.Section == currentSectionName)
                        == null)
                    {
                        throw new WrongAddressException(index + 1);
                    }
                }
                
                binaryCode = operation.BinaryCode * 4 + 1;
            }
            
            if (lineElementsCount == 3)
            {
                if (!Checks.IsDirectAddressing(line[2]))
                {
                    if (Checks.IsRightRelativeAddressing(line[2]) && addressing.AddressType == AddressingType.DIRECT)
                    {
                        throw new WrongAddressException(index + 1);    
                    } if (!Checks.IsRightLabel(line[2]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
                    }
                    if (addressing.AddressType == AddressingType.RELATIVE)
                    {
                        if (symbolicNames.FirstOrDefault(n => n.Name == line[2]
                                && n.Type == NameTypes.ExternalReference
                                && n.Section == currentSectionName)
                            == null)
                        {
                            throw new WrongAddressException(index + 1);
                        }
                    }
                }    
            }
        }

        var auxOperation = new AuxiliaryOperation
        {
            Address = Converters.ToSixDigits(countAddress.ToString("X")),
            BinaryCode = Converters.ToTwoDigits(binaryCode.ToString("X"))
        };

        if (line[0] != commandName)
        {
            if (lineElementsCount > 2)
            {
                if (commandLength == 1)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (!Checks.IsConstant(line[2]) && !Checks.IsRegister(line[2]) &&
                    !Checks.IsOnlyLettersAndNumbers(line[2]) && !Checks.IsRightRelativeAddressing(line[2]))
                {
                    throw new Exception($"Строка {index + 1}: недопустимые символы");
                }

                if (commandLength == 4 && (Checks.IsRegister(line[2]) || lineElementsCount == 4))
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (!Checks.IsRegister(line[2]) && commandLength == 2)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                auxOperation.FirstOperand = line[2];
            }

            if (lineElementsCount == 4)
            {
                if (!Checks.IsConstant(line[3]) && !Checks.IsRegister(line[3]) &&
                    !Checks.IsOnlyLettersAndNumbers(line[3]) && !Checks.IsRightRelativeAddressing(line[3]))
                {
                    throw new Exception($"Строка {index + 1}: недопустимые символы");
                }
                
                if (commandLength == 4 )
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (!Checks.IsRegister(line[3]) && commandLength == 2)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                auxOperation.SecondOperand = line[3];
            }
        }
        else
        {
            if (lineElementsCount > 1)
            {
                if (commandLength == 1)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (commandLength == 4 && lineElementsCount == 3)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }

                if (!Checks.IsConstant(line[1]) && !Checks.IsRegister(line[1]) &&
                    !Checks.IsOnlyLettersAndNumbers(line[1]) && !Checks.IsRightRelativeAddressing(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: недопустимые символы");
                }
                
                if (Checks.IsRegister(line[1]) && commandLength == 4)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (!Checks.IsRegister(line[1]) && commandLength == 2)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                auxOperation.FirstOperand = line[1];
            }

            if (lineElementsCount == 3)
            {
                if (!Checks.IsConstant(line[2]) && !Checks.IsRegister(line[2]) &&
                    !Checks.IsOnlyLettersAndNumbers(line[2]) && !Checks.IsRightRelativeAddressing(line[2]))
                {
                    throw new Exception($"Строка {index + 1}: недопустимые символы");
                }
                
                if (Checks.IsRegister(line[2]) && commandLength == 4)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                if (!Checks.IsRegister(line[2]) && commandLength == 2)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
                
                auxOperation.SecondOperand = line[2];
            }
        }
        
        section.AuxiliaryOperations.Add(auxOperation);

        countAddress += operation.CommandLength;
        if (countAddress > MAX_MEMORY_VOLUME)
        {
            throw new Exception("Ошибка. Произошло переполнение памяти");
        }

    }
    
    // Обработка директив
    private void HandleDirective(string[] line, int index, string dirName) 
    {
        if (line[0].ToUpper() != dirName && line[1].ToUpper() != dirName)
        {
            throw new Exception($"Строка {index + 1}: в строке не может более одной метки");
        }

        if (line[0].ToUpper() == dirName && line.Length == 4)
        {
            throw new Exception($"Строка {index + 1}: oперандов не может быть более 2-х");   
        }
        
        switch (dirName)
        {
            case "START":

                if (index != 0 || isStarted)
                {
                    throw new Exception("Директива START должна встречаться один раз и в начале программы");
                }

                loadAddress = 0;

                section.LoadAddress = 0;

                isStarted = true;

                if (line.Length == 3) {
                    var isAddressOk = Int32.TryParse(
                        line[2],
                        NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture,
                        out int addressInCommand
                    );

                    if (!isAddressOk || addressInCommand != 0)
                    {
                        throw new Exception("В адресе загрузки должен быть 0");
                    }
                }

                if (line[0].Length > 6)
                {
                    throw new Exception("Строка 1. Имя программы должно содержать не более 6 символов");
                }

                if (!Checks.IsOnlyLettersAndNumbers(line[0].ToUpper()))
                {
                    throw new Exception("Строка 1. Имя программы должно содержать только латиницу и цифры");
                }

                if (loadAddress < 0)
                {
                    throw new Exception("В адресе загрузки указано отрицательное число");
                }

                if (loadAddress >= MAX_MEMORY_VOLUME)
                {
                    throw new Exception("В адресе загрузки указан недопустимый адрес");
                }

                countAddress = loadAddress;

                if (Checks.IsRegister(line[0]))
                {
                    throw new Exception("Имя программы не может быть регистром");
                }

                currentSectionName = line[0];
                section.Name = line[0];

                sectionNames.Add(line[1].ToUpper());

                section.AuxiliaryOperations.Add(new AuxiliaryOperation
                {
                    Address = line[0],
                    BinaryCode = line[1],
                    FirstOperand =  line.Length == 3 ? "000000" : null,
                });
                
                break;
            case "END":
                if (line.Length > 2)
                {
                    throw new Exception($"Строка { index + 1 }: в строке с директивой END должно быть 1 или 2 элемента");
                }

                if (line[0].ToUpper() != "END")
                {
                    throw new Exception($"Строка { index + 1 }: в строке с директивой END не должно быть метки");
                }

                isEnded = true;
                
                if (line.Length == 1)
                {
                    endAddress = loadAddress;
                    section.AuxiliaryOperations.Add(new AuxiliaryOperation
                    {
                        Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                        BinaryCode = "END"
                    });
                    break;
                }

                var isEndAddressOk = Int32.TryParse(
                    line[1],
                    NumberStyles.HexNumber,
                    CultureInfo.CurrentCulture,
                    out endAddress
                );

                if (!isEndAddressOk)
                {
                    throw new Exception("В адресе входа в программу указано не число");
                }

                if (endAddress < loadAddress || endAddress > countAddress)
                {
                    throw new Exception("Неверный адрес входа в программу");
                }

                section.AuxiliaryOperations.Add(new AuxiliaryOperation {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "END", 
                    FirstOperand = line[1] 
                });

                section.Length = countAddress;
                section.SymbolicNames = symbolicNames.Where(n => n.Section == currentSectionName).ToList();
                sections.Add(section);

                break;
            case "EXTDEF":
                var lastAddedOperationBeforeExtdef = section.AuxiliaryOperations.Last();

                if (lastAddedOperationBeforeExtdef.BinaryCode.ToUpper() != "START" &&
                    lastAddedOperationBeforeExtdef.BinaryCode.ToUpper() != "CSEC" &&
                    lastAddedOperationBeforeExtdef.BinaryCode.ToUpper() != "EXTDEF" )
                {
                    throw new Exception($"Строка { index + 1 }: неверная позиция EXTDEF");
                }


                if (line.Length != 2 ||
                    !String.Equals(line[0].ToUpper(), "EXTDEF") ||
                    !Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка { index + 1 }: неверный формат директивы EXTDEF");
                }

                var exportName = line[1];

                if (tableOfExtdef.IndexOf(exportName.ToUpper()) != -1 || tableOfExtdef.IndexOf(exportName) != -1)
                {
                    throw new Exception($"Строка { index + 1 }: имя не уникально для EXTDEF");
                }

                section.AuxiliaryOperations.Add(new AuxiliaryOperation
                {
                    BinaryCode = "EXTDEF",
                    FirstOperand = exportName,
                    LineType = Line.DIRECTIVE
                });

                symbolicNames.Add(new SymbolicName
                {
                    Name = exportName,
                    Section = currentSectionName,
                    Type = NameTypes.ExternalName
                });

                tableOfExtdef.Add(exportName.ToUpper());


                break;
            case "EXTREF":
                var lastAddedOperationBeforeExtref = section.AuxiliaryOperations.Last();

                if (lastAddedOperationBeforeExtref.BinaryCode.ToUpper() != "START" &&
                    lastAddedOperationBeforeExtref.BinaryCode.ToUpper() != "CSEC" &&
                    lastAddedOperationBeforeExtref.BinaryCode.ToUpper() != "EXTDEF" &&
                    lastAddedOperationBeforeExtref.BinaryCode.ToUpper() != "EXTREF")
                {
                    throw new Exception($"Строка { index + 1 }: неверная позиция EXTREF");
                }


                if (line.Length != 2 ||
                    !String.Equals(line[0].ToUpper(), "EXTREF") ||
                    !Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка { index + 1 }: неверный формат директивы EXTREF");
                }

                var importName = line[1];

                if (tableOfExtdef.IndexOf(importName.ToUpper()) != -1 || tableOfExtdef.IndexOf(importName) != -1)
                {
                    throw new Exception($"Строка { index + 1 }: имя не уникально для EXTREF");
                }

                section.AuxiliaryOperations.Add(new AuxiliaryOperation
                {
                    BinaryCode = "EXTREF",
                    FirstOperand = importName,
                    LineType = Line.DIRECTIVE
                });

                symbolicNames.Add(new SymbolicName
                {
                    Name = importName,
                    Section = currentSectionName,
                    Type = NameTypes.ExternalReference
                });

                tableOfExtref.Add(importName);

                break;
            case "CSEC":

                if (line.Length != 2 || String.Equals(line[0].ToUpper(), "CSEC"))
                {
                    throw new Exception($"Строка { index + 1 }: неверный формат директивы CSEC");
                }

                if (!Checks.IsRightLabel(line[0]))
                {
                    throw new Exception($"Строка { index + 1 }: неверный формат метки в директиве CSEC");
                }

                if (symbolicNames.FirstOrDefault(n => n.Name == line[0]) != null ||
                    sectionNames.IndexOf(line[0].ToUpper()) != -1)
                {
                    throw new Exception($"Строка { index + 1 }: имя { line[0] } уже использовано ранее ");
                }

                section.Length = countAddress;
                section.SymbolicNames = symbolicNames.Where(n => n.Section == currentSectionName).ToList();

                section.AuxiliaryOperations.Add(new AuxiliaryOperation
                {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")),
                    BinaryCode = "END",
                    FirstOperand = "000000",
                });

                sections.Add(section);
                section = new SectionInfo
                {
                    AuxiliaryOperations = new List<AuxiliaryOperation>(),
                    LoadAddress = 0
                };

                section.AuxiliaryOperations.Add(new AuxiliaryOperation
                {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")),
                    BinaryCode = "CSEC",
                    FirstOperand = line[0],
                });

                sectionNames.Add(line[0].ToUpper());

                countAddress = 0;
                currentIndex = 0;

                currentSectionName = line[0];
                section.Name = currentSectionName;

                tableOfExtdef.Clear();
                tableOfExtref.Clear();

                break;
            case "BYTE":

                string byteLabel = null;
                string byteStringOperand;

                if (line.Length != 2 && line.Length != 3)
                {
                    throw new Exception($"Строка { index + 1}: неверный формат директивы BYTE");
                }
                
                if (line.Length == 3)
                {
                    if (line[0].ToUpper() == "BYTE" || line[2].ToUpper() == "BYTE")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы BYTE");
                    }

                    if (Checks.IsRegister(line[0]))
                    {
                        throw new Exception($"Строка { index + 1 }: метка не может быть регистром");
                    }

                    if (!Checks.IsRightLabel(line[0]))
                    {
                        throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                            $"и начинаться с буквы или знака \'_\'");
                    }

                    byteLabel = line[0];
                    byteStringOperand = line[2];
                } else
                {
                    if (line[0].ToUpper() != "BYTE")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы BYTE");
                    }

                    byteStringOperand = line[1];
                }

                int addingToAddress = 1;

                var isOperandOk = Int32.TryParse(byteStringOperand, out int byteOperand);

                if (!isOperandOk)
                {
                    if (byteStringOperand[1] != (char)39 || byteStringOperand[byteStringOperand.Length - 1] != (char)39)
                    {
                        throw new Exception($"Строка { index + 1 }: неверный операнд");    
                    }

                    if (byteStringOperand[0] == 'x' || byteStringOperand[0] == 'X')
                    {
                        if (!Checks.IsContainsOnlyHexSymbols(byteStringOperand.Substring(2, byteStringOperand.Length - 3)))
                        {
                            throw new Exception($"Строка { index + 1 }: неверный операнд");
                        }

                        addingToAddress = (int) Math.Ceiling( (double)byteStringOperand.Substring(2, byteStringOperand.Length - 3).Length / 2);
                    }

                    else if (byteStringOperand[0] == 'c' || byteStringOperand[0] == 'C')
                    {
                        addingToAddress = byteStringOperand.Substring(2, byteStringOperand.Length - 3).Length;
                    }

                    else
                    {
                        throw new Exception($"Строка { index + 1 }: неверный операнд");
                    }
                }

                if (byteOperand < 0 || byteOperand > MAX_BYTE)
                {
                    throw new Exception($"Строка { index + 1 }: отрицательный или превышающий максимальное значение операнд");
                }

                if (sectionNames.FirstOrDefault(name => String.Equals(name, byteLabel, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception($"Строка {index + 1}: имя метки занято");
                }

                if (byteLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == byteLabel.ToUpper() && n.Section == currentSectionName);

                    ProcessLabel(tempNameInfo, index, byteLabel);
                }
                
                
                section.AuxiliaryOperations.Add(new AuxiliaryOperation {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "BYTE", 
                    FirstOperand = byteStringOperand
                });

                countAddress += addingToAddress;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "WORD":
                string wordLabel = null;
                string wordStringOperand;

                if (line.Length != 2 && line.Length != 3)
                {
                    throw new Exception($"Строка { index + 1}: неверный формат директивы WORD");
                }

                if (line.Length == 3)
                {
                    if (line[0].ToUpper() == "WORD" || line[2].ToUpper() == "WORD")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы WORD");
                    }

                    if (Checks.IsRegister(line[0]))
                    {
                        throw new Exception($"Строка { index + 1 }: метка не может быть регистром");
                    }

                    if (!Checks.IsRightLabel(line[0]))
                    {
                        throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                            $"и начинаться с буквы или знака \'_\'");
                    }

                    wordLabel = line[0];
                    wordStringOperand = line[2];
                } else
                {
                    if (line[0].ToUpper() != "WORD")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы WORD");
                    }

                    wordStringOperand = line[1];
                }

                var isWordOperandOk = Int32.TryParse(wordStringOperand, out int wordOperand);

                if (!isWordOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (wordOperand < 0 || wordOperand > MAX_WORD)
                {
                    throw new Exception($"Строка { index + 1 }: отрицательный или превышающий максимальное значение операнд");
                }
                
                if (sectionNames.FirstOrDefault(name => String.Equals(name, wordLabel, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception($"Строка {index + 1}: имя метки занято");
                }

                if (wordLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == wordLabel.ToUpper() && n.Section == currentSectionName);
                    ProcessLabel(tempNameInfo, index, wordLabel);
                }
                
                section.AuxiliaryOperations.Add(new AuxiliaryOperation {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "WORD", 
                    FirstOperand = wordStringOperand
                });

                countAddress += 3;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "RESB":

                string resbLabel = null;
                string resbStringOperand;

                if (line.Length != 2 && line.Length != 3)
                {
                    throw new Exception($"Строка { index + 1}: неверный формат директивы RESB");
                }

                if (line.Length == 3)
                {
                    if (line[0].ToUpper() == "RESB" || line[2].ToUpper() == "RESB")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы RESB");
                    }

                    if (Checks.IsRegister(line[0]))
                    {
                        throw new Exception($"Строка { index + 1 }: метка не может быть регистром");
                    }

                    if (!Checks.IsRightLabel(line[0]))
                    {
                        throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                            $"и начинаться с буквы или знака \'_\'");
                    }

                    resbLabel = line[0];
                    resbStringOperand = line[2];
                } else
                {
                    if (line[0].ToUpper() != "RESB")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы RESB");
                    }

                    resbStringOperand = line[1];
                }

                var isResbOperandOk = Int32.TryParse(resbStringOperand, out int resbOperand);

                if (!isResbOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (resbOperand <= 0)
                {
                    throw new Exception($"Строка { index + 1 }: нельзя резервировать отрицательное или нулевое количество байт");
                }

                if (sectionNames.FirstOrDefault(name => String.Equals(name, resbLabel, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception($"Строка {index + 1}: имя метки занято");
                }


                if (resbLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == resbLabel.ToUpper() && n.Section == currentSectionName);

                    ProcessLabel(tempNameInfo, index, resbLabel);
                }
                
                section.AuxiliaryOperations.Add(new AuxiliaryOperation {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "RESB", 
                    FirstOperand = resbStringOperand
                });

                countAddress += resbOperand;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "RESW":
                string reswLabel = null;
                string reswStringOperand;

                if (line.Length != 2 && line.Length != 3)
                {
                    throw new Exception($"Строка { index + 1}: неверный формат директивы RESW");
                }

                if (line.Length == 3)
                {
                    if (line[0].ToUpper() == "RESW" || line[2].ToUpper() == "RESW")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы RESW");
                    }

                    if (Checks.IsRegister(line[0]))
                    {
                        throw new Exception($"Строка { index + 1 }: метка не может быть регистром");
                    }

                    if (!Checks.IsRightLabel(line[0]))
                    {
                        throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                            $"и начинаться с буквы или знака \'_\'");
                    }

                    reswLabel = line[0];
                    reswStringOperand = line[2];
                } else
                {
                    if (line[0].ToUpper() != "RESW")
                    {
                        throw new Exception($"Строка { index + 1}: неверный формат директивы RESW");
                    }

                    reswStringOperand = line[1];
                }
                
                var isReswOperandOk = Int32.TryParse(reswStringOperand, out int reswOperand);

                if (!isReswOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (reswOperand <= 0)
                {
                    throw new Exception($"Строка { index + 1 }: нельзя резервировать отрицательное или нулевое количество байт");
                }

                if (sectionNames.FirstOrDefault(name => String.Equals(name, reswLabel, StringComparison.OrdinalIgnoreCase)) != null)
                {
                    throw new Exception($"Строка {index + 1}: имя метки занято");
                }

                if (reswLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == reswLabel.ToUpper() && n.Section == currentSectionName);

                    ProcessLabel(tempNameInfo, index, reswLabel);
                }
                
                section.AuxiliaryOperations.Add(new AuxiliaryOperation {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "RESW", 
                    FirstOperand = reswStringOperand
                });

                countAddress += reswOperand * 3;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }
                break;
            default:
                break;
        }
    }

    private void ProcessLabel(SymbolicName nameFoundInTable, int index, string label)
    {
        if (nameFoundInTable != null)
        {
            switch (nameFoundInTable.Type)
            {
                case NameTypes.ExternalReference:
                    throw new Exception($"строка {index + 1}: метка {label.ToUpper()} уже есть в ТСИ как ВС");

                case NameTypes.SymbolicName:
                    throw new Exception($"строка {index + 1}: метка {label.ToUpper()} уже есть в ТСИ");

                case NameTypes.ExternalName:
                    if (nameFoundInTable.Address != null)
                    {
                        throw new Exception($"строка {index + 1}: имени {label.ToUpper()} уже назаначен адрес");
                    }

                    nameFoundInTable.Address = (Converters.ToSixDigits(countAddress.ToString("X")));
                    break;
            }
        }
        else
        {


            symbolicNames.Add(new SymbolicName()
            {
                Name = label.ToUpper(),
                Address = Converters.ToSixDigits(countAddress.ToString("X")),
                Section = currentSectionName
            });
        }

    }
}