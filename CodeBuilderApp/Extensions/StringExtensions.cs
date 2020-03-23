namespace CodeBuilderApp.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceTextWithTag(this string text, string replaceText, string tag) => text.Replace(replaceText, $"${tag}$");
        public static string ReplaceTagWithText(this string text, string replaceText, string tag) => text.Replace($"${tag}$", replaceText);
    }
}