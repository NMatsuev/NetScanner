﻿namespace NetScanner
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
            comboBox1 = new ComboBox();
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
            BtnStart.Location = new Point(58, 306);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(185, 58);
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
            LViewNodes.Size = new Size(610, 433);
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
            columnHeader4.Width = 100;
            // 
            // PanelRight
            // 
            PanelRight.BackColor = SystemColors.ControlDark;
            PanelRight.Controls.Add(LabelInterface);
            PanelRight.Controls.Add(comboBox1);
            PanelRight.Controls.Add(BtnStart);
            PanelRight.Location = new Point(644, 12);
            PanelRight.Name = "PanelRight";
            PanelRight.Size = new Size(300, 433);
            PanelRight.TabIndex = 4;
            // 
            // LabelInterface
            // 
            LabelInterface.AutoSize = true;
            LabelInterface.Location = new Point(58, 96);
            LabelInterface.Name = "LabelInterface";
            LabelInterface.Size = new Size(170, 25);
            LabelInterface.TabIndex = 5;
            LabelInterface.Text = "Сетевой интерфейс";
            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(58, 154);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(182, 33);
            comboBox1.TabIndex = 3;
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
        private ComboBox comboBox1;
    }
}