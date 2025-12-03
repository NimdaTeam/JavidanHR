using System.Security.Cryptography;
using System.Text;

namespace _0_Framework.Utilities.Security
{
    public static class PasswordHelper
    {



        public static string EncodePasswordMd5(string pass) //Encrypt using MD5   
        {
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;
            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)   
#pragma warning disable SYSLIB0021
            md5 = new MD5CryptoServiceProvider();
#pragma warning restore SYSLIB0021
            originalBytes = ASCIIEncoding.Default.GetBytes(pass);
            encodedBytes = md5.ComputeHash(originalBytes);
            //Convert encoded bytes back to a 'readable' string   
            return BitConverter.ToString(encodedBytes);
        }


    }
}
