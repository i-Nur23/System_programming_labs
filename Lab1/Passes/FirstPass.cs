using Lab1.Models;
using Lab1.Tables;
using System.Globalization;
using System.Text.RegularExpressions;
using Lab1.Exceptions;

namespace Lab1.Passes;

public class FirstPass
{
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

    private List<AuxiliaryOperation> auxiliaryOperations = new List<AuxiliaryOperation>();
    private List<SymbolicName> symbolicNames = new List<SymbolicName>();

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

        for (int i = 0; i < linesCount; i++)
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

            auxiliaryOperations[i].LineType = line;
            
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

        foreach (var op in auxiliaryOperations)
        {
            auxiliaryTable.Add(op);
        }

        foreach (var name in symbolicNames)
        {
            symbolicNamesTable.Add(name);
        }

        return new FirstPassResult
        {
            AuxiliaryOperations = auxiliaryOperations,
            SymbolicNames = symbolicNames,
            ProgramLength = countAddress - loadAddress,
            LoadAddress = loadAddress
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
            
            if (symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper()) != null)
            {
                throw new Exception($"строка {index + 1}: метка {line[0].ToUpper()} уже есть в ТСИ");
            }

            if (Checks.IsRegister(line[0].ToUpper()))
            {
                throw new Exception($"Строка {index + 1}: метка не может быть регистром");
            }
            
            symbolicNames.Add(new SymbolicName()
            {
                Name = line[0].ToUpper(), 
                Address = Converters.ToSixDigits(countAddress.ToString("X"))
            });
            
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
                    throw new WrongAddressException(index + 1);
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
                    else if (!Checks.IsRightLabel(line[3]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
                    }
                    else if (addressing.AddressType == AddressingType.RELATIVE)
                    {
                        throw new WrongAddressException(index + 1);
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
                    throw new WrongAddressException(index + 1);
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
                    } else if (!Checks.IsRightLabel(line[2]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
                    }
                    else if (addressing.AddressType == AddressingType.RELATIVE)
                    {
                        throw new WrongAddressException(index + 1);
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

                if (Checks.IsRegister(line[2]) && commandLength == 4)
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
                
                if (Checks.IsRegister(line[3]) && commandLength == 4)
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
        
        auxiliaryOperations.Add(auxOperation);

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

                auxiliaryOperations.Add(new AuxiliaryOperation
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
                    auxiliaryOperations.Add(new AuxiliaryOperation
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

                auxiliaryOperations.Add(new AuxiliaryOperation { 
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "END", 
                    FirstOperand = line[1] 
                });

                break;
            case "BYTE":
                if (line.Length != 3 || line[0].ToUpper() == "BYTE" || line[2].ToUpper() == "BYTE")
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

                int addingToAddress = 1;

                var isOperandOk = Int32.TryParse(line[2], out int byteOperand);

                if (!isOperandOk)
                {
                    if (line[2][1] != (char)39 || line[2][line[2].Length - 1] != (char)39)
                    {
                        throw new Exception($"Строка { index + 1 }: неверный операнд");    
                    }

                    if (line[2][0] == 'x' || line[2][0] == 'X')
                    {
                        if (!Checks.IsContainsOnlyHexSymbols(line[2].Substring(2, line[2].Length - 3)))
                        {
                            throw new Exception($"Строка { index + 1 }: неверный операнд");
                        }

                        addingToAddress = (int) Math.Ceiling( (double)line[2].Substring(2, line[2].Length - 3).Length / 2);
                    }
                    
                    else if (line[2][0] == 'c' || line[2][0] == 'C')
                    {
                        addingToAddress = line[2].Substring(2, line[2].Length - 3).Length;
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

                if (symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper()) != null)
                {
                    throw new Exception($"строка {index + 1}: метка {line[0].ToUpper()} уже есть в ТСИ");
                }
                
                symbolicNames.Add(new SymbolicName { Address = Converters.ToSixDigits(countAddress.ToString("X")), Name = line[0]});
                
                auxiliaryOperations.Add(new AuxiliaryOperation { 
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "BYTE", 
                    FirstOperand = line[2]
                });

                countAddress += addingToAddress;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "WORD":
                if (line.Length != 3 || line[0].ToUpper() == "WORD" || line[2].ToUpper() == "WORD")
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы WORD");
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

                var isWordOperandOk = Int32.TryParse(line[2], out int wordOperand);

                if (!isWordOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (wordOperand < 0 || wordOperand > MAX_WORD)
                {
                    throw new Exception($"Строка { index + 1 }: отрицательный или превышающий максимальное значение операнд");
                }
                
                if (symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper()) != null)
                {
                    throw new Exception($"строка { index + 1 }: метка {line[0].ToUpper()} уже есть в ТСИ");
                }
                
                symbolicNames.Add(new SymbolicName
                {
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    Name = line[0].ToUpper()
                });
                
                auxiliaryOperations.Add(new AuxiliaryOperation { 
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "WORD", 
                    FirstOperand = line[2]
                });

                countAddress += 3;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "RESB":
                if (line.Length != 3 || line[0].ToUpper() == "RESB" || line[2].ToUpper() == "RESB")
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы RESB");
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

                var isResbOperandOk = Int32.TryParse(line[2], out int resbOperand);

                if (!isResbOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper()) != null)
                {
                    throw new Exception($"строка {index + 1}: метка {line[0].ToUpper()} уже есть в ТСИ");
                }
                
                symbolicNames.Add(new SymbolicName { Address = Converters.ToSixDigits(countAddress.ToString("X")), Name = line[0].ToUpper()});
                
                auxiliaryOperations.Add(new AuxiliaryOperation { 
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "RESB", 
                    FirstOperand = line[2]
                });

                countAddress += resbOperand;
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }

                break;
            case "RESW":
                if (line.Length != 3 || line[0].ToUpper() == "RESW" || line[2].ToUpper() == "RESW")
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы RESW");
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
                
                var isReswOperandOk = Int32.TryParse(line[2], out int reswOperand);

                if (!isReswOperandOk)
                {
                    throw new Exception($"Строка { index + 1 }: операнд не является числом или превышает допустимые значения");
                }
                
                if (symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper()) != null)
                {
                    throw new Exception($"строка {index + 1}: метка {line[0].ToUpper()} уже есть в ТСИ");
                }
                
                symbolicNames.Add(new SymbolicName { Address = Converters.ToSixDigits(countAddress.ToString("X")), Name = line[0].ToUpper()});
                
                auxiliaryOperations.Add(new AuxiliaryOperation { 
                    Address = Converters.ToSixDigits(countAddress.ToString("X")), 
                    BinaryCode = "RESW", 
                    FirstOperand = line[2]
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
}