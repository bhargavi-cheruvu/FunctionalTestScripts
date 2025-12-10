using Helper;
using LogViewManager;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using UniversalBoardTestApp;
using System.Drawing;
public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    string muteState = string.Empty;

    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        if (CalibrateLeakSensor())
        { return true; }
        else return false;
    }

    public bool CalibrateLeakSensor()
    {
        string calibOffSetVal = string.Empty;
        Logger.LogMessage(Level.Info, LeakSensorParameters.LEAKSENSOR_TEST);

        // ---------------------------------------------------------
        // Helper to exit with logging
        bool Fail(string message)
        {
            Logger.LogMessage(Level.Error, message);
            return false;
        }
        // ---------------------------------------------------------

        // 1. Send Calibrate
        HardwareParameters.SetParameter(LeakSensorParameters.LEAKSENSOR_CALIBRATE, Handler.Nothing);

        //  waiting for 10 seconds for calibration to complete.
        if (!WaitForCalibExpectedResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE, "OK", 10000))
        {
            Logger.LogMessage(Level.Error, LeakSensorParameters.NoResponseFromDevice);
            return false;
        }

        // 2. Read calibration offset
        HardwareParameters.SetParameter(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET, Handler.REQUEST);

        if (!WaitForResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET,
                             LeakSensorParameters.TimeInterval,
                             out string calibrateOffsetVal))
            return Fail("Calib offset not received.");

        // retreive the offset value.
        var lines = calibrateOffsetVal.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > 0 && token[0] == LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET_VAL)
            {
                string key = token[0];
                calibOffSetVal = token[1];
                Logger.LogMessage(Level.Info, $"LeakSensor CalibOffset: {calibOffSetVal}");               
            }
        }

        // 3. Ask user to apply water
        if (MessageBox.Show("Apply 20 ml water to the Leak Sensor",
                            "Leak Sensor",
                            MessageBoxButtons.OKCancel) != DialogResult.OK)
            return Fail("User cancelled: did not apply water.");

        // Allow stabilization
        Thread.Sleep(LeakSensorParameters.ReactionTime_AfterApplyingWater);
               
        // 4. Now confirm that water was actually applied        
        if (!WaitForResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET,
                             1000,
                             out string waterReply))
            return Fail("Timeout: No leak detected within 60 seconds.");

        // Parse actual integer value
        int leakVal = ParseLeakValue(waterReply);

        if (leakVal ==0)
            return Fail($"Water not detected – value too low: {leakVal}");

        Logger.LogMessage(Level.Info, $"Water detected successfully. Leak value = {leakVal}");

        // retreive the offset value.  Expected Alarm = 1 and err.
        lines = waterReply.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > 0 && token[0] == "~Alarm")
            {
                string key = token[0];
                calibOffSetVal = token[1];
                Logger.LogMessage(Level.Info, $"LeakSensor - Alarm : {calibOffSetVal}");
               // break;
            }
            if (token.Length > 0 && token[0] == "~Err")
            {
                string key = token[0];
                calibOffSetVal = token[1];
                if(calibOffSetVal == null)
                    Logger.LogMessage(Level.Info, $"LeakSensor - Error : {calibOffSetVal}");      
            }
        }

        // 5. Mute alarm
        if (leakVal == 1)
        {
            HardwareParameters.SetParameter(LeakSensorParameters.Dry_LeakSensor, "?");
            HardwareParameters.GetParameter(LeakSensorParameters.Dry_LeakSensor, out string dryState, true);
          
            // retreive the offset value.
            lines = dryState.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == LeakSensorParameters.Dry_LeakSensor_Resp)
                {
                    string key = token[0];
                    calibOffSetVal = token[1];
                    Logger.LogMessage(Level.Info, $"LeakSensor - ~Leak : {calibOffSetVal}");
                }
                if (token.Length > 0 && token[0] == "~Alarm")
                {
                    string key = token[0];
                    calibOffSetVal = token[1];
                    Logger.LogMessage(Level.Info, $"LeakSensor - ~Alarm : {calibOffSetVal}");                   
                }
                if (token.Length > 0 && token[0] == "~MuteAlarm")
                {
                    string key = token[0];
                    calibOffSetVal = token[1];
                    Logger.LogMessage(Level.Info, $"LeakSensor - ~MuteAlarm : {calibOffSetVal}");
                }
            }
        }

        // 6. Ask user to dry the leak sensor
        if (MessageBox.Show("Dry the Leak Sensor",
                            "Leak Sensor",
                            MessageBoxButtons.YesNo) != DialogResult.Yes)
            return Fail("User did not dry the leak sensor.");

        if (!WaitForExpectedResponse(LeakSensorParameters.Dry_LeakSensor,
                                     LeakSensorParameters.Dry_LeakSensor_ExpectedResp,
                                     LeakSensorParameters.TimeInterval_DryLeakSensor))
            return Fail("Leak Sensor still wet after 30 seconds.");

        Logger.LogMessage(Level.Info, "Leak Sensor calibration completed successfully.");
        return true;
    }

    private int ParseLeakValue(string msg)
    {
        var lines = msg.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN },
                                       StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > 1 && token[0] == LeakSensorParameters.Dry_LeakSensor_Resp)//LEAKSENSOR_CALIBRATE_OFFSET_VAL)
            {
                int val = Convert.ToInt32(token[1]);
                return val;
            }
        }

        return 0;
    }

    private string ParseMuteAlarmResponse(string muteResponse)
    {
        var lines = muteResponse.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN },
                                       StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > 1 && token[0] == LeakSensorParameters.MUTE_ALARM_RESP)
                return token[1];
        }

        return string.Empty;
    }   

    private bool WaitForResponse(string parameterName, int timeoutMs, out string response)
    {
        int elapsed = 0;
        int interval = 100;
        response = null;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);
            if (!string.IsNullOrEmpty(response))
                return true;

            Thread.Sleep(interval);
            elapsed += interval;
        }

        response = null;
        return false;
    }

    private bool WaitForExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    {
        int elapsed = 0;
        int interval = 300;
        string response;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);
            // Parse and Validate for Leak Sensor calibrate and Leak Sensor CalibrateOffset values.           
            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == LeakSensorParameters.Dry_LeakSensor_Resp)
                {
                    string key = token[0];
                    response = token[1];
                    Logger.LogMessage(Level.Info, $"~Leak : {response}");
                    break;
                }               
            }

            if (!string.IsNullOrEmpty(response) && response == expectedValue)
                return true;

            Thread.Sleep(interval);
            elapsed += interval;
        }

        return false;
    }

    private bool WaitForCalibExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    {
        int elapsed = 0;
        int interval = 300;
        string response;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);
            // Parse and Validate for Leak Sensor calibrate and Leak Sensor CalibrateOffset values.           
            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
              
                if (token.Length > 0 && token[0] == LeakSensorParameters.LEAKSENSOR_CALIBRATE_RESP)
                {
                    string key = token[0];
                    response = token[1];
                    Logger.LogMessage(Level.Info, $"Leak Calibrate Response: {response}");                   
                }
            }

            if (!string.IsNullOrEmpty(response) && response == expectedValue)
                return true;

            Thread.Sleep(interval);
            elapsed += interval;
        }

        return false;
    }
}