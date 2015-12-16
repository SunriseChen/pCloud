// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Sunrise" file="JoinFile.cs">
//   Copyright (c) Sunrise
//   All rights reserved.
// </copyright>
// <summary>
//   合并文件处理类。
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace PersonalCloud.Command
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 合并文件处理类。
    /// </summary>
    public class JoinFile
    {
        /// <summary>
        /// 待处理文件路径。
        /// </summary>
        private readonly string path;

        /// <summary>
        /// 缓冲区大小。
        /// </summary>
        private readonly int bufferSize;

        /// <summary>
        /// 初始化 <see cref="JoinFile"/> 类的新实例。
        /// </summary>
        /// <param name="path">待处理文件路径。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        public JoinFile(string path, int bufferSize = 4096)
        {
            this.path = path;
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
            var lastPoint = this.path.LastIndexOf('.');
            var lastNumber = this.path.Substring(lastPoint + 1, this.path.Length - lastPoint - 1);
            var outputFilePath = this.path.Substring(0, lastPoint);

            var dir = Path.GetDirectoryName(this.path);
            if (string.IsNullOrEmpty(dir))
            {
                dir = ".";
            }

            var searchPattern = Path.GetFileName(outputFilePath) + "." + new string('?', lastNumber.ToString(CultureInfo.InvariantCulture).Length);
            var files = Directory.EnumerateFiles(dir, searchPattern).Where(name =>
                {
                    var ext = Path.GetExtension(name);
                    if (!string.IsNullOrEmpty(ext) && ext.StartsWith("."))
                    {
                        ext = ext.Substring(1);
                    }

                    uint number;
                    return uint.TryParse(ext, out number) && number != 0;
                }).OrderBy(name => name);

            var tasks = new Task[files.Count()];
            var length = 0L;
            var i = 0;

            // TODO: 下面的代码用于测试，正式使用应去掉。
            outputFilePath += "." + new string('0', lastNumber.ToString(CultureInfo.InvariantCulture).Length);
            foreach (var file in files)
            {
                var inputFilePath = file;
                var offset = length;
                tasks[i++] = Task.Run(() => this.Process(inputFilePath, outputFilePath, offset), CancellationToken);
                var fileInfo = new FileInfo(inputFilePath);
                length += fileInfo.Length;
            }

            Task.WaitAll(tasks, CancellationToken);

            if (CancellationToken.IsCancellationRequested && File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }

        /// <summary>
        /// 处理某段文件区域。
        /// </summary>
        /// <param name="inputFilePath">读取的文件路径。</param>
        /// <param name="outputFilePath">写入的文件路径。</param>
        /// <param name="offset">写入文件的分段偏移。</param>
        private void Process(string inputFilePath, string outputFilePath, long offset)
        {
            using (var reader = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using (var writer = File.Open(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                {
                    writer.Seek(offset, SeekOrigin.Begin);
                    var size = reader.Length;
                    var buffer = new byte[Math.Min(this.bufferSize, size)];
                    var blocks = size / buffer.Length;
                    for (var i = 0; i < blocks && !CancellationToken.IsCancellationRequested; ++i)
                    {
                        var count = reader.Read(buffer, 0, buffer.Length);
                        writer.Write(buffer, 0, count);
                    }

                    var lastBlock = size % buffer.Length;
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