using IronPython.Hosting;

using Microsoft.Scripting.Hosting;

using NotepadOnlineDesktopExtensions;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;

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
        Bracket[] brackets;
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
            
            snippets = Importer.LoadSnippets(Directory.GetCurrentDirectory() + "\\Config\\Snippets.ini");
            brackets = Importer.LoadBrackets(Directory.GetCurrentDirectory() + "\\Config\\Brackets.ini");
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
            if (app.SelectionLength > 0 || e.Key == '\0')
            {
                middle = false;
                return;
            }

            // Spaces
            if (Properties.Settings.Default.spaces)
            {
                if (e.Key == '\r' && !middle && app.SelectionStart != 0)
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
            }

            var text = app.Text;
            var pos = app.SelectionStart;

            // Middle input
            if (middle && (app.SelectionStart < currentPos + Snippet.Index(middleValue, middleIndex) || app.SelectionStart > currentPos + Snippet.Index(middleValue, middleIndex) + middleWord.Length))
                middle = false;
            if (middle)
            {
                int delta;
                var newCursorPos = pos - currentPos - Snippet.Index(middleValue, middleIndex);
                var newMiddle = "";
                if (e.Key != '\b' && e.Key != '\a')
                    newMiddle = InsertText(middleWord, e.Key, ref newCursorPos);
                string recog = RecognizeSimpleSnippets(newMiddle, out delta);
                if (e.Key != '\t' || newMiddle != recog)
                {
                    string value;

                    if (e.Key == '\b')
                    {
                        if (newCursorPos > 0)
                        {
                            newMiddle = middleWord.Remove(newCursorPos - 1, 1);
                            value = Snippet.MiddleUpdate(middleValue, middleWord, newMiddle, middleIndex);
                            middleWord = newMiddle;
                            newCursorPos--;
                        }
                        else
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                    else if (e.Key == '\a')
                    {
                        if (newCursorPos < middleWord.Length)
                        {
                            newMiddle = middleWord.Remove(newCursorPos, 1);
                            value = Snippet.MiddleUpdate(middleValue, middleWord, newMiddle, middleIndex);
                            middleWord = newMiddle;
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

                    middleValue = value;
                    value = Snippet.ClearValue(value);
                    text = text.Remove(currentPos, currentLength);
                    text = text.Insert(currentPos, value);
                    currentLength = value.Length;
                    app.Text = text;
                    app.SelectionStart = currentPos + Snippet.Index(middleValue, middleIndex) + newCursorPos + delta;
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
            if (Properties.Settings.Default.snippets)
            {
                foreach (var snippet in snippets)
                {
                    var _text = text.Insert(pos, e.Key.ToString());
                    var _pos = pos + 1;

                    if (snippet.UsesRegex)
                    {
                        var tail = _text.Substring(_pos);
                        _text = _text.Substring(0, _pos);

                        var matches = Regex.Matches(_text, snippet.Template, RegexOptions.Multiline);
                        if (matches.Count == 0)
                            continue;
                        var match = matches[matches.Count - 1];
                        if (match.Index + match.Length < _text.Count())
                            continue;

                        var value = snippet.Value;
                        if (snippet.ContainsPythonCode)
                        {
                            scope.SetVariable("word", match.Value);
                            engine.Execute(snippet.PythonCode, scope);
                            value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                        }

                        middleValue = value;
                        value = Snippet.ClearValue(value);
                        var customMiddlePositions = Snippet.Index(middleValue, 1) != -1;
                        if (customMiddlePositions)
                        {
                            middle = true;
                            middleIndex = 1;
                            middleWord = "";
                            currentPos = _pos - match.Value.Length;
                            currentLength = value.Length;
                        }

                        _text = _text.Remove(_pos - match.Value.Length, match.Value.Length);
                        _text = _text.Insert(_pos - match.Value.Length, value);

                        app.Text = _text + tail;
                        app.SelectionStart = _pos - match.Value.Length;

                        if (customMiddlePositions)
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
                    else if (_pos >= snippet.Template.Length && _text.Substring(_pos - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
                    {
                        if (snippet.BeginOnly && pos >= snippet.Template.Length && text[pos - snippet.Template.Length] != '\n')
                            continue;

                        var value = snippet.Value;

                        if (snippet.ContainsPythonCode)
                        {
                            engine.Execute(snippet.PythonCode, scope);
                            value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                        }

                        middleValue = value;
                        value = Snippet.ClearValue(value);
                        var customMiddlePositions = Snippet.Index(middleValue, 1) != -1;
                        if (customMiddlePositions)
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

                        if (customMiddlePositions)
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
                }
            }

            if (e.Key != '\b' && e.Key != '\a')
            {
                app.Text = InsertText(text, e.Key, ref pos);
                app.SelectionStart = pos;
                e.Handled = true;
            }
        }

        string InsertText(string text, char key, ref int pos)
        {
            if (Properties.Settings.Default.brackets)
            {
                // Skip braces
                for (int i = 0; i < brackets.Length; i++)
                    if (key == brackets[i].End && pos < text.Length && text[pos] == brackets[i].End)
                    {
                        pos++;
                        return text;
                    }

                // Double braces
                for (int i = 0; i < brackets.Length; i++)
                    if (key == brackets[i].Start)
                    {
                        text = text.Insert(pos, brackets[i].Start.ToString() + brackets[i].End);
                        pos++;
                        return text;
                    }
            }

            text = text.Insert(pos, key.ToString());
            pos++;
            return text;
        }

        string RecognizeSimpleSnippets(string word, out int delta)
        {
            foreach (var snippet in snippets)
                if (Snippet.Index(snippet.Value, 1) == -1 && !snippet.BeginOnly)
                    if (snippet.UsesRegex)
                    {
                        var matches = Regex.Matches(word, snippet.Template);
                        if (matches.Count == 0)
                            continue;
                        var match = matches[matches.Count - 1];
                        if (match.Index + match.Length < word.Count())
                            continue;

                        var value = snippet.Value;
                        if (snippet.ContainsPythonCode)
                        {
                            scope.SetVariable("word", match.Value);
                            engine.Execute(snippet.PythonCode, scope);
                            value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                        }

                        var middleValue = value;
                        value = Snippet.ClearValue(value);

                        delta = Snippet.Index(middleValue, 0) - match.Value.Length;
                        return word.Substring(0, word.Length - match.Value.Length) + value;
                    }
                    else if (word.Length >= snippet.Template.Length && word.Substring(word.Length - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
                    {
                        var value = snippet.Value;

                        if (snippet.ContainsPythonCode)
                        {
                            engine.Execute(snippet.PythonCode, scope);
                            value = value.Insert(snippet.PythonPosition, scope.GetVariable("value"));
                        }

                        var middleValue = value;
                        value = Snippet.ClearValue(value);

                        delta = Snippet.Index(middleValue, 0) - snippet.Template.Length;
                        return word.Substring(0, word.Length - snippet.Template.Length) + value;
                    }

            delta = 0;
            return word;
        }

        void Properties_Click(object sender, RoutedEventArgs e)
        {
            new PropertiesWindow(app).ShowDialog();
        }
    }
}
