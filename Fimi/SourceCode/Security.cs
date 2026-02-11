using System.Security.Cryptography;
using System.Text;

namespace Fimi
{
    public static class Security
    {
        public static bool CheckMD5(string s, string hash)
        {
            return MD5(s) == hash;
        }

        public static string CheckAllMessages(string md5Hash, string md5Message, string rsaEncryptedMessage)
        {
            if (md5Hash != MD5(md5Message))
            {
                throw new ShukrMoliyaException("Incorrect md5 sign");
            }

            return DecryptRSA(rsaEncryptedMessage);
        }

        public static string EncryptRSA(string message)
        {
            using var sr = new StreamReader(Utils.PathToRSAPublicPrivateKey);
            var key = sr.ReadToEnd();

            using var rsa = new RSACryptoServiceProvider(1024);
            rsa.FromXmlString(key);

            byte[] bytesMessage = Encoding.UTF8.GetBytes(message);
            byte[] encryptByteMessage = rsa.Encrypt(bytesMessage, false);
            string encryptX64String = Convert.ToBase64String(encryptByteMessage);

            return encryptX64String;
        }

        public static string DecryptRSA(string message)
        {
            using var sr = new StreamReader(Utils.PathToRSAPublicPrivateKey);
            var key = sr.ReadToEnd();

            using var rsa = new RSACryptoServiceProvider(1024);
            rsa.FromXmlString(key);

            byte[] encryptByteMessage = Convert.FromBase64String(message);
            byte[] decryptByteMessage = rsa.Decrypt(encryptByteMessage, false);
            string decryptMessage = Encoding.UTF8.GetString(decryptByteMessage);

            return decryptMessage;
        }

        public static string MD5(string str)
        {
            using MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(str);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}
