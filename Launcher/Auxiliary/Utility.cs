/* 
 *  Utility.cs
 *  Author: RealNickk
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Starlight
{
    internal class Utility
    {
        public static Dictionary<string, string> ParsePayload(string szPayload)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            string[] split = HttpUtility.UrlDecode(szPayload).Split(' ');
            for (int i = 0; i < split.Length; i++)
            {
                string[] kv = split[i].Split(':');
                dictionary.Add(kv[0], string.Join(":", kv.Skip(1)));
            }
            return dictionary;
        }

        public static int SecureRandomInteger()
        {
            using RNGCryptoServiceProvider rng = new();
            byte[] seed = new byte[4];
            rng.GetBytes(seed);
            return new Random(BitConverter.ToInt32(seed, 0)).Next();
        }

        public static bool TryGetCultureInfo(string szName, out CultureInfo ci)
        {
            try
            {
                ci = new CultureInfo(szName);
                return true;
            }
            catch (CultureNotFoundException)
            {
                ci = null;
                return false;
            }
        }
    }
}
