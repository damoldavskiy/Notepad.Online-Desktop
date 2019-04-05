namespace SnippetsExtension
{
    public class Snippet
    {
        public string Template { get; set; }
        public string Value { get; set; }
        public bool CustomEndPosition { get; set; }
        public bool BeginOnly { get; set; }
        public int EndPosition { get; set; }
    }
}
