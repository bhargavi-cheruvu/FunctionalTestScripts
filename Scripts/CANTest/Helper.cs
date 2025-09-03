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
    }

    public class CANNodeParameters
    {
        public const string LeftCAN_CMD_NMTSTOP = "CanE0.NmtStop";
        public const string LeftCAN_CMD_NMTSTART = "CanE0.NmtStart";
        public const string LeftCANConnector = "Left CAN Connector";
        public const string RIGHTCANConnector = "Right CAN Connector";

        public const string LEFTCAN_CONNECTOR_FAILED = "Left CAN Connector Failed.";
        public const string RIGHTCAN_CONNECTOR_FAILED = "Right CAN Connector Failed.";

        public const string LEFTCAN_CONNECTOR_PASSED = "Left CAN Connector: test OK.";
        public const string RIGHTCAN_CONNECTOR_PASSED = "Right CAN Connector: test OK.";

        public const string CAN_TEST_COMPLETED = "Test Completed Successfully.";
        public const string POWER_DOWN = "Power down ...";

        public const string CONNECTION_REESTABLISHED = "Connection re-established ...";
        public const string WAIT_FOR_BOOTPROCESS = "Wait for boot process completion ...";

        public const string WAIT_FOR_DEVICEREBOOT = "Checking for device reboot via USB loss marker...";
        public const double WaitTimeinSeconds = 1.0;
        public const double WaitReadyinSeconds = 5.0;
        public const double WaitForReconnectinSeconds = 60.0;

        public const int TimeinMilliSeconds = 1000;
    }
}
