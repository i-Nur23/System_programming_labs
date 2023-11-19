using Lab1.Singletons;

namespace Lab1.Tables
{
    public class ModificationsTable
    {
        private DataGridView table;

        public ModificationsTable(DataGridView table)
        {
            this.table = table;
            var bindingList = ModifiersList.GetInstance();
            var source = new BindingSource(bindingList, null);
            table.DataSource = source;
        }

        public void Clear()
        {
            table.Rows.Clear();
        }
    }
}
