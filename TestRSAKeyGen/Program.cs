using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TestRSAKeyGen
{
    internal class Program
    {
        private static string[] ParsePEMKeyPair(Process proc)
        {
            string publicKey = null;
            string privateKey = null;
            StringBuilder sb = null;
            while (true)
            {
                string s = proc.StandardOutput.ReadLine();
                if (s == null)
                    break;
                if (s.StartsWith("********"))
                {
                    sb = new StringBuilder();
                }
                else if (s.StartsWith("++++++++"))
                {
                    if (privateKey == null)
                        privateKey = sb.ToString();
                    else if (publicKey == null)
                        publicKey = sb.ToString();
                }
                else if (sb != null)
                    sb.Append(s);
            }
            string[] keyPair = new string[2];
            keyPair[0] = privateKey;
            keyPair[1] = publicKey;
            return keyPair;
        }
        private static string[] GenerateKeyPair()
        {
            var pr = new System.Diagnostics.ProcessStartInfo("RSAKeyGen", "generate")
            {
                UseShellExecute = false,
                Verb = "open",
                RedirectStandardInput = true,
                StandardInputEncoding = Encoding.ASCII,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                RedirectStandardError = false,
                CreateNoWindow = false
            };
            Process proc = System.Diagnostics.Process.Start(pr);
            return ParsePEMKeyPair(proc);
        }
        private static string Encrypt(string publicKeyPEM, byte[] plainText)
        {
            var pr = new System.Diagnostics.ProcessStartInfo("RSAKeyGen", "encrypt")
            {
                UseShellExecute = false,
                Verb = "open",
                RedirectStandardInput = true,
                StandardInputEncoding = Encoding.ASCII,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.ASCII,
                CreateNoWindow = false
            };
            Process proc = System.Diagnostics.Process.Start(pr);
            string s = Convert.ToBase64String(plainText);
            proc.StandardInput.WriteLine(publicKeyPEM);
            proc.StandardInput.WriteLine("++++++++");
            proc.StandardInput.WriteLine(s);
            var output = proc.StandardOutput.ReadLine();
            proc.WaitForExit();
            return output;
        }
        private static string Decrypt(string privateKeyPEM, byte[] cipherText)
        {
            var pr = new System.Diagnostics.ProcessStartInfo("RSAKeyGen", "decrypt")
            {
                UseShellExecute = false,
                Verb = "open",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.ASCII,
                CreateNoWindow = false
            };
            Process proc = System.Diagnostics.Process.Start(pr);
            string s = Convert.ToBase64String(cipherText);
            proc.StandardInput.WriteLine(privateKeyPEM);
            proc.StandardInput.WriteLine("++++++++");
            proc.StandardInput.WriteLine(s);
            var output = proc.StandardOutput.ReadLine();
            proc.WaitForExit();
            return output;
        }
        static void Main(string[] args)
        {
            try
            {
                if( args.Length < 3)
                {
                    Console.WriteLine("args: <generateKey>|<encrypt>|<descrypt> <keyFile> <inputFile> <outputFile>");
                    Console.WriteLine();
                    //
                    string[] keyPair = GenerateKeyPair();
                    Console.WriteLine(keyPair[0]);
                    Console.WriteLine(keyPair[1]);
                    //
                    byte[] b = new byte[64];
                    for (int i = 0; i < b.Length; i++)
                        b[i] = (byte)i;
                    string s = Encrypt(keyPair[1], b);
                    Console.WriteLine("Encrypt: " + s);
                    byte[] b2 = Convert.FromBase64String(s);
                    //
                    string s2 = Decrypt(keyPair[0], b2);
                    Console.WriteLine("Decrypt: " + s2);
                    byte[] b3 = Convert.FromBase64String(s2);
                    for (int i = 0; i < b3.Length; i++)
                        Console.WriteLine(b3[i]);
                    return;
                }
                if (args[0].Equals("generateKey"))
                {
                    string[] keyPair = GenerateKeyPair();
                    Console.WriteLine(keyPair[0]);
                    Console.WriteLine(keyPair[1]);
                    //
                    using (StreamWriter sw = new StreamWriter(args[1]))
                    {
                        sw.WriteLine(keyPair[0]);
                    }
                    using (StreamWriter sw = new StreamWriter(args[2]))
                    {
                        sw.WriteLine(keyPair[1]);
                    }
                }
                else if (args[0].Equals("encrypt"))
                {
                    string publicKey = File.ReadAllText(args[1]);
                    byte[] b = File.ReadAllBytes(args[2]);
                    byte[] _b = new byte[32];
                    for (int i = 0; i < _b.Length; i++)
                    {
                        if (i < b.Length)
                            _b[i] = b[i];
                        else
                            _b[i] = 0x20;
                    }
                    string s = Encrypt(publicKey, _b);
                    Console.WriteLine(s);
                    byte[] b2 = Convert.FromBase64String(s);
                    File.WriteAllBytes(args[3], b2);
                }
                else if (args[0].Equals("decrypt"))
                {
                    string privateKey = File.ReadAllText(args[1]);
                    byte[] b = File.ReadAllBytes(args[2]);
                    string s = Decrypt(privateKey, b);
                    Console.WriteLine(s);
                    byte[] b2 = Convert.FromBase64String(s);
                    File.WriteAllBytes(args[3], b2);
                }
                else
                {
                    Console.WriteLine("args: <generateKey>|<encrypt>|<descrypt> <keyFile> <inputFile> <outputFile>");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
