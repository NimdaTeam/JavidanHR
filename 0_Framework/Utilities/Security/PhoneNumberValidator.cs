using System.Text.RegularExpressions;

namespace _0_Framework.Utilities.Security;

public class PhoneNumberValidator
{
    public static bool IsValid(string phoneNumber)
    {
        // Regular expression pattern for Persian phone numbers
        string pattern = @"^(?:\+98|0)?9\d{9}$";

        // Using Regex to validate the phone number
        return Regex.IsMatch(phoneNumber, pattern);
    }
}