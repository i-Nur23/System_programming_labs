using System.Data;
using Lab1.Models;

namespace Lab1.Tables;

public class SettingTable
{
    private readonly DataGridView _table;

    public SettingTable(DataGridView table)
    {
        _table = table;
    }

    public void Add(List<Modifier> settings)
    {
        foreach (var setting in settings)
        {
            _table.Rows.Add(setting.Address, setting.Label);   
        }
    }

    public void Clear()
    {
        _table.Rows.Clear();
    }
}