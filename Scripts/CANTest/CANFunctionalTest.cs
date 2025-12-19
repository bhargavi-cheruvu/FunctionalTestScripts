using Helper;
using LogViewManager;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using UniversalBoardTestApp;
using UniversalBoardTestApp.XmlModels;

public class Test
{
    private const string TestVersion = Handler.TEST_VERSION;
    private const int PollIntervalMs = 100;

    public bool Start()
    {
        try
        {
            if (ScriptHelper.CheckIfProcedureIsCancelled()) return false;

            Logger.LogMessage(Level.Info, "Starting CAN Functional Test...");

            // Test Left CAN Connector
            if (!TestCanNode(0x11, 0x611, "Left CAN"))
            {
                Fail("Left CAN Connector Test Failed", CANNodeParameters.LEFTCAN_CONNECTOR_FAILED);
                return false;
            }
            Pass("Left CAN Connector Test Passed", CANNodeParameters.LEFTCAN_CONNECTOR_PASSED);

            // Test Right CAN Connector
            if (!TestCanNode(0x12, 0x612, "Right CAN"))
            {
                Fail("Right CAN Connector Test Failed", CANNodeParameters.RIGHTCAN_CONNECTOR_FAILED);
                return false;
            }
            Pass("Right CAN Connector Test Passed", CANNodeParameters.RIGHTCAN_CONNECTOR_PASSED);

            Pass("CAN Connector Test Completed Successfully", CANNodeParameters.CAN_TEST_COMPLETED);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogMessage(Level.Error, ex.ToString());
            return false;
        }
    }

    private bool TestCanNode(byte nodeId, int sdoNodeId, string nodeName)
    {
        Logger.LogMessage(Level.Info, $"Starting {nodeName} test...");

        if (!SendNmtStop(nodeId, nodeName)) return false;
        if (!SendNmtStart(nodeId, nodeName)) return false;
        if (!SendSdoSW(sdoNodeId, 0x2014, 0x01, 0x0C00, nodeName)) return false;
        if (!SendSdoSB(sdoNodeId, 0x2011, 0x01, 0xFF, nodeName)) return false;

        Logger.LogMessage(Level.Warning, CANNodeParameters.POWER_DOWN);
        // Step 5: Increase timeout to 60 seconds for disconnect scenario
        if (!SendSdoSB(sdoNodeId, 0x6200, 0x01, 0x01, nodeName, 30)) return false;

        // Wait for USB reconnect after step 5
        if (!WaitReconnect(30)) return false;

        return SendNmtStop(nodeId, nodeName);
    }

    private bool SendNmtStop(byte nodeId, string stepName)
    {
        Logger.LogMessage(Level.Info, $"Sending NMT Stop to Node {nodeId:X2}");
        HardwareParameters.SetParameter("CanE0.NmtStop", nodeId);
        return WaitForResult("CanE0.NmtStop", stepName, 5);
    }

    private bool SendNmtStart(byte nodeId, string stepName)
    {
        Logger.LogMessage(Level.Info, $"Sending NMT Start to Node {nodeId:X2}");
        HardwareParameters.SetParameter("CanE0.NmtStart", nodeId);
        return WaitForResult("CanE0.NmtStart", stepName, 5);
    }

    private bool SendSdoSW(int nodeId, ushort index, byte subIndex, int value, string stepName)
    {
        Logger.LogMessage(Level.Info, $"Sending SDO WriteWord to Node {nodeId:X2}: {index:X4}:{subIndex:X2} = {value:X4}");
        HardwareParameters.SetParameter("CanE0.SdoSW", $"{nodeId},{index},{subIndex},{value}");
        return WaitForResult("CanE0.SdoSW", stepName, 5);
    }

    private bool SendSdoSB(int nodeId, ushort index, byte subIndex, byte value, string stepName, double timeoutSeconds = 5)
    {
        Logger.LogMessage(Level.Info, $"Sending SDO WriteByte to Node {nodeId:X2}: {index:X4}:{subIndex:X2} = {value:X2}");
        HardwareParameters.SetParameter("CanE0.SdoSB", $"{nodeId},{index},{subIndex},{value}");
        return WaitForResult("CanE0.SdoSB", stepName, timeoutSeconds);
    }

    private bool WaitForResult(string resultKey, string stepName, double timeoutSeconds = 5)
    {
        int timeoutMs = (int)(timeoutSeconds * 1000);
        int elapsed = 0;

        Logger.LogMessage(Level.Info, $"Polling for {stepName} response (timeout {timeoutSeconds}s)...");

        while (elapsed < timeoutMs)
        {
            if (ScriptHelper.CheckIfProcedureIsCancelled()) return false;

            bool success = HardwareParameters.GetParameter(resultKey, out string response, true);
            if (success)
            {
                var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    var token = line.Split(Handler.DELIMITER);
                    if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == "CanE0.NmtStop")
                    {
                        string key = token[Handler.INDEX_ZERO];
                        response = token[Handler.INDEX_ONE];
                        Logger.LogMessage(Level.Info, $"Raw response for {stepName}: {response}");
                        break;
                    }
                    if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == "CanE0.NmtStart")
                    {
                        string key = token[Handler.INDEX_ZERO];
                        response = token[Handler.INDEX_ONE];
                        Logger.LogMessage(Level.Info, $"Raw response for {stepName}: {response}");
                        break;
                    }
                    if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == "CanE0.SdoSW")
                    {
                        string key = token[Handler.INDEX_ZERO];
                        response = token[Handler.INDEX_ONE];
                        Logger.LogMessage(Level.Info, $"Raw response for {stepName}: {response}");
                        break;
                    }
                    if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == "CanE0.SdoSB")
                    {
                        string key = token[Handler.INDEX_ZERO];
                        response = token[Handler.INDEX_ONE];
                        Logger.LogMessage(Level.Info, $"Raw response for {stepName}: {response}");
                        break;
                    }
                }
                //Logger.LogMessage(Level.Info, $"Raw response for {stepName}: {response}");
            }

            if (!string.IsNullOrEmpty(response) && response.Contains("OK"))
            {
                Logger.LogMessage(Level.Info, $"{stepName} succeeded: OK");
                return true;
            }

            Thread.Sleep(PollIntervalMs);
            elapsed += PollIntervalMs;
        }

        Logger.LogMessage(Level.Error, $"Timeout waiting for {stepName} to return OK");
        return false;
    }

    private bool WaitReconnect(double timeoutSeconds)
    {
        Logger.LogMessage(Level.Info, $"Waiting for CAN reconnect (timeout {timeoutSeconds}s)...");
        int timeoutMs = (int)(timeoutSeconds * 1000);
        int elapsed = 0;

        while (elapsed < timeoutMs)
        {
            bool connected = HardwareParameters.GetParameter("CanDL.Tx.Enabled", out string val, true);
            var lines = val.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == "~CanDL.Tx.Enabled")
                {
                    string key = token[Handler.INDEX_ZERO];
                    val = token[Handler.INDEX_ONE];
                    Logger.LogMessage(Level.Info, $"Reconnect poll: CanDL.Tx.Enabled = {val}");
                    break;
                }
                //Logger.LogMessage(Level.Info, $"Reconnect poll: CanDL.Tx.Enabled = {val}");
            }

            if (connected && val.Contains("1"))
            {
                Logger.LogMessage(Level.Info, "CAN connection restored");
                return true;
            }
            else
            {
                Logger.LogMessage(Level.Error, "CAN Adapter disconnected");
                return false;
            }

                Thread.Sleep(PollIntervalMs);
            elapsed += PollIntervalMs;
        }

        Logger.LogMessage(Level.Error, "Timeout waiting for CAN reconnect");
        return false;
    }

    private void Pass(string msg, string log)
    {
        new TestDetail(CANNodeParameters.CAN_TESTDETAIL, msg, true);
        Logger.LogMessage(Level.Info, log);
    }

    private void Fail(string msg, string log)
    {
        new TestDetail(CANNodeParameters.CAN_TESTDETAIL, msg, false);
        Logger.LogMessage(Level.Error, log);
    }
}
