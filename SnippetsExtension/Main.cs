using IronPython.Hosting;

using Microsoft.Scripting.Hosting;

using NotepadOnlineDesktopExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SnippetsExtension
{
    [Export(typeof(IExtension))]
    class Main : IExtension
    {
        public string Name => Properties.Resources.Name;

        public string Version => "1.0";

        public string Author => "DMSoft";

        public string Description => Properties.Resources.Info;

        readonly string configPath = AppDomain.CurrentDomain.BaseDirectory;
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
            properties = new MenuItem() { Header = Properties.Resources.Properties };
            properties.Click += Properties_Click;

            snippets = Importer.LoadSnippets(configPath + "\\Config\\Snippets.ini");
            brackets = Importer.LoadBrackets(configPath + "\\Config\\Brackets.ini");
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
            if (app.SelectionLength > 0 || e.SpecKey == SpecKey.Escape)
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
                    var t = txt.Substring(ind + 1);

                    int c = 0;
                    for (int i = 0; i < t.Length; i++)
                        if (t[i] == ' ')
                            c++;
                        else
                            break;
                    if (c == 0)
                        return;

                    var s = new String(' ', c);
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
                var delta = 0;
                var newCursorPos = pos - currentPos - Snippet.Index(middleValue, middleIndex);
                var newMiddle = "";
                var recog = "";
                if (e.SpecKey == SpecKey.None)
                {
                    newMiddle = InsertText(middleWord, e.Key, ref newCursorPos);
                    recog = RecognizeSimpleSnippets(newMiddle, newCursorPos, out delta);
                }
                if (e.Key != '\t' || newMiddle != recog)
                {
                    string value;

                    if (e.SpecKey == SpecKey.Backspace)
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
                    else if (e.SpecKey == SpecKey.Delete)
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
            if (Properties.Settings.Default.snippets && e.SpecKey == SpecKey.None)
            {
                var _text = text.Insert(pos, e.Key.ToString());
                var _pos = pos + 1;
                var tail = _text.Substring(_pos);
                var head = _text.Substring(0, _pos);

                foreach (var snippet in snippets)
                {
                    if (snippet.UsesRegex)
                    {
                        var match = Regex.Match(head, snippet.Template + @"\Z", RegexOptions.Multiline | RegexOptions.RightToLeft);
                        if (!match.Success)
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

                        _text = head;
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
                        var value = snippet.Value;

                        if (pos >= snippet.Template.Length)
                        {
                            var subtext = text.Substring(0, pos - snippet.Template.Length + 1);
                            var textForBeginCheck = subtext.TrimEnd(' ');
                            if (snippet.BeginOnly && !(textForBeginCheck.Length == 0 || textForBeginCheck.Last() == '\n'))
                                continue;
                            value = value.Replace("\n", "\n" + new string(' ', subtext.Length - textForBeginCheck.Length));

                        }

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

            if (Properties.Settings.Default.brackets && e.SpecKey == SpecKey.None)
            {
                for (int i = 0; i < brackets.Length; i++)
                    if (e.Key == brackets[i].Start || e.Key == brackets[i].End)
                    {
                        var inserted = InsertBracket(text, e.Key, ref pos, brackets[i].Start, brackets[i].End);
                        if (inserted != null)
                        {
                            app.Text = inserted;
                            app.SelectionStart = pos;
                            e.Handled = true;
                        }
                        break;
                    }
            }

            if (Properties.Settings.Default.tabs && e.SpecKey == SpecKey.None)
            {
                var inserted = InsertTab(text, e.Key, ref pos);
                if (inserted != null)
                {
                    app.Text = inserted;
                    app.SelectionStart = pos;
                    e.Handled = true;
                }
            }
        }

        string InsertText(string text, char key, ref int pos)
        {
            if (Properties.Settings.Default.brackets)
            {
                for (int i = 0; i < brackets.Length; i++)
                    if (key == brackets[i].Start || key == brackets[i].End)
                    {
                        var inserted = InsertBracket(text, key, ref pos, brackets[i].Start, brackets[i].End);
                        if (inserted != null)
                            return inserted;
                        else
                            break;
                    }
            }

            return text.Insert(pos++, key.ToString());
        }

        string InsertBracket(string text, char key, ref int pos, char start, char end)
        {
            // Skip braces
            if (key == end && pos < text.Length && text[pos] == end)
            {
                pos++;
                return text;
            }

            // Double braces
            if (key == start)
            {
                text = text.Insert(pos, start.ToString() + end);
                pos++;
                return text;
            }

            return null;
        }

        string InsertTab(string text, char key, ref int pos)
        {
            if (key == '\t')
            {
                text = text.Insert(pos, "    ");
                pos += 4;
                return text;
            }

            return null;
        }

        string RecognizeSimpleSnippets(string word, int pos, out int delta)
        {
            var part1 = word.Substring(0, pos);
            var part2 = word.Length > pos ? word.Substring(pos) : "";

            foreach (var snippet in snippets)
                if (Snippet.Index(snippet.Value, 1) == -1 && !snippet.BeginOnly)
                    if (snippet.UsesRegex)
                    {
                        var matches = Regex.Matches(part1, snippet.Template);
                        if (matches.Count == 0)
                            continue;
                        var match = matches[matches.Count - 1];
                        if (match.Index + match.Length < part1.Count())
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
                        return part1.Substring(0, part1.Length - match.Value.Length) + value + part2;
                    }
                    else if (part1.Length >= snippet.Template.Length && part1.Substring(part1.Length - snippet.Template.Length, snippet.Template.Length) == snippet.Template)
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
                        return part1.Substring(0, part1.Length - snippet.Template.Length) + value + part2;
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
