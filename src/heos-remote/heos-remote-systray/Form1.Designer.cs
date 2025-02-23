namespace heos_remote_systray
{
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItemPlay = new ToolStripMenuItem();
            toolStripMenuItemStop = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemFav1 = new ToolStripMenuItem();
            toolStripMenuItemFav2 = new ToolStripMenuItem();
            toolStripMenuItemFav3 = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItemExit = new ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItemPlay, toolStripMenuItemStop, toolStripSeparator1, toolStripMenuItemFav1, toolStripMenuItemFav2, toolStripMenuItemFav3, toolStripSeparator2, toolStripMenuItemExit });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(135, 160);
            // 
            // toolStripMenuItemPlay
            // 
            toolStripMenuItemPlay.Name = "toolStripMenuItemPlay";
            toolStripMenuItemPlay.Size = new Size(134, 24);
            toolStripMenuItemPlay.Text = "Play";
            toolStripMenuItemPlay.Click += toolStripMenuItem_Click;
            // 
            // toolStripMenuItemStop
            // 
            toolStripMenuItemStop.Name = "toolStripMenuItemStop";
            toolStripMenuItemStop.Size = new Size(134, 24);
            toolStripMenuItemStop.Text = "Stop";
            toolStripMenuItemStop.Click += toolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(131, 6);
            // 
            // toolStripMenuItemFav1
            // 
            toolStripMenuItemFav1.Name = "toolStripMenuItemFav1";
            toolStripMenuItemFav1.Size = new Size(134, 24);
            toolStripMenuItemFav1.Text = "Fav 1";
            toolStripMenuItemFav1.Click += toolStripMenuItem_Click;
            // 
            // toolStripMenuItemFav2
            // 
            toolStripMenuItemFav2.Name = "toolStripMenuItemFav2";
            toolStripMenuItemFav2.Size = new Size(134, 24);
            toolStripMenuItemFav2.Text = "Fav 2";
            toolStripMenuItemFav2.Click += toolStripMenuItem_Click;
            // 
            // toolStripMenuItemFav3
            // 
            toolStripMenuItemFav3.Name = "toolStripMenuItemFav3";
            toolStripMenuItemFav3.Size = new Size(134, 24);
            toolStripMenuItemFav3.Text = "Fav 3";
            toolStripMenuItemFav3.Click += toolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(131, 6);
            // 
            // toolStripMenuItemExit
            // 
            toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            toolStripMenuItemExit.Size = new Size(134, 24);
            toolStripMenuItemExit.Text = "Exit App";
            toolStripMenuItemExit.Click += toolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItemPlay;
        private ToolStripMenuItem toolStripMenuItemStop;
        private ToolStripMenuItem toolStripMenuItemFav1;
        private ToolStripMenuItem toolStripMenuItemFav2;
        private ToolStripMenuItem toolStripMenuItemFav3;
        private ToolStripMenuItem toolStripMenuItemExit;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
    }
}
