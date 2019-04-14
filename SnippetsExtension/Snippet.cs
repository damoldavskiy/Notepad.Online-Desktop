using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SnippetsExtension
{
    public class Snippet
    {
        public string Template { get; set; }
        public string Value { get; set; }
        public bool BeginOnly { get; set; }
        public bool UsesRegex { get; set; }
        public bool ContainsPythonCode { get; set; }
        public string PythonCode { get; set; }
        public int PythonPosition { get; set; }

        public static string ClearValue(string value)
        {
            return Regex.Replace(value, @"(?<!\\)\$\d", "");
        }

        public static string ClearValue(string value, int index)
        {
            return Regex.Replace(value, @"(?<!\\)\$" + index, "");
        }

        public static int Index(string value, int index)
        {
            value = Regex.Replace(value, $@"(?<!\\)\$[^{index}]", "");
            var match = Regex.Match(value, @"(?<!\\)\$" + index);
            return match.Success ? match.Index : -1;
        }

        public static string MiddleUpdate(string value, string oldword, string newword, int index)
        {
            var find = @"(?<!\\)\$" + index;
            return Regex.Replace(value, find + Regex.Escape(oldword), "$$" + index + newword.Replace("$", "$$"));
        }
    }
}
