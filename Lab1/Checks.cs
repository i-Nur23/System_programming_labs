using Lab1.Models;
using System.Text.RegularExpressions;

namespace Lab1;

public static class Checks
{
    private static List<string> directives = new List<string> { "START", "END", "BYTE", "WORD", "RESB", "RESW" };
    private static List<string> registers = new List<string> { "R0", "R1", "R2", "R3", "R4", "R5", "R6","R7","R8","R9",
        "R10","R11","R12", "R13","R14","R15" };
    private static List<char> hexSymbols = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7','8','9','A','B','C','D','E','F' };

    public static bool IsDirective(string command)
    {
        return directives.IndexOf(command.ToUpper()) != -1;
    }
    
    public static bool IsRegister(string command)
    {
        return registers.IndexOf(command.ToUpper()) != -1;
    }
    
    public static bool IsLetter(char symbol)
    {
        return symbol >= 65 && symbol <= 90;
    }

    public static Line getTypeOfLine(string[] line, int index, List<Operation> operations, out string name)
    {
        name = "";
        var CommandsCount = 0;
        var DirectivesCount = 0;

        foreach (var lineItem in line)
        {
            var item = lineItem.ToUpper();
            if (IsDirective(item))
            {
                DirectivesCount++;
                name = item;
            }

            else if (operations.FirstOrDefault(op => op.MnemonicCode == item) != null)
            {
                CommandsCount++;
                name = item;
            }
        }

        if (CommandsCount > 0 && DirectivesCount > 0)
        {
            throw new Exception($"Строка {index + 1}: в одной строке могут быть или директива, или команда");
        }

        if (CommandsCount == 0 && DirectivesCount == 0)
        {
            throw new Exception($"Строка {index + 1}: в строке должна быть или директива, или команда");
        }

        if (CommandsCount > 0)
        {
            if (CommandsCount > 1)
            {
                throw new($"Строка {index + 1}: в строке не может быть более одной команды");
            }
            
            return Line.COMMAND;
        }

        if (DirectivesCount > 1)
        {
            throw new($"Строка {index + 1}: в строке не может быть более одной директивы");
        }

        return Line.DIRECTIVE;
       
    }

    public static bool IsContainsOnlyHexSymbols(string str)
    {
        
        foreach (var symbol in str.ToUpper())
        {
            if (hexSymbols.IndexOf(symbol) == -1) return false;
        }

        return true;
    }

    public static bool IsNumber(string str)
    {
        return Int32.TryParse(str, out _);
    }

    public static bool IsByteArray(string str)
    {
        return (str[0] == 'x' || str[0] == 'X') && str[1] == (char)39 && str[str.Length - 1] == (char)39;
    }
    
    public static bool IsCharString(string str)
    {
        return (str[0] == 'c' || str[0] == 'C') && str[1] == (char)39 && str[str.Length - 1] == (char)39;
    }

    public static bool IsDirectAddressing(string operand)
    {
        return registers.IndexOf(operand.ToUpper()) != -1 || IsConstant(operand);
    }

    public static bool IsConstant(string operand)
    {
        return IsNumber(operand) || IsByteArray(operand) || IsCharString(operand);
    }
    
    public static bool IsStartsWithUnderscoreOrLetter(string operand)
    {
        if (operand.Length == 0) return false;
        var symbol = (operand.ToUpper())[0];
        return symbol == 95 || IsLetter(symbol);
    }

    public static bool IsOnlyLettersAndNumbers(string operand)
    {
        if (Char.IsDigit(operand[0])) return false;
        
        foreach (var symbol in operand.ToUpper())
        {
            if (!(IsLetter(symbol) || Char.IsDigit(symbol) || symbol == 95))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsRightLabel(string label)
    {
        return IsOnlyLettersAndNumbers(label) && IsStartsWithUnderscoreOrLetter(label);
    }

    public static bool IsRightRelativeAddressing(string operand)
    {
        var upperOperand = operand.ToUpper();
        if (upperOperand[0] != '[' || upperOperand[operand.Length - 1] != ']')
        {
            return false;
        }

        var label = upperOperand.Substring(1, operand.Length - 2);

        return IsRightLabel(label) && !IsDirective(label) && !IsRegister(label);
    }

}