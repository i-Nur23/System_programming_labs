
namespace Lab1.Models
{

    public enum RecordType
    {
        H,
        D,
        R,
        T,
        M,
        E
    }

    public class ObjectModuleRecord
    {
        public RecordType Type { get; set; }

        public string Address { get; set; }

        public string Length { get; set; }

        public string OperandPart { get; set; }


    }
}
