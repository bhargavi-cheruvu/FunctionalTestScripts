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
        public const string TEST_VERSION = "0.0.0.1"; // Version Number
        public const string ComputeModuleTest = "Starting Compute Module Test Script.";
       // public const string SwitchToServiceLevel = "Starting Service Level switch test...";
        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const char DELIMITER = '=';
        public const int TOKEN_LENGTH = 2;
        public const string Nothing = "";

        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;

        public const string TestDCFAlignment_STATUS = "~TestDCFAlignment";
        public const string TestDCFAlignmentResponse = "OK";
        public const string TestDCFAlignmentErrorResponse = "DCF Alignment Test has completed. Code 4257.";
        public const string TESTDCFAlignment_INVALID_RESPONSE = "Invalid Response";
    }
    //public class ServiceLevelParameterNames
    //{
    //    public const string ServiceChallange = "Service.Challenge";        
    //    public const string ServiceChallange_Resp = "~Service.Challenge";
    //    public const string ServiceCodeRequest = "Service.Code";
    //    public const string ValidateServiceCode = "~Service.Code";
    //    public const string NoResponseFromDevice = "No response from device for Service.Challenge.";

    //    public const string ServiceCodeFailure = "Service.Code was not accepted. Expected '1', got unexpected result.";
    //    public const string ServiceCodeSuccess = "Device successfully switched to Service level.";

    //    public const string ServiceLevelUserMode = "Service.Lock";        
    //    public const string ServiceLock_Response = "~Service.Lock";
    //    public const string ServiceChallangeResponse = "Challenge received: Service.Challenge=";
    //    public const string UserMode = "Switched back to User Mode from ServiceLevel Mode";
    //    public const int ServiceChallangeVal = 0;
    //    public const int ServiceCode = 87794;
    //    public const string ExpectedServiceCode = "1";
    //    public const string ServiceLockResult = "OK";
    //    public const int TimeInterval = 1000;      
    //}
}
