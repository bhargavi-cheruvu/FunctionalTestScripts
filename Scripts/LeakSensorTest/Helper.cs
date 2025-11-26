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
        public const string REQUEST = "?";
    }

    public class LeakSensorParameters
    {
        public const string LEAKSENSOR_TEST = "Leak Sensor Test Script.";
        public const string LEAKSENSOR_CALIBRATE = "LeakSensor.Calibrate";
        public const string LEAKSENSOR_CALIBRATE_RESP = "~LeakSensor.Calibrate";
        public const string NoResponseFromDevice = "No response from device for LeakSensor.Calibrate";
        public const string LEAKSENSOR_CALIBRATE_OFFSET = "LeakSensor.CalibOffset";
        public const int TimeInterval = 1000;
        public const string MUTE_ALARM = "MuteAlarm";
        public const string MUTE_ALARM_RESP = "~MuteAlarm";
        public const string ExpectedResult = "OK";
        public const int ReactionTime_AfterApplyingWater = 20000;
        public const string Dry_LeakSensor = "Leak";
        public const string Dry_LeakSensor_Resp = "~Leak";
        public const string Dry_LeakSensor_ExpectedResp = "0";
        public const int TimeInterval_DryLeakSensor = 30000;      
    }

    public class ServiceLevelParameterNames
    {
        public const string ServiceChallange = "Service.Challenge";        
        public const string ServiceCodeRequest = "Service.Code";
        public const string ValidateServiceCode = "~Service.Code";
        public const string NoResponseFromDevice = "No response from device for Service.Challenge.";
        public const string ServiceCodeFailure = "Service.Code was not accepted. Expected '1', got unexpected result.";
        public const string ServiceCodeSuccess = "Device successfully switched to Service level.";       
        public const string ServiceChallangeResponse = "Challenge received: Service.Challenge=";
        public const string ServiceCode = "87794";
        public const string ExpectedServiceCode = "1";
        public const string ServiceLockResult = "OK";
        public const int TimeInterval = 3000; //1000;      
    }  
}
