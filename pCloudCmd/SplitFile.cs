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
        private readonly string path;

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
        /// <param name="path">待处理文件路径。</param>
        /// <param name="size">拆分成的大小，单位：字节。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        public SplitFile(string path, long size, int bufferSize = 4096)
        {
            this.path = path;
            this.size = size;
            this.bufferSize = bufferSize;
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
            var fileInfo = new FileInfo(this.path);
            var count = (fileInfo.Length + this.size - 1) / this.size;
            if (count < 2)
            {
                return;
            }

            var format = new string('0', count.ToString(CultureInfo.InvariantCulture).Length);
            var files = new List<string>();
            var tasks = new Task[count];
            for (var i = 0; i < count && !CancellationToken.IsCancellationRequested; ++i)
            {
                var offset = i * this.size;
                var outputFilePath = this.path + "." + (i + 1).ToString(format);
                files.Add(outputFilePath);
                tasks[i] = Task.Run(() => this.Process(offset, outputFilePath), CancellationToken);
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
        private void Process(long offset, string outputFilePath)
        {
            using (var reader = File.Open(this.path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var writer = File.Open(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    reader.Seek(offset, SeekOrigin.Begin);
                    var buffer = new byte[Math.Min(this.bufferSize, this.size)];
                    var blocks = this.size / buffer.Length;
                    for (var i = 0; i < blocks && !CancellationToken.IsCancellationRequested; ++i)
                    {
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