namespace Lab1.Models;

public class FirstPassResult
{
    public List<AuxiliaryOperation> AuxiliaryOperations { get; set; }
    
    public List<SymbolicName> SymbolicNames { get; set; }

    public int LoadAddress { get; set; }
    
    public int ProgramLength { get; set; }
    
}