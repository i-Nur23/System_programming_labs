namespace Lab1.Models;

public enum NameTypes
{
    SymbolicName = 0,
    ExternalName = 1,
    ExternalReference = 2,
}

public class SymbolicName
{
    public string Name { get; set; }
    
    public string Address { get; set; }

    internal List<string> OperandAddressList { get; set; }

    public string OperandAddress => OperandAddressList == null ? null : String.Join("\n", OperandAddressList);

    internal NameTypes Type { get; set; } = NameTypes.SymbolicName;

    public string StringType {
        get
        {
            return Type switch
            {
                NameTypes.SymbolicName => "",
                NameTypes.ExternalName => "ВИ",
                NameTypes.ExternalReference => "ВС",
            };
        }
    }

    public string Section { get; set; }
}