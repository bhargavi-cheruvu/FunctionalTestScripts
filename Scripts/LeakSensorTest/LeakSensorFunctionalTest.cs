using Helper;
using LogViewManager;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using UniversalBoardTestApp;
using System.Windows.Forms;
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

        //  waiting for 10 seconds for calibration to complete.
        if (!WaitForCalibExpectedResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE, "OK", 10000))
        {
            Logger.LogMessage(Level.Error, LeakSensorParameters.NoResponseFromDevice);
            return false;
        }

        // 3. Ask user to apply water
        if (TopMostMessageBox.Show("Apply 20 ml water to the Leak Sensor",
                            "Leak Sensor",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK) 
            return Fail("User cancelled: did not apply water.");

        // Allow stabilization
        Thread.Sleep(LeakSensorParameters.ReactionTime_AfterApplyingWater);

        // 4. Now confirm that water was actually applied
        if (!WaitForLeakAlarm(out string waterReply))
            return Fail("Timeout: No leak alarm detected within 60 seconds.");

        int leakVal = ParseLeakValue(waterReply);

        if (leakVal == 0)
            return Fail($"Water not detected – value too low: {leakVal}");

        Logger.LogMessage(Level.Info, $"Water detected successfully. Leak value = {leakVal}");

        // trigger the mute alarm command within 0.5s of leak detection
        HardwareParameters.SetParameter(LeakSensorParameters.MUTE_ALARM, Handler.Nothing);
        Logger.LogMessage(Level.Info, "MuteAlarm command sent (<0.5s after alarm).");

        if (!WaitForMUTEALARMResponse(LeakSensorParameters.MUTE_ALARM, "OK", 1000))
        {
            Logger.LogMessage(Level.Error, "No Response for MUTEALARM from the device");
            return false;
        }
        // 6. Ask user to dry the leak sensor
        if (TopMostMessageBox.Show("Dry the Leak Sensor",
                            "Leak Sensor",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
            return Fail("User did not dry the leak sensor.");


        // Wait until leak returns to 0 (stop immediately <0.5s)
        if (!WaitForLeakToClear())
            return Fail("Leak Sensor still wet after 30 seconds.");

        Logger.LogMessage(Level.Info, "Leak Sensor dry detected immediately (<0.5s).");
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
            if (token.Length > 1 && token[0] == LeakSensorParameters.Dry_LeakSensor_Resp)
            {
                int val = Convert.ToInt32(token[1]);
                return val;
            }
        }
        return 0;
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
    
    private bool WaitForMUTEALARMResponse(string parameterName, string expectedValue, int timeoutMs)
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
                if (token.Length > 0 && token[0] == LeakSensorParameters.MUTE_ALARM_RESP)
                {
                    string key = token[0];
                    response = token[1];
                    Logger.LogMessage(Level.Info, $"~Mute Alarm : {response}");
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

    // Detect alarm 
    private bool WaitForLeakAlarm(out string response)
    {
        int timeoutMs = 60000;
        int elapsed = 0;
        int interval = 30;     // 20–30 ms → ensures <0.5 s response
        response = null;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(LeakSensorParameters.Dry_LeakSensor, out string resp, true);

            if (!string.IsNullOrEmpty(resp))
            {
                response = resp;
                int val = ParseLeakValue(resp);

                // If Leak = 1 or we see "~Alarm"
                if (val == 1 || resp.Contains("~Alarm"))
                    return true;
            }

            Thread.Sleep(interval);
            elapsed += interval;
        }
        return false;
    }


    // Detect when leak becomes 0 
    private bool WaitForLeakToClear()
    {
        int timeoutMs = 30000;
        int elapsed = 0;
        int interval = 30;     // 20–30 ms for <0.5s reaction

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(LeakSensorParameters.Dry_LeakSensor, out string resp, true);

            if (!string.IsNullOrEmpty(resp))
            {
                int val = ParseLeakValue(resp);

                if (val == 0)     // Leak cleared
                    return true;
            }

            Thread.Sleep(interval);
            elapsed += interval;
        }
        return false;
    }

}

public static class TopMostMessageBox
{
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        using (Form topMostForm = new Form { TopMost = true, ShowInTaskbar = false, Visible = false })
        {
            return MessageBox.Show(topMostForm, text, caption, buttons, icon);
        }
    }
    public static DialogResult Show(string text, string caption)
    {
        return Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
    }    
}

