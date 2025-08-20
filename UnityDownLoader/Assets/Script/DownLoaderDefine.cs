using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public class DownLoaderDefine
    {
        public static string Cdn
        {
            get
            {
                return "http://192.168.103.77:80";
            }
        }
    }

    public class ChunkInfo
    {
        public long Start { get; set; }
        public long End { get; set; }
        public string TempFilePath { get; set; }
        public bool IsCompleted { get; set; }
        public long DownloadedSize { get; set; }
    }

    public class DownloadProgress
    {
        public long TotalBytes { get; set; }
        public long DownloadedBytes { get; set; }
        public double Percentage => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
        public int ActiveThreads { get; set; }
    }
    public class FileItem
    {
        public string filePathName;
        public string md5;
        public long fileSize;

        public override string ToString()
        {
            return $"路径：{filePathName},size={fileSize},md5= ,{md5}";
        }
    }

}
