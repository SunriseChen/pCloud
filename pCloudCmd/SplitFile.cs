// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Sunrise" file="SplitFile.cs">
//   Copyright (c) Sunrise
//   All rights reserved.
// </copyright>
// <summary>
//   拆分文件处理类。
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace PersonalCloud.Command
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 拆分文件处理类。
    /// </summary>
    public class SplitFile
    {
        /// <summary>
        /// 待处理文件路径。
        /// </summary>
        private readonly string inputFilePath;

        /// <summary>
        /// 输出文件路径。
        /// </summary>
        private readonly string outputFileDir;

        /// <summary>
        /// 拆分成的大小，单位：字节。
        /// </summary>
        private readonly long size;

        /// <summary>
        /// 缓冲区大小。
        /// </summary>
        private readonly int bufferSize;

        /// <summary>
        /// 初始化 <see cref="SplitFile"/> 类的新实例。
        /// </summary>
        /// <param name="inputFilePath">待处理文件路径。</param>
        /// <param name="outputFileDir">输出文件路径。</param>
        /// <param name="size">拆分成的大小，单位：字节。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        public SplitFile(string inputFilePath, string outputFileDir = null, long size = 1024L * 1024L * 1024L, int bufferSize = 4 * 1024)
        {
            this.inputFilePath = Path.GetFullPath(inputFilePath);
            if (!File.Exists(this.inputFilePath))
            {
                throw new ArgumentException("Input file path not exists.", "inputFilePath");
            }

            this.outputFileDir = Path.GetDirectoryName(
                string.IsNullOrWhiteSpace(outputFileDir) ? this.inputFilePath : Path.GetFullPath(outputFileDir));
            if (this.outputFileDir == null || !Directory.Exists(this.outputFileDir))
            {
                throw new ArgumentException("Output directory not exists.", "outputFileDir");
            }

            this.size = size;
            if (this.size <= 0L)
            {
                throw new ArgumentOutOfRangeException("size", size, "Split file size must be greater than zero.");
            }

            this.bufferSize = bufferSize;
            if (this.bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must be greater than zero.");
            }

            CancellationToken = new CancellationToken();
        }

        /// <summary>
        /// 获取或设置应取消操作的通知。
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// 处理入口。
        /// </summary>
        public void Process()
        {
            var fileInfo = new FileInfo(this.inputFilePath);
            var count = (int)((fileInfo.Length + this.size - 1) / this.size);
            if (count < 2)
            {
                return;
            }

            Console.Write(this.inputFilePath + ": ");
            var message = string.Empty;

            var format = string.Format("{{0}}.{{1:D{0}}}", count.ToString(CultureInfo.InvariantCulture).Length);
            var files = new List<string>();
            var tasks = new Task[count];
            for (var i = 0; i < count && !CancellationToken.IsCancellationRequested; ++i)
            {
                var offset = i * this.size;
                var outputFilePath = Path.Combine(this.outputFileDir, string.Format(format, fileInfo.Name, i + 1));
                files.Add(outputFilePath);
                var progress = new Progress<float>(percent =>
                {
                    lock (this)
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            Console.Write(new string((char)8, message.Length));
                        }

                        message = string.Format("{0:P}", percent);
                        Console.Write(message);
                    }
                });
                tasks[i] = Task.Run(() => this.Process(offset, outputFilePath, progress), CancellationToken);
            }

            Task.WaitAll(tasks, CancellationToken);

            if (CancellationToken.IsCancellationRequested)
            {
                foreach (var outputFilePath in files.Where(File.Exists))
                {
                    File.Delete(outputFilePath);
                }
            }
        }

        /// <summary>
        /// 处理某段文件区域。
        /// </summary>
        /// <param name="offset">读取文件的分段偏移。</param>
        /// <param name="outputFilePath">写入的文件路径。</param>
        /// <param name="progress">进度更新。</param>
        private void Process(long offset, string outputFilePath, IProgress<float> progress)
        {
            using (var reader = File.Open(this.inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var writer = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    reader.Seek(offset, SeekOrigin.Begin);
                    var buffer = new byte[Math.Min(this.bufferSize, this.size)];
                    var blocks = this.size / buffer.Length;
                    for (var i = 0; i < blocks && !CancellationToken.IsCancellationRequested; ++i)
                    {
                        if (progress != null)
                        {
                            progress.Report((float)i / blocks);
                        }

                        var count = reader.Read(buffer, 0, buffer.Length);
                        writer.Write(buffer, 0, count);
                    }

                    var lastBlock = this.size % buffer.Length;
                    if (lastBlock != 0)
                    {
                        var count = reader.Read(buffer, 0, (int)lastBlock);
                        writer.Write(buffer, 0, count);
                    }
                }
            }
        }
    }
}