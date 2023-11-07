using Lab1.Models;

namespace Lab1.Tables;

public class SymbolicNames
{
    private readonly DataGridView table;
    
    private List<SymbolicName> names = new List<SymbolicName>();

    public SymbolicNames(DataGridView table)
    {
        this.table = table;
    }

    public void Add(SymbolicName name)
    {
        var nameType = name.Type != NameTypes.SymbolicName ?
            name.Type == NameTypes.ExternalName ? "ВИ" : "ВС"
            : "";

        table.Rows.Add(name.Name, name.Address, name.Section, nameType);
    }

    public void Clear()
    {
        table.Rows.Clear();
    }
}