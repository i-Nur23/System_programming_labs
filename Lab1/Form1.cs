using Lab1.Models;
using Lab1.Passes;
using Lab1.Tables;
using System.Runtime.CompilerServices;

namespace Lab1;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        operationCodesTable = new OperationCodes(dg_operCodes);
        auxiliaryTable = new AuxiliaryTable(dg_aux);
        symbolicNames = new SymbolicNames(dg_symbolNames);

        FillExample();
    }

    private OperationCodes operationCodesTable;
    private AuxiliaryTable auxiliaryTable;
    private SymbolicNames symbolicNames;

    // Заполнение примера из лекции
    private void FillExample()
    {
        var code = new string[] {
            "Program START 100",
            "   JMP L1",
            "A1 RESB 10",
            "A2 RESW 20",
            "B1 WORD 4096",
            "B2 BYTE x\'2F4c008A\'",
            "B3 BYTE c\'Hello!\'",
            "B4 BYTE 120",
            "L1 LOADR1 B1",
            "   LOADR2 B4",
            "   ADD R1 R2",
            "   SAVER1 B1",
            "   END 100",
        };

        tb_initCode.AppendText(String.Join('\n', code));


        var operations = new List<Operation> {
            new Operation { MnemonicCode = "JMP", BinaryCode = 1, CommandLength = 4 },
            new Operation { MnemonicCode = "LOADR1", BinaryCode = 2, CommandLength = 4 },
            new Operation { MnemonicCode = "LOADR2", BinaryCode = 3, CommandLength = 4 },
            new Operation { MnemonicCode = "ADD", BinaryCode = 4, CommandLength = 2 },
            new Operation { MnemonicCode = "SAVER1", BinaryCode = 19, CommandLength = 4 },
        };

        operationCodesTable?.AddRange(operations);

    }

    // Удаление пустых строк из кода
    private List<string> GetInitCodeWithoutEmptyRows()
    {
        var initCodeRows = tb_initCode.Text.Split("\n").ToList();

        return initCodeRows.FindAll(str => !String.IsNullOrWhiteSpace(str));
    }

    // Первый проход
    private void btn_firstPass_Click(object sender, EventArgs e)
    {
        var initCode = GetInitCodeWithoutEmptyRows();

        try
        {
            TB_firstPassError.Text = "";
            auxiliaryTable.Clear();
            symbolicNames.Clear();

            var fp = new FirstPass(initCode, operationCodesTable.GetOperations(), auxiliaryTable, symbolicNames);
            fp.Run();

            btn_firstPass.Enabled = false;
            btn_secondPass.Enabled = true;
        }
        catch (Exception ex)
        {
            TB_firstPassError.Text = ex.Message;
        }

    }


    // Второй проход
    private void btn_secondPass_Click(object sender, EventArgs e)
    {
        try
        {
            btn_firstPass.Enabled = true;
            btn_secondPass.Enabled = false;
        }
        catch (Exception ex)
        {
            TB_secondPassError.Text = ex.Message;
        }
    }
}