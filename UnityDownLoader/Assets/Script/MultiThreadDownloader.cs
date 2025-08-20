


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
//多线程下载器 
public class MultiThreadDownloader
{
    private readonly HttpClient _httpClient;
    private const int ChunkSize = 1024 * 1024; // 1MB 块大小

    public MultiThreadDownloader()
    {
        _httpClient = new HttpClient();
    }

    public async Task DownloadFileAsync(string url, string savePath,
        int maxThreads = 4, IProgress<long> progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 获取文件大小
            var fileSize = await GetFileSizeAsync(url);

            // 创建临时文件
            using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fileStream.SetLength(fileSize);
            }

            // 计算分块
            var chunks = CalculateChunks(fileSize, ChunkSize);

            // 创建下载任务
            var downloadTasks = new List<Task>();
            var totalBytesDownloaded = 0L;
            var progressLock = new object();

            foreach (var chunk in chunks)
            {
                // 限制并发线程数
                if (downloadTasks.Count >= maxThreads)
                {
                    await Task.WhenAny(downloadTasks);
                    downloadTasks.RemoveAll(t => t.IsCompleted);
                }

                var task = DownloadChunkAsync(url, savePath, chunk.Start, chunk.End,
                    bytesDownloaded =>
                    {
                        lock (progressLock)
                        {
                            totalBytesDownloaded += bytesDownloaded;
                            progress?.Report(totalBytesDownloaded * 100 / fileSize);
                        }
                    }, cancellationToken);

                downloadTasks.Add(task);
            }

            // 等待所有下载完成
            await Task.WhenAll(downloadTasks);
        }
        catch (Exception ex)
        {
            var environment =  Application.dataPath.Replace("Assets", "Environment");
            if(!File.Exists(environment + "/log.txt"))
            {
                File.Create(environment + "/log.txt");
            }
            var time = DateTime.Now.ToString("hh:mm:ss:fff");
            File.WriteAllText(environment + "/log.txt", $"[{time}]" + ex.ToString());
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            throw;
        }
    }

    private async Task<long> GetFileSizeAsync(string url)
    {
        using (var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)))
        {
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength ?? 0;
        }
    }

    private List<(long Start, long End)> CalculateChunks(long fileSize, int chunkSize)
    {
        var chunks = new List<(long, long)>();
        var position = 0L;

        while (position < fileSize)
        {
            var end = Math.Min(position + chunkSize - 1, fileSize - 1);
            chunks.Add((position, end));
            position = end + 1;
        }

        return chunks;
    }

    private async Task DownloadChunkAsync(string url, string savePath, long start, long end,
        Action<long> progress, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

        using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
        {
            //EnsureSuccessStatusCode它是一个便捷的方法，用于检查HTTP请求是否成功。如果响应状态码表示失败（即不在200-299范围内），它就抛出一个异常，让程序在这里中止；如果成功，它就什么都不做，让代码继续正常执行。
            response.EnsureSuccessStatusCode();

            using (var fileStream = new FileStream(savePath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                fileStream.Seek(start, SeekOrigin.Begin);

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var buffer = new byte[8192];
                    var totalBytesRead = 0L;
                    var bytesToRead = end - start + 1;

                    while (totalBytesRead < bytesToRead)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0,
                            (int)Math.Min(buffer.Length, bytesToRead - totalBytesRead), cancellationToken);

                        if (bytesRead == 0) break;

                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;
                        progress?.Invoke(bytesRead);
                    }
                }
            }
        }
    }
}