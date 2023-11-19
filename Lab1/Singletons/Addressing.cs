using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Singletons
{
    public enum AddressingType
    {
        DIRECT = 0,
        RELATIVE = 1,
        MIXED = 2
    }

    public class Addressing
    {
        public AddressingType AddressType { get; set; }

        private static Addressing addressing;

        private Addressing(int index)
        {
            AddressType = (AddressingType)index;
        }

        public static Addressing GetAddressing()
        {
            CreateAddressing(0);
            return addressing;
        }

        public static void CreateAddressing(int index)
        {
            if (addressing == null)
                addressing = new Addressing(index);
        }
    }
}
