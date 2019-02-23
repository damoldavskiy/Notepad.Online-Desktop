using System;

namespace NotepadOnlineDesktop.Model
{
    public class FindEventArgs : EventArgs
    {
        public string Word { get; private set; }
        public bool IgnoreCase { get; private set; }
        public bool UpDirection { get; private set; }
        public bool DownDirection { get; private set; }

        public FindEventArgs(string word, bool ignoreCase, bool upDirection, bool downDirection)
        {
            Word = word ?? throw new ArgumentNullException(nameof(word));
            IgnoreCase = ignoreCase;
            UpDirection = upDirection;
            DownDirection = downDirection;
        }
    }
}
