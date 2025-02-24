namespace heos_remote_systray
{
    partial class FormPlaySearch
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            comboBoxSource = new ComboBox();
            label2 = new Label();
            comboBoxKind = new ComboBox();
            textBoxText = new TextBox();
            labelText = new Label();
            buttonGo = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(57, 20);
            label1.TabIndex = 0;
            label1.Text = "Source:";
            // 
            // comboBoxSource
            // 
            comboBoxSource.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxSource.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxSource.FormattingEnabled = true;
            comboBoxSource.Location = new Point(75, 6);
            comboBoxSource.Name = "comboBoxSource";
            comboBoxSource.Size = new Size(543, 28);
            comboBoxSource.TabIndex = 1;
            comboBoxSource.SelectedIndexChanged += comboBoxSource_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 43);
            label2.Name = "label2";
            label2.Size = new Size(42, 20);
            label2.TabIndex = 2;
            label2.Text = "Kind:";
            // 
            // comboBoxKind
            // 
            comboBoxKind.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxKind.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxKind.FormattingEnabled = true;
            comboBoxKind.Location = new Point(75, 40);
            comboBoxKind.Name = "comboBoxKind";
            comboBoxKind.Size = new Size(543, 28);
            comboBoxKind.TabIndex = 3;
            // 
            // textBoxText
            // 
            textBoxText.Location = new Point(75, 74);
            textBoxText.Name = "textBoxText";
            textBoxText.Size = new Size(543, 27);
            textBoxText.TabIndex = 4;
            // 
            // labelText
            // 
            labelText.AutoSize = true;
            labelText.Location = new Point(15, 77);
            labelText.Name = "labelText";
            labelText.Size = new Size(39, 20);
            labelText.TabIndex = 5;
            labelText.Text = "Text:";
            // 
            // buttonGo
            // 
            buttonGo.Location = new Point(75, 107);
            buttonGo.Name = "buttonGo";
            buttonGo.Size = new Size(543, 44);
            buttonGo.TabIndex = 6;
            buttonGo.Text = "Go!";
            buttonGo.UseVisualStyleBackColor = true;
            buttonGo.Click += buttonGo_Click;
            // 
            // FormPlaySearch
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(630, 161);
            Controls.Add(buttonGo);
            Controls.Add(labelText);
            Controls.Add(textBoxText);
            Controls.Add(comboBoxKind);
            Controls.Add(label2);
            Controls.Add(comboBoxSource);
            Controls.Add(label1);
            KeyPreview = true;
            MaximumSize = new Size(1200, 208);
            MinimumSize = new Size(200, 208);
            Name = "FormPlaySearch";
            Text = "Play details";
            KeyDown += FormPlaySearch_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ComboBox comboBoxSource;
        private Label label2;
        private ComboBox comboBoxKind;
        private TextBox textBoxText;
        private Label labelText;
        private Button buttonGo;
    }
}