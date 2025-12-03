namespace _0_Framework.Utilities.Helpers
{
    public static class PdfTextHelper
    {
        public static string ReverseString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
