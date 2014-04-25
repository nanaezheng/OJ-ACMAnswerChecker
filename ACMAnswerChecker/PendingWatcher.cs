using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACMAnswerChecker
{
    class PendingWatcher
    {
        public static void StartWatcher()
        {
            var threadWatchPendingAnswer = new Thread(WatchPendingAnswer);
            threadWatchPendingAnswer.Start();
        }
        public static void WatchPendingAnswer()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;

                var thatAnswer = DatabaseConnector.GetEarliestPendingAnswer();

                if (thatAnswer != null)
                {
                    var workingDirectory = Environment.GetEnvironmentVariable("TEMP") + "\\" + "OJSYSTEM" + "\\" + thatAnswer.Id + "-" + new Random().Next() + "\\";

                    try
                    {
                        //更新数据库
                        thatAnswer.StatusCode = Const.StatusCodeCompiling;
                        thatAnswer.UpdateMarkedTime();
                        DatabaseConnector.UpdateAnswer(thatAnswer);

                        if (!Directory.Exists(workingDirectory)) Directory.CreateDirectory(workingDirectory);

                        Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] Compiling...", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);

                        //编译
                        Compiler.Compile(workingDirectory, thatAnswer);

                        if (Compiler.CompilerProcess.ExitCode != 0)
                        {
                            //编译失败
                            thatAnswer.StatusCode = Const.StatusCodeCompileError;
                            thatAnswer.Info = Compiler.ErrorStringBuilder.ToString().Replace(workingDirectory, "").Replace(Environment.GetEnvironmentVariable("TEMP") + "", "");
                        }
                        else
                        {
                            //编译成功

                            var thatProblem = DatabaseConnector.GetProblemById(thatAnswer.ProblemId);

                            thatAnswer.StatusCode = Const.StatusCodeRunning;
                            thatAnswer.UpdateMarkedTime();
                            DatabaseConnector.UpdateAnswer(thatAnswer);

                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] Running...", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);

                            //更新数据库
                            thatAnswer.StatusCode = Const.StatusCodeRunning;
                            thatAnswer.UpdateMarkedTime();
                            DatabaseConnector.UpdateAnswer(thatAnswer);

                            //运行
                            Runner.Run(workingDirectory, thatAnswer, thatProblem);

                            thatAnswer.InputData = thatProblem.StandardInput;
                            thatAnswer.OutputData = Runner.OutputStringBuilder.ToString();
                            thatAnswer.UsedTime = Runner.UsedTime;
                            thatAnswer.UsedMemory = Runner.UsedMemory / 1000;

                            if (Runner.StatusCode != Const.StatusCodeAccepted)
                            {
                                //运行不通过
                                thatAnswer.StatusCode = Runner.StatusCode;
                                thatAnswer.Info = Runner.Info;
                            }
                            else
                            {
                                //运行通过

                                //检查输出结果
                                Checker.Check(thatAnswer, thatProblem);

                                thatAnswer.StatusCode = Checker.StatusCode;

                                StreamWriter inputStreamWriter = File.CreateText(workingDirectory + "in.txt");
                                inputStreamWriter.Write(thatAnswer.InputData);
                                inputStreamWriter.Close();

                                StreamWriter outputStreamWriter = File.CreateText(workingDirectory + "out.txt");
                                outputStreamWriter.Write(thatAnswer.OutputData);
                                outputStreamWriter.Close();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] SystemError.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                        thatAnswer.StatusCode = Const.StatusCodeSystemError;
                        thatAnswer.Info = exception.Message;
                    }

                    //过滤敏感数据
                    thatAnswer.Info = thatAnswer.Info.Replace(workingDirectory, "").Replace(Environment.GetEnvironmentVariable("TEMP") + "", "");
                    //更新判题完成时间
                    thatAnswer.UpdateMarkedTime();

                    //结果写入数据库
                    DatabaseConnector.UpdateAnswer(thatAnswer);

                    //控制台输出结果
                    switch (thatAnswer.StatusCode)
                    {
                        case Const.StatusCodeAccepted:
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] Accepted.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodePresentationError:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] PresentationError.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeWrongAnswer:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] WrongAnswer.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeTimeLimitExceeded:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] TimeLimitExceeded.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeMemoryLimitExceeded:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] MemoryLimitExceeded.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeOutputLimitExceeded:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] OutputLimitExceeded.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeRuntimeError:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] RuntimeError.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeCompileError:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] CompileError.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        case Const.StatusCodeSystemError:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] SystemError.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[{0}] [AnswerID: {1}] [ProblemID: {2}] [UserID: {3}] UnknownStatus.", DateTime.Now, thatAnswer.Id, thatAnswer.ProblemId, thatAnswer.UserId);
                            break;
                    }
                }
                else
                {
                    //无Answer在Pending，坐等
                    Console.WriteLine("[{0}] NoPendingAnswer,Waiting...", DateTime.Now);
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
