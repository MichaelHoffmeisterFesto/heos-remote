namespace heos_remote_systray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;

            // notifyIcon1.ShowBalloonTip(500, "First Notify App", "Application is in Notification Tray", ToolTipIcon.Info);

            notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);
        }

        void notifyIcon1_DoubleClick(object? sender, EventArgs e)
        {
            //Showing the original window when the Application Icon   
            // In Notification Tray is Double Clicked
            this.Show();
            //Hiding the Application from Notification Tray
            notifyIcon1.Visible = false;
        }

        private void toolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender == toolStripMenuItemExit)
            {
                //Exiting The Application
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();
                Application.Exit();
            }
        }
    }
}
