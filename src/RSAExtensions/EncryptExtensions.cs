﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IO;

namespace RSAExtensions
{
    public static class EncryptExtensions
    {
        static readonly Dictionary<RSAEncryptionPadding,int> PaddingLimitDic=new Dictionary<RSAEncryptionPadding, int>()
        {
            [RSAEncryptionPadding.Pkcs1]=11,
            [RSAEncryptionPadding.OaepSHA1]=42,
            [RSAEncryptionPadding.OaepSHA256]=66,
            [RSAEncryptionPadding.OaepSHA384]=98,
            [RSAEncryptionPadding.OaepSHA512]=130,
        };
        
        private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="dataStr"></param>
        /// <param name="padding"></param>
        /// <param name="connChar"></param>
        /// <returns></returns>
        public static string EncryptBigData(this RSA rsa,string dataStr, RSAEncryptionPadding padding,char connChar='$')
        {
            var data = Encoding.UTF8.GetBytes(dataStr);
            var modulusLength = rsa.KeySize / 8;
            var splitLength = modulusLength - PaddingLimitDic[padding];

            var sb=new StringBuilder();

            var splitsNumber = Convert.ToInt32(Math.Ceiling(data.Length * 1.0 / splitLength));

            var pointer = 0;
            for (int i = 0; i < splitsNumber; i++)
            {
                byte[] current = pointer + splitLength < data.Length ? data[pointer..(pointer+splitLength)] : data[pointer..];

                sb.Append(Convert.ToBase64String(rsa.Encrypt(current, padding)));
                sb.Append(connChar);
                pointer += splitLength;
            }

            return sb.ToString().TrimEnd(connChar);
        }

        public static string DecryptBigData(this RSA rsa, string dataStr, RSAEncryptionPadding padding, char connChar = '$')
        {
            var data = dataStr.Split(connChar, StringSplitOptions.RemoveEmptyEntries);
            using var ms = MemoryStreamManager.GetStream();

            foreach (var item in data)
            {
                ms.Write(rsa.Decrypt(Convert.FromBase64String(item), padding));
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}