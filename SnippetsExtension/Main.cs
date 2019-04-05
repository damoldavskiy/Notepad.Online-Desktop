using NotepadOnlineDesktopExtensions;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SnippetsExtension
{
    [Export(typeof(IExtension))]
    class Main : IExtension
    {
        public string Name => "Snippets";

        public string Version => "1.0";

        public string Author => "DMSoft";

        public string Description => "Snippets allows user to use fast replace strings";

        Snippet[] snippets;

        public List<MenuItem> Menu
        {
            get
            {
                return new List<MenuItem>()
                {
                    properties
                };
            }
        }
        
        MenuItem properties;
        IApplicationInstance app;

        public Main()
        {
            properties = new MenuItem() { Header = "Properties" };
            properties.Click += Properties_Click;

            snippets = Importer.Load(Directory.GetCurrentDirectory() + "\\snippets.ini");
        }

        public async Task OnStart(IApplicationInstance instance)
        {
            app = instance;
            app.OnInput += App_OnInput;
            await Task.CompletedTask;
        }

        public async Task OnStop()
        {
            app.OnInput -= App_OnInput;
            await Task.CompletedTask;
        }

        void App_OnInput(object sender, InputEventArgs e)
        {
            if (app.SelectionLength > 0)
                return;

            var text = app.Text;
            var pos = app.SelectionStart;
            var math = text.Substring(0, pos).Count(c => c == '$') % 2 == 1;

            // Snippets
            var _text = text.Insert(pos, e.Key.ToString());
            var _pos = pos + 1;
            foreach (var snippet in snippets)
                if (_pos >= snippet.Template.Length && _text.Substring(_pos - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
                {
                    if (snippet.BeginOnly && pos > snippet.Template.Length && text[pos - snippet.Template.Length] != '\n')
                        return;
                    _text = _text.Remove(_pos - snippet.Template.Length, snippet.Template.Length);
                    _text = _text.Insert(_pos - snippet.Template.Length, math || snippet.BeginOnly ? snippet.Value : '$' + snippet.Value + '$');
                    app.Text = _text;
                    app.SelectionStart = _pos - snippet.Template.Length + snippet.Value.Length + (math ? 0 : 2);
                    if (snippet.CustomEndPosition)
                        app.SelectionStart += snippet.EndPosition - snippet.Value.Length;
                    e.Handled = true;
                    return;
                }

            // Double braces
            char[] sB = { '$', '{', '[', '(' };
            char[] eB = { '$', '}', ']', ')' };
            for (int i = 0; i < sB.Length; i++)
                if (e.Key == sB[i])
                {
                    app.Text = text.Insert(pos, sB[i].ToString() + eB[i]);
                    app.SelectionStart = pos + 1;
                    e.Handled = true;
                    return;
                }

            // Counted variables
            if ('0' <= e.Key && e.Key <= '9')
                if (--pos >= 0)
                    if ('a' <= text[pos] && text[pos] <= 'z' || 'A' <= text[pos] && text[pos] <= 'Z')
                    {
                        app.Text = text.Insert(++pos, "_" + e.Key);
                        app.SelectionStart = pos + 2;
                        e.Handled = true;
                        return;
                    }
        }

        void Properties_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented");
        }
    }
}
