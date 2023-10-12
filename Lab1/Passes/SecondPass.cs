using System.Globalization;
using System.Text;
using Lab1.Models;
using Lab1.Tables;

namespace Lab1.Passes;

public class SecondPass
{

    private readonly List<AuxiliaryOperation> _auxiliaryOperations;
    private readonly List<SymbolicName> _symbolicNames;
    private readonly BinaryCodeTextBox _binaryCodeTextBox;
    private readonly int _loadAddress;
    private readonly int _programLength;

    private List<string> binaryCodeLines = new List<string>();

    public SecondPass(
        FirstPassResult firstPassResult,
        BinaryCodeTextBox binaryCodeTextBox
        )
    {
        _loadAddress = firstPassResult.LoadAddress;
        _programLength = firstPassResult.ProgramLength;
        _auxiliaryOperations = firstPassResult.AuxiliaryOperations;
        _symbolicNames = firstPassResult.SymbolicNames;
        _binaryCodeTextBox = binaryCodeTextBox;
    }
    
    public void Run()
    {
        CreateHeader();

        for (int i = 1; i < _auxiliaryOperations.Count - 1; i++)
        {
            CreateBodyLine(i);    
        }
        
        CreateModuleEnd();
        
        _binaryCodeTextBox.AddRange(binaryCodeLines);
    }

    private void CreateHeader()
    {
        binaryCodeLines.Add($"H {_auxiliaryOperations[0].Address} " +
                            $"{Converters.ToSixDigits(_loadAddress.ToString("X"))} " +
                            $"{Converters.ToSixDigits(_programLength.ToString("X"))}");    
    }

    private void CreateBodyLine(int i)
    {
        StringBuilder sb = new StringBuilder($"T {_auxiliaryOperations[i].Address} ");

        var currentAddress = Int32.Parse(_auxiliaryOperations[i].Address, NumberStyles.HexNumber);
        var nextAddress = Int32.Parse(_auxiliaryOperations[i + 1].Address, NumberStyles.HexNumber);

        sb.Append(Converters.ToTwoDigits((nextAddress - currentAddress).ToString("X")));
        sb.Append(" ");

        Func<int, string> GetRecord = _ => "";

        switch (_auxiliaryOperations[i].LineType)
        {
            case Line.DIRECTIVE:
                GetRecord = GetRecordForDirective;
                break;
            case Line.COMMAND:
                GetRecord = GetRecordForCommand;
                break;
        }

        sb.Append(GetRecord(i));

        binaryCodeLines.Add(sb.ToString());

    }

    private void CreateModuleEnd()
    {
        binaryCodeLines.Add(
            $"E {Converters.ToSixDigits(_auxiliaryOperations[_auxiliaryOperations.Count - 1].FirstOperand 
                                        ?? _loadAddress.ToString("X"))}");
    }

    private string GetRecordForCommand(int i)
    {
        var command = _auxiliaryOperations[i];
        
        var commandStringBuilder = new StringBuilder(command.BinaryCode);

        commandStringBuilder.Append(" ");
        commandStringBuilder.Append(ProcessOperand(command.FirstOperand));
        commandStringBuilder.Append(" "); 
        commandStringBuilder.Append(ProcessOperand(command.SecondOperand));

        return commandStringBuilder.ToString();
    }
    
    
    
    private string GetRecordForDirective(int i)
    {
        var directiveStringBuilder = new StringBuilder();

        switch (_auxiliaryOperations[i].BinaryCode.ToUpper())
        {
            case "RESB":
            case "RESW":
                return "";
            
            case "BYTE":
            case "WORD":
                directiveStringBuilder.Append(ProcessOperand(_auxiliaryOperations[i].FirstOperand));
                directiveStringBuilder.Append(" "); 
                directiveStringBuilder.Append(ProcessOperand(_auxiliaryOperations[i].SecondOperand));
                break;
        }
        
        return directiveStringBuilder.ToString();
    }


    private string ProcessOperand(string operand)
    {
        if (operand == null) return "";
        
        if (Checks.IsNumber(operand))
        {
            var hexNumber = Int32.Parse(operand);
            return Converters.ToSixDigits(hexNumber.ToString("X"));
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
            return Converters.ToTwoDigits(operand.Substring(1));
        }

        var symbolicName = _symbolicNames.FirstOrDefault(n => String.Equals(n.Name, operand));

        if (symbolicName == null)
        {
            throw new Exception($"Ошибка. Имя {operand} не найдено в ТСИ");
        }

        return symbolicName.Address;
        
    }
}