using LogViewManager;
using System;
using System.Threading;
using UniversalBoardTestApp;
using Helper;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
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
        Logger.LogMessage(Level.Info, LeakSensorParameters.LEAKSENSOR_TEST);
        string MuteAlarmState = null;

        // LeakSensor Calibrate command        
        HardwareParameters.SetParameter(LeakSensorParameters.LEAKSENSOR_CALIBRATE, Handler.Nothing);

        // Wait for response ~LeakSensor.Calibrate
        if (!WaitForResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE, LeakSensorParameters.TimeInterval, out string calibrateValue))     
        {
            Logger.LogMessage(Level.Error, LeakSensorParameters.NoResponseFromDevice);
            return false;
        }

        // ~LeakSensor.CalibOffset? command
        HardwareParameters.SetParameter(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET, Handler.REQUEST);

        // Wait for expected response: ~LeakSensor.CalibOffset
        if (!WaitForResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET, LeakSensorParameters.TimeInterval, out string calibrateOffsetVal))
        {
            return false;
        }
   
        // apply xx ml water to the leak sensor.
        if (MessageBox.Show($"Apply 20 ml  to the Leak Sensor", "Leak Sensor", MessageBoxButtons.OKCancel) == DialogResult.OK)
        {
            Thread.Sleep(LeakSensorParameters.ReactionTime_AfterApplyingWater);

            // check for the Response, if it is null, then throw/log error.
            if (WaitForResponse(LeakSensorParameters.LEAKSENSOR_CALIBRATE_OFFSET, 60 * 1000, out string reply)) 
            {
                Logger.LogMessage(Level.Info, reply);                
            }
            else
            {
                //log the error.
                Logger.LogMessage(Level.Error, "Leak detected after 180 seconds");
                return false;
            }
        }
        
        // Send MuteAlarm
        HardwareParameters.SetParameter(LeakSensorParameters.MUTE_ALARM, Handler.Nothing);
        HardwareParameters.GetParameter(LeakSensorParameters.MUTE_ALARM, out MuteAlarmState, true);

        if (MuteAlarmState.Length > 0)
        {
            var lines = MuteAlarmState.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == LeakSensorParameters.MUTE_ALARM_RESP)
                {
                    string key = token[0];
                    MuteAlarmState = token[1];
                    break;
                }
            }

            if (MuteAlarmState == LeakSensorParameters.ExpectedResult)
            {
                Logger.LogMessage(Level.Info, $"MuteAlarm is OK");
            }
        }

        if(MessageBox.Show($"Dry the Leak Sensor", "Leak Sensor", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            //~Leak=0
            if(!WaitForExpectedResponse(LeakSensorParameters.Dry_LeakSensor, 
                LeakSensorParameters.Dry_LeakSensor_ExpectedResp,
                LeakSensorParameters.TimeInterval_DryLeakSensor))
            {
                return false;
            }
            else return true;
        }

        return true;
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
}