using System;

namespace NotepadOnlineDesktopExtensions
{
    public delegate void InputHandler(object sender, InputEventArgs e);

    public class InputEventArgs : EventArgs
    {
        public char Key { get; }
        public SpecKey SpecKey { get; }
        public bool Handled { get; set; }

        public InputEventArgs(char key)
        {
            Key = key;
            SpecKey = SpecKey.None;
        }

        public InputEventArgs(SpecKey specKey)
        {
            Key = '\0';
            SpecKey = specKey;
        }
    }
}
