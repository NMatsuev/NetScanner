namespace NetScanner
{
    partial class FrmMain
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
            ProgressBar = new ProgressBar();
            BtnStart = new Button();
            LViewNodes = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            PanelRight = new Panel();
            LabelInterface = new Label();
            ComboBoxInterface = new ComboBox();
            PanelRight.SuspendLayout();
            SuspendLayout();
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new Point(12, 476);
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(932, 34);
            ProgressBar.TabIndex = 1;
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(17, 346);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(222, 58);
            BtnStart.TabIndex = 2;
            BtnStart.Text = "Начать";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // LViewNodes
            // 
            LViewNodes.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            LViewNodes.FullRowSelect = true;
            LViewNodes.GridLines = true;
            LViewNodes.Location = new Point(12, 12);
            LViewNodes.Name = "LViewNodes";
            LViewNodes.Size = new Size(659, 433);
            LViewNodes.TabIndex = 3;
            LViewNodes.UseCompatibleStateImageBehavior = false;
            LViewNodes.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "IP-адрес";
            columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "MAC-адрес";
            columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Название";
            columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Порты";
            columnHeader4.Width = 150;
            // 
            // PanelRight
            // 
            PanelRight.BackColor = SystemColors.ControlDark;
            PanelRight.Controls.Add(LabelInterface);
            PanelRight.Controls.Add(ComboBoxInterface);
            PanelRight.Controls.Add(BtnStart);
            PanelRight.Location = new Point(689, 12);
            PanelRight.Name = "PanelRight";
            PanelRight.Size = new Size(255, 433);
            PanelRight.TabIndex = 4;
            // 
            // LabelInterface
            // 
            LabelInterface.AutoSize = true;
            LabelInterface.Location = new Point(45, 150);
            LabelInterface.Name = "LabelInterface";
            LabelInterface.Size = new Size(170, 25);
            LabelInterface.TabIndex = 5;
            LabelInterface.Text = "Сетевой интерфейс";
            // 
            // ComboBoxInterface
            // 
            ComboBoxInterface.AllowDrop = true;
            ComboBoxInterface.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBoxInterface.FormattingEnabled = true;
            ComboBoxInterface.Location = new Point(17, 201);
            ComboBoxInterface.Name = "ComboBoxInterface";
            ComboBoxInterface.Size = new Size(222, 33);
            ComboBoxInterface.TabIndex = 3;
            ComboBoxInterface.SelectedIndexChanged += ComboBoxInterface_SelectedIndexChanged;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(956, 542);
            Controls.Add(PanelRight);
            Controls.Add(LViewNodes);
            Controls.Add(ProgressBar);
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NetScanner";
            PanelRight.ResumeLayout(false);
            PanelRight.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ProgressBar ProgressBar;
        private Button BtnStart;
        private ListView LViewNodes;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Panel PanelRight;
        private Label LabelInterface;
        private ComboBox ComboBoxInterface;
    }
}