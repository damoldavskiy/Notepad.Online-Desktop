using IronPython.Hosting;

using Microsoft.Scripting.Hosting;

using NotepadOnlineDesktopExtensions;

using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public string Description => "Snippets allows user to use fast replace strings and other IDE tools";

        Snippet[] snippets;
        ScriptEngine engine;
        ScriptScope scope;
        bool middle;
        int middleIndex;
        string middleWord;
        string middleValue;
        int currentPos;
        int currentLength;

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
            engine = Python.CreateEngine();
            scope = engine.CreateScope();
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
            {
                middle = false;
                return;
            }

            // Spaces
            if (e.Key == '\r' && !middle)
            {
                var remember_pos = app.SelectionStart;
                var txt = app.Text.Substring(0, remember_pos);
                var ind = txt.LastIndexOf('\n');
                if (ind < 0)
                    ind = 0;
                var t = txt.Substring(ind + 1);
                
                int c = 0;
                for (int i = 0; i < t.Length; i++)
                    if (t[i] == ' ')
                        c++;
                    else
                        break;
                string s = "";
                for (int i = 0; i < c; i++)
                    s += " ";
                app.Text = app.Text.Insert(app.SelectionStart, "\n" + s);
                app.SelectionStart = remember_pos + c + 1;
                e.Handled = true;
                return;
            }

            var text = app.Text;
            var pos = app.SelectionStart;

            // Middle input
            if (middle)
            {
                string recog = RecognizeSimpleSnippets(middleWord + e.Key);
                if (e.Key != '\t' || middleWord + e.Key != recog)
                {
                    string value;

                    if (e.Key == '\b')
                    {
                        if (middleWord.Length > 0)
                        {
                            value = Snippet.MiddleDelete(middleValue, middleWord, middleIndex);
                            middleWord = middleWord.Substring(0, middleWord.Length - 1);
                        }
                        else
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    else
                    {
                        var oldword = middleWord;
                        middleWord = recog;
                        value = Snippet.MiddleUpdate(middleValue, oldword, middleWord, middleIndex);
                    }

                    if (app.SelectionStart < currentPos || app.SelectionStart > currentPos + value.Length)
                    {
                        middle = false;
                        return;
                    }

                    middleValue = value;
                    value = Snippet.ClearValue(value);
                    text = text.Remove(currentPos, currentLength);
                    text = text.Insert(currentPos, value);
                    currentLength = value.Length;
                    app.Text = text;
                    app.SelectionStart = currentPos + Snippet.Index(middleValue, middleIndex) + middleWord.Length;
                    e.Handled = true;
                    return;
                }
                else
                {
                    middleValue = Snippet.ClearValue(middleValue, middleIndex);
                    if (Snippet.Index(middleValue, ++middleIndex) != -1)
                    {
                        middleWord = "";
                        app.SelectionStart = currentPos + Snippet.Index(middleValue, middleIndex);
                    }
                    else
                    {
                        if (Snippet.Index(middleValue, 0) != -1)
                        {
                            app.SelectionStart = currentPos + Snippet.Index(middleValue, 0);
                        }
                        middle = false;
                    }

                    e.Handled = true;
                    return;
                }
            }

            // Snippets
            var _text = text.Insert(pos, e.Key.ToString());
            var _pos = pos + 1;
            foreach (var snippet in snippets)
                if (_pos >= snippet.Template.Length && _text.Substring(_pos - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
                {
                    if (snippet.BeginOnly && pos >= snippet.Template.Length && text[pos - snippet.Template.Length] != '\n')
                        return;
                    
                    var value = snippet.Value;

                    if (snippet.ContainsPythonCode)
                    {
                        engine.Execute(snippet.PythonCode, scope);
                        value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                    }

                    middleValue = value;
                    value = Snippet.ClearValue(value);
                    if (snippet.CustomMiddlePositions)
                    {
                        middle = true;
                        middleIndex = 1;
                        middleWord = "";
                        currentPos = _pos - snippet.Template.Length;
                        currentLength = value.Length;
                    }

                    _text = _text.Remove(_pos - snippet.Template.Length, snippet.Template.Length);
                    _text = _text.Insert(_pos - snippet.Template.Length, value);

                    app.Text = _text;
                    app.SelectionStart = _pos - snippet.Template.Length;

                    if (snippet.CustomMiddlePositions)
                    {
                        app.SelectionStart += Snippet.Index(middleValue, 1);
                    }
                    else
                    {
                        app.SelectionStart += Snippet.Index(middleValue, 0);
                    }

                    e.Handled = true;
                    return;
                }

            char[] sB = { '$', '{', '[', '(' };
            char[] eB = { '$', '}', ']', ')' };

            // Skip braces
            for (int i = 0; i < sB.Length; i++)
                if (e.Key == eB[i] && pos < text.Length && text[pos] == eB[i])
                {
                    app.SelectionStart++;
                    e.Handled = true;
                    return;
                }

            // Double braces
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

        string RecognizeSimpleSnippets(string word)
        {
            foreach (var snippet in snippets)
                if (Snippet.Index(snippet.Value, 1) == -1 && !snippet.BeginOnly)
                    if (word.Length >= snippet.Template.Length && word.Substring(word.Length - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
                    {
                        var value = snippet.Value;

                        if (snippet.ContainsPythonCode)
                        {
                            engine.Execute(snippet.PythonCode, scope);
                            value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                        }

                        var middleValue = value;
                        value = Snippet.ClearValue(value);
                        return word.Substring(0, word.Length - snippet.Template.Length) + value;
                    }
            return word;
        }

        void Properties_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented");
        }
    }
}
