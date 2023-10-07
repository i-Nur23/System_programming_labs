using Lab1.Models;
using Lab1.Tables;
using System.Globalization;
using System.Net.Mail;

namespace Lab1.Passes;

public class FirstPass
{
    private int loadAddress;
    private int countAddress;
    private int endAddress;

    private const int MAX_MEMORY_VOLUME = 16_777_216;

    private readonly List<string> code;
    private readonly List<Operation> operations;
    private readonly AuxiliaryTable auxiliaryTable;
    private readonly SymbolicNames symbolicNames;

    private bool isStarted = false;
    private bool isEnded = false;

    public FirstPass(
        List<string> code, 
        List<Operation> operations, 
        AuxiliaryTable auxiliaryTable, 
        SymbolicNames symbolicNames
        )

    {
        this.code = code;
        this.operations = operations;
        this.auxiliaryTable = auxiliaryTable;   
        this.symbolicNames = symbolicNames;
    }


    public string ToSixDigits(string initStr)
    {
        var strWithZeros = "000000" + initStr;
        return strWithZeros.Substring(strWithZeros.Length - 6);
    }

    public void Run()
    {
        var linesCount = code.Count;
        string[] splittedLine;

        for (int i = 0; i < linesCount; i++)
        {
            splittedLine = code[i].Trim().Split(" ");
            if (splittedLine.Length > 4)
            {
                throw new Exception($"Строка {i + 1}: неыерный формат строки - больше 4-х элементов");
            }

            Line line = Checks.getTypeOfLine(splittedLine, i, operations, out string name);

            switch (line)
            {
                case Line.DIRECTIVE:
                    HandleDirective(splittedLine, i, name);
                    break;
                case Line.COMMAND:
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleDirective(string[] line, int index, string dirName) 
    {
        switch (dirName)
        {
            case "START":

                if (index != 0 || isStarted)
                {
                    throw new Exception("Директива START должна встречаться один раз и в начале программы");
                }

                if (line.Length != 3)
                {
                    throw new Exception("Не указано имя программы или адрес загрузки");
                }

                isStarted = true;

                var isAddressOk = Int32.TryParse(
                    line[2], 
                    NumberStyles.HexNumber, 
                    CultureInfo.CurrentCulture,
                    out loadAddress
                );

                if (!isAddressOk)
                {
                    throw new Exception("В адресе загрузки указано не число");
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

                auxiliaryTable.Add(new AuxiliaryOperation
                {
                    Address = line[0],
                    BinaryCode = line[1],
                    FirstOperand = line[2],
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

                if (line.Length == 1)
                {
                    endAddress = loadAddress;
                    auxiliaryTable.Add(new AuxiliaryOperation { Address = ToSixDigits(countAddress.ToString("X")), BinaryCode = "END" });
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

                auxiliaryTable.Add(new AuxiliaryOperation { Address = ToSixDigits(countAddress.ToString("X")), BinaryCode = "END", FirstOperand = line[1] });

                break;
            case "BYTE":
                break;
            case "WORD":
                break;
            case "RESB":
                break;
            case "RESW":
                break;
            default:
                break;
        }
    }
}