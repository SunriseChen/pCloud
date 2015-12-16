// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Sunrise" file="Program.cs">
//   Copyright (c) Sunrise
//   All rights reserved.
// </copyright>
// <summary>
//   主程序入口类。
// </summary>
// 
// --------------------------------------------------------------------------------------------------------------------

namespace PersonalCloud.Command
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// 主程序入口类。
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 主程序入口。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        public static void Main(string[] args)
        {
            OutputArgs(args);

            if (string.Compare(args[0], "s", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // 拆分文件。
                SplitFile(args);
            }
            else if (string.Compare(args[0], "j", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // 合并文件。
                JoinFile(args);
            }
            else if (string.Compare(args[0], "c", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // 验证文件。
                CheckFile(args);
            }
            else if (string.Compare(args[0], "u", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // 上传文件。
                Console.WriteLine("Upload");
            }
            else if (string.Compare(args[0], "d", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // 下载文件。
                Console.WriteLine("Download");
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 获取大小字符串的数值。
        /// </summary>
        /// <param name="size">大小字符串。</param>
        /// <returns>数值。</returns>
        private static long GetValue(string size)
        {
            var valueString = size.Substring(0, size.Length - 1);
            if (size.EndsWith("b", StringComparison.InvariantCultureIgnoreCase))
            {
                size = size.Substring(0, size.Length - 1);
                valueString = size.Substring(0, size.Length - 1);
            }

            if (char.IsDigit(size[size.Length - 1]))
            {
                valueString = size;
            }

            long value;
            if (!long.TryParse(valueString, out value))
            {
                return 0L;
            }

            const long OneUnit = 1024L;
            if (size.EndsWith("k", StringComparison.InvariantCultureIgnoreCase))
            {
                return value * OneUnit;
            }

            if (size.EndsWith("m", StringComparison.InvariantCultureIgnoreCase))
            {
                return value * OneUnit * OneUnit;
            }

            if (size.EndsWith("g", StringComparison.InvariantCultureIgnoreCase))
            {
                return value * OneUnit * OneUnit * OneUnit;
            }

            if (size.EndsWith("t", StringComparison.InvariantCultureIgnoreCase))
            {
                return value * OneUnit * OneUnit * OneUnit * OneUnit;
            }

            if (size.EndsWith("p", StringComparison.InvariantCultureIgnoreCase))
            {
                return value * OneUnit * OneUnit * OneUnit * OneUnit * OneUnit;
            }

            return value;
        }

        /// <summary>
        /// 拆分文件。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        private static void SplitFile(string[] args)
        {
            var path = args[1];
            if (File.Exists(path))
            {
                var size = GetValue(args[2]);
                var split = new SplitFile(path, size);
                split.Process();
                Console.WriteLine("Split '{0}' file completed.", path);
            }
            else
            {
                Console.WriteLine("File '{0}' not found.", path);
            }
        }

        /// <summary>
        /// 合并文件。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        private static void JoinFile(string[] args)
        {
            var path = args[1];
            var join = new JoinFile(path);
            join.Process();
            Console.WriteLine("Join '{0}' file completed.", path);
        }

        /// <summary>
        /// 合并文件。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        private static void CheckFile(string[] args)
        {
            var path = args[1];
            if (File.Exists(path))
            {
                var size = GetValue(args[2]);
                var split = new SplitFile(path, size);
                split.Process();
                Console.WriteLine("Check '{0}' file completed.", path);
            }
            else
            {
                Console.WriteLine("File '{0}' not found.", path);
            }
        }

        /// <summary>
        /// 在控制台输出参数信息，以便调试之用。
        /// </summary>
        /// <param name="args">命令行参数。</param>
        private static void OutputArgs(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine("{0}", arg);
            }
        }
    }
}