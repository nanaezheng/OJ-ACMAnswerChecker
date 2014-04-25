using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACMAnswerChecker
{
    static class Const
    {
        public const short StatusCodeSystemError = -1;
        public const short StatusCodeUnknownStatus = 0;
        public const short StatusCodePending = 1;
        public const short StatusCodeCompiling = 2;
        public const short StatusCodeRunning = 3;
        public const short StatusCodeAccepted = 4;
        public const short StatusCodePresentationError = 5;
        public const short StatusCodeWrongAnswer = 6;
        public const short StatusCodeTimeLimitExceeded = 7;
        public const short StatusCodeMemoryLimitExceeded = 8;
        public const short StatusCodeOutputLimitExceeded = 9;
        public const short StatusCodeRuntimeError = 10;
        public const short StatusCodeCompileError = 11;

        public const short LanguageCodeUnknownLanguage = 0;
        public const short LanguageCodeC = 1;
        public const short LanguageCodeCpp = 2;
        public const short LanguageCodeJava = 3;

    }
}
