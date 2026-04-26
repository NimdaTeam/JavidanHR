using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _0_Framework.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveNonDigits(this string input)
        {
            if(string.IsNullOrWhiteSpace(input)) 
                return "";

            return new string(input.Where(char.IsDigit).ToArray());
        }
    }
}
