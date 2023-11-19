using Lab1.Models;
using System.ComponentModel;

namespace Lab1.Singletons
{
    public class ObjectModuleList
    {
        private static BindingList<ObjectModuleRecord> instance;

        private ObjectModuleList()
        { }

        public static BindingList<ObjectModuleRecord> GetInstance()
        {
            if (instance == null)
                instance = new BindingList<ObjectModuleRecord>();
            return instance;
        }
    }
}
