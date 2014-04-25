using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACMAnswerChecker
{
    static class Runner
    {
        private static readonly Dictionary<long, string> ExitCodeDictionary = new Dictionary<long, string>();

        public static Process ThatProgramProcess { get; private set; }
        public static Int64 TimeLimit { get; private set; }
        public static Int64 MemoryLimit { get; private set; }
        public static Int64 UsedTime { get; private set; }
        public static Int64 UsedMemory { get; private set; }
        public static Int16 StatusCode { get; private set; }
        public static String Info { get; private set; }
        public static StringBuilder OutputStringBuilder { get; private set; }
        public static StringBuilder ErrorStringBuilder { get; private set; }
        public static long OutputLimit { get; private set; }

        public static void InitExitCodeDictionary()
        {
            ExitCodeDictionary.Add(-1073741510, "CONTROL_C_EXIT");
            ExitCodeDictionary.Add(1073807369, "DBG_COMMAND_EXCEPTION");
            ExitCodeDictionary.Add(65538, "DBG_CONTINUE");
            ExitCodeDictionary.Add(1073807368, "DBG_CONTROL_BREAK");
            ExitCodeDictionary.Add(1073807365, "DBG_CONTROL_C");
            ExitCodeDictionary.Add(65537, "DBG_EXCEPTION_HANDLED");
            ExitCodeDictionary.Add(-2147418111, "DBG_EXCEPTION_NOT_HANDLED");
            ExitCodeDictionary.Add(1073807364, "DBG_TERMINATE_PROCESS");
            ExitCodeDictionary.Add(1073807363, "DBG_TERMINATE_THREAD");
            ExitCodeDictionary.Add(-1073741819, "EXCEPTION_ACCESS_VIOLATION");
            ExitCodeDictionary.Add(-1073741684, "EXCEPTION_ARRAY_BOUNDS_EXCEEDED");
            ExitCodeDictionary.Add(-2147483645, "EXCEPTION_BREAKPOINT");
            ExitCodeDictionary.Add(-2147483646, "EXCEPTION_DATATYPE_MISALIGNMENT");
            ExitCodeDictionary.Add(-1073741683, "EXCEPTION_FLT_DENORMAL_OPERAND");
            ExitCodeDictionary.Add(-1073741682, "EXCEPTION_FLT_DIVIDE_BY_ZERO");
            ExitCodeDictionary.Add(-1073741681, "EXCEPTION_FLT_INEXACT_RESULT");
            ExitCodeDictionary.Add(-1073741680, "EXCEPTION_FLT_INVALID_OPERATION");
            ExitCodeDictionary.Add(-1073741679, "EXCEPTION_FLT_OVERFLOW");
            ExitCodeDictionary.Add(-1073741678, "EXCEPTION_FLT_STACK_CHECK");
            ExitCodeDictionary.Add(-1073741677, "EXCEPTION_FLT_UNDERFLOW");
            ExitCodeDictionary.Add(-2147483647, "EXCEPTION_GUARD_PAGE");
            ExitCodeDictionary.Add(-1073741795, "EXCEPTION_ILLEGAL_INSTRUCTION");
            ExitCodeDictionary.Add(-1073741676, "EXCEPTION_INT_DIVIDE_BY_ZERO");
            ExitCodeDictionary.Add(-1073741675, "EXCEPTION_INT_OVERFLOW");
            ExitCodeDictionary.Add(-1073741786, "EXCEPTION_INVALID_DISPOSITION");
            ExitCodeDictionary.Add(-1073741816, "EXCEPTION_INVALID_HANDLE");
            ExitCodeDictionary.Add(-1073741818, "EXCEPTION_IN_PAGE_ERROR");
            ExitCodeDictionary.Add(-1073741787, "EXCEPTION_NONCONTINUABLE_EXCEPTION");
            ExitCodeDictionary.Add(-1073741674, "EXCEPTION_PRIV_INSTRUCTION");
            ExitCodeDictionary.Add(-2147483644, "EXCEPTION_SINGLE_STEP");
            ExitCodeDictionary.Add(-1073741571, "EXCEPTION_STACK_OVERFLOW");
            ExitCodeDictionary.Add(128, "STATUS_ABANDONED_WAIT_0");
            ExitCodeDictionary.Add(-1073741801, "STATUS_NO_MEMORY");
            ExitCodeDictionary.Add(259, "STATUS_PENDING");
            ExitCodeDictionary.Add(-1073741111, "STATUS_REG_NAT_CONSUMPTION");
            ExitCodeDictionary.Add(1073741829, "STATUS_SEGMENT_NOTIFICATION");
            ExitCodeDictionary.Add(-1072365553, "STATUS_SXS_EARLY_DEACTIVATION");
            ExitCodeDictionary.Add(-1072365552, "STATUS_SXS_INVALID_DEACTIVATION");
            ExitCodeDictionary.Add(258, "STATUS_TIMEOUT");
            ExitCodeDictionary.Add(192, "STATUS_USER_APC");
        }

        public static void Run(string workingDirectory, Answer thatAnswer, Problem thatProblem)
        {
            StatusCode = Const.StatusCodeAccepted;
            OutputStringBuilder = new StringBuilder();
            ErrorStringBuilder = new StringBuilder();
            OutputLimit = thatProblem.StandardOutput.Length * 2;

            ThatProgramProcess = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            switch (thatAnswer.LanguageCode)
            {
                case Const.LanguageCodeC:
                case Const.LanguageCodeCpp:
                    ThatProgramProcess.StartInfo.FileName = workingDirectory + "Main.exe";
                    TimeLimit = thatProblem.TimeLimitNormal;
                    MemoryLimit = thatProblem.MemoryLimitNormal * 1000;
                    break;
                case Const.LanguageCodeJava:
                    ThatProgramProcess.StartInfo.FileName = "java";
                    ThatProgramProcess.StartInfo.Arguments = "-cp " + workingDirectory + " " + "Main";
                    TimeLimit = thatProblem.TimeLimitJava * 1000;
                    MemoryLimit = thatProblem.MemoryLimitJava * 1000;
                    break;
                default:
                    throw new Exception("不支持的语言类型");
            }

            try
            {
                //启动进程
                ThatProgramProcess.Start();

                //启动时间内存监控线程
                var threadWatchTimeAndMemory = new Thread(WatchTimeAndMemory);
                threadWatchTimeAndMemory.Start();

                //启动输出流监控线程
                var threadWatchOutputStream = new Thread(WatchOutputStream);
                threadWatchOutputStream.Start();

                //启动错误流监控线程
                var threadWatchErrorStream = new Thread(WatchErrorStream);
                threadWatchErrorStream.Start();

                //设置最大使用内存
                if (ThatProgramProcess.MaxWorkingSet.ToInt64() < MemoryLimit)
                    ThatProgramProcess.MaxWorkingSet = new IntPtr(MemoryLimit);
                else
                    throw new OutOfMemoryException();

                //输入数据
                ThatProgramProcess.StandardInput.Write(thatProblem.StandardInput);
                ThatProgramProcess.StandardInput.Close();

                //等待进程结束
                ThatProgramProcess.WaitForExit();
            }
            catch (OutOfMemoryException)
            {
                StatusCode = Const.StatusCodeMemoryLimitExceeded;
            }
            catch (IOException)
            {
            }
            catch (Exception)
            {
                StatusCode = Const.StatusCodeSystemError;
            }
            finally
            {
                //强制结束进程
                try { ThatProgramProcess.Kill(); }
                catch (Exception) { }

                switch (StatusCode)
                {
                    case Const.StatusCodeAccepted:
                        if (ThatProgramProcess.ExitCode != 0)
                        {
                            StatusCode = Const.StatusCodeRuntimeError;
                            Info = ExitCodeDictionary.ContainsKey(ThatProgramProcess.ExitCode) ? ExitCodeDictionary[ThatProgramProcess.ExitCode] : ErrorStringBuilder.ToString();
                        }
                        else if (ErrorStringBuilder.ToString() != "")
                        {
                            StatusCode = Const.StatusCodeRuntimeError;
                            Info = ErrorStringBuilder.ToString();
                        }
                        break;
                    case Const.StatusCodeMemoryLimitExceeded:
                        UsedMemory = MemoryLimit;
                        Info = "";
                        break;
                    case Const.StatusCodeTimeLimitExceeded:
                        UsedTime = TimeLimit;
                        Info = "";
                        break;
                    case Const.StatusCodeOutputLimitExceeded:
                        Info = "";
                        break;
                    default:
                        throw new Exception("状态码错误");
                }
            }
        }

        private static void WatchOutputStream()
        {
            while (!ThatProgramProcess.StandardOutput.EndOfStream)
            {
                OutputStringBuilder.AppendLine(ThatProgramProcess.StandardOutput.ReadLine());

                //检测OLE
                if (OutputStringBuilder.Length > OutputLimit)
                {
                    //强制结束进程
                    try { ThatProgramProcess.Kill(); }
                    catch (Exception) { }
                    StatusCode = Const.StatusCodeOutputLimitExceeded;
                    break;
                }
            }
        }

        private static void WatchErrorStream()
        {
            while (!ThatProgramProcess.StandardError.EndOfStream)
            {
                ErrorStringBuilder.AppendLine(ThatProgramProcess.StandardError.ReadLine());
            }
        }

        private static void WatchTimeAndMemory()
        {
            bool cpuIdleFlag = false;
            long cpuIdleStartTime = 0;
            long lastUsedTime = 0;

            while (!ThatProgramProcess.HasExited)
            {
                try
                {
                    UsedTime = Convert.ToInt64(ThatProgramProcess.TotalProcessorTime.TotalMilliseconds);
                    UsedMemory = Convert.ToInt64(ThatProgramProcess.PeakWorkingSet64);

                    //检测是否挂起
                    if (UsedTime != lastUsedTime)
                    {
                        cpuIdleFlag = false;
                        cpuIdleStartTime = 0;
                        lastUsedTime = UsedTime;
                    }
                    else
                    {
                        if (cpuIdleFlag == false)
                        {
                            cpuIdleFlag = true;
                            cpuIdleStartTime = DateTime.Now.Ticks;
                        }

                        if ((DateTime.Now.Ticks - cpuIdleStartTime) / 10 / 1000 / 1000 == 10)
                        {
                            //CPU空闲超过10秒,强制TLE
                            UsedTime = TimeLimit;
                        }
                    }

                    //检测TLE
                    if (UsedTime >= TimeLimit)
                    {
                        //强制结束进程
                        try { ThatProgramProcess.Kill(); }
                        catch (Exception) { }
                        StatusCode = Const.StatusCodeTimeLimitExceeded;
                        break;
                    }

                    //检测MLE
                    if (UsedMemory >= MemoryLimit)
                    {
                        //强制结束进程
                        try { ThatProgramProcess.Kill(); }
                        catch (Exception) { }
                        StatusCode = Const.StatusCodeMemoryLimitExceeded;
                        break;
                    }

                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        }

    }
}
