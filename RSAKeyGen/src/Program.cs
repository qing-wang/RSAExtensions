using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RSAExtensions.ConsoleApp
{
    class Program
    {
        private static void GenerateRSAKeyPair()
        {
            var rsa = RSA.Create(1024);
            Console.WriteLine("********");
            Console.WriteLine(rsa.ExportPrivateKey(RSAKeyType.Pkcs1, true));
            Console.WriteLine("++++++++");
            Console.WriteLine("********");
            Console.WriteLine(rsa.ExportPublicKey(RSAKeyType.Pkcs8, true));
            Console.WriteLine("++++++++");
        }
        private static void Encrypt()
        {
            string publicKeyPEM = ReadPEMKey();
            string plainTextBase64 = Console.ReadLine();
            //
            byte[] plainText = Convert.FromBase64String(plainTextBase64);
            var rsa = RSA.Create(1024);
            //
            rsa.ImportPublicKey(RSAKeyType.Pkcs8, publicKeyPEM, true);
            //
            byte[] cipherText = rsa.Encrypt(plainText, RSAEncryptionPadding.OaepSHA1);
            string cipherTextBase64 = Convert.ToBase64String(cipherText);
            Console.WriteLine(cipherTextBase64);
        }
        private static void Decrypt()
        {
            string privateKeyPEM = ReadPEMKey();
            string cipherTextBase64 = Console.ReadLine();
            //
            byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
            var rsa = RSA.Create(1024);
            //
            rsa.ImportPrivateKey(RSAKeyType.Pkcs1, privateKeyPEM, true);
            //
            byte[] plainText = rsa.Decrypt(cipherText, RSAEncryptionPadding.OaepSHA1);
            string plainTextBase64 = Convert.ToBase64String(plainText);
            Console.WriteLine(plainTextBase64);
        }
        private static string ReadPEMKey()
        {
            StringBuilder sb = new StringBuilder();
            while(true)
            {
                string s = Console.ReadLine();
                if (s == null)
                    break;
                if (s.StartsWith("++++++++"))
                    break;
                sb.Append(s);
            }
            return sb.ToString();
        }
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || args[0].Equals("generate"))
                {
                    GenerateRSAKeyPair();
                    return;
                }
                if (args[0].Equals("encrypt"))
                {
                    Encrypt();
                    return;
                }
                if (args[0].Equals("decrypt"))
                {
                    Decrypt();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
