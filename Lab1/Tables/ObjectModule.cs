using Lab1.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Tables
{
    public class ObjectModule
    {
        private readonly DataGridView table;

        public ObjectModule(DataGridView table)
        {
            this.table = table;
            var bindingList = ObjectModuleList.GetInstance();
            var source = new BindingSource(bindingList, null);
            table.DataSource = source;
        }

        public void Clear()
        {
            table.Rows.Clear();
        }

    }
}
