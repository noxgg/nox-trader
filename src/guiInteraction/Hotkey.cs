using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace noxiousET.src.guiInteraction
{
    public class Hotkey : IMessageFilter
    {
        #region Interop

        private const uint WmHotkey = 0x312;

        private const uint ModAlt = 0x1;
        private const uint ModControl = 0x2;
        private const uint ModShift = 0x4;
        private const uint ModWin = 0x8;

        private const uint ErrorHotkeyAlreadyRegistered = 1409;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        private const int MaximumId = 0xBFFF;
        private static int _currentId;

        private bool _alt;
        private bool _control;

        [XmlIgnore] private int _id;
        private Keys _keyCode;
        [XmlIgnore] private bool _registered;
        private bool _shift;
        [XmlIgnore] private Control _windowControl;
        private bool _windows;

        public Hotkey() : this(Keys.None, false, false, false, false)
        {
            // No work done here!
        }

        public Hotkey(Keys keyCode, bool shift, bool control, bool alt, bool windows)
        {
            // Assign properties
            KeyCode = keyCode;
            Shift = shift;
            Control = control;
            Alt = alt;
            Windows = windows;

            // Register us as a message filter
            Application.AddMessageFilter(this);
        }

        public bool Empty
        {
            get { return _keyCode == Keys.None; }
        }

        public bool Registered
        {
            get { return _registered; }
        }

        public Keys KeyCode
        {
            get { return _keyCode; }
            set
            {
                // Save and reregister
                _keyCode = value;
                Reregister();
            }
        }

        public bool Shift
        {
            get { return _shift; }
            set
            {
                // Save and reregister
                _shift = value;
                Reregister();
            }
        }

        public bool Control
        {
            get { return _control; }
            set
            {
                // Save and reregister
                _control = value;
                Reregister();
            }
        }

        public bool Alt
        {
            get { return _alt; }
            set
            {
                // Save and reregister
                _alt = value;
                Reregister();
            }
        }

        public bool Windows
        {
            get { return _windows; }
            set
            {
                // Save and reregister
                _windows = value;
                Reregister();
            }
        }

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message message)
        {
            // Only process WM_HOTKEY messages
            if (message.Msg != WmHotkey)
            {
                return false;
            }

            // Check that the ID is our key and we are registerd
            if (_registered && (message.WParam.ToInt32() == _id))
            {
                // Fire the event and pass on the event if our handlers didn't handle it
                return OnPressed();
            }
            else
            {
                return false;
            }
        }

        #endregion

        public event HandledEventHandler Pressed;

        ~Hotkey()
        {
            // Unregister the hotkey if necessary
            if (Registered)
            {
                Unregister();
            }
        }

        public Hotkey Clone()
        {
            // Clone the whole object
            return new Hotkey(_keyCode, _shift, _control, _alt, _windows);
        }

        public bool GetCanRegister(Control windowControl)
        {
            // Handle any exceptions: they mean "no, you can't register" :)
            try
            {
                // Attempt to register
                if (!Register(windowControl))
                {
                    return false;
                }

                // Unregister and say we managed it
                Unregister();
                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public bool Register(Control windowControl)
        {
            // Check that we have not registered
            if (_registered)
            {
                throw new NotSupportedException("You cannot register a hotkey that is already registered");
            }

            // We can't register an empty hotkey
            if (Empty)
            {
                throw new NotSupportedException("You cannot register an empty hotkey");
            }

            // Get an ID for the hotkey and increase current ID
            _id = _currentId;
            _currentId = _currentId + 1%MaximumId;

            // Translate modifier keys into unmanaged version
            uint modifiers = (Alt ? ModAlt : 0) | (Control ? ModControl : 0) |
                             (Shift ? ModShift : 0) | (Windows ? ModWin : 0);

            // Register the hotkey
            if (RegisterHotKey(windowControl.Handle, _id, modifiers, _keyCode) == 0)
            {
                // Is the error that the hotkey is registered?
                if (Marshal.GetLastWin32Error() == ErrorHotkeyAlreadyRegistered)
                {
                    return false;
                }
                throw new Win32Exception();
            }

            // Save the control reference and register state
            _registered = true;
            _windowControl = windowControl;

            // We successfully registered
            return true;
        }

        public void Unregister()
        {
            // Check that we have registered
            if (!_registered)
            {
                throw new NotSupportedException("You cannot unregister a hotkey that is not registered");
            }

            // It's possible that the control itself has died: in that case, no need to unregister!
            if (!_windowControl.IsDisposed)
            {
                // Clean up after ourselves
                if (UnregisterHotKey(_windowControl.Handle, _id) == 0)
                {
                    throw new Win32Exception();
                }
            }

            // Clear the control reference and register state
            _registered = false;
            _windowControl = null;
        }

        private void Reregister()
        {
            // Only do something if the key is already registered
            if (!_registered)
            {
                return;
            }

            // Save control reference
            Control windowControl = _windowControl;

            // Unregister and then reregister again
            Unregister();
            Register(windowControl);
        }

        private bool OnPressed()
        {
            // Fire the event if we can
            var handledEventArgs = new HandledEventArgs(false);
            if (Pressed != null)
            {
                Pressed(this, handledEventArgs);
            }

            // Return whether we handled the event or not
            return handledEventArgs.Handled;
        }

        public override string ToString()
        {
            // We can be empty
            if (Empty)
            {
                return "(none)";
            }

            // Build key name
            string keyName = Enum.GetName(typeof (Keys), _keyCode);
            ;
            switch (_keyCode)
            {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    // Strip the first character
                    keyName = keyName.Substring(1);
                    break;
                    // Leave everything alone
            }

            // Build modifiers
            string modifiers = "";
            if (_shift)
            {
                modifiers += "Shift+";
            }
            if (_control)
            {
                modifiers += "Control+";
            }
            if (_alt)
            {
                modifiers += "Alt+";
            }
            if (_windows)
            {
                modifiers += "Windows+";
            }

            // Return result
            return modifiers + keyName;
        }
    }
}