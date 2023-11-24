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
        dg_settings = new DataGridView();
        label6 = new Label();
        label5 = new Label();
        label4 = new Label();
        dg_symbolNames = new DataGridView();
        TB_firstPassError = new RichTextBox();
        panel3 = new Panel();
        dg_objectModule = new DataGridView();
        Type = new DataGridViewTextBoxColumn();
        Address = new DataGridViewTextBoxColumn();
        Length = new DataGridViewTextBoxColumn();
        OperandPart = new DataGridViewTextBoxColumn();
        label8 = new Label();
        btn_start = new Button();
        btn_doStep = new Button();
        btn_reset = new Button();
        comboBox1 = new ComboBox();
        label3 = new Label();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        LabelInSymbolicNames = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
        Тип = new DataGridViewTextBoxColumn();
        NameType = new DataGridViewTextBoxColumn();
        Section = new DataGridViewTextBoxColumn();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_operCodes).BeginInit();
        panel2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_settings).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dg_symbolNames).BeginInit();
        panel3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dg_objectModule).BeginInit();
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
        panel1.Location = new Point(5, 2);
        panel1.Margin = new Padding(4);
        panel1.Name = "panel1";
        panel1.Size = new Size(583, 908);
        panel1.TabIndex = 0;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(146, 583);
        label2.Margin = new Padding(4, 0, 4, 0);
        label2.Name = "label2";
        label2.Size = new Size(269, 25);
        label2.TabIndex = 4;
        label2.Text = "Таблица кодов операций (ТКО)";
        // 
        // dg_operCodes
        // 
        dg_operCodes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_operCodes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_operCodes.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3 });
        dg_operCodes.Location = new Point(6, 612);
        dg_operCodes.Margin = new Padding(4);
        dg_operCodes.Name = "dg_operCodes";
        dg_operCodes.RowHeadersVisible = false;
        dg_operCodes.RowHeadersWidth = 51;
        dg_operCodes.RowTemplate.Height = 29;
        dg_operCodes.Size = new Size(569, 283);
        dg_operCodes.TabIndex = 3;
        dg_operCodes.CellValueChanged += dg_operCodes_CellValueChanged;
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
        label1.Location = new Point(209, 6);
        label1.Margin = new Padding(4, 0, 4, 0);
        label1.Name = "label1";
        label1.Size = new Size(130, 25);
        label1.TabIndex = 2;
        label1.Text = "Исходный код";
        // 
        // tb_initCode
        // 
        tb_initCode.Location = new Point(4, 36);
        tb_initCode.Margin = new Padding(4);
        tb_initCode.Name = "tb_initCode";
        tb_initCode.Size = new Size(570, 531);
        tb_initCode.TabIndex = 1;
        tb_initCode.Text = "";
        tb_initCode.TextChanged += tb_initCode_TextChanged;
        // 
        // panel2
        // 
        panel2.BackColor = SystemColors.ControlLight;
        panel2.BorderStyle = BorderStyle.Fixed3D;
        panel2.Controls.Add(dg_settings);
        panel2.Controls.Add(label6);
        panel2.Controls.Add(label5);
        panel2.Controls.Add(label4);
        panel2.Controls.Add(dg_symbolNames);
        panel2.Controls.Add(TB_firstPassError);
        panel2.Location = new Point(596, 2);
        panel2.Margin = new Padding(4);
        panel2.Name = "panel2";
        panel2.Size = new Size(583, 908);
        panel2.TabIndex = 1;
        // 
        // dg_settings
        // 
        dg_settings.AllowUserToAddRows = false;
        dg_settings.AllowUserToDeleteRows = false;
        dg_settings.AllowUserToResizeColumns = false;
        dg_settings.AllowUserToResizeRows = false;
        dg_settings.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_settings.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_settings.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, LabelInSymbolicNames });
        dg_settings.Location = new Point(6, 446);
        dg_settings.Margin = new Padding(4);
        dg_settings.Name = "dg_settings";
        dg_settings.ReadOnly = true;
        dg_settings.RowHeadersVisible = false;
        dg_settings.RowHeadersWidth = 51;
        dg_settings.RowTemplate.Height = 29;
        dg_settings.Size = new Size(569, 270);
        dg_settings.TabIndex = 10;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(196, 417);
        label6.Margin = new Padding(4, 0, 4, 0);
        label6.Name = "label6";
        label6.Size = new Size(199, 25);
        label6.TabIndex = 9;
        label6.Text = "Таблица модификаций";
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(251, 720);
        label5.Margin = new Padding(4, 0, 4, 0);
        label5.Name = "label5";
        label5.Size = new Size(79, 25);
        label5.TabIndex = 7;
        label5.Text = "Ошибки";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(148, 6);
        label4.Margin = new Padding(4, 0, 4, 0);
        label4.Name = "label4";
        label4.Size = new Size(304, 25);
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
        dg_symbolNames.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, Тип, NameType, Section });
        dg_symbolNames.Location = new Point(5, 36);
        dg_symbolNames.Margin = new Padding(4);
        dg_symbolNames.Name = "dg_symbolNames";
        dg_symbolNames.ReadOnly = true;
        dg_symbolNames.RowHeadersVisible = false;
        dg_symbolNames.RowHeadersWidth = 51;
        dg_symbolNames.RowTemplate.Height = 29;
        dg_symbolNames.Size = new Size(569, 368);
        dg_symbolNames.TabIndex = 6;
        // 
        // TB_firstPassError
        // 
        TB_firstPassError.Location = new Point(4, 749);
        TB_firstPassError.Margin = new Padding(4);
        TB_firstPassError.Name = "TB_firstPassError";
        TB_firstPassError.ReadOnly = true;
        TB_firstPassError.Size = new Size(570, 146);
        TB_firstPassError.TabIndex = 5;
        TB_firstPassError.Text = "";
        // 
        // panel3
        // 
        panel3.BackColor = SystemColors.ControlLight;
        panel3.BorderStyle = BorderStyle.Fixed3D;
        panel3.Controls.Add(dg_objectModule);
        panel3.Controls.Add(label8);
        panel3.Location = new Point(1188, 2);
        panel3.Margin = new Padding(4);
        panel3.Name = "panel3";
        panel3.Size = new Size(583, 908);
        panel3.TabIndex = 2;
        // 
        // dg_objectModule
        // 
        dg_objectModule.AllowUserToAddRows = false;
        dg_objectModule.AllowUserToDeleteRows = false;
        dg_objectModule.AllowUserToResizeColumns = false;
        dg_objectModule.AllowUserToResizeRows = false;
        dg_objectModule.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
        dg_objectModule.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dg_objectModule.ColumnHeadersVisible = false;
        dg_objectModule.Columns.AddRange(new DataGridViewColumn[] { Type, Address, Length, OperandPart });
        dg_objectModule.Location = new Point(6, 36);
        dg_objectModule.Margin = new Padding(4);
        dg_objectModule.Name = "dg_objectModule";
        dg_objectModule.ReadOnly = true;
        dg_objectModule.RowHeadersVisible = false;
        dg_objectModule.RowHeadersWidth = 51;
        dg_objectModule.RowTemplate.Height = 29;
        dg_objectModule.Size = new Size(569, 859);
        dg_objectModule.TabIndex = 8;
        // 
        // Type
        // 
        Type.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Type.DataPropertyName = "Type";
        Type.HeaderText = "Type";
        Type.MinimumWidth = 6;
        Type.Name = "Type";
        Type.ReadOnly = true;
        // 
        // Address
        // 
        Address.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Address.DataPropertyName = "Address";
        Address.HeaderText = "Address";
        Address.MinimumWidth = 6;
        Address.Name = "Address";
        Address.ReadOnly = true;
        // 
        // Length
        // 
        Length.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Length.DataPropertyName = "Length";
        Length.HeaderText = "Length";
        Length.MinimumWidth = 6;
        Length.Name = "Length";
        Length.ReadOnly = true;
        // 
        // OperandPart
        // 
        OperandPart.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        OperandPart.DataPropertyName = "OperandPart";
        OperandPart.HeaderText = "OperandPart";
        OperandPart.MinimumWidth = 6;
        OperandPart.Name = "OperandPart";
        OperandPart.ReadOnly = true;
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Location = new Point(211, 5);
        label8.Margin = new Padding(4, 0, 4, 0);
        label8.Name = "label8";
        label8.Size = new Size(171, 25);
        label8.TabIndex = 10;
        label8.Text = "Объектный модуль";
        // 
        // btn_start
        // 
        btn_start.Location = new Point(5, 917);
        btn_start.Name = "btn_start";
        btn_start.Size = new Size(583, 44);
        btn_start.TabIndex = 3;
        btn_start.Text = "Запуск/Продолжить";
        btn_start.UseVisualStyleBackColor = true;
        btn_start.Click += btn_start_Click;
        // 
        // btn_doStep
        // 
        btn_doStep.Location = new Point(596, 917);
        btn_doStep.Name = "btn_doStep";
        btn_doStep.Size = new Size(283, 44);
        btn_doStep.TabIndex = 4;
        btn_doStep.Text = "Шаг";
        btn_doStep.UseVisualStyleBackColor = true;
        btn_doStep.Click += btn_doStep_Click;
        // 
        // btn_reset
        // 
        btn_reset.Location = new Point(885, 917);
        btn_reset.Name = "btn_reset";
        btn_reset.Size = new Size(294, 44);
        btn_reset.TabIndex = 5;
        btn_reset.Text = "Перезагрузить";
        btn_reset.UseVisualStyleBackColor = true;
        btn_reset.Click += btn_reset_Click;
        // 
        // comboBox1
        // 
        comboBox1.FormattingEnabled = true;
        comboBox1.Items.AddRange(new object[] { "Только прямая адресация", "Только относительная адресация", "Смешанная адресация" });
        comboBox1.Location = new Point(1269, 924);
        comboBox1.Name = "comboBox1";
        comboBox1.Size = new Size(502, 33);
        comboBox1.TabIndex = 6;
        comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(1188, 927);
        label3.Name = "label3";
        label3.Size = new Size(79, 25);
        label3.TabIndex = 7;
        label3.Text = "Пример";
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn1.DataPropertyName = "Address";
        dataGridViewTextBoxColumn1.HeaderText = "Адрес";
        dataGridViewTextBoxColumn1.MinimumWidth = 6;
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        // 
        // LabelInSymbolicNames
        // 
        LabelInSymbolicNames.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        LabelInSymbolicNames.DataPropertyName = "Section";
        LabelInSymbolicNames.HeaderText = "Секция";
        LabelInSymbolicNames.MinimumWidth = 6;
        LabelInSymbolicNames.Name = "LabelInSymbolicNames";
        LabelInSymbolicNames.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn4
        // 
        dataGridViewTextBoxColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn4.DataPropertyName = "Name";
        dataGridViewTextBoxColumn4.HeaderText = "СИ";
        dataGridViewTextBoxColumn4.MinimumWidth = 6;
        dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
        dataGridViewTextBoxColumn4.ReadOnly = true;
        // 
        // dataGridViewTextBoxColumn5
        // 
        dataGridViewTextBoxColumn5.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dataGridViewTextBoxColumn5.DataPropertyName = "Address";
        dataGridViewTextBoxColumn5.HeaderText = "Адрес СИ";
        dataGridViewTextBoxColumn5.MinimumWidth = 6;
        dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
        dataGridViewTextBoxColumn5.ReadOnly = true;
        // 
        // Тип
        // 
        Тип.DataPropertyName = "OperandAddress";
        Тип.HeaderText = "Значение СА";
        Тип.MinimumWidth = 6;
        Тип.Name = "Тип";
        Тип.ReadOnly = true;
        Тип.Width = 125;
        // 
        // NameType
        // 
        NameType.DataPropertyName = "StringType";
        NameType.HeaderText = "Тип";
        NameType.MinimumWidth = 6;
        NameType.Name = "NameType";
        NameType.ReadOnly = true;
        NameType.Width = 125;
        // 
        // Section
        // 
        Section.DataPropertyName = "Section";
        Section.HeaderText = "Секция";
        Section.MinimumWidth = 6;
        Section.Name = "Section";
        Section.ReadOnly = true;
        Section.Width = 125;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlLightLight;
        ClientSize = new Size(1786, 973);
        Controls.Add(label3);
        Controls.Add(comboBox1);
        Controls.Add(btn_reset);
        Controls.Add(btn_doStep);
        Controls.Add(btn_start);
        Controls.Add(panel3);
        Controls.Add(panel2);
        Controls.Add(panel1);
        Font = new Font("Segoe UI", 10.8F, FontStyle.Regular, GraphicsUnit.Point);
        Location = new Point(10, 0);
        Margin = new Padding(4);
        Name = "Form1";
        StartPosition = FormStartPosition.Manual;
        Text = "Однопросмотровый ассемблер для программ в абсолютном формате";
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_operCodes).EndInit();
        panel2.ResumeLayout(false);
        panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_settings).EndInit();
        ((System.ComponentModel.ISupportInitialize)dg_symbolNames).EndInit();
        panel3.ResumeLayout(false);
        panel3.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dg_objectModule).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Panel panel1;
    private Panel panel2;
    private Panel panel3;
    private RichTextBox tb_initCode;
    private Label label1;
    private Label label2;
    private DataGridView dg_operCodes;
    private Label label5;
    private Label label4;
    private DataGridView dg_symbolNames;
    private RichTextBox TB_firstPassError;
    private Label label8;
    private DataGridViewTextBoxColumn Column1;
    private DataGridViewTextBoxColumn Column2;
    private DataGridViewTextBoxColumn Column3;
    private DataGridView dg_objectModule;
    private Button btn_start;
    private Button btn_doStep;
    private Button btn_reset;
    private DataGridViewTextBoxColumn Type;
    private DataGridViewTextBoxColumn Address;
    private DataGridViewTextBoxColumn Length;
    private DataGridViewTextBoxColumn OperandPart;
    private ComboBox comboBox1;
    private Label label3;
    private Label label6;
    private DataGridView dg_settings;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn LabelInSymbolicNames;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
    private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
    private DataGridViewTextBoxColumn Тип;
    private DataGridViewTextBoxColumn NameType;
    private DataGridViewTextBoxColumn Section;
}