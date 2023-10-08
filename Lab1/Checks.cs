using Lab1.Models;
using System.Text.RegularExpressions;

namespace Lab1;

public static class Checks
{
    private static List<string> directives = new List<string> { "START", "END", "BYTE", "WORD", "RESB", "RESW" };
    private static List<string> registers = new List<string> { "R0", "R1", "R2", "R3", "R4", "R5", "R6","R7","R8","R9",
        "R10","R11","R12", "R13","R14","R15" };
    
    private static List<char> hexSymbols = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7','8','9','A','B','C','D','E','F' };
    
    public static Func<string, bool> isDirective = command => directives.IndexOf(command.ToUpper()) != -1;
    
    public static Func<string, bool> isRegister = command => registers.IndexOf(command.ToUpper()) != -1;

    public static Line getTypeOfLine(string[] line, int index, List<Operation> operations, out string name)
    {
        name = "";
        var CommandsCount = 0;
        var DirectivesCount = 0;

        foreach (var item in line)
        {
            if (isDirective(item))
            {
                DirectivesCount++;
                name = item.ToUpper();
            }

            else if (operations.FirstOrDefault(op => op.MnemonicCode == item) != null)
            {
                CommandsCount++;
                name = item.ToUpper();
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

        if (CommandsCount > 0) return Line.COMMAND;

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

}