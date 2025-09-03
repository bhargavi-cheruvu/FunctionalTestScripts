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
        public const string USBHubTest = "USB Hub Test Script.";
        public const string CheckUSBDrives = "Checking for Connected USB Drives...";
        public const string NO_USB_DRIVES = "No USB Drives detected.";
        public const string WIN32_LOGICALDISKTOPARTITION = "SELECT * FROM Win32_LogicalDiskToPartition";
        public const string DEPENDENT = "Dependent";
        public const string ANTECEDENT = "antecedent";
        public const char DELIMITER = '=';
        public const char PATH_DELIMITER = '\\';
        public const string OLD_VALUE = "\\";
        public const string NEW_VALUE = "";
        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
        public const int INDEX_TWO = 2;
        public const int INDEX_THREE = 3;

        public const string PNPDEVICEID = "PNPDeviceID";
        public const string DEVICEID = "DeviceID";
        public const string MODEL = "Model";

        public const string ONE_USB_DETECTED = "One USB sticks detected.";
        public const string TWO_USB_DETECTED = "Two USB sticks detected.";
        public const string THREE_USB_DETECTED = "Three USB sticks detected.";

        public const string UNKNOWN = "UnKnown";
    }    
}
