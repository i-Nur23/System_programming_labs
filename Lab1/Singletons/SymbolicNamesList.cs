

using Lab1.Models;
using System.ComponentModel;

namespace Lab1.Singletons
{
    public class SymbolicNamesList
    {
        private static BindingList<SymbolicName> instance;

        private SymbolicNamesList()
        { }

        public static BindingList<SymbolicName> GetInstance()
        {
            if (instance == null)
                instance = new BindingList<SymbolicName>();
            return instance;
        }
    }
}
