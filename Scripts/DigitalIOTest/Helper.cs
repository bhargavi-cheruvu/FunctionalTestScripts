using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class HardwareParameterNames
    {
        public const string Relay1State = "Relay1.State";
        public const string Relay2State = "Relay2.State";
        public const string Relay3State = "Relay3.State";
        public const string Relay4State = "Relay4.State";
        public const string InitialState = "Initial.State";
    }

    public class ResponsefromDevice
    {
        public const int ZERO = 0;
        public const int ONE = 1;
        public const int SIZE = 4;

        public string[] InitialRelayResponse = new string[]
        {
            "~Relay1.State=0",
            "~Relay2.State=0",
            "~Relay3.State=0",
            "~Relay4.State=0"
        };
        public string[] relay1ExpectedRespforSetState = new string[] {
            "~Relay1.State=1",
            "~Input2.State=0",
            "~Input1.State=0"
        };
        public string[] relay1ExpectedRespforReSetState = new string[] {
            "~Relay1.State=0",
            "~Input2.State=1",
            "~Input1.State=1"
        };
        public string[] relay2ExpectedRespforSetState = new string[] {
            "~Relay2.State=1",
            "~Input2.State=0",
            "~Input1.State=0"
        };
        public string[] relay2ExpectedRespforReSetState = new string[] {
            "~Relay2.State=0",
            "~Input2.State=1",
            "~Input1.State=1"
        };

        public string[] relay3ExpectedRespforSetState = new string[] {
            "~Relay3.State=1",
            "~Input4.State=1",
            "~Input3.State=1"
        };
        public string[] relay3ExpectedRespforReSetState = new string[] {
            "~Relay3.State=0",
            "~Input4.State=0",
            "~Input3.State=0"
        };
        public string[] relay4ExpectedRespforSetState = new string[] {
            "~Relay4.State=1",
            "~Input4.State=1",
            "~Input3.State=1"
        };
        public string[] relay4ExpectedRespforReSetState = new string[] {
            "~Relay4.State=0",
            "~Input4.State=0",
            "~Input3.State=0"
        };
    }

    public class Handler
    {
        // Failure conditions
        public const string MISSING_ADAPTER = "Missing adapter or digital I/O error.";
        public const string VALIDATION_SUCCESSFUL = "Parsed response validation passed.";
        public const string VALIDATION_FAILED = "Parsed response validation failed.";

        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const char DELIMITER = '=';
        public const int TOKEN_LENGTH = 2;       

        public const string TEST_VERSION = "1.0.0.0"; // Version Number

        public const string DIGI_TESTDETAIL = "DigitalIOTestDetail";
    }

    public class RequestToDevice
    {
        public const string DigiIOTest = "Starting Digital Input/Output Test Script.";
        
        public const int InitialRelayValue = 0;
        public const int ActivatedRelayValue = 1;
        public const int RelayTestDelayMilliseconds = 50;
        public const int RelayResetDelayMilliseconds = 1000;
    }
}
