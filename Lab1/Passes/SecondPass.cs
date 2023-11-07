using System.Globalization;
using System.Text;
using Lab1.Models;
using Lab1.Tables;

namespace Lab1.Passes;

public class SecondPass
{

    private readonly BinaryCodeTextBox _binaryCodeTextBox;
    private readonly SettingTable _settingTable;
    private readonly List<AuxiliaryOperation> _auxiliaryOperations;
    private readonly List<SymbolicName> _symbolicNames;
    private readonly int _loadAddress;
    private readonly int _programLength;
    private readonly string _sectionName;

    private readonly List<Modifier> _settings = new List<Modifier>();

    private List<string> binaryCodeLines = new List<string>();

    public SecondPass(
        SectionInfo section,
        BinaryCodeTextBox binaryCodeTextBox,
        SettingTable settingTable
    )
    {
        _loadAddress = section.LoadAddress;
        _programLength = section.Length;
        _auxiliaryOperations = section.AuxiliaryOperations;
        _symbolicNames = section.SymbolicNames;
        _sectionName = section.Name;
        _binaryCodeTextBox = binaryCodeTextBox;
        _settingTable = settingTable;
    }
    
    public void Run()
    {
        CreateHeader();
        var defsCount = CreateDefinitions();
        var refsCount = CreateReferences();

        for (int i = defsCount + refsCount + 1; i < _auxiliaryOperations.Count - 1; i++)
        {
            CreateBodyLine(i);    
        }

        _settingTable.Add(_settings);
        
        foreach (var setting in _settings)
        {
            CreateModificationLine(setting);
        }
        
        CreateModuleEnd();
        
        _binaryCodeTextBox.AddRange(binaryCodeLines);
    }

    private void CreateHeader()
    {
        binaryCodeLines.Add($"H {_sectionName} " +
                            $"{Converters.ToSixDigits(_loadAddress.ToString("X"))} " +
                            $"{Converters.ToSixDigits(_programLength.ToString("X"))}");    
    }

    private int CreateDefinitions()
    {
        var defs = _symbolicNames.Where(n => n.Type == NameTypes.ExternalName).ToList();

        var defsCount = defs.Count;

        foreach (var def in defs)
        {
            binaryCodeLines.Add($"D {def.Name} {def.Address}");
        }

        return defsCount;
    }

    private int CreateReferences()
    {
        var refs = _symbolicNames.Where(n => n.Type == NameTypes.ExternalReference).ToList();

        var refsCount = refs.Count;

        foreach (var reference in refs)
        {
            binaryCodeLines.Add($"R {reference.Name}");
        }

        return refsCount;
    }


    private void CreateBodyLine(int i)
    {
        StringBuilder sb = new StringBuilder($"T {_auxiliaryOperations[i].Address} ");

        var currentAddress = Int32.Parse(_auxiliaryOperations[i].Address, NumberStyles.HexNumber);
        var nextAddress = Int32.Parse(_auxiliaryOperations[i + 1].Address, NumberStyles.HexNumber);

        sb.Append(Converters.ToTwoDigits(((nextAddress - currentAddress) * 2).ToString("X")));
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

    private void CreateModificationLine(Modifier setting)
    {
        binaryCodeLines.Add($"M {setting.Address}");
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

        //commandStringBuilder.Append(" ");
        commandStringBuilder.Append(ProcessOperand(command.FirstOperand, command.BinaryCode, i));
        //commandStringBuilder.Append(" "); 
        commandStringBuilder.Append(ProcessOperand(command.SecondOperand, command.BinaryCode, i));

        return commandStringBuilder.ToString();
    }
    
    
    
    private string GetRecordForDirective(int i)
    {
        var directiveStringBuilder = new StringBuilder();

        var operation = _auxiliaryOperations[i].BinaryCode.ToUpper(); 
        
        switch (operation)
        {
            case "RESB":
            case "RESW":
                return "";
            
            case "BYTE":
            case "WORD":
                directiveStringBuilder.Append(ProcessOperand(_auxiliaryOperations[i].FirstOperand, operation, i));
                //directiveStringBuilder.Append(" "); 
                directiveStringBuilder.Append(ProcessOperand(_auxiliaryOperations[i].SecondOperand, operation, i));
                break;
        }
        
        return directiveStringBuilder.ToString();
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

        if (Checks.IsRightRelativeAddressing(operand))
        {
            symbolicName = _symbolicNames
                .FirstOrDefault(n => String.Equals(n.Name, operand.Substring(1, operand.Length - 2), StringComparison.OrdinalIgnoreCase)
                && n.Type != NameTypes.ExternalReference );

            if (symbolicName == null)
            {
                HandleException($"Ошибка. Имя {operand.Substring(1, operand.Length - 2)} не найдено в ТСИ");
            }

            var symbolAddress = Int32.Parse(symbolicName.Address, NumberStyles.HexNumber);
            var nextAddress = Int32.Parse(_auxiliaryOperations[index + 1].Address, NumberStyles.HexNumber);

            string relativeAddress = (symbolAddress - nextAddress).ToString("X");
            if (relativeAddress.Length > 6)
            {
                relativeAddress = relativeAddress.Substring(relativeAddress.Length - 6);
            }
            return relativeAddress;
        }

        symbolicName = _symbolicNames.FirstOrDefault(n => String.Equals(n.Name, operand, StringComparison.OrdinalIgnoreCase));

        if (symbolicName == null)
        {
            HandleException($"Ошибка. Имя {operand} не найдено ни в ТСИ, ни в ТВС");
        }

        if (symbolicName.Type == NameTypes.ExternalReference)
        {
            _settings.Add(new Modifier
            {
                Address = $"{_auxiliaryOperations[index].Address} {operand}",
                Label = _sectionName
            });

            return "000000";
        }

        _settings.Add(new Modifier
        {
            Address = _auxiliaryOperations[index].Address,
            Label = _sectionName
        });
        
        return symbolicName.Address;
        
    }

    private void HandleException(string text)
    {
        _settingTable.Clear();
        _binaryCodeTextBox.Clear();

        throw new Exception(text);
    }
}