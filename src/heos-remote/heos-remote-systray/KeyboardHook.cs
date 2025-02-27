using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using heos_remote_lib;

// see: https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

namespace heos_remote_systray
{
    public sealed class KeyboardHook : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable
        {
            private static int WM_HOTKEY = 0x0312;

            public Window()
            {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY)
                {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    if (KeyPressed != null)
                        KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                }
            }

            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }

        private Window _window = new Window();
        private int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            var uim = (uint)modifier;
            var uik = (uint)key;
            if (!RegisterHotKey(_window.Handle, _currentId, uim, uik)) 
            {
                var x = Marshal.GetLastWin32Error();
                throw new InvalidOperationException("Couldn’t register the hot key.");
            }
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                UnregisterHotKey(_window.Handle, i);
            }

            // dispose the inner native window.
            _window.Dispose();
        }

        #endregion

        //
        // added
        //

        /// <summary>
        /// Evaluates an string such as "Shift+Alt+F12"
        /// </summary>
        /// <param name="kst"></param>
        /// <returns></returns>
        public static Tuple<ModifierKeys, Keys>? ParseKeyDesignation(string kst)
        {
            // access
            if (kst?.HasContent() != true)
                return null;
            var parts = kst.Split('+');
            if (parts.Length < 1)
                return null;

            ModifierKeys mk = 0;
            Keys key = 0;
            foreach (var part in parts)
            {
                if (part.Trim().Equals("Shift", StringComparison.InvariantCultureIgnoreCase))
                    mk = mk | ModifierKeys.Shift;
                else if (part.Trim().Equals("Control", StringComparison.InvariantCultureIgnoreCase))
                    mk = mk | ModifierKeys.Control;
                else if (part.Trim().Equals("Alt", StringComparison.InvariantCultureIgnoreCase))
                    mk = mk | ModifierKeys.Alt;
                else if (part.Trim().Equals("Win", StringComparison.InvariantCultureIgnoreCase))
                    mk = mk | ModifierKeys.Win;
                else
                {
                    //is expected to be the key!
                    foreach (var ke in Enum.GetValues(typeof(Keys)))
                        if (part.Trim().Equals(Enum.GetName(typeof(Keys), ke), StringComparison.InvariantCultureIgnoreCase))
                            key = (Keys)ke;
                }
            }

            if (key == 0)
                return null;
            return new Tuple<ModifierKeys, Keys>(mk, key);
        }

    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
