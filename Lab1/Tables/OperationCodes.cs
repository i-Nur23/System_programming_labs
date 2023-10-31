using Lab1.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1.Tables
{
    public class OperationCodes
    {
        private readonly DataGridView table;
        private List<Operation> operations = new List<Operation>();

        public OperationCodes(DataGridView table)
        {
            this.table = table;
        }

        public List<Operation> GetOperations()
        {

            operations.Clear();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];

                var mnemonicCodeCell = row.Cells[0].Value;
                var binaryCodeCell = row.Cells[1].Value;
                var lengthCell = row.Cells[2].Value;

                int binaryCode, length;
                string mnemonicCode;

                if (mnemonicCodeCell == null && binaryCodeCell == null && lengthCell == null) continue;

                if (mnemonicCodeCell == null || String.IsNullOrWhiteSpace(mnemonicCodeCell.ToString()))
                {
                    throw new Exception($"ТКО, строка {i + 1}: мнемоника не может быть пустым полем");
                }

                mnemonicCode = mnemonicCodeCell.ToString().ToUpper();
                
                if (mnemonicCode.Length > 7)
                {
                    throw new Exception($"ТКО, строка {i + 1}: мнемоника должна содержать не более 7 символов");
                }

                if (!Checks.IsOnlyLettersAndNumbers(mnemonicCode))
                {
                    throw new Exception($"ТКО, строка {i + 1}: мнемоника должна содержать только латинские буквы и цифры");
                }

                if (Checks.IsDirective(mnemonicCode) || Checks.IsRegister(mnemonicCode))
                {
                    throw new Exception($"ТКО, строка {i + 1}: мнемоника не может быть зарезервированным именем");
                }

                if (binaryCodeCell == null)
                {
                    throw new Exception($"ТКО, строка {i + 1}: двоичный код не может быть пустым полем");
                }
                
                var isBinaryCodeOk = Int32.TryParse(
                    binaryCodeCell.ToString(),
                    NumberStyles.HexNumber,
                    CultureInfo.CurrentCulture, 
                    out binaryCode
                );

                if (!isBinaryCodeOk)
                {
                    throw new Exception($"ТКО, строка {i + 1}: двоичный код должен быть положительным числом");
                }

                if (binaryCode < 1 || binaryCode > 63)
                {
                    throw new Exception($"ТКО, строка {i + 1}: двоичный код должен быть числом больше 0, но меньше 3F");
                }

                if (lengthCell == null)
                {
                    throw new Exception($"ТКО, строка {i + 1}: длина команды не может быть пустым полем");
                }

                var isLengthOk = Int32.TryParse(lengthCell.ToString(), out length);

                if (!isLengthOk)
                {
                    throw new Exception($"ТКО, строка {i + 1}: длина команды должна быть числом");
                }

                if (operations.FirstOrDefault(x => String.Equals(x.MnemonicCode, mnemonicCode)) != null)
                {
                    throw new Exception($"ТКО: мнемокод {mnemonicCode} встречается в таблице более одного раза");
                }

                if (operations.FirstOrDefault(x => x.BinaryCode == binaryCode) != null)
                {
                    throw new Exception($"ТКО: двоичный код {binaryCode} встречается в таблице более чем у двух операции");
                }

                if (binaryCode < 1)
                {
                    throw new Exception($"ТКО, строка {i + 1}: двоичный код должен быть больше 0");
                }

                if (length < 1 || length > 4 || length == 3)
                {
                    throw new Exception($"ТКО, строка {i + 1}: длина команды должно быть числом 1, 2 или 4");
                }

                operations.Add(new Operation { MnemonicCode = mnemonicCode, BinaryCode = binaryCode, CommandLength = length });

            }

            return operations;
        }

        public void AddRange(IEnumerable<Operation> addingOperations)
        {
            foreach (var op in addingOperations)
            {
                table.Rows.Add(
                    op.MnemonicCode,
                    op.BinaryCode.ToString("X"), 
                    op.CommandLength
                );
            }
        }


    }
}
