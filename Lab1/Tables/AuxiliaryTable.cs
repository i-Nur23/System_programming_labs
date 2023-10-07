using Lab1.Models;

namespace Lab1.Tables;

public class AuxiliaryTable
{
    private readonly DataGridView table;
    
    private List<AuxiliaryOperation> operations = new List<AuxiliaryOperation>();

    public AuxiliaryTable(DataGridView table)
    {
        this.table = table;
    }

    public void Add(AuxiliaryOperation operation)
    {
        table.Rows.Add(
            operation.Address,
            operation.BinaryCode,
            operation.FirstOperand,
            operation.SecondOperand
        );
    }

    public void Clear()
    {
        table.Rows.Clear();
    }

}