using System.ComponentModel;

namespace Lab1.Models;

public class Operation
{
    [DisplayName("MnemonicCode")]
    public string MnemonicCode { get; set; }

    [DisplayName("BinaryCode")]
    public int BinaryCode { get; set; }

    [DisplayName("CommandLength")]
    public int CommandLength { get; set; }
}