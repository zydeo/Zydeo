using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;

namespace ZD.AU
{
    /// <summary>
    /// <para>Provides functionality to verify a string's or file's signature with our own public key.</para>
    /// <para>Private key is secret of Zydeo publisher.</para>
    /// </summary>
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

        private static byte[] generateHash(FileInfo inputFile)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                using (FileStream fs = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return sha1.ComputeHash(fs);
                }
            }
        }

        private static bool verifySignature(byte[] hash, byte[] signature)
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
                return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
            }
        }

        /// <summary>
        /// Verifies signature of a file.
        /// </summary>
        public static bool VerifySignature(FileInfo inputFile, string sigStr)
        {
            return verifySignature(generateHash(inputFile), hexStringToByteArray(sigStr));
        }

        /// <summary>
        /// Verifies signature of a string.
        /// </summary>
        public static bool VerifySignature(string inputString, string sigStr)
        {
            return verifySignature(generateHash(inputString), hexStringToByteArray(sigStr));
        }

        /// <summary>
        /// Signs a file with the full (private) RSA key provided as XML.
        /// </summary>
        public static string Sign(FileInfo file, string keyXml)
        {
            byte[] hash = generateHash(file);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(keyXml);
                byte[] sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
                return byteArrayToHexString(sig);
            }
        }

        /// <summary>
        /// Sings a string with the full (private) RSA key provided as XML.
        /// </summary>
        public static string Sign(string inputString, string keyXml)
        {
            byte[] hash = generateHash(inputString);
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(keyXml);
                byte[] sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));
                return byteArrayToHexString(sig);
            }
        }

        /// <summary>
        /// Parses a hex string into a byte array.
        /// </summary>
        private static byte[] hexStringToByteArray(string hexString)
        {
            // See if hex string has an even number of characters
            if (hexString.Length % 2 != 0)
                throw new InvalidDataException("Invalid string length");

            List<byte> bytes = new List<byte>();

            // Convert two char to a byte
            for (int i = 0; i < hexString.Length / 2; i++)
                bytes.Add(charsToByte(hexString[i * 2], hexString[i * 2 + 1]));

            return bytes.ToArray();
        }

        /// <summary>
        /// Converts a pair of chars (in hex) to a byte.
        /// </summary>
        private static byte charsToByte(char hiChar, char loChar)
        {
            return (byte)((charToByte(hiChar) & 15) << 4 | (charToByte(loChar) & (byte)15));
        }

        /// <summary>
        /// Converts a single HEX char to a byte; case-insensitive, and erm, weird.
        /// </summary>
        private static byte charToByte(char chr)
        {
            switch (chr)
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

        /// <summary>
        /// Converts byte array to string.
        /// </summary>
        private static string byteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
