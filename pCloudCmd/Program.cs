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
    using CommandLine;

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
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                if (options.Verbose)
                {
                    Output(options);
                }

                try
                {
                    if (options.Operation.StartsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 拆分文件。
                        if (File.Exists(options.InputFilePath))
                        {
                            var size = GetValue(options.SplitFileSize);
                            var split = new SplitFile(options.InputFilePath, options.OutputFileDir, size);
                            split.Process();
                            Console.WriteLine("Split '{0}' file completed.", options.InputFilePath);
                        }
                        else
                        {
                            Console.WriteLine("File '{0}' not found.", options.InputFilePath);
                        }
                    }
                    else if (options.Operation.StartsWith("j", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 合并文件。
                        var join = new JoinFile(options.InputFilePath);
                        join.Process();
                        Console.WriteLine("Join '{0}' file completed.", options.InputFilePath);
                    }
                    else if (options.Operation.StartsWith("c", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 验证文件。
                        if (File.Exists(options.InputFilePath))
                        {
                            var size = GetValue(options.SplitFileSize);
                            var split = new SplitFile(options.InputFilePath, options.OutputFileDir, size);
                            split.Process();
                            Console.WriteLine("Check '{0}' file completed.", options.InputFilePath);
                        }
                        else
                        {
                            Console.WriteLine("File '{0}' not found.", options.InputFilePath);
                        }
                    }
                    else if (options.Operation.StartsWith("u", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 上传文件。
                        Console.WriteLine("Upload");
                    }
                    else if (options.Operation.StartsWith("d", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // 下载文件。
                        Console.WriteLine("Download");
                    }
                }
                catch (Exception ex)
                {
                    Output(ex);
                }
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 输出命令行参数信息，以便调试之用。
        /// </summary>
        /// <param name="options">命令行参数。</param>
        private static void Output(Options options)
        {
            Console.WriteLine("Operation: {0}", options.Operation);
            Console.WriteLine("InputFilePath: {0}", options.InputFilePath);
            Console.WriteLine("OutputFileDir: {0}", options.OutputFileDir);
            Console.WriteLine("SplitFileSize: {0}", options.SplitFileSize);
            Console.WriteLine("BufferSize: {0}", options.BufferSize);
            Console.WriteLine("Verbose: {0}", options.Verbose);
            Console.WriteLine();
        }

        /// <summary>
        /// 输出异常信息，以便调试之用。
        /// </summary>
        /// <param name="ex">异常。</param>
        /// <param name="indent">缩进。</param>
        private static void Output(Exception ex, short indent = 0)
        {
            var indentString = new string('\t', indent);
            Console.WriteLine("{0}HResult: 0x{1:X}", indentString, ex.HResult);

            if (!string.IsNullOrEmpty(ex.Message))
            {
                Console.WriteLine("{0}Message: {1}", indentString, ex.Message);
            }

            if (!string.IsNullOrEmpty(ex.Source))
            {
                Console.WriteLine("{0}Source: {1}", indentString, ex.Source);
            }

            if (ex.TargetSite != null)
            {
                Console.WriteLine("{0}TargetSite: {1}", indentString, ex.TargetSite);
            }

            if (ex.Data.Count > 0)
            {
                Console.WriteLine("{0}Data:", indentString);
                foreach (KeyValuePair<object, object> item in ex.Data)
                {
                    Console.WriteLine("{0}\t{1}: {2}", indentString, item.Key, item.Value);
                }
            }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                Console.WriteLine("{0}StackTrace:\n{1}", indentString, ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(ex.HelpLink))
            {
                Console.WriteLine("{0}HelpLink: {1}", indentString, ex.HelpLink);
            }

            if (ex.InnerException != null)
            {
                Console.WriteLine("{0}InnerException:", indentString);
                Output(ex.InnerException, ++indent);
            }

            Console.WriteLine();
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
    }
}