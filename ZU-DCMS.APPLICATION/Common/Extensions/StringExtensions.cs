namespace ZU_DCMS.APPLICATION.Common.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] ArabicDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static readonly string[] EnglishDigits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public static string NormalizeDigits(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = input;
            for (int i = 0; i < 10; i++)
            {
                result = result.Replace(ArabicDigits[i], EnglishDigits[i]);
            }
            return result;
        }
    }
}
