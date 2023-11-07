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

    public string Section { get; set; }

    public NameTypes Type { get; set; }
}