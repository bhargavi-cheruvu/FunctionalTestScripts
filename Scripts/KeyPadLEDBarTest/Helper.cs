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
        public const string TEST_VERSION = "0.0.0.1";
        public const string LEDBarTest = "Key Pad / LEDs Test Script.";
      
        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
        public const int INDEX_TWO = 2;
        public const int INDEX_THREE = 3;
        public const int INDEX_FOUR = 4;
        public const int INDEX_FIVE = 5;
        public const char DELIMITER = '=';

        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
        public const string LED_CAPTION = "LEDBar";

        public const string LEDBAR_REQ_CMD = "LedBar.ForceColor";
        public const string LEDBAR_RESP_CMD = "~LedBar.ForceColor";

        public const string DELETE_LEDCOLORS = "Deletes the Overwrite of the LED Colors";
        //public const string CHECK_BEEP_SOUND = "Press the Enabled Button and Verify there is a Beep Sound";

        public const string PRESS_MUTEALARM_BUTTON = "Press MUTEALARM Button and check for the Beep Sound. \n If Beep Sound is heard then click OK button to continue";
        public const string PRESS_SELECT_BUTTON = "Press SELECT Button and check for the Beep Sound. \n If Beep Sound is heard then click OK button to continue";
        public const string PRESS_DOCK_BUTTON = "Press DOCK Button and check for the Beep Sound. \n If Beep Sound is heard then click OK button to continue";
        public const string PRESS_PURGE_BUTTON = "Press PURGE Button and check for the Beep Sound. \n If Beep Sound is heard then click OK button to continue";
        public const string PRESS_FLOW_BUTTON = "Press Flow Button and check for the Beep Sound. \n If Beep Sound is heard then click OK button to continue";

        public const string KEYPAD_CAPTION = "KeyPad";
        public const string RESPONSE_KEYPROPERTY = "Keys";
        public const string KEYS_RESP_CMD = "~Keys";

        public const string KEYPAD_TESTMODE = "Key.TestMode";
    }

    public enum LEDSTATUSCOLOR
    {
        //OFF,
        RED=1,
        GREEN,
        BLUE,
        YELLOW,
        OFF,
        //DELETE,
    }
}
