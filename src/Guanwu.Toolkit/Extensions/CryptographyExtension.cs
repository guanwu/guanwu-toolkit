using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Guanwu.Toolkit.Cryptography;

namespace Guanwu.Toolkit.Extensions.Cryptography
{
    public static class CryptographyExtension
    {
        public static string ToSm4(this string input, string key, string iv, int codepage = 65001)
        {
            byte[] hash = ToSm4Hash(input, key, iv, codepage);
            return Convert.ToBase64String(hash);
        }

        public static byte[] ToSm4Hash(this string input, string key, string iv, int codepage = 65001)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            return ToSm4Hash(encoding.GetBytes(input), encoding.GetBytes(key), encoding.GetBytes(iv));
        }

        private static byte[] ToSm4Hash(this byte[] input, byte[] key, byte[] iv)
        {
            var sm4 = new Sm4();
            var ctx = new Sm4Context { IsPadding = true };
            sm4.SetKeyEnc(ctx, key);
            return sm4.EncryptCBC(ctx, iv, input);
        }

        public static string ToRsa(this string input, string keyInfoXml, int codepage = 65001, bool fOAEP = false)
        {
            byte[] hash = input.ToRsaHash(keyInfoXml, codepage, fOAEP);
            return Convert.ToBase64String(hash);
        }

        public static byte[] ToRsaHash(this string input, string keyInfoXml, int codepage = 65001, bool fOAEP = false)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            return ToRsaHash(encoding.GetBytes(input), keyInfoXml, fOAEP);
        }

        private static byte[] ToRsaHash(this byte[] input, string keyInfoXml, bool fOAEP = false)
        {
            using (var rsa = new RSACryptoServiceProvider()) {
                rsa.FromXmlString(keyInfoXml);
                var blockSize = rsa.KeySize / 8 - 11;
                var block = new byte[blockSize];
                using (var inputStream = new MemoryStream(input))
                using (var outputStream = new MemoryStream()) {
                    int bufferSize;
                    while ((bufferSize = inputStream.Read(block, 0, blockSize)) > 0) {
                        var buffer = new byte[bufferSize];
                        Array.Copy(block, 0, buffer, 0, bufferSize);
                        var output = rsa.Encrypt(buffer, fOAEP);
                        outputStream.Write(output, 0, output.Length);
                    }
                    return outputStream.ToArray();
                }
            }
        }

        public static string FromRsa(this string input, string keyInfoXml, int codepage = 65001, bool fOAEP = false)
        {
            byte[] bytes = FromRsaHash(input, keyInfoXml, fOAEP);
            Encoding encoding = Encoding.GetEncoding(codepage);
            return encoding.GetString(bytes);
        }

        public static byte[] FromRsaHash(this string input, string keyInfoXml, bool fOAEP = false)
        {
            byte[] hash = Convert.FromBase64String(input);
            return FromRsaHash(hash, keyInfoXml, fOAEP);
        }

        private static byte[] FromRsaHash(this byte[] input, string keyInfoXml, bool fOAEP = false)
        {
            using (var rsa = new RSACryptoServiceProvider()) {
                rsa.FromXmlString(keyInfoXml);
                return rsa.Decrypt(input, fOAEP);
            }
        }

        public static string FromAes(this string input, string key, string iv, int codepage = 65001)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            byte[] rgbKey = encoding.GetBytes(key);
            byte[] rgbIV = encoding.GetBytes(iv);
            return FromAesHash(input, rgbKey, rgbIV);
        }

        public static string FromAesHash(this string input, byte[] rgbKey, byte[] rgbIV)
        {
            byte[] hash = Convert.FromBase64String(input);
            return FromAesHash(hash, rgbKey, rgbIV);
        }

        private static string FromAesHash(this byte[] input, byte[] rgbKey, byte[] rgbIV)
        {
            using (var aes = new AesCryptoServiceProvider()) {
                aes.Padding = PaddingMode.Zeros;
                var decryptor = aes.CreateDecryptor(rgbKey, rgbIV);
                using (var msDecrypt = new MemoryStream(input))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt)) {
                    return srDecrypt.ReadToEnd().Replace("\0", "");
                }
            }
        }

        public static string ToSha256(this string input, int codepage = 65001)
        {
            byte[] hash = input.ToSha256Hash(codepage);
            return ToHex(hash);
        }

        public static string ToSha256(this byte[] input)
        {
            byte[] hash = input.ToSha256Hash();
            return ToHex(hash);
        }

        public static byte[] ToSha256Hash(this string input, int codepage = 65001)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            return ToSha256Hash(encoding.GetBytes(input));
        }

        public static byte[] ToSha256Hash(this byte[] input)
        {
            SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(input);
        }

        public static string ToSha1(this string input, int codepage = 65001)
        {
            byte[] hash = input.ToSha1Hash(codepage);
            return ToHex(hash);
        }

        public static string ToSha1(this byte[] input)
        {
            byte[] hash = input.ToSha1Hash();
            return ToHex(hash);
        }

        public static byte[] ToSha1Hash(this string input, int codepage = 65001)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            return ToSha1Hash(encoding.GetBytes(input));
        }

        public static byte[] ToSha1Hash(this byte[] input)
        {
            SHA1 sha1 = SHA1.Create();
            return sha1.ComputeHash(input);
        }

        public static string ToMd5(this string input, int codepage = 65001)
        {
            byte[] hash = input.ToMd5Hash(codepage);
            return ToHex(hash);
        }

        public static string ToMd5(this byte[] input)
        {
            byte[] hash = input.ToMd5Hash();
            return ToHex(hash);
        }

        public static byte[] ToMd5Hash(this string input, int codepage = 65001)
        {
            Encoding encoding = Encoding.GetEncoding(codepage);
            return ToMd5Hash(encoding.GetBytes(input));
        }

        public static byte[] ToMd5Hash(this byte[] input)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(input);
        }

        public static string ToHex(this byte[] hash)
        {
            var sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
                sBuilder.Append(hash[i].ToString("X2"));
            return sBuilder.ToString();
        }
    }
}
