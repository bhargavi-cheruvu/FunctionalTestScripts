using Helper;
using LEDBarTest;
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
    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.LEDBarTest);
        SetLEDBarForeColor();
        return true;
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
                case LEDSTATUSCOLOR.DELETE: // if LEDBar.ForceColor = DELETE and Value is 0.               
                    break;
                default: break;
            }
        }

        if (IsLEDBarMode)
        {
            if (MessageBox.Show($"Complete LEDBar is {ledStatusColor}", Handler.LED_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                result++;
                if(result > 5) //reset to 0
                {
                    if (MessageBox.Show(Handler.DELETE_LEDCOLORS, Handler.LED_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        result = 0;
                        if (MessageBox.Show(Handler.CHECK_BEEP_SOUND, Handler.KEYPAD_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            // Call the KeyPad Form.
                            IsLEDBarMode = false;
                            KeyPadForm frm = new KeyPadForm();
                            frm.ShowDialog();
                        }
                    }
                    else return;
                }
                SetLEDBarForeColor();
            }
        }
        else return;
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
        else
            Logger.LogMessage(Level.Success, "Received expected response.");

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
}