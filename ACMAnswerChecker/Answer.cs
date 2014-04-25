using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMAnswerChecker
{
    internal class Answer
    {
        internal Int64 Id { get; private set; }
        internal Int64 ProblemId { get; private set; }
        internal Int64 UserId { get; private set; }
        internal Int16 LanguageCode { get; private set; }
        internal String SourceCode { get; private set; }
        internal String InputData { get; set; }
        internal String OutputData { get; set; }
        internal Int64 UsedTime { get; set; }
        internal Int64 UsedMemory { get; set; }
        internal Int16 StatusCode { get; set; }
        internal String Info { get; set; }
        internal DateTime SubmitTime { get; private set; }
        internal DateTime MarkedTime { get; private set; }

        public Answer(long id, long problemId, long userId, short languageCode, string sourceCode, string inputData, string outputData, long usedTime, long usedMemory, short statusCode, string info, DateTime submitTime, DateTime markedTime)
        {
            Id = id;
            ProblemId = problemId;
            UserId = userId;
            LanguageCode = languageCode;
            SourceCode = sourceCode;
            InputData = inputData;
            OutputData = outputData;
            UsedTime = usedTime;
            UsedMemory = usedMemory;
            StatusCode = statusCode;
            Info = info;
            SubmitTime = submitTime;
            MarkedTime = markedTime;
        }

        public void UpdateMarkedTime()
        {
            MarkedTime = DateTime.Now;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Id: {0}, ProblemId: {1}, UserId: {2}, LanguageCode: {3}, UsedTime: {4}, UsedMemory: {5}, StatusCode: {6}, SubmitTime: {7}, MarkedTime: {8}",
                    Id, ProblemId, UserId, LanguageCode, UsedTime, UsedMemory, StatusCode, SubmitTime, MarkedTime);
        }
    }
}