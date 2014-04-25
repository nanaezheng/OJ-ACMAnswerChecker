using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ACMAnswerChecker
{
    class Problem
    {
        public Int64 Id { get; private set; }
        public Int64 TimeLimitNormal { get; private set; }
        public Int64 TimeLimitJava { get; private set; }
        public Int64 MemoryLimitNormal { get; private set; }
        public Int64 MemoryLimitJava { get; private set; }
        public string StandardInput { get; private set; }
        public string StandardOutput { get; private set; }

        public Problem(long id, long timeLimitNormal, long timeLimitJava, long memoryLimitNormal, long memoryLimitJava, string standardInput, string standardOutput)
        {
            Id = id;
            TimeLimitNormal = timeLimitNormal;
            TimeLimitJava = timeLimitJava;
            MemoryLimitNormal = memoryLimitNormal;
            MemoryLimitJava = memoryLimitJava;

            var regex = new Regex("^https?://(.*)$");

            if (standardInput == "" || standardOutput == "" || standardInput[standardInput.Length - 1] == '/' || standardOutput[standardOutput.Length - 1] == '/')
            {
                throw new WebException("标准输入或输出未设置");
            }

            var standardInputDataURL = Program.WebServerURL + standardInput;
            var standardOutputDataURL = Program.WebServerURL + standardOutput;

            if (regex.IsMatch(standardInputDataURL))
            {
                try
                {
                    var client = new WebClient { Encoding = Encoding.UTF8 };
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    StandardInput = client.UploadString(standardInputDataURL, "POST", "KEY=" + Program.Key);
                }
                catch (WebException)
                {
                    throw;
                }
            }
            else
            {
                StandardInput = standardInput;
            }

            if (regex.IsMatch(standardOutputDataURL))
            {
                try
                {
                    var client = new WebClient { Encoding = Encoding.UTF8 };
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    StandardOutput = client.UploadString(standardOutputDataURL, "POST", "KEY=" + Program.Key);
                }
                catch (WebException)
                {
                    throw;
                }
            }
            else
            {
                StandardOutput = standardOutput;
            }
        }
    }
}
