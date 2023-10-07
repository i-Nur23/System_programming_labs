namespace Lab1;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        panel1 = new Panel();
        label2 = new Label();
        dg_operCodes = new DataGridView();
        Column1 = new DataGridViewTextBoxColumn();
        Column2 = new DataGridViewTextBoxColumn();
        Column3 = new DataGridViewTextBoxColumn();
        label1 = new Label();
        tb_initCode = new RichTextBox();
        panel2 = new Panel();
        label5 = new Label();
        label4 = new Label();
        dg_symbolNames = new DataGridView();
        TB_firstPassError = new RichTextBox();
        label3 = new Label();
        dg_aux = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        Column4 = new DataGridViewTextBoxColumn();
        panel3 = new Panel();
        label8 = new Label();
        label7 = new Label();
        TB_secondPassError = new RichTextBox();
        TB_binaryCode = new RichTextBox();
        label6 = new Label();
        dg_objModuleHeader = new DataGridView();
        dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn10 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn11 = new DataGridViewTextBoxColumn();
        btn_firstPass = new Button();
        btn_secondPass = new Button();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_operCodes).BeginInit();
        panel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_symbolNames).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dg_aux).BeginInit();
        panel3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_objModuleHeader).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.BackColor = SystemColors.ControlLight;
        panel1.BorderStyle = BorderStyle.Fixed3D;
        panel1.Controls.Add(label2);
        panel1.Controls.Add(dg_operCodes);
        panel1.Controls.Add(label1);
        panel1.Controls.Add(tb_initCode);
        panel1.Location = new Point(4, 2);
        panel1.Name = "panel1";
        panel1.Size = new Size(467, 781);
        panel1.TabIndex = 0;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(117, 402);
        label2.Name = "label2";
        label2.Size = new Size(229, 20);
        label2.TabIndex = 4;
        label2.Text = "Таблица кодов операций (ТКО)";
        // 
        // dg_operCodes
        // 
        dg_operCodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_operCodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_operCodes.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3 });
        dg_operCodes.Location = new Point(5, 425);
        dg_operCodes.Name = "dg_operCodes";
        dg_operCodes.RowHeadersVisible = false;
        dg_operCodes.RowHeadersWidth = 51;
        dg_operCodes.RowTemplate.Height = 29;
        dg_operCodes.Size = new Size(455, 350);
        dg_operCodes.TabIndex = 3;
        // 
        // Column1
        // 
        Column1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column1.DataPropertyName = "MnemonicCode";
        Column1.HeaderText = "МКОП";
        Column1.MinimumWidth = 6;
        Column1.Name = "Column1";
        Column1.Resizable = DataGridViewTriState.True;
        Column1.ToolTipText = "МКОП";
        // 
        // Column2
        // 
        Column2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column2.DataPropertyName = "BinaryCode";
        Column2.HeaderText = "Двоичный код";
        Column2.MinimumWidth = 6;
        Column2.Name = "Column2";
        // 
        // Column3
        // 
        Column3.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column3.DataPropertyName = "CommandLength";
        Column3.HeaderText = "Длина команды";
        Column3.MinimumWidth = 6;
        Column3.Name = "Column3";
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(167, 5);
        label1.Name = "label1";
        label1.Size = new Size(108, 20);
        label1.TabIndex = 2;
        label1.Text = "Исходный код";
        // 
        // tb_initCode
        // 
        tb_initCode.Location = new Point(3, 29);
        tb_initCode.Name = "tb_initCode";
        tb_initCode.Size = new Size(457, 355);
        tb_initCode.TabIndex = 1;
        tb_initCode.Text = "";
        // 
        // panel2
        // 
        panel2.BackColor = SystemColors.ControlLight;
        panel2.BorderStyle = BorderStyle.Fixed3D;
        panel2.Controls.Add(label5);
        panel2.Controls.Add(label4);
        panel2.Controls.Add(dg_symbolNames);
        panel2.Controls.Add(TB_firstPassError);
        panel2.Controls.Add(label3);
        panel2.Controls.Add(dg_aux);
        panel2.Location = new Point(477, 2);
        panel2.Name = "panel2";
        panel2.Size = new Size(467, 781);
        panel2.TabIndex = 1;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(134, 633);
        label5.Name = "label5";
        label5.Size = new Size(191, 20);
        label5.TabIndex = 7;
        label5.Text = "Ошибки первого прохода";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(101, 382);
        label4.Name = "label4";
        label4.Size = new Size(262, 20);
        label4.TabIndex = 5;
        label4.Text = "Таблица символических имен (ТСИ)";
        // 
        // dg_symbolNames
        // 
        dg_symbolNames.AllowUserToAddRows = false;
        dg_symbolNames.AllowUserToDeleteRows = false;
        dg_symbolNames.AllowUserToResizeColumns = false;
        dg_symbolNames.AllowUserToResizeRows = false;
        dg_symbolNames.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_symbolNames.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_symbolNames.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5 });
        dg_symbolNames.Enabled = false;
        dg_symbolNames.Location = new Point(5, 405);
        dg_symbolNames.Name = "dg_symbolNames";
        dg_symbolNames.ReadOnly = true;
        dg_symbolNames.RowHeadersVisible = false;
        dg_symbolNames.RowHeadersWidth = 51;
        dg_symbolNames.RowTemplate.Height = 29;
        dg_symbolNames.Size = new Size(455, 216);
        dg_symbolNames.TabIndex = 6;
        // 
        // TB_firstPassError
        // 
        TB_firstPassError.Location = new Point(3, 656);
        TB_firstPassError.Name = "TB_firstPassError";
        TB_firstPassError.ReadOnly = true;
        TB_firstPassError.Size = new Size(457, 118);
        TB_firstPassError.TabIndex = 5;
        TB_firstPassError.Text = "";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(136, 5);
        label3.Name = "label3";
        label3.Size = new Size(193, 20);
        label3.TabIndex = 5;
        label3.Text = "Вспомогательная таблица";
        // 
        // dg_aux
        // 
        dg_aux.AllowUserToAddRows = false;
        dg_aux.AllowUserToDeleteRows = false;
        dg_aux.AllowUserToResizeColumns = false;
        dg_aux.AllowUserToResizeRows = false;
        dg_aux.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_aux.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_aux.ColumnHeadersVisible = false;
        dg_aux.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, Column4 });
        dg_aux.Enabled = false;
        dg_aux.Location = new Point(5, 29);
        dg_aux.Name = "dg_aux";
        dg_aux.ReadOnly = true;
        dg_aux.RowHeadersVisible = false;
        dg_aux.RowHeadersWidth = 51;
        dg_aux.RowTemplate.Height = 29;
        dg_aux.Size = new Size(455, 339);
        dg_aux.TabIndex = 5;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn1.HeaderText = "Column1";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn2
        // 
        dataGridViewTextBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn2.HeaderText = "Column2";
        dataGridViewTextBoxColumn2.MinimumWidth = 6;
        dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
        dataGridViewTextBoxColumn2.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn3
        // 
        dataGridViewTextBoxColumn3.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn3.HeaderText = "Column3";
        dataGridViewTextBoxColumn3.MinimumWidth = 6;
        dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
        dataGridViewTextBoxColumn3.ReadOnly = true;
        // 
        // Column4
        // 
        Column4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Column4.HeaderText = "Column4";
        Column4.MinimumWidth = 6;
        Column4.Name = "Column4";
        Column4.ReadOnly = true;
        // 
        // panel3
        // 
        panel3.BackColor = SystemColors.ControlLight;
        panel3.BorderStyle = BorderStyle.Fixed3D;
        panel3.Controls.Add(label8);
        panel3.Controls.Add(label7);
        panel3.Controls.Add(TB_secondPassError);
        panel3.Controls.Add(TB_binaryCode);
        panel3.Controls.Add(label6);
        panel3.Controls.Add(dg_objModuleHeader);
        panel3.Location = new Point(950, 2);
        panel3.Name = "panel3";
        panel3.Size = new Size(467, 781);
        panel3.TabIndex = 2;
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Location = new Point(181, 160);
        label8.Name = "label8";
        label8.Size = new Size(110, 20);
        label8.TabIndex = 10;
        label8.Text = "Двоичный код";
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new Point(124, 633);
        label7.Name = "label7";
        label7.Size = new Size(189, 20);
        label7.TabIndex = 8;
        label7.Text = "Ошибки второго прохода";
        // 
        // TB_secondPassError
        // 
        TB_secondPassError.Location = new Point(3, 656);
        TB_secondPassError.Name = "TB_secondPassError";
        TB_secondPassError.ReadOnly = true;
        TB_secondPassError.Size = new Size(457, 118);
        TB_secondPassError.TabIndex = 9;
        TB_secondPassError.Text = "";
        // 
        // TB_binaryCode
        // 
        TB_binaryCode.Location = new Point(3, 183);
        TB_binaryCode.Name = "TB_binaryCode";
        TB_binaryCode.ReadOnly = true;
        TB_binaryCode.Size = new Size(457, 369);
        TB_binaryCode.TabIndex = 8;
        TB_binaryCode.Text = "";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(124, 6);
        label6.Name = "label6";
        label6.Size = new Size(221, 20);
        label6.TabIndex = 8;
        label6.Text = "Заголовок объектного модуля";
        // 
        // dg_objModuleHeader
        // 
        dg_objModuleHeader.AllowUserToAddRows = false;
        dg_objModuleHeader.AllowUserToDeleteRows = false;
        dg_objModuleHeader.AllowUserToResizeColumns = false;
        dg_objModuleHeader.AllowUserToResizeRows = false;
        dg_objModuleHeader.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_objModuleHeader.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_objModuleHeader.ColumnHeadersVisible = false;
        dg_objModuleHeader.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9, dataGridViewTextBoxColumn10, dataGridViewTextBoxColumn11 });
        dg_objModuleHeader.Enabled = false;
        dg_objModuleHeader.Location = new Point(5, 29);
        dg_objModuleHeader.Name = "dg_objModuleHeader";
        dg_objModuleHeader.ReadOnly = true;
        dg_objModuleHeader.RowHeadersVisible = false;
        dg_objModuleHeader.RowHeadersWidth = 51;
        dg_objModuleHeader.RowTemplate.Height = 29;
        dg_objModuleHeader.Size = new Size(455, 34);
        dg_objModuleHeader.TabIndex = 8;
        // 
        // dataGridViewTextBoxColumn8
        // 
        dataGridViewTextBoxColumn8.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn8.HeaderText = "Column1";
        dataGridViewTextBoxColumn8.MinimumWidth = 6;
        dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
        dataGridViewTextBoxColumn8.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn9
        // 
        dataGridViewTextBoxColumn9.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn9.HeaderText = "Column2";
        dataGridViewTextBoxColumn9.MinimumWidth = 6;
        dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
        dataGridViewTextBoxColumn9.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn10
        // 
        dataGridViewTextBoxColumn10.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn10.HeaderText = "Column3";
        dataGridViewTextBoxColumn10.MinimumWidth = 6;
        dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
        dataGridViewTextBoxColumn10.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn11
        // 
        dataGridViewTextBoxColumn11.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn11.HeaderText = "Column4";
        dataGridViewTextBoxColumn11.MinimumWidth = 6;
        dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
        dataGridViewTextBoxColumn11.ReadOnly = true;
        // 
        // btn_firstPass
        // 
        btn_firstPass.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        btn_firstPass.Location = new Point(4, 789);
        btn_firstPass.Name = "btn_firstPass";
        btn_firstPass.Size = new Size(467, 63);
        btn_firstPass.TabIndex = 3;
        btn_firstPass.Text = "Первый проход";
        btn_firstPass.UseVisualStyleBackColor = true;
        btn_firstPass.Click += btn_firstPass_Click;
        // 
        // btn_secondPass
        // 
        btn_secondPass.Enabled = false;
        btn_secondPass.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
        btn_secondPass.Location = new Point(477, 789);
        btn_secondPass.Name = "btn_secondPass";
        btn_secondPass.Size = new Size(467, 63);
        btn_secondPass.TabIndex = 4;
        btn_secondPass.Text = "Второй проход";
        btn_secondPass.UseVisualStyleBackColor = true;
        btn_secondPass.Click += btn_secondPass_Click;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn4.HeaderText = "СИ";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn5
        // 
        dataGridViewTextBoxColumn5.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn5.HeaderText = "Адрес СИ";
        dataGridViewTextBoxColumn5.MinimumWidth = 6;
        dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
        dataGridViewTextBoxColumn5.ReadOnly = true;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlLightLight;
        ClientSize = new Size(1429, 855);
        Controls.Add(btn_secondPass);
        Controls.Add(btn_firstPass);
        Controls.Add(panel3);
        Controls.Add(panel2);
        Controls.Add(panel1);
        Location = new Point(10, 5);
        Name = "Form1";
        Text = "Двухпросмотровый ассемблер в абсолютном формате";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_operCodes).EndInit();
        panel2.ResumeLayout(false);
        panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_symbolNames).EndInit();
        ((System.ComponentModel.ISupportInitialize)dg_aux).EndInit();
        panel3.ResumeLayout(false);
        panel3.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_objModuleHeader).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel panel1;
    private Panel panel2;
    private Panel panel3;
    private Button btn_firstPass;
    private Button btn_secondPass;
    private RichTextBox tb_initCode;
    private Label label1;
    private Label label2;
    private DataGridView dg_operCodes;
    private Label label5;
    private Label label4;
    private DataGridView dg_symbolNames;
    private RichTextBox TB_firstPassError;
    private Label label3;
    private DataGridView dg_aux;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    private DataGridViewTextBoxColumn Column4;
    private Label label8;
    private Label label7;
    private RichTextBox TB_secondPassError;
    private RichTextBox TB_binaryCode;
    private Label label6;
    private DataGridView dg_objModuleHeader;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
    private DataGridViewTextBoxColumn Column1;
    private DataGridViewTextBoxColumn Column2;
    private DataGridViewTextBoxColumn Column3;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
}