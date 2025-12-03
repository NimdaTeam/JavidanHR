namespace _0_Framework.Utilities.Generators
{
    public static class OTPCodeGenerator
    {
        public static string GenerateOTPCode()
        {
            var random = new Random();

            var code = random.Next(1000, 9999);

            return code.ToString();
        }
    }
}
