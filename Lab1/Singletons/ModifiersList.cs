using Lab1.Models;
using System.ComponentModel;

namespace Lab1.Singletons
{
    public class ModifiersList
    {
        private static BindingList<Modifier> instance;

        private ModifiersList()
        { }

        public static BindingList<Modifier> GetInstance()
        {
            if (instance == null)
                instance = new BindingList<Modifier>();
            return instance;
        }

    }
}
