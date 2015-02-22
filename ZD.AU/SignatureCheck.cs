using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;

namespace ZD.AU
{
    class SignatureCheck
    {
        /* Generating a new key
            var rsa = new System.Security.Cryptography.RSACryptoServiceProvider(2048);
            rsa.PersistKeyInCsp = false;
            string pplXml = rsa.ToXmlString(true);
         */

        private static byte[] generateHash(string InputString)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(InputString));
            }
        }

        private static byte[] generateHash(FileInfo InputFile)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                using (FileStream fs = new FileStream(InputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return sha1.ComputeHash(fs);
                }
            }
        }

        private static bool verifySignature(byte[] Hash, byte[] Signature)
        {
            string keyXml;
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream s = a.GetManifestResourceStream("ZD.AU.Resources.PubKey.xml"))
            using (StreamReader sr = new StreamReader(s))
            {
                keyXml = sr.ReadToEnd();
            }

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(keyXml);
                return rsa.VerifyHash(Hash, CryptoConfig.MapNameToOID("SHA1"), Signature);
            }
        }

        public static bool VerifySignature(FileInfo InputFile, string Signature)
        {
            // TO-DO: check!
            return true;
            return verifySignature(generateHash(InputFile), hexStringToByteArray(Signature));
        }

        public static bool VerifySignature(string InputString, string Signature)
        {
            // TO-DO: check!
            return true;
            return verifySignature(generateHash(InputString), hexStringToByteArray(Signature));
        }

        private static byte[] hexStringToByteArray(string HexString)
        {
            // See if hex string has an even number of characters
            if (HexString.Length % 2 != 0)
                throw new InvalidDataException("Invalid string length");

            List<byte> bytes = new List<byte>();

            // Convert two char to a byte
            for (int i = 0; i < HexString.Length / 2; i++)
                bytes.Add(charsToByte(HexString[i * 2], HexString[i * 2 + 1]));

            return bytes.ToArray();
        }

        private static byte charsToByte(char HighChar, char LowChar)
        {
            return (byte)((charToByte(HighChar) & 15) << 4 | (charToByte(LowChar) & (byte)15));
        }

        private static byte charToByte(char Char)
        {
            switch (Char)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a':
                case 'A': return 10;
                case 'b':
                case 'B': return 11;
                case 'c':
                case 'C': return 12;
                case 'd':
                case 'D': return 13;
                case 'e':
                case 'E': return 14;
                case 'f':
                case 'F': return 15;
            }
            throw new Exception("Unexpected hex character.");
        }

        private static string byteToString(byte[] Bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in Bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
