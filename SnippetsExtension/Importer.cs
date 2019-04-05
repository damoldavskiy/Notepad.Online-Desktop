using System;
using System.Collections.Generic;
using System.IO;

namespace SnippetsExtension
{
    static class Importer
    {
        static readonly string _snippet = "snippet";

        public static Snippet[] Load(string path)
        {
            var snippets = new List<Snippet>();

            var state = ImporterState.ReadingTemplate;
            var template = "";
            var headers = "";
            var value = "";

            using (var stream = new StreamReader(path))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();

                    if (state == ImporterState.ReadingTemplate)
                        if (line.StartsWith("snippet"))
                        {
                            line = line.Remove(0, _snippet.Length).Trim();
                            if (line[0] != '\'')
                                throw new Exception(); // После snippet нету кавычки
                            line = line.Substring(1);

                            int pos = -1;
                            for (int i = 0; i < line.Length - 1; i++)
                                if (line[i] != '\\' && line[i + 1] == '\'')
                                {
                                    pos = i + 1;
                                    break;
                                }

                            if (pos == -1)
                                throw new Exception(); // Нету символа окончания шаблона

                            template = line.Substring(0, pos);
                            headers = line.Substring(pos + 1).Trim();

                            state = ImporterState.ReadingValue;
                            continue;
                        }
                        else if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                            continue;
                        else
                            throw new Exception(); // Ожидалось слово snippet

                    if (state == ImporterState.ReadingValue)
                    {
                        if (line == "")
                        {
                            snippets.Add(DecodeSnippet(template, headers, value));

                            state = ImporterState.ReadingTemplate;
                            template = "";
                            headers = "";
                            value = "";
                        }
                        else if (line == ".")
                            value += "\n";
                        else if (line == "\\.")
                            value += ".";
                        else
                            value += (value.Length > 0 ? "\n" : "") + line;
                        continue;
                    }
                }

                snippets.Add(DecodeSnippet(template, headers, value));
            }

            snippets.Sort((a, b) => a.Template.Contains(b.Template) ? 0 : 1);
            snippets.Reverse();

            return snippets.ToArray();
        }

        static Snippet DecodeSnippet(string template, string headers, string value)
        {
            headers = headers.ToUpper();

            var snippet = new Snippet
            {
                Template = template + (headers.Contains("A") ? "" : "\t"),
                BeginOnly = headers.Contains("B")
            };

            for (int i = 0; i < value.Length - 2; i++)
                if (value[i] != '\\' && value[i + 1] == '$' && char.IsDigit(value[i + 2]))
                {
                    if (value[i + 2] == '0')
                    {
                        snippet.CustomEndPosition = true;
                        snippet.EndPosition = i;
                        value = value.Remove(i + 1, 2);
                    }
                }

            snippet.Value = value;

            return snippet;
        }
    }
}
