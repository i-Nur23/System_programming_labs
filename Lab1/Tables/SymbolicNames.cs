using Lab1.Models;
using Lab1.Singletons;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Linq;

namespace Lab1.Tables;

public class SymbolicNames
{
    private readonly DataGridView table;
    
    private List<SymbolicName> names = new List<SymbolicName>();

    public SymbolicNames(DataGridView table)
    {
        this.table = table;

        var bindingList = SymbolicNamesList.GetInstance();
        var source = new BindingSource(bindingList, null);
        table.DataSource = source;
    }

    public void Clear()
    {
        table.Rows.Clear();
    }
}