using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter
{
    using System;
    using System.Text;

    /// <summary>
    /// A class for encrypting and decrypting a string into base64 format which makes it safe for transfer
    /// between applications.
    ///
    /// Reference:
    /// Based upon the javascript implementation of xxtea by: Chris Veness
    /// www.movable-type.co.uk/tea-block.html
    ///
    /// Using the corrected block tea algorithm developed by: David Wheeler & Roger Needham
    ///
    /// But written for c# by me :D
    /// </summary>
    public class XXTea
    {

        /// <summary>
        /// Encryption using corrected Block TEA (xxtea) algorithm
        /// </summary>
        /// <param name="text">String to be encrypted (multi-byte safe)</param>
        /// <param name="password">Password to be used for encryption (1st 16 chars)</param>
        /// <returns></returns>
        public static String Encrypt(String text, String password)
        {

            if (text.Length == 0)
                return "";  // nothing to encrypt

            // Check the user has passed a large enough salt to encrypt the data
            if (password.Length < 8)
            {
                throw new ArgumentException("The salt for encryption is too short");
            }

            // The salt needs to be at least 16 chars in size so if less than 16 double it until it reaches that size
            while (password.Length < 16) { password += password; }

            // Convert the text into UTF-8 encoding (byte size)
            var v = ToLongs((new UTF8Encoding()).GetBytes(text));

            // algorithm doesn't work for n<2 so fudge by adding an ascii null
            if (v.Length == 1) { v[0] = 0; }

            // Simply convert first 16 chars of password as key
            var k = ToLongs((new UTF8Encoding()).GetBytes(password.Substring(0, 16)));

            // Use UInt32 as the original is based on 'unsigned long' in C, which is equiv to UInt32 in .Net (and not ulong)
            UInt32 n = (UInt32)v.Length,
                   z = v[n - 1],
                   y = v[0],
                   delta = 0x9e3779b9,
                   e,
                   q = (UInt32)(6 + (52 / n)),
                   sum = 0,
                   p = 0;

            while (q-- > 0)
            {
                sum += delta;
                e = sum >> 2 & 3;

                for (p = 0; p < (n - 1); p++)
                {
                    y = v[(p + 1)];
                    z = v[p] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                }

                y = v[0];
                z = v[n - 1] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
            }

            // Convert to Base64 so that Control characters doesnt break it
            return Convert.ToBase64String(ToBytes(v));
        }

        /// <summary>
        /// Decryption using Corrected Block TEA (xxtea) algorithm
        /// </summary>
        /// <param name="ciphertext">String to be decrypted</param>
        /// <param name="password">Password to be used for decryption (1st 16 chars)</param>
        /// <returns></returns>
        public static byte[] Decrypt(String ciphertext)
        {

            if (ciphertext.Length == 0) { return null; }

            var v = ToLongs(Convert.FromBase64String(ciphertext));
            uint[] k = new uint[] { 0x53527737, 0x7506499E, 0xBD39AEE3, 0xA59E7268 };
            string password = "0123456789abcdef";
            //var k = ToLongs((new UTF8Encoding()).GetBytes(password.Substring(0, 16)));

            UInt32 n = (UInt32)v.Length,
                   z = v[n - 1],
                   y = v[0],
                   delta = 0x61C88647,//0x9e3779b9,
                   e,
                   q = (UInt32)(6 + (52 / n)),
                   sum = 0xC6EF3720,//q * delta,
                   p = 0;

            while (sum != 0)
            {
                e = sum >> 2 & 3;

                for (p = (n - 1); p > 0; p--)
                {
                    z = v[p - 1];
                    y = v[p] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
                }

                z = v[n - 1];
                y = v[0] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);

                sum -= delta;
            }

            return ToBytes(v);
        }

        /// <summary>
        /// convert utf-8 byte to array of longs, each containing 4 chars to be manipulated
        /// </summary>
        /// <param name="s"></param>
        private static UInt32[] ToLongs(byte[] s)
        {

            // note chars must be within ISO-8859-1 (with Unicode code-point < 256) to fit 4/long
            var l = new UInt32[(int)Math.Ceiling(((decimal)s.Length / 4))];

            // Create an array of long, each long holding the data of 4 characters, if the last block is less than 4
            // characters in length, fill with ascii null values
            for (int i = 0; i < l.Length; i++)
            {
                // Note: little-endian encoding - endianness is irrelevant as long as it is the same in ToBytes()
                l[i] = ((s[i * 4])) +
                       ((i * 4 + 1) >= s.Length ? (UInt32)0 << 8 : ((UInt32)s[i * 4 + 1] << 8)) +
                       ((i * 4 + 2) >= s.Length ? (UInt32)0 << 16 : ((UInt32)s[i * 4 + 2] << 16)) +
                       ((i * 4 + 3) >= s.Length ? (UInt32)0 << 24 : ((UInt32)s[i * 4 + 3] << 24));
            }

            return l;
        }

        /// <summary>
        /// Convert array of longs back to utf-8 byte array
        /// </summary>
        /// <returns></returns>
        private static byte[] ToBytes(UInt32[] l)
        {
            byte[] b = new byte[l.Length * 4];

            // Split each long value into 4 separate characters (bytes) using the same format as ToLongs()
            for (Int32 i = 0; i < l.Length; i++)
            {
                b[(i * 4)] = (byte)(l[i] & 0xFF);
                b[(i * 4) + 1] = (byte)(l[i] >> (8 & 0xFF));
                b[(i * 4) + 2] = (byte)(l[i] >> (16 & 0xFF));
                b[(i * 4) + 3] = (byte)(l[i] >> (24 & 0xFF));
            }
            return b;
        }

    }
}
