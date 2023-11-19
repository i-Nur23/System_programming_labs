﻿using Lab1.Models;
using Lab1.Tables;
using System.Globalization;
using System.Text.RegularExpressions;
using Lab1.Exceptions;
using System.Runtime.CompilerServices;
using Lab1.Singletons;
using System.ComponentModel;
using System.Collections;
using System.Runtime;
using System.Text;

namespace Lab1.Passes;

public class Pass : IEnumerable<ObjectModuleRecord>
{
    private SymbolicName tempNameInfo;
    

    private int loadAddress;
    private int countAddress;
    private int endAddress;

    private const int MAX_MEMORY_VOLUME = 16_777_215;
    private const int MAX_BYTE = 255;
    private const int MAX_WORD = 16_777_215;

    private readonly List<string> code;
    private readonly List<Operation> operations;

    private bool isStarted = false;
    private bool isEnded = false;
    private BindingList<SymbolicName> symbolicNames = SymbolicNamesList.GetInstance();
    private ObjectModuleRecord tempRecord; 
    private BindingList<ObjectModuleRecord> binaryCodeLines = ObjectModuleList.GetInstance();

    private StringBuilder sb = new StringBuilder();


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

            tempNameInfo = symbolicNames.FirstOrDefault(n => n.Name == line[0].ToUpper());

            ProcessLabel(tempNameInfo, index, line[0]);
            
            if (line.Length == 2 || Checks.IsDirectAddressing(line[2]))
            {
                binaryCode = operation.BinaryCode * 4;
            }
            else
            {
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
                    if (!Checks.IsRightLabel(line[3]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
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
            else
            {
                if (!Checks.IsRightLabel(line[1]))
                {
                    throw new Exception($"Строка {index + 1}: неверный формат операнда");
                }
                
                binaryCode = operation.BinaryCode * 4 + 1;

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
            
            if (lineElementsCount == 3)
            {
                if (!Checks.IsDirectAddressing(line[2]))
                {
                    if (!Checks.IsRightLabel(line[2]))
                    {
                        throw new Exception($"Строка {index + 1}: неверный формат операнда");
                    }
                }    
            }
        }

        sb.Append(Converters.ToTwoDigits(binaryCode.ToString("X")));

        if (!String.Equals(line[0], commandName, StringComparison.OrdinalIgnoreCase))
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
                
                if (!Checks.IsRegister(line[1]) && commandLength == 2)
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

                isStarted = true;

                var isAddressOk = Int32.TryParse(
                        line[2],
                        NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture,
                        out int addressInCommand
                    );

                if (!isAddressOk)
                {
                    throw new Exception("В адресе загрузки не число или число превышающее память");
                }

                if (addressInCommand == 0)
                {
                    throw new Exception("В адресе загрузки не должно быть 0");
                }

                loadAddress = addressInCommand;

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
                tempRecord.Length = Converters.ToSixDigits(line[2]);
                
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
                }

                if (endAddress < loadAddress || endAddress > countAddress)
                {
                    throw new Exception("Неверный адрес входа в программу");
                }

                tempRecord.Type = RecordType.E;
                tempRecord.Address =  Converters.ToSixDigits(endAddress.ToString("X"));

                binaryCodeLines.First().OperandPart = Converters.ToSixDigits((countAddress - loadAddress).ToString("X"));

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
                    tempNameInfo = symbolicNames.FirstOrDefault(n => String.Equals(n.Name, byteLabel, StringComparison.OrdinalIgnoreCase));
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

                if (wordLabel != null)
                {
                    tempNameInfo = symbolicNames.FirstOrDefault(n => String.Equals(n.Name, wordLabel, StringComparison.OrdinalIgnoreCase));
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
                    tempNameInfo = symbolicNames.FirstOrDefault(n => String.Equals(n.Name, resbLabel, StringComparison.OrdinalIgnoreCase));

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
                    tempNameInfo = symbolicNames.FirstOrDefault(n => String.Equals(n.Name, reswLabel, StringComparison.OrdinalIgnoreCase));
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
    }

    private void ProcessLabel(SymbolicName nameFoundInTable, int index, string label)
    {
        if (nameFoundInTable == null)
        {
            symbolicNames.Add(new SymbolicName()
            {
                Name = label.ToUpper(),
                Address = Converters.ToSixDigits(countAddress.ToString("X"))
            });

            return;
        }


        if (nameFoundInTable.Address != null)
        {
            throw new Exception($"строка {index + 1}: имени {label.ToUpper()} уже назаначен адрес");
        }

        var indexOfName = symbolicNames.IndexOf(nameFoundInTable);
        var countOfName = symbolicNames.Count(n => String.Equals(n.Name, nameFoundInTable.Name));

        var substringTemplate = $"${label}$";

        foreach (var line in binaryCodeLines)
        {
            if (line.OperandPart != null && line.OperandPart.Contains(substringTemplate, StringComparison.OrdinalIgnoreCase))
            {
                line.OperandPart = line.OperandPart.Replace(substringTemplate, Converters.ToSixDigits(countAddress.ToString("X")), StringComparison.OrdinalIgnoreCase);
            }
        }

        for (int j = 0; j < countOfName; j++)
        {
            symbolicNames
                .Remove(symbolicNames.FirstOrDefault(n => String.Equals(n.Name, nameFoundInTable.Name, StringComparison.OrdinalIgnoreCase)));
        }

        symbolicNames.Insert(indexOfName, new SymbolicName()
        {
            Name = label.ToUpper(),
            Address = Converters.ToSixDigits(countAddress.ToString("X"))
        });

    }

    private void ProcessLabelOperand(string name, int index)
    {
        int symbolicNameIndex;

        if (symbolicNames.Count == 0)
        {
            symbolicNameIndex = 0;

            symbolicNames.Insert(symbolicNameIndex, new SymbolicName()
            {
                Name = name.ToUpper(),
                OperandAddress = Converters.ToSixDigits(countAddress.ToString("X"))
            });

            return;
        }

        var names = symbolicNames
            .Where(sn => String.Equals(sn.Name, name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (names.Count != 0 && names[0].Address != null)
        {
            return;
        }

        if (names.Count == 0)
        {
            symbolicNameIndex = symbolicNames.Count - 1;
        }
        else
        {
            symbolicNameIndex = symbolicNames.IndexOf(names.Last());
        }

        symbolicNames.Insert(symbolicNameIndex, new SymbolicName()
        {
            Name = name.ToUpper(),
            OperandAddress = Converters.ToSixDigits(countAddress.ToString("X"))
        }); 

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
                    return Converters.ToSixDigits(hexNumber.ToString("X"));
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

        SymbolicName symbolicName;

        symbolicName = symbolicNames
            .FirstOrDefault(n => String.Equals(n.Name, operand, StringComparison.OrdinalIgnoreCase));

        if (symbolicName == null || symbolicName.Address == null )
        {
            return $"${operand}$";    
        }

        return symbolicName.Address;

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}