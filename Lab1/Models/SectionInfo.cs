namespace Lab1.Models;

public class SectionInfo {
    
    public string Name { get; set; }

    public int LoadAddress { get; set; }
    
    public int Length { get; set; }
    
    //public List<AuxiliaryOperation> AuxiliaryOperations { get; set; }
    
    public List<SymbolicName> SymbolicNames { get; set; }
    
}