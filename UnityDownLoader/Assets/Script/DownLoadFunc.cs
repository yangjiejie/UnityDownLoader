
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public static class DownLoadFunc
    {
        /// <summary>
        /// 获取文件头大小 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<long> GetRemoteFileSizeAsync(string url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            httpReq.Method = "HEAD";// 只请求头 避免请求整个文件 
            using (HttpWebResponse response = (HttpWebResponse) await httpReq.GetResponseAsync())
            {
                long contentLength = response.ContentLength; // 文件大小（字节数）
                return contentLength;
            }
        }

        public static string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    // 计算文件的 MD5 哈希值
                    byte[] hashBytes = md5.ComputeHash(stream);

                    // 将字节数组转换为十六进制字符串
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("x2")); // "x2" 表示两位小写十六进制
                    }

                    return sb.ToString();
                }
            }
        }


        public static string ToLinuxPath(this string content)
        {
            if (string.IsNullOrEmpty(content)) return null;
            return content.Replace("\\", "/");
        }
    }
}
