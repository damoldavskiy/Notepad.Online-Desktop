namespace NotepadOnlineDesktopExtensions
{
    public interface IApplicationInstance
    {
        event InputHandler OnInput;
        string Text { get; set; }
        string Name { get; }
        int SelectionStart { get; set; }
        int SelectionLength { get; set; }
        void Open(string name);
        void OpenFolder(string path);
    }
}
