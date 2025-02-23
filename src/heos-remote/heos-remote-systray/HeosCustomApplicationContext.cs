using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace heos_remote_systray
{
    public class HeosCustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private ContextMenuStrip contextMenu;

        private FormInfo? formInfo = null;

        public HeosCustomApplicationContext()
        {
            // Initialize Tray Icon
            contextMenu = new ContextMenuStrip()
            {
            };
            
            contextMenu.Items.Add("Play", image: WinFormsUtils.ByteToImage(Resources.heos_remote_play), 
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Pause", image: WinFormsUtils.ByteToImage(Resources.heos_remote_pause),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Fav 1", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav1),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 2", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav2),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 3", image: WinFormsUtils.ByteToImage(Resources.heos_remote_fav3),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Info", image: null,
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", image: null, 
                onClick: contextMenuItemHandler);

            trayIcon = new NotifyIcon()
            {
                Icon = WinFormsUtils.BytesToIcon(Resources.heos_remote_icon_I5p_icon),
                Visible = true,
                ContextMenuStrip = contextMenu,
            };

            trayIcon.DoubleClick += new System.EventHandler(trayIcon_DoubleClick);
        }

        void contextMenuItemHandler(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                if (tsmi.Text == "Info")
                {
                    if (formInfo == null)
                        formInfo = new FormInfo();
                    formInfo.Show();
                    formInfo.FormClosed += new FormClosedEventHandler(formInfoClosedEventHandler);
                }

                if (tsmi.Text == "Exit")
                {
                    // Hide tray icon, otherwise it will remain shown until user mouses over it
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    Application.Exit();
                }
            }
        }

        void trayIcon_DoubleClick(object? sender, EventArgs e)
        {
            ;
        }

        void formInfoClosedEventHandler(object? sender, EventArgs e)
        {
            formInfo = null;
        }

    }
}
