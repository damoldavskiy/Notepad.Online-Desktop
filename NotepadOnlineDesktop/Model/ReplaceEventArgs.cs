using System;

namespace NotepadOnlineDesktop.Model
{
    public class ReplaceEventArgs : EventArgs
    {
        public string OldWord { get; private set; }
        public string NewWord { get; private set; }
        public bool IgnoreCase { get; private set; }

        public ReplaceEventArgs(string oldWord, string newWord, bool ignoreCase)
        {
            OldWord = oldWord ?? throw new ArgumentNullException(nameof(oldWord));
            NewWord = newWord ?? throw new ArgumentNullException(nameof(newWord));
            IgnoreCase = ignoreCase;
        }
    }
}
