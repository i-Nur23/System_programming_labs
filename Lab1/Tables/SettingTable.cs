using System.Data;

namespace Lab1.Tables;

public class SettingTable
{
    private readonly DataGridView _table;

    public SettingTable(DataGridView table)
    {
        _table = table;
    }

    public void Add(List<string> settings)
    {
        foreach (var setting in settings)
        {
            _table.Rows.Add(setting);   
        }
    }

    public void Clear()
    {
        _table.Rows.Clear();
    }
}