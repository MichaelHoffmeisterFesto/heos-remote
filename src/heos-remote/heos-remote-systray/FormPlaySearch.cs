using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using heos_remote_lib;

namespace heos_remote_systray
{
    public partial class FormPlaySearch : Form
    {
        public string ResultSource = "";
        public string ResultKind = "";
        public string ResultText = "";

        public FormPlaySearch()
        {
            InitializeComponent();

            // customise this window
            this.Icon = WinFormsUtils.BytesToIcon(Resources.heos_remote_icon_I5p_icon);

            // customize source
            comboBoxSource.Items.Clear();
            comboBoxSource.Items.Add("Internet radio");
            comboBoxSource.Items.Add("TuneIn");
            comboBoxSource.Items.Add("Amazon");
            comboBoxSource.SelectedIndex = 0;
        }

        private void comboBoxSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var st = comboBoxSource.SelectedItem as string;
            if (st?.HasContent() != true)
                return;

            if (st == "Internet radio")
            {
                comboBoxKind.Items.Clear();
                comboBoxKind.Items.Add("URL");
                comboBoxKind.SelectedIndex = 0;
                labelText.Text = "URL:";
            }
            else
            {
                comboBoxKind.Items.Clear();
                comboBoxKind.Items.Add("Artist");
                comboBoxKind.Items.Add("Album");
                comboBoxKind.Items.Add("Station");
                comboBoxKind.SelectedIndex = 0;
                labelText.Text = "Text:";
            }
        }

        //protected override bool ProcessDialogKey(Keys keyData)
        //{
        //    if (keyData == Keys.Escape)
        //    {
        //        this.Close();
        //        return true;
        //    } 
        //    else if (keyData == Keys.Enter /* && ((Control.ModifierKeys & Keys.Control) != 0) */)
        //    {
        //        this.Close();
        //        return true;
        //    }
        //    else
        //        return base.ProcessDialogKey(keyData);
        //}

        private void SetResults()
        {
            ResultSource = comboBoxSource.SelectedItem as string ?? string.Empty;
            ResultKind = comboBoxKind.SelectedItem as string ?? string.Empty;
            ResultText = textBoxText.Text;
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            SetResults();
            this.DialogResult = DialogResult.OK;
        }

        private void FormPlaySearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                SetResults();
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
