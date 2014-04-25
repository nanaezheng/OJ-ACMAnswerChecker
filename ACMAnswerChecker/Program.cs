using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ACMAnswerChecker
{
    class Program
    {
        public static string WebServerURL { get; private set; }
        public static string Key { get; private set; }
        public static int Offset { get; private set; }

        static void Main(string[] args)
        {
            var server = "";
            var database = "";
            var uid = "";
            var pwd = "";

            var configStreamReader = File.OpenText(@"config.txt");
            while (!configStreamReader.EndOfStream)
            {
                var thisLine = configStreamReader.ReadLine();
                var regex = new Regex("^(.*)=(.*)$");
                if (thisLine != null)
                {
                    if (regex.IsMatch(thisLine))
                    {
                        var thisMatch = regex.Match(thisLine);
                        var k = thisMatch.Groups[1].Value;
                        var v = thisMatch.Groups[2].Value;
                        switch (k)
                        {
                            case "Server":
                                server = v;
                                break;
                            case "Database":
                                database = v;
                                break;
                            case "UID":
                                uid = v;
                                break;
                            case "PWD":
                                pwd = v;
                                break;
                            case "Offset":
                                Offset = Convert.ToInt32(v);
                                break;
                            case "WebServerURL":
                                WebServerURL = v;
                                break;
                            case "Key":
                                Key = v;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            configStreamReader.Close();

            Console.WriteLine("======================================================");
            Console.WriteLine("[{0}] ACMAnswerChecker Started.", DateTime.Now);
            Console.WriteLine("======================================================");

            Runner.InitExitCodeDictionary();
            DatabaseConnector.Init(server, database, uid, pwd);
            PendingWatcher.StartWatcher();

            Console.ReadLine();
        }
    }
}
