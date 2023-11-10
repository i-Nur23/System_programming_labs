using Lab1.Exceptions;
using Lab1.Models;
using Lab1.Passes;
using Lab1.Tables;

namespace Lab1;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        operationCodesTable = new OperationCodes(dg_operCodes);
        auxiliaryTable = new AuxiliaryTable(dg_aux);
        symbolicNamesTable = new SymbolicNames(dg_symbolNames);
        binaryCodeTextBox = new BinaryCodeTextBox(TB_binaryCode);
        settingTable = new SettingTable(dg_setting);

        FillExample();

        Addressing.CreateAddressing(0);
        addressing = Addressing.GetAddressing();

        comboBox1.SelectedIndex = 0;

    }

    private OperationCodes operationCodesTable;
    private AuxiliaryTable auxiliaryTable;
    private SymbolicNames symbolicNamesTable;
    private BinaryCodeTextBox binaryCodeTextBox;
    private SettingTable settingTable;

    private Addressing addressing;


    private FirstPassResult firstPassResult;

    // ���������� ������� �� ������
    private void FillExample()
    {
        var code = DirectCode();

        tb_initCode.AppendText(String.Join('\n', code));

        var operations = new List<Operation> {
            new Operation { MnemonicCode = "JMP", BinaryCode = 1, CommandLength = 4 },
            new Operation { MnemonicCode = "LOADR1", BinaryCode = 2, CommandLength = 4 },
            new Operation { MnemonicCode = "LOADR2", BinaryCode = 3, CommandLength = 4 },
            new Operation { MnemonicCode = "ADD", BinaryCode = 4, CommandLength = 2 },
            new Operation { MnemonicCode = "SAVER1", BinaryCode = 5, CommandLength = 4 },
        };

        operationCodesTable?.AddRange(operations);

    }

    private string[] DirectCode()
    {
        return File.ReadAllLines("..\\..\\..\\Examples\\direct.txt");
    }

    private string[] RelativeCode()
    {
        return File.ReadAllLines("..\\..\\..\\Examples\\relative.txt");
    }

    private string[] MixedCode()
    {
        return File.ReadAllLines("..\\..\\..\\Examples\\mixed.txt");
    }

    // �������� ������ ����� �� ����
    private List<string> GetInitCodeWithoutEmptyRows()
    {
        var initCodeRows = tb_initCode.Text.Split("\n").ToList();

        return initCodeRows.FindAll(str => !String.IsNullOrWhiteSpace(str));
    }

    // ������ ������
    private void btn_firstPass_Click(object sender, EventArgs e)
    {
        var initCode = GetInitCodeWithoutEmptyRows();

        try
        {
            TB_firstPassError.Clear();
            auxiliaryTable.Clear();
            symbolicNamesTable.Clear();
            binaryCodeTextBox.Clear();
            settingTable.Clear();

            var fp = new FirstPass(
                initCode,
                operationCodesTable.GetOperations(),
                auxiliaryTable,
                symbolicNamesTable
                );

            firstPassResult = fp.Run();

            btn_firstPass.Enabled = false;
            btn_secondPass.Enabled = true;
        }

        catch (WrongAddressException ex)
        {
            MessageBox.Show(ex.Message);
        }

        catch (Exception ex)
        {
            TB_firstPassError.Text = ex.Message;
            //TB_firstPassError.AppendText(ex.StackTrace);
        }

    }


    // ������ ������
    private void btn_secondPass_Click(object sender, EventArgs e)
    {
        try
        {

            foreach (var sectionInfo in firstPassResult.SectionInfos)
            {
                var sp = new SecondPass(
                    sectionInfo,
                    binaryCodeTextBox,
                    settingTable
                    );
                sp.Run();
            }

            btn_firstPass.Enabled = true;
            btn_secondPass.Enabled = false;
        }

        catch (Exception ex)
        {
            TB_secondPassError.Text = ex.Message;
            //TB_secondPassError.AppendText(ex.StackTrace);
        }
    }

    // ������ ���� �������� ��� ��������� ��������� ����
    private void tb_initCode_TextChanged(object sender, EventArgs e)
    {
        ClearAll();
    }

    // ������ ���� �������� ��� ��������� ���
    private void dg_operCodes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        ClearAll();
    }

    private void ClearAll()
    {
        TB_firstPassError.Clear();
        TB_secondPassError.Clear();

        auxiliaryTable?.Clear();
        symbolicNamesTable?.Clear();
        binaryCodeTextBox?.Clear();
        settingTable?.Clear();

        btn_firstPass.Enabled = true;
        btn_secondPass.Enabled = false;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] code = null;

        switch (comboBox1.SelectedIndex)
        {
            case 0:
                code = DirectCode();
                addressing.AddressType = AddressingType.DIRECT;
                break;
            case 1:
                code = RelativeCode();
                addressing.AddressType = AddressingType.RELATIVE;
                break;
            case 2:
                code = MixedCode();
                addressing.AddressType = AddressingType.MIXED;
                break;
        }

        ClearAll();
        tb_initCode.Clear();

        tb_initCode.AppendText(String.Join('\n', code));
    }
}