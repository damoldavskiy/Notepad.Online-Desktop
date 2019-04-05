using System;

namespace NotepadOnlineDesktopExtensions
{
    public delegate void InputHandler(object sender, InputEventArgs e);

    public class InputEventArgs : EventArgs
    {
        public char Key { get; set; }
        public bool Handled { get; set; }

        public InputEventArgs(char key)
        {
            Key = key;
        }
    }
}
