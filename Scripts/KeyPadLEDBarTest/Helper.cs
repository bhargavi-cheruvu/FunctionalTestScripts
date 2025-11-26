using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Helper
{
    public class Handler
    {
        public const string TEST_VERSION = "0.0.0.1";
        public const string LEDBarTest = "Key Pad / LEDs Test Script.";

        public const string StartServiceLevelTest = "Starting Service Level Test Script.";
        public const string SwitchToServiceLevel = "Starting Service Level switch test...";

        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
        public const int INDEX_TWO = 2;
        public const int INDEX_THREE = 3;
        public const int INDEX_FOUR = 4;
        public const int INDEX_FIVE = 5;
        public const char DELIMITER = '=';
        public const string Nothing = "";
        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const string LED_CAPTION = "LEDBar";

        public const string LEDBAR_REQ_CMD = "LedBar.ForceColor";
        public const string LEDBAR_RESP_CMD = "~LedBar.ForceColor";

        public const string DELETE_LEDCOLORS = "Deletes the Overwrite of the LED Colors";
        //public const string CHECK_BEEP_SOUND = "Press the Enabled Button and Verify there is a Beep Sound";

        public const string PRESS_MUTEALARM_BUTTON = "\n\n Press MUTEALARM Button and check for the Beep Sound. \n\n If Beep Sound is heard then click Yes button to continue. \n\n\n Please confirm";
        public const string PRESS_SELECT_BUTTON = "Please Confirm the LED Status:\r\n\r\nPress the SELECT button on the hardware and check whether the L 🟢, R 🟢, and L+R 🟢 LEDs.\r\n\r\n 'L 🟢' LED – Green\r\n\r\n 'R 🟢' LED – Green\r\n\r\nBoth 'L 🟢' and 'R 🟢' LEDs – Green \r\n\r\n After verifying, please confirm to continue.";
        public const string PRESS_DOCK_BUTTON = "\n Press DOCK Button and check for the Beep Sound. \r\n\r\n If Beep Sound is heard then click Yes button \n to continue. \n\n\n Please confirm";
        public const string PRESS_PURGE_BUTTON = "\n Press PURGE Button and check for the Beep Sound. \r\n\r\n If Beep Sound is heard then click Yes button \n to continue. \n\n\n Please confirm";
        public const string PRESS_FLOW_BUTTON = "Please Confirm the LED Status:\r\n\r\nPress the FLOW button on the hardware and check whether the L 🟢, R 🟢, and L+R 🟢 LEDs.\r\n\r\n 'L 🟢' LED – Green\r\n\r\n 'R 🟢' LED – Green\r\n\r\nBoth 'L 🟢' and 'R 🟢' LEDs – Green \r\n\r\n After verifying, please confirm to continue.";

        // Press the SELECT button on the hardware and check whether the 
        // L 🟢, R 🟢, and L+R 🟢 LEDs are glowing green.

        public const string KEYPAD_CAPTION = "KeyPad";
        public const string RESPONSE_KEYPROPERTY = "Keys";
        public const string KEYS_RESP_CMD = "~Keys";

        public const string KEYPAD_TESTMODE = "Key.TestMode";
    }

    public enum LEDSTATUSCOLOR
    {        
        RED=1,
        GREEN,
        BLUE,
        YELLOW,
        OFF,
    }

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
}
