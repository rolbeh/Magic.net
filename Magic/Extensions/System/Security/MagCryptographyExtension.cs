﻿using System.IO;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Security.Cryptography
{
    [PublicAPI]
    public static class MagCryptographyExtension
    {
        /// <summary>
        /// Get the MD5 hash value as a HEX-string
        /// </summary>
        /// <param name="md5">Instance of MD5</param>
        /// <param name="filepath">The input filepath for the hash code is to be calculated. </param>
        /// <returns>The calculated hash code as Hex-string</returns>
        public static string FromFile(this MD5 md5, string filepath)
        {
            using (FileStream stream = File.OpenRead(filepath))
            {
                return md5.ComputeHash(stream).ToHexString();
            }
        }
    }
}
 
