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

namespace heos_remote_systray
{
    public class HeosCustomApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        private ContextMenuStrip contextMenu;

        public HeosCustomApplicationContext()
        {
            // Initialize Tray Icon
            contextMenu = new ContextMenuStrip()
            {
            };
            
            contextMenu.Items.Add("Play", image: ByteToImage(Resources.heos_remote_play), 
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Pause", image: ByteToImage(Resources.heos_remote_pause),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Fav 1", image: ByteToImage(Resources.heos_remote_fav1),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 2", image: ByteToImage(Resources.heos_remote_fav2),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add("Fav 3", image: ByteToImage(Resources.heos_remote_fav3),
                onClick: contextMenuItemHandler);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", image: null, 
                onClick: contextMenuItemHandler);

            trayIcon = new NotifyIcon()
            {
                Icon = BytesToIcon(Resources.heos_remote_icon_I5p_icon),
                Visible = true,
                ContextMenuStrip = contextMenu
            };
        }

        void contextMenuItemHandler(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsmi)
            {
                if (tsmi.Text == "Exit")
                {
                    // Hide tray icon, otherwise it will remain shown until user mouses over it
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                    Application.Exit();
                }
            }
        }

        public static byte[] IconToBytes(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                return ms.ToArray();
            }
        }

        public static Icon BytesToIcon(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return new Icon(ms);
            }
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }
    }
}
