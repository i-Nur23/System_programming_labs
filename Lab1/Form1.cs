using Lab1.Exceptions;
using Lab1.Models;
using Lab1.Passes;
using Lab1.Singletons;
using Lab1.Tables;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab1;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        operationCodesTable = new OperationCodes(dg_operCodes);
        symbolicNamesTable = new SymbolicNames(dg_symbolNames);
        objectModule = new ObjectModule(dg_objectModule);
        modificationsTable = new ModificationsTable(dg_settings);

        FillExample();

        Addressing.CreateAddressing(0);
        addressing = Addressing.GetAddressing();
        comboBox1.SelectedIndex = 0;

    }

    private OperationCodes operationCodesTable;
    private SymbolicNames symbolicNamesTable;
    private ObjectModule objectModule;
    private ModificationsTable modificationsTable;
    private BindingList<ObjectModuleRecord> objectModuleList = ObjectModuleList.GetInstance();
    private IEnumerator<ObjectModuleRecord> passEnumerator;

    private Addressing addressing;

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

    // Заполнение примера
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

    // удаление пустых строк в коде
    private List<string> GetInitCodeWithoutEmptyRows()
    {
        var initCodeRows = tb_initCode.Text.Split("\n").ToList();

        var initCode = initCodeRows.FindAll(str => !String.IsNullOrWhiteSpace(str));

        if (initCode.Count == 0)
        {
            throw new Exception("Ошибка. Пустой исходный код");
        }

        return initCode;
    }

    // реакция на изменение кода
    private void tb_initCode_TextChanged(object sender, EventArgs e)
    {
        ClearAll();
    }

    // реакция на изменение ТКО
    private void dg_operCodes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        ClearAll();
    }

    private void ClearAll()
    {
        TB_firstPassError.Clear();
        symbolicNamesTable?.Clear();
        objectModule?.Clear();
        modificationsTable?.Clear();
        passEnumerator = null;
    }

    private void btn_start_Click(object sender, EventArgs e)
    {
        try
        {
            ClearAll();

            var initCode = GetInitCodeWithoutEmptyRows();

            TB_firstPassError.Clear();
            symbolicNamesTable.Clear();

            var pass = new Pass(initCode, operationCodesTable.GetOperations());

            foreach (var record in pass)
            {
                objectModuleList.Add(record);
            }

            pass.CheckIfEnded();
            pass.CheckSymbolicNames();

            passEnumerator = null;

        }

        catch (WrongAddressException ex)
        {
            MessageBox.Show(ex.Message);
        }

        catch (OverflowException ex)
        {
            TB_firstPassError.Text = "Ошибка. Произошло переполнение";
            //TB_firstPassError.AppendText(ex.StackTrace);
        }

        catch (Exception ex)
        {
            TB_firstPassError.Text = ex.Message;
            //TB_firstPassError.AppendText(ex.StackTrace);
        }
    }

    private void btn_doStep_Click(object sender, EventArgs e)
    {
        try
        {

            if (passEnumerator == null)
            {
                ClearAll();
                var initCode = GetInitCodeWithoutEmptyRows();
                var pass = new Pass(initCode, operationCodesTable.GetOperations());
                passEnumerator = pass.GetEnumerator();
            }

            if (passEnumerator.MoveNext())
            {
                objectModuleList.Add(passEnumerator.Current);
            }
            else
            {
                ClearAll();
                passEnumerator = null;
            }


        }

        catch (OverflowException ex)
        {
            TB_firstPassError.Text = "Ошибка. Произошло переполнение";
            //TB_firstPassError.AppendText(ex.StackTrace);
        }

        catch (Exception ex)
        {
            TB_firstPassError.Text = ex.Message;
            //TB_firstPassError.AppendText(ex.StackTrace);
        }
    }

    private void btn_reset_Click(object sender, EventArgs e)
    {
        ClearAll();
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