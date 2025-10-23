using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class Handler
    {
        public const string TEST_VERSION = "0.0.0.1"; // Version Number
        public const string StartServiceLevelTest = "Starting Service Level Test Script.";
        public const string SwitchToServiceLevel = "Starting Service Level switch test...";
        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const char DELIMITER = '=';
        public const int TOKEN_LENGTH = 2;
        public const string Nothing = "";

        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
    }
    //public class ServiceLevelParameterNames
    //{
    //    public const string ServiceChallange = "Service.Challenge";        
    //    public const string ServiceCodeRequest = "Service.Code";
    //    public const string ValidateServiceCode = "~Service.Code";
    //    public const string NoResponseFromDevice = "No response from device for Service.Challenge.";

    //    public const string ServiceCodeFailure = "Service.Code was not accepted. Expected '1', got unexpected result.";
    //    public const string ServiceCodeSuccess = "Device successfully switched to Service level.";

    //    public const string ServiceChallangeResponse = "Challenge received: Service.Challenge=";

    //    public const string ServiceCode = "87794";
    //    public const string ExpectedServiceCode = "1";
    //    public const string ServiceLockResult = "OK";
    //    public const int TimeInterval = 3000; //1000;      
    //}

    public class ServiceLevelParameterNames
    {
        public const string ServiceChallange = "Service.Challenge";
        public const string ServiceChallange_Resp = "~Service.Challenge";
        public const string ServiceCodeRequest = "Service.Code";
        public const string ValidateServiceCode = "~Service.Code";
        public const string NoResponseFromDevice = "No response from device for Service.Challenge.";

        public const string ServiceCodeFailure = "Service.Code was not accepted. Expected '1', got unexpected result.";
        public const string ServiceCodeSuccess = "Device successfully switched to Service level.";

        public const string ServiceLevelUserMode = "Service.Lock";
        public const string ServiceLock_Response = "~Service.Lock";
        public const string ServiceChallangeResponse = "Challenge received: Service.Challenge=";
        public const string UserMode = "Switched back to User Mode from ServiceLevel Mode";
        public const int ServiceChallangeVal = 0;
        public const int ServiceCode = 87794;
        public const string ExpectedServiceCode = "1";
        public const string ServiceLockResult = "OK";
        public const int TimeInterval = 1000;
    }

    public class DegasserParameterNames
    {
        public const string DEGASSER_CMD = "Degasser";
        public const string DEGASSER_PRESSURE = "Degasser.Pressure";
        public const string DEGASSER_PRESSURE_OFFSET = "Degasser.Pressure.Offset";

        public const int WAITTIME = 1000;
        public const int WAITTIME_THIRTY_SECONDS = 30 * 1000;
        public const int WAITTIME_THREE_MINS = 3 * 60 * 1000;
        public const int WAITTIME_ONE_MIN = 60 * 1000;

        public const int SPEED_MULTIPLE_OFFSET = 15;

        public const string DEGASSER_PRESSURE_VAL = "~Degasser.Pressure";
    }

    public enum GHM_TransmitFunktion : short
    {
        // AnzeigewertLesen = 0,
        ReadDisplayUnit = 0,
        ReadIDNumber = 1,
        ReadMeasurementRangeAndType = 2,
        ReadDisplayMeasurementType = 3, // 199
        ReadMeasurementRangeUnit = 4,
       
    }
}
