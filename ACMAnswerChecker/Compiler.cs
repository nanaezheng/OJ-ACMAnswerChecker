using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ACMAnswerChecker
{
    static class Compiler
    {
        public static string SourceFilePath { get; private set; }
        public static StringBuilder OutputStringBuilder { get; private set; }
        public static StringBuilder ErrorStringBuilder { get; private set; }
        public static Process CompilerProcess { get; private set; }

        public static void Compile(string workingDirectory, Answer thatAnswer)
        {
            SourceFilePath = "";
            OutputStringBuilder = new StringBuilder();
            ErrorStringBuilder = new StringBuilder();
            CompilerProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            switch (thatAnswer.LanguageCode)
            {
                case Const.LanguageCodeC:
                    SourceFilePath = workingDirectory + "Main.c";
                    CompilerProcess.StartInfo.FileName = "gcc";
                    CompilerProcess.StartInfo.Arguments = SourceFilePath + " -o " + workingDirectory + "Main.exe" + " -O2 -Wall -lm --static -std=c99 -DONLINE_JUDGE";
                    break;
                case Const.LanguageCodeCpp:
                    SourceFilePath = workingDirectory + "Main.cpp";
                    CompilerProcess.StartInfo.FileName = "g++";
                    CompilerProcess.StartInfo.Arguments = SourceFilePath + " -o " + workingDirectory + "Main.exe" + " -O2 -Wall -lm --static -DONLINE_JUDGE";
                    break;
                case Const.LanguageCodeJava:
                    SourceFilePath = workingDirectory + "Main.java";
                    CompilerProcess.StartInfo.FileName = "javac";
                    CompilerProcess.StartInfo.Arguments = "-J-Xms32m -J-Xmx256m " + SourceFilePath;
                    break;
                default:
                    throw new Exception("不支持的语言类型");
            }

            //写入源文件
            var thatSourceFileStreamWriter = new StreamWriter(File.Create(SourceFilePath));
            thatSourceFileStreamWriter.Write(thatAnswer.SourceCode);
            thatSourceFileStreamWriter.Close();

            //启动编译器
            CompilerProcess.Start();

            //启动输出流流监控线程
            var threadWatchOutputStream = new Thread(WatchOutputStream);
            threadWatchOutputStream.Start();

            //启动错误流监控线程
            var threadWatchErrorStream = new Thread(WatchErrorStream);
            threadWatchErrorStream.Start();

            //等待编译器编译完成
            CompilerProcess.WaitForExit();
        }

        private static void WatchErrorStream()
        {
            while (!CompilerProcess.StandardError.EndOfStream)
            {
                ErrorStringBuilder.AppendLine(CompilerProcess.StandardError.ReadLine());
            }
        }

        private static void WatchOutputStream()
        {
            while (!CompilerProcess.StandardOutput.EndOfStream)
            {
                OutputStringBuilder.AppendLine(CompilerProcess.StandardOutput.ReadLine());
            }
        }

    }
}
