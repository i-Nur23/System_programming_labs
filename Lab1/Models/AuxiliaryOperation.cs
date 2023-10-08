namespace Lab1.Models;

public enum Line {
    DIRECTIVE,
    COMMAND
}

public class AuxiliaryOperation
{
    public string Address { get; set; }

    public string BinaryCode { get; set; }

    public string FirstOperand { get; set; } = null;

    public string SecondOperand { get; set; } = null;

    public Line LineType { get; set; }    
}