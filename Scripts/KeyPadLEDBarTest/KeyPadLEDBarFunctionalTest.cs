using Helper;
//using KeyPadLEDBarTest;
using LogViewManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using UniversalBoardTestApp;
public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    private int result = 1;
    private bool IsLEDBarMode = true;
    private string responseVal = null;

    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.LEDBarTest);
        SetLEDBarForeColor();

        if (KeyPadActivity()) return true;
        else return false;
    }
    private void SetLEDBarForeColor()
    {
        LEDSTATUSCOLOR ledStatusColor = new LEDSTATUSCOLOR();
        string response = UpdateLEDValue();
        //extract LEDBar color from the response.
        var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == Handler.LEDBAR_RESP_CMD)
            {
                string key = token[Handler.INDEX_ZERO];
                response = token[Handler.INDEX_ONE];
                Logger.LogMessage(Level.Info, $"Response for LEDBar.ForceColor is {token[Handler.INDEX_ONE]}");
                break;
            }
        }

        if (response.Length > 0)
        {
            result = int.Parse(response);
            ledStatusColor = (LEDSTATUSCOLOR)result;

            switch (ledStatusColor)
            {
                case LEDSTATUSCOLOR.RED:
                case LEDSTATUSCOLOR.GREEN:
                case LEDSTATUSCOLOR.BLUE:
                case LEDSTATUSCOLOR.YELLOW:
                    //result++;
                    break;
                case LEDSTATUSCOLOR.OFF: // if LEDBar.ForceColor = OFF and value is 5.
                                         //result = 0; break;
                    break;
                //case LEDSTATUSCOLOR.DELETE: // if LEDBar.ForceColor = DELETE and Value is 0.               
                //    break;
                default: break;
            }
        }

        if (IsLEDBarMode)
        {
            if (MessageBox.Show($"Complete LEDBar is {ledStatusColor}", Handler.LED_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                result++;
                if (result > 5) //reset to 0
                {
                    result = 0;
                    IsLEDBarMode = false;                    
                }
                if (IsLEDBarMode) { SetLEDBarForeColor(); }
            }
        }
        else return;
    }

    private bool KeyPadActivity()
    {
        // Press MUTE ALARM Button
        if (MessageBox.Show(Handler.PRESS_MUTEALARM_BUTTON, Handler.KEYPAD_CAPTION, MessageBoxButtons.OKCancel) == DialogResult.OK)
        {
            // KeyPad functionality.
            IsLEDBarMode = false;

            HardwareParameters.SetParameter(Handler.KEYPAD_TESTMODE, 0);
            HardwareParameters.GetParameter(Handler.KEYPAD_TESTMODE, out responseVal);

            // ASK TO CLICK ON MUTEALARM.
            if (WaitForKeyPadExpectedResponse(Handler.RESPONSE_KEYPROPERTY, "1", 3000))
            {
                // After MUTE ALARM Button is pressed.
                // Ask the user to proceed with SELECT Button to be pressed.
                if (MessageBox.Show(Handler.PRESS_SELECT_BUTTON, Handler.KEYPAD_CAPTION, MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    HardwareParameters.GetParameter(Handler.KEYPAD_TESTMODE, out responseVal);

                    // POLL for 3 seconds continously, so that we receive Keys = 2 for SELECT BUTTON.
                    if (WaitForKeyPadExpectedResponse(Handler.RESPONSE_KEYPROPERTY, "2", 3000))
                    {
                        // After SELECT Button is pressed.
                        // Ask the user to proceed with DOCK Button to be pressed.
                        if (MessageBox.Show(Handler.PRESS_DOCK_BUTTON, Handler.KEYPAD_CAPTION, MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            HardwareParameters.GetParameter(Handler.KEYPAD_TESTMODE, out responseVal);

                            // POLL for 3 seconds continously, so that we receive Keys = 4 for DOCK BUTTON.
                            if (WaitForKeyPadExpectedResponse(Handler.RESPONSE_KEYPROPERTY, "4", 3000))
                            {
                                // After DOCK Button is pressed.
                                // Ask the user to proceed with PURGE Button to be pressed.
                                if (MessageBox.Show(Handler.PRESS_PURGE_BUTTON, Handler.KEYPAD_CAPTION, MessageBoxButtons.OKCancel) == DialogResult.OK)
                                {
                                    HardwareParameters.GetParameter(Handler.KEYPAD_TESTMODE, out responseVal);

                                    // POLL for 3 seconds continously, so that we receive Keys = 8 for PURGE BUTTON.
                                    if (WaitForKeyPadExpectedResponse(Handler.RESPONSE_KEYPROPERTY, "8", 3000))
                                    {
                                        // After PURGE Button is pressed.
                                        // Ask the user to proceed with FLOW Button to be pressed.
                                        if (MessageBox.Show(Handler.PRESS_FLOW_BUTTON, Handler.KEYPAD_CAPTION, MessageBoxButtons.OKCancel) == DialogResult.OK)
                                        {
                                            HardwareParameters.GetParameter(Handler.KEYPAD_TESTMODE, out responseVal);

                                            // POLL for 3 seconds continously, so that we receive Keys = 8 for PURGE BUTTON.
                                            if (WaitForKeyPadExpectedResponse(Handler.RESPONSE_KEYPROPERTY, "16", 3000))
                                            {
                                                return true;
                                            }
                                        }
                                        else { return false; }
                                    }
                                }
                                else return false;
                            }
                        }
                        else return false;
                    }
                }
                else return false;
            }
        }
        return false;
    }

    private string UpdateLEDValue()
    {
        string response;
        HardwareParameters.SetParameter(Handler.LEDBAR_REQ_CMD, result);
        response = result.ToString();
        // HardwareParameters.GetParameter(Handler.LEDBAR_REQ_CMD, out response);
        if (!WaitForExpectedResponse(Handler.LEDBAR_REQ_CMD, response, 3000))
        {
            Logger.LogMessage(Level.Error, "Unexpected Response");
            return "";
        }

        return response;
    }

    private bool WaitForExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    {
        int elapsed = 0;
        int interval = 300;
        string response;
        
        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response);

            // Parse and Validate for Service Challenge and Service code values.           
            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == Handler.LEDBAR_RESP_CMD && token[1] == result.ToString())
                {
                    string key = token[0];
                    response = token[1];
                    break;
                }              
            }

            if (!string.IsNullOrEmpty(response) && response == expectedValue)
            {
                Logger.LogMessage(Level.Info, $"Response for LEDBar.ForceColor is {response}");
                return true;
            }

            Thread.Sleep(interval);
            elapsed += interval;
        }

        return false;
    }

    private bool WaitForKeyPadExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    {
        int elapsed = 0;
        int interval = 300;
        string response;
        int rVal = -1;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response);
            // Parse and Validate for Service Challenge and Service code values.           
            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);                
                if (token.Length > 0 && token[0] == Handler.KEYS_RESP_CMD)
                {
                    string k = token[0];
                    response = token[1];
                    rVal = Convert.ToInt32(token[1]);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(response) && rVal == Convert.ToInt32(expectedValue))
            {
                Logger.LogMessage(Level.Info, $"Response for KeyPad.Keys is {rVal}");
                return true;
            }

            Thread.Sleep(interval);
            elapsed += interval;
        }

        return false;
    }
}