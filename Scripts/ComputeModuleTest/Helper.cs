using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class Handler
    {
        public const string TEST_VERSION = "1.0.0.0"; // Version Number
        public const string ComputeModuleTest = "Starting Compute Module Test Script.";
        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const char DELIMITER = '=';
        public const int TOKEN_LENGTH = 2;
        public const string Nothing = "";
        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
        public const string TestDCFAlignment_STATUS = "~Err";
        public const string TestDCFAlignmentResponse = "OK";
        public const string TestDCFAlignmentErrorResponse = "DCF Alignment Test has completed. Code 4257.";
        public const string TestDCF_MissingTestAdapter = "Improper or missing test adapter or CPU board alignment failure. Code 4254.";
        public const string TESTDCFAlignment_INVALID_RESPONSE = "Invalid Response";
        public const int ComputeModule_WaitTime = 1000;
        public const string COMPUTEMODULE_TESTDETAIL = "ComputeModuleTestDetail";
        public const string TestDCFAlignment_ERRORSTATUS = "~TestDCFAlignment";
    }
}
