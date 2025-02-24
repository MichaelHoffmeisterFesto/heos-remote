namespace heos_remote_systray
{
    partial class FormContainerBrowser
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
            components = new System.ComponentModel.Container();
            buttonUp = new Button();
            buttonPlayAll = new Button();
            dataGridView1 = new DataGridView();
            ColumnImage = new DataGridViewImageColumn();
            ColumnText = new DataGridViewTextBoxColumn();
            ColumnLink = new DataGridViewTextBoxColumn();
            timer1 = new System.Windows.Forms.Timer(components);
            comboBoxAction = new ComboBox();
            label1 = new Label();
            labelQueueCount = new Label();
            buttonQueueClear = new Button();
            label2 = new Label();
            label3 = new Label();
            labelQueueNext = new Label();
            labelQueueLast = new Label();
            progressBarQueue = new ProgressBar();
            label4 = new Label();
            comboBoxStarting = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // buttonUp
            // 
            buttonUp.Location = new Point(12, 12);
            buttonUp.Name = "buttonUp";
            buttonUp.Size = new Size(94, 29);
            buttonUp.TabIndex = 0;
            buttonUp.Text = "Up";
            buttonUp.UseVisualStyleBackColor = true;
            buttonUp.Click += button_Click;
            // 
            // buttonPlayAll
            // 
            buttonPlayAll.Location = new Point(112, 12);
            buttonPlayAll.Name = "buttonPlayAll";
            buttonPlayAll.Size = new Size(94, 29);
            buttonPlayAll.TabIndex = 1;
            buttonPlayAll.Text = "Play all ..";
            buttonPlayAll.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ColumnHeadersVisible = false;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { ColumnImage, ColumnText, ColumnLink });
            dataGridView1.Location = new Point(12, 47);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridView1.RowTemplate.DefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView1.RowTemplate.DefaultCellStyle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridView1.RowTemplate.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.RowTemplate.DefaultCellStyle.Padding = new Padding(2);
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 224, 224);
            dataGridView1.RowTemplate.DefaultCellStyle.SelectionForeColor = Color.Black;
            dataGridView1.RowTemplate.Height = 60;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(776, 349);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellMouseDoubleClick += dataGridView1_CellMouseDoubleClick;
            // 
            // ColumnImage
            // 
            ColumnImage.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            ColumnImage.DataPropertyName = "DisplayImage";
            ColumnImage.FillWeight = 64.17112F;
            ColumnImage.HeaderText = "Image";
            ColumnImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            ColumnImage.MinimumWidth = 6;
            ColumnImage.Name = "ColumnImage";
            ColumnImage.ReadOnly = true;
            ColumnImage.Width = 60;
            // 
            // ColumnText
            // 
            ColumnText.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ColumnText.DataPropertyName = "DisplayName";
            ColumnText.FillWeight = 135.828888F;
            ColumnText.HeaderText = "Text";
            ColumnText.MinimumWidth = 6;
            ColumnText.Name = "ColumnText";
            ColumnText.ReadOnly = true;
            // 
            // ColumnLink
            // 
            ColumnLink.DataPropertyName = "DisplayLink";
            ColumnLink.HeaderText = "Link";
            ColumnLink.MinimumWidth = 60;
            ColumnLink.Name = "ColumnLink";
            ColumnLink.ReadOnly = true;
            ColumnLink.Width = 60;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Tick += timer1_Tick;
            // 
            // comboBoxAction
            // 
            comboBoxAction.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxAction.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxAction.FormattingEnabled = true;
            comboBoxAction.Items.AddRange(new object[] { "Play now", "Play next", "Add to end", "Replace and play" });
            comboBoxAction.Location = new Point(624, 12);
            comboBoxAction.Name = "comboBoxAction";
            comboBoxAction.Size = new Size(164, 28);
            comboBoxAction.TabIndex = 3;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.Location = new Point(8, 402);
            label1.Margin = new Padding(0);
            label1.Name = "label1";
            label1.Size = new Size(55, 20);
            label1.TabIndex = 4;
            label1.Text = "Queue:";
            // 
            // labelQueueCount
            // 
            labelQueueCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelQueueCount.BackColor = Color.SteelBlue;
            labelQueueCount.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelQueueCount.ForeColor = Color.White;
            labelQueueCount.Location = new Point(73, 402);
            labelQueueCount.Name = "labelQueueCount";
            labelQueueCount.Size = new Size(84, 39);
            labelQueueCount.TabIndex = 5;
            labelQueueCount.Text = "#233";
            labelQueueCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // buttonQueueClear
            // 
            buttonQueueClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonQueueClear.Location = new Point(163, 402);
            buttonQueueClear.Name = "buttonQueueClear";
            buttonQueueClear.Size = new Size(54, 39);
            buttonQueueClear.TabIndex = 6;
            buttonQueueClear.Text = "Clear";
            buttonQueueClear.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 6F);
            label2.Location = new Point(223, 406);
            label2.Name = "label2";
            label2.Size = new Size(27, 12);
            label2.TabIndex = 7;
            label2.Text = "Next:";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 6F);
            label3.Location = new Point(223, 426);
            label3.Name = "label3";
            label3.Size = new Size(24, 12);
            label3.TabIndex = 8;
            label3.Text = "Last:";
            // 
            // labelQueueNext
            // 
            labelQueueNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelQueueNext.BackColor = Color.SteelBlue;
            labelQueueNext.Font = new Font("Segoe UI", 6F);
            labelQueueNext.ForeColor = Color.White;
            labelQueueNext.Location = new Point(257, 402);
            labelQueueNext.Name = "labelQueueNext";
            labelQueueNext.Size = new Size(531, 18);
            labelQueueNext.TabIndex = 9;
            labelQueueNext.Text = "Abba";
            labelQueueNext.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelQueueLast
            // 
            labelQueueLast.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelQueueLast.BackColor = Color.SteelBlue;
            labelQueueLast.Font = new Font("Segoe UI", 6F);
            labelQueueLast.ForeColor = Color.White;
            labelQueueLast.Location = new Point(257, 423);
            labelQueueLast.Name = "labelQueueLast";
            labelQueueLast.Size = new Size(531, 18);
            labelQueueLast.TabIndex = 10;
            labelQueueLast.Text = "Zappa";
            labelQueueLast.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // progressBarQueue
            // 
            progressBarQueue.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            progressBarQueue.Location = new Point(12, 423);
            progressBarQueue.Maximum = 30;
            progressBarQueue.Name = "progressBarQueue";
            progressBarQueue.Size = new Size(55, 18);
            progressBarQueue.Step = 1;
            progressBarQueue.TabIndex = 11;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(209, 16);
            label4.Margin = new Padding(0);
            label4.Name = "label4";
            label4.Size = new Size(75, 20);
            label4.TabIndex = 12;
            label4.Text = "Start with:";
            // 
            // comboBoxStarting
            // 
            comboBoxStarting.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxStarting.FormattingEnabled = true;
            comboBoxStarting.Location = new Point(287, 12);
            comboBoxStarting.Name = "comboBoxStarting";
            comboBoxStarting.Size = new Size(331, 28);
            comboBoxStarting.TabIndex = 13;
            comboBoxStarting.SelectedIndexChanged += comboBoxStarting_SelectedIndexChanged;
            // 
            // FormContainerBrowser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(comboBoxStarting);
            Controls.Add(label4);
            Controls.Add(progressBarQueue);
            Controls.Add(labelQueueLast);
            Controls.Add(labelQueueNext);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(buttonQueueClear);
            Controls.Add(labelQueueCount);
            Controls.Add(label1);
            Controls.Add(comboBoxAction);
            Controls.Add(dataGridView1);
            Controls.Add(buttonPlayAll);
            Controls.Add(buttonUp);
            Name = "FormContainerBrowser";
            Text = "FormContainerBrowser";
            Load += FormContainerBrowser_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonUp;
        private Button buttonPlayAll;
        private DataGridView dataGridView1;
        private System.Windows.Forms.Timer timer1;
        private DataGridViewImageColumn ColumnImage;
        private DataGridViewTextBoxColumn ColumnText;
        private DataGridViewTextBoxColumn ColumnLink;
        private ComboBox comboBoxAction;
        private Label label1;
        private Label labelQueueCount;
        private Button buttonQueueClear;
        private Label label2;
        private Label label3;
        private Label labelQueueNext;
        private Label labelQueueLast;
        private ProgressBar progressBarQueue;
        private Label label4;
        private ComboBox comboBoxStarting;
    }
}