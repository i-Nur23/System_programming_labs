using Lab1.Models;
using System.Globalization;
using Lab1.Singletons;
using System.ComponentModel;
using System.Collections;
using System.Runtime;
using System.Text;
using Lab1.Exceptions;
using static System.Collections.Specialized.BitVector32;

namespace Lab1.Passes;

public class Pass : IEnumerable<ObjectModuleRecord>
{
    private SymbolicName tempNameInfo;
    private ObjectModuleRecord tempRecord;

    private int loadAddress;
    private int countAddress;
    private int endAddress;

    private const int MAX_MEMORY_VOLUME = 16_777_215;
    private const int MAX_BYTE = 255;
    private const int MAX_WORD = 16_777_215;

    private readonly List<string> code;
    private readonly List<Operation> operations;

    private List<string> sectionNames = new List<string>();

    private List<string> tableOfExtdef = new List<string>();
    private List<string> tableOfExtref = new List<string>();

    private bool isStarted = false;
    private bool isEnded = false;
    private BindingList<SymbolicName> symbolicNames = SymbolicNamesList.GetInstance();
    private BindingList<ObjectModuleRecord> binaryCodeLines = ObjectModuleList.GetInstance();
    private BindingList<Modifier> modifiers = ModifiersList.GetInstance();

    private Addressing addressing = Addressing.GetAddressing();

    private StringBuilder sb = new StringBuilder();

    private string lastAddedDirective;
    private string currentSectionName;


    public Pass(
        List<string> code, 
        List<Operation> operations
    )
    {
        this.code = code;
        this.operations = operations;
    }

    public IEnumerator<ObjectModuleRecord> GetEnumerator()
    {
        var linesCount = code.Count;
        string[] splittedLine;

        for (int i = 0; i < linesCount && !isEnded; i++)
        {
            tempRecord = new ObjectModuleRecord();

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

            if (countAddress > MAX_MEMORY_VOLUME)
            {
                throw new Exception("Ошибка. Произошло переполнение памяти");
            }

            if (i == linesCount - 1 && !isEnded)
            {
                CheckIfEnded();
            }

            sb.Clear();

            yield return tempRecord;
        }
    }

    public void CheckSymbolicNames()
    {
        var nameWithoutAddress = symbolicNames.FirstOrDefault(name => name.OperandAddress != null);

        if (nameWithoutAddress != null)
        {
            throw new Exception($"Ошибка. Имени {nameWithoutAddress.Name} не назначен адрес");
        }
    }

    public void CheckIfEnded()
    {
        if (!isEnded)
        {
            throw new Exception("Ошибка. В программе должна присутствовать директива END");
        }
    }

    // Обработка команд
    private void HandleCommand(string[] line, int index, string commandName)
    {
        bool isOperandNumber = false;

        if (!isStarted)
        {
            throw new Exception("Ошибка. В программе должна присутствовать директива START");
        }

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

        // Если имеется метка
        if (line[0].ToUpper() != commandName)
        {
            if (operation.CommandLength == 1 && line.Length != 2)
            {
                throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
            }

            if (operation.CommandLength == 2 && line.Length != 4)
            {
                if (Int32.TryParse(line[2], out int x) && x <= 255 && x >= -256)
                {
                    isOperandNumber = true; 
                }

                else
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
            }

            if (operation.CommandLength == 4 && line.Length != 3)
            {    
                throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
            }

            if  (operation.CommandLength != 1 && Int32.TryParse(line[2], out int a) && a <= MAX_WORD && a >= -(MAX_WORD + 1))
            {
                isOperandNumber = true;
            } 

            if (!Checks.IsRightLabel(line[0].ToUpper()))
            {
                throw new Exception($"строка {index + 1}: метка должна содержать только латинские буквы и цифры, " +
                                    $"и начинаться с буквы или знака \'_\'");
            }

            if (Checks.IsRegister(line[0].ToUpper()))
            {
                throw new Exception($"Строка {index + 1}: метка не может быть регистром");
            }

            tempNameInfo = symbolicNames.FirstOrDefault(n => IsSame(n.Name, line[0]) && IsSame(n.Section, currentSectionName));
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

                if (!isOperandNumber)
                {
                    switch (line.Length)
                    {
                        case 4:
                            ProcessLabelOperand(line[2], index);
                            ProcessLabelOperand(line[3], index);
                            break;
                        case 3:
                            ProcessLabelOperand(line[2], index);
                            break;
                        default:
                            break;
                    }
                }            
            }
            else
            {
                if (addressing.AddressType == AddressingType.RELATIVE)
                {
                    if (symbolicNames.FirstOrDefault(n => IsSame(n.Name, line[2]) 
                        && IsSame(n.Section, currentSectionName) 
                        && n.Type == NameTypes.ExternalReference) == null)
                    {
                        throw new WrongAddressException(index + 1);
                    }
                }

                binaryCode = operation.BinaryCode * 4 + 1;

                switch (line.Length)
                {
                    case 4:
                        ProcessLabelOperand(line[2], index);
                        ProcessLabelOperand(line[3], index);
                        break;
                    case 3:
                        ProcessLabelOperand(line[2], index);
                        break;
                    default:
                        break;
                }
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
                    if (addressing.AddressType == AddressingType.RELATIVE && !Checks.IsRegister(line[3]))
                    {
                        throw new WrongAddressException(index + 1);
                    }
                }
            }
        }
        else
        {
            if (operation.CommandLength == 1 && line.Length != 1)
            {
                throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
            }

            if (operation.CommandLength == 2 && line.Length != 3)
            {
                if (Int32.TryParse(line[1], out int y) && y <= 255 && y >= -256)
                {
                    isOperandNumber = true;
                } 
                else
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }
            }

            if (operation.CommandLength != 1 && Int32.TryParse(line[1], out int z) && z <= MAX_WORD && z >= -(MAX_WORD + 1))
            {
                isOperandNumber = true;
            }

            if (operation.CommandLength == 4 && line.Length != 2)
            {
                throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
            }

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

                switch (line.Length)
                {
                    case 3:
                        ProcessLabelOperand(line[1], index);
                        ProcessLabelOperand(line[2], index);
                        break;
                    case 2:
                        ProcessLabelOperand(line[1], index);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (addressing.AddressType == AddressingType.RELATIVE)
                {
                    if (symbolicNames.FirstOrDefault(n => IsSame(n.Name, line[1])
                        && IsSame(n.Section, currentSectionName)
                        && n.Type == NameTypes.ExternalReference) == null)
                    {
                        throw new WrongAddressException(index + 1);
                    }
                }

                if (!Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат операнда");
                }
                
                binaryCode = operation.BinaryCode * 4 + 1;

                if (!isOperandNumber)
                {
                    switch (line.Length)
                    {
                        case 3:
                            ProcessLabelOperand(line[1], index);
                            ProcessLabelOperand(line[2], index);
                            break;
                        case 2:
                            ProcessLabelOperand(line[1], index);
                            break;
                        default:
                            break;
                    }
                }
            }
            
            if (lineElementsCount == 3)
            {
                if (Checks.IsRightRelativeAddressing(line[2]) && addressing.AddressType == AddressingType.DIRECT)
                {
                    throw new WrongAddressException(index + 1);
                }
                if (!Checks.IsRightLabel(line[2]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат операнда");
                }
                if (addressing.AddressType == AddressingType.RELATIVE && !Checks.IsRegister(line[2]))
                {
                    throw new WrongAddressException(index + 1);
                }    
            }
        }

        sb.Append(Converters.ToTwoDigits(binaryCode.ToString("X")));

        if (!IsSame(line[0], commandName))
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
                
                if (!Checks.IsRegister(line[2]) && commandLength == 2 && !isOperandNumber)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }

                sb.Append(ProcessOperand(line[2], commandName, index));

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

                sb.Append(ProcessOperand(line[3], commandName, index));
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
                
                if (!Checks.IsRegister(line[1]) && commandLength == 2 && !isOperandNumber)
                {
                    throw new Exception($"Строка {index + 1}: строка не соответсвует длине команды");
                }

                sb.Append(ProcessOperand(line[1], commandName, index));
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

                sb.Append(ProcessOperand(line[2], commandName, index));
            }
        }

        tempRecord.Type = RecordType.T;
        tempRecord.Address = Converters.ToSixDigits(countAddress.ToString("X"));
        tempRecord.Length = Converters.ToTwoDigits((operation.CommandLength * 2).ToString("X"));
        tempRecord.OperandPart = sb.ToString();


        checked
        {
            countAddress += operation.CommandLength;
        }
        
        if (countAddress > MAX_MEMORY_VOLUME)
        {
            throw new Exception("Ошибка. Произошло переполнение памяти");
        }

    }
    
    // Обработка директив
    private void HandleDirective(string[] line, int index, string dirName) 
    {
        if (dirName != "START" && !isStarted)
        {
            throw new Exception("Ошибка. В программе должна присутствовать директива START");
        }

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

                if (line.Length < 2)
                {
                    throw new Exception("Ошибка. Неверный формат директивы START");
                }

                isStarted = true;

                if (line.Length > 2)
                {
                    var isAddressOk = Int32.TryParse(line[2], out int startAddress);
                    if (!isAddressOk || startAddress != 0)
                    {
                        throw new Exception("В адресе загрузки должен быть 0");
                    }
                }

                loadAddress = 0;

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

                tempRecord.Type = RecordType.H;
                tempRecord.Address = line[0];
                tempRecord.Length = "000000";

                currentSectionName = line[0];
                sectionNames.Add(currentSectionName);
                
                break;
            case "END":
                CheckSymbolicNames();

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
                }

                if (line.Length > 1)
                {
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
                } else
                {
                    endAddress = 0;
                }

                if (endAddress < loadAddress || endAddress > countAddress)
                {
                    throw new Exception("Неверный адрес входа в программу");
                }

                ControlSectionEnding(currentSectionName);

                tempRecord.Type = RecordType.E;
                tempRecord.Address =  Converters.ToSixDigits(endAddress.ToString("X"));

                break;

            case "EXTDEF":

                if (lastAddedDirective != "START" &&
                    lastAddedDirective != "CSEC" &&
                    lastAddedDirective != "EXTDEF")
                {
                    throw new Exception($"Строка {index + 1}: неверная позиция EXTDEF");
                }


                if (line.Length != 2 ||
                    !IsSame(line[0], "EXTDEF") ||
                    !Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы EXTDEF");
                }

                var exportName = line[1];

                if (
                    tableOfExtdef.Contains(exportName, StringComparer.OrdinalIgnoreCase)
                )
                {  
                    throw new Exception($"Строка {index + 1}: имя не уникально для EXTDEF");
                }

                if (tableOfExtref.Contains(exportName, StringComparer.OrdinalIgnoreCase) || sectionNames.Contains(exportName, StringComparer.OrdinalIgnoreCase))
                {
                    throw new Exception($"Строка {index + 1}: имя не уникально");
                }

                tempRecord.Type = RecordType.D;
                tempRecord.Address = exportName;

                symbolicNames.Add(new SymbolicName
                {
                    Name = exportName,
                    Section = currentSectionName,
                    Type = NameTypes.ExternalName,
                    OperandAddressList = new List<string>()
                });

                tableOfExtdef.Add(exportName.ToUpper());


                break;
            case "EXTREF":
                if (lastAddedDirective != "START" &&
                    lastAddedDirective != "CSEC" &&
                    lastAddedDirective != "EXTDEF" &&
                    lastAddedDirective != "EXTREF")
                {
                    throw new Exception($"Строка {index + 1}: неверная позиция EXTREF");
                }


                if (line.Length != 2 ||
                    !String.Equals(line[0].ToUpper(), "EXTREF") ||
                    !Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы EXTREF");
                }

                var importName = line[1];

                if (
                    tableOfExtref.Contains(importName, StringComparer.OrdinalIgnoreCase)
                )
                {
                    throw new Exception($"Строка {index + 1}: имя не уникально для EXTREF");
                }

                if (tableOfExtdef.Contains(importName, StringComparer.OrdinalIgnoreCase) || 
                    sectionNames.Contains(importName, StringComparer.OrdinalIgnoreCase))
                {
                    throw new Exception($"Строка {index + 1}: имя не уникально");
                }

                tempRecord.Type = RecordType.R;
                tempRecord.Address = importName;

                symbolicNames.Add(new SymbolicName
                {
                    Name = importName,
                    Section = currentSectionName,
                    Type = NameTypes.ExternalReference,
                });

                tableOfExtref.Add(importName.ToUpper());


                break;
            case "CSEC":
                if (line.Length != 2 || IsSame(line[0].ToUpper(), "CSEC"))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат директивы CSEC");
                }

                if (!Checks.IsRightLabel(line[0]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат метки в директиве CSEC");
                }

                if (symbolicNames.FirstOrDefault(n => n.Name == line[0]) != null ||
                    sectionNames.IndexOf(line[0].ToUpper()) != -1)
                {
                    throw new Exception($"Строка {index + 1}: имя {line[0]} уже использовано ранее ");
                }

                ControlSectionEnding(sectionNames.Last());

                binaryCodeLines.Add(new ObjectModuleRecord
                {
                    Type = RecordType.E, 
                    Address = "000000",
                });

                tempRecord.Type = RecordType.H;
                tempRecord.Address = line[0];
                tempRecord.Length = "000000";

                sectionNames.Add(line[0]);

                countAddress = 0;

                currentSectionName = line[0];

                tableOfExtdef.Clear();
                tableOfExtref.Clear();

                var beforeDir = binaryCodeLines[binaryCodeLines.Count - 2];

                if (beforeDir.Type == RecordType.H)
                {
                    beforeDir.OperandPart = "000000";
                }

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

                if (byteLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => IsSame(n.Name, byteLabel) && IsSame(n.Section, currentSectionName));
                    ProcessLabel(tempNameInfo, index, byteLabel);
                }

                tempRecord.Type = RecordType.T;
                tempRecord.Address = Converters.ToSixDigits(countAddress.ToString("X"));
                tempRecord.Length = Converters.ToTwoDigits((addingToAddress * 2).ToString("X"));
                tempRecord.OperandPart = ProcessOperand(byteStringOperand, "BYTE", index);

                checked
                {
                    countAddress += addingToAddress;
                }
                
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
                    if (IsSame(line[0], "WORD") || IsSame(line[2], "WORD"))
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

                if (wordLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => IsSame(n.Name, wordLabel) && IsSame(n.Section, currentSectionName));
                    ProcessLabel(tempNameInfo, index, wordLabel);
                }

                tempRecord.Type = RecordType.T;
                tempRecord.Address = Converters.ToSixDigits(countAddress.ToString("X"));
                tempRecord.Length = Converters.ToTwoDigits(6.ToString("X"));
                tempRecord.OperandPart = ProcessOperand(wordStringOperand, "WORD", index);

                checked
                {
                    countAddress += 3;
                }

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


                tempRecord.Type = RecordType.T;
                tempRecord.Address = Converters.ToSixDigits(countAddress.ToString("X"));
                tempRecord.Length = Converters.ToTwoDigits((resbOperand * 2).ToString("X"));

                if (resbLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => IsSame(n.Name, resbLabel) && IsSame(n.Section, currentSectionName));

                    ProcessLabel(tempNameInfo, index, resbLabel);
                }


                checked
                {
                    countAddress += resbOperand;
                }
               
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

                if (reswLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => IsSame(n.Name, reswLabel) && IsSame(n.Section, currentSectionName));
                    ProcessLabel(tempNameInfo, index, reswLabel);
                }

                tempRecord.Type = RecordType.T;
                tempRecord.Address = Converters.ToSixDigits(countAddress.ToString("X"));
                tempRecord.Length = Converters.ToTwoDigits((reswOperand * 6).ToString("X"));

                checked
                {
                    countAddress += reswOperand * 3;
                }

               
                if (countAddress > MAX_MEMORY_VOLUME)
                {
                    throw new Exception("Ошибка. Произошло переполнение памяти");
                }
                break;
            default:
                break;
        }

        lastAddedDirective = dirName;
    }

    private void ProcessLabel(SymbolicName nameFoundInTable, int index, string label)
    {

        // Если нет в ТСИ, то добавляем
        if (nameFoundInTable == null)
        {
            symbolicNames.Add(new SymbolicName()
            {
                Name = label.ToUpper(),
                Address = Converters.ToSixDigits(countAddress.ToString("X")),
                Section = currentSectionName
            });

            return;
        }

        // Если внешняя ссылка - ошибка
        if (nameFoundInTable.Type == NameTypes.ExternalReference)
        {
            throw new Exception($"Строка {index + 1}: имя {nameFoundInTable.Name} уже есть в ТСИ как ВС");
        }


        // Если уже назначен адрес в ТСИ, то ошибка
        if (nameFoundInTable.Address != null)
        {
            throw new Exception($"строка {index + 1}: имени {label.ToUpper()} уже назаначен адрес");
        }

        if (nameFoundInTable.Type == NameTypes.ExternalName)
        {
            var defRecord = binaryCodeLines.FirstOrDefault(line => line.Type == RecordType.D
                && IsSame(line.Address, label)
                && line.Length == null // это условие позволяет не проверять секцию
            );

            defRecord.Length = Converters.ToSixDigits(countAddress.ToString("X"));
        }
        

        var substringTemplateDirect = $"${label}$";
        var substringTemplateRelative = $"#{label}#";

        for (int k = 0; k < binaryCodeLines.Count; k++)
        {
            var line = binaryCodeLines[k];

            if (line.OperandPart != null)
            {
                if (line.OperandPart.Contains(substringTemplateDirect, StringComparison.OrdinalIgnoreCase))
                {
                    line.OperandPart =
                        line.OperandPart.Replace(
                        substringTemplateDirect,
                        Converters.ToSixDigits(countAddress.ToString("X")),
                        StringComparison.OrdinalIgnoreCase
                    );
                }
                else if (line.OperandPart.Contains(substringTemplateRelative, StringComparison.OrdinalIgnoreCase))
                {
                    int nextAddress;

                    if (k == binaryCodeLines.Count - 1)
                    {
                        nextAddress = countAddress;
                    }
                    else
                    {
                        nextAddress = Int32.Parse(binaryCodeLines[k + 1].Address, NumberStyles.HexNumber);
                    }

                    line.OperandPart =
                        line.OperandPart.Replace(
                        substringTemplateRelative,
                        Converters.ToSixDigits((countAddress - nextAddress).ToString("X")),
                        StringComparison.OrdinalIgnoreCase
                        );

                }
            }
        }


        nameFoundInTable.Address = Converters.ToSixDigits(countAddress.ToString("X"));
        nameFoundInTable.OperandAddressList = null;

    }

    private void ProcessLabelOperand(string name, int index)
    {
        // Проверка и вычленение имени из оператора []
        bool isRelative = Checks.IsRightRelativeAddressing(name);

        if (isRelative)
        {
            name = name.Substring(1, name.Length - 2);
        }

        int symbolicNameIndex;

        // Случай, когда ТСИ пуста
        if (symbolicNames.Count == 0)
        {

            symbolicNames.Add(new SymbolicName()
            {
                Name = name.ToUpper(),
                OperandAddressList = new List<string> { Converters.ToSixDigits(countAddress.ToString("X")) },
                Section = currentSectionName
            });

            // Вставка в ТМ, если адресация не относительная
            if (!isRelative)
            {
                var operandAddress = Converters.ToSixDigits(countAddress.ToString("X"));

                if (symbolicNames.FirstOrDefault(sn => IsSame(sn.Name, name) 
                    && sn.Type == NameTypes.ExternalReference 
                    && sn.Section == currentSectionName) != null)
                {
                    operandAddress += $" {name}";
                }

                modifiers.Add(new Modifier
                {
                    Address = operandAddress,
                    Section = currentSectionName
                });
            }

            return;
        }

        // Иначе находим все поля в ТСИ с этим же именем
        var symbolicName = symbolicNames.FirstOrDefault(sn => IsSame(sn.Name, name) && IsSame(sn.Section, currentSectionName));

        // Если имя уже определено ранее, то вставляем в ТМ, если не относительная адресация
        if (symbolicName != null && symbolicName.Address != null)
        {
            if (!isRelative)
            {
                var operandAddress = Converters.ToSixDigits(countAddress.ToString("X"));

                if (symbolicNames.FirstOrDefault(sn => IsSame(sn.Name, name)
                    && sn.Type == NameTypes.ExternalReference
                    && sn.Section == currentSectionName) != null)
                {
                    operandAddress += $" {name}";
                }

                modifiers.Add(new Modifier
                {
                    Address = operandAddress,
                    Section = currentSectionName
                });
            }
            return;
        }


        if (symbolicName == null)
        {
            symbolicNames.Add(new SymbolicName()
            {
                Name = name.ToUpper(),
                OperandAddressList = new List<string> { Converters.ToSixDigits(countAddress.ToString("X")) },
                Section = currentSectionName
            });
        } 
        else if (symbolicName.Type != NameTypes.ExternalReference)
        {

            if (symbolicName.OperandAddressList == null)
            {
                symbolicName.OperandAddressList = new List<string>();
            }

            symbolicName.OperandAddressList.Add(Converters.ToSixDigits(countAddress.ToString("X")));
        }

        // Вставка в ТМ, если адресация не относительная
        if (!isRelative)
        {

            var operandAddress = Converters.ToSixDigits(countAddress.ToString("X"));

            if (symbolicNames.FirstOrDefault(sn => IsSame(sn.Name, name)
                    && sn.Type == NameTypes.ExternalReference
                    && sn.Section == currentSectionName) != null)
            {
                operandAddress += $" {name}";
            }

            modifiers.Add(new Modifier
            {
                Address = operandAddress,
                Section = currentSectionName
            });
        }

    }

    private string ProcessOperand(string operand, string operation, int index)
    {
        if (operand == null) return "";

        if (Checks.IsNumber(operand))
        {
            var hexNumber = Int32.Parse(operand);
            switch (operation)
            {
                case "BYTE":
                    return Converters.ToTwoDigits(hexNumber.ToString("X"));
                case "WORD":
                    return Converters.ToSixDigits(hexNumber.ToString("X"));
                default:
                    var op = operations.FirstOrDefault(o => IsSame(o.MnemonicCode, operation));
                    if (op == null || op.CommandLength == 4)
                    {
                        return Converters.ToSixDigits(hexNumber.ToString("X"));
                    }

                    return Converters.ToTwoDigits(hexNumber.ToString("X"));
            }
        }



        if (Checks.IsByteArray(operand))
        {
            return operand.Substring(2, operand.Length - 3).ToUpper();
        }

        if (Checks.IsCharString(operand))
        {
            var charsStringBuilder = new StringBuilder();
            var chars = Encoding.ASCII.GetBytes(operand.Substring(2, operand.Length - 3));

            foreach (var character in chars)
            {
                charsStringBuilder.Append(Converters.ToTwoDigits(character.ToString("X")));
            }

            return charsStringBuilder.ToString();
        }

        if (Checks.IsRegister(operand))
        {
            return Int32.Parse(operand.Substring(1)).ToString("X");
        }

        bool isRelative = Checks.IsRightRelativeAddressing(operand);

        SymbolicName symbolicName;

        if (isRelative)
        {
            operand = operand.Substring(1, operand.Length - 2);
        }

        symbolicName = symbolicNames
            .FirstOrDefault(n => IsSame(n.Name, operand) && IsSame(n.Section, currentSectionName));



        if (symbolicName != null && symbolicName.Type == NameTypes.ExternalReference)
        {
            if (isRelative)
            {
                throw new Exception($"Строка {index + 1}: внешняя ссылка не может участвовать в относительной адресации");
            }

            return "000000"; 
        }

        if (symbolicName == null || symbolicName.Address == null )
        {
            if (isRelative)
            {
                return $"#{operand}#";
            }

            return $"${operand}$";    
        }

        if (isRelative)
        {
            var nameAddress = Int32.Parse(symbolicName.Address, NumberStyles.HexNumber);
            var length = operations
                .First(op => IsSame(operation, op.MnemonicCode))
                .CommandLength;

            var nextAddress = countAddress + length;

            return Converters.ToSixDigits((nameAddress - nextAddress).ToString("X"));
        }

        return symbolicName.Address;

    }

    private void AddModifyingLinesToObjectModule()
    {
        ObjectModuleRecord modRecord;

        foreach (var mod in modifiers)
        {
            modRecord = new ObjectModuleRecord
            {
                Type = RecordType.M,
                Address = mod.Address
            };

            binaryCodeLines.Add(modRecord);
        }
    }

    public static bool IsSame(string str1, string str2)
    {
        return String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }

    private void ControlSectionEnding(string sectionName)
    {
        var sectionPart = symbolicNames
            .Where(name => IsSame(name.Section, sectionName) && name.Type != NameTypes.ExternalReference)
            .ToList();

        if (sectionPart.Count == 0) return;

        var nameWithoiutAddress = sectionPart.FirstOrDefault(name => name.Address == null);

        if (nameWithoiutAddress != null)
        {
            throw new Exception($"Ошибка: имени {nameWithoiutAddress.Name} не присвоен адрес");
        }

        var sectionHeaderRecord = binaryCodeLines.FirstOrDefault(
            line => line.Type == RecordType.H 
                && IsSame(line.Address, sectionName));

        sectionHeaderRecord.OperandPart = Converters.ToSixDigits(countAddress.ToString("X"));

        var modifications = modifiers
            .Where(m => IsSame(m.Section, sectionName))
            .ToList();

        if (modifications.Count > 0)
        {
            foreach (var item in modifications)
            {
                binaryCodeLines.Add(new ObjectModuleRecord
                {
                    Type = RecordType.M,
                    Address = item.Address
               });
            }
        }

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}