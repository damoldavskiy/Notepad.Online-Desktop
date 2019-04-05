using System.Collections.Generic;

namespace SnippetsExtension
{
    public class Snippet
    {
        public string Template { get; set; }
        public string Value { get; set; }
        public bool CustomEndPosition { get; set; }
        public bool BeginOnly { get; set; }
        public int EndPosition { get; set; }
        public bool CustomMiddlePositions { get; set; }
        public List<MultiCaret> MiddlePositions { get; set; }
        public bool ContainsPythonCode { get; set; }
        public string PythonCode { get; set; }
        public int PythonPosition { get; set; }
    }
}
