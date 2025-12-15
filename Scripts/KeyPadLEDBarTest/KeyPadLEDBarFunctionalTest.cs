using Helper;
using KeyPadLEDBarTest;
using LogViewManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    private CustomMessageBox customMessageBox = null;

    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.LEDBarTest);
        if (SetLEDBarForeColor())
        {
            if (SwitchToServiceLevel())
            {
                if (KeyPadFunctionality())
                {
                    SwitchToUserLevel();
                    return true;
                }
                return false;
            }
            else
            {
                SwitchToUserLevel();
                return false;
            }
        }
        else { return false; }
    }

    private bool SetLEDBarForeColor()
    {
        LEDSTATUSCOLOR ledStatusColor = new LEDSTATUSCOLOR();
        string response = UpdateLEDValue();

        // Extract LEDBar color
        var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == Handler.LEDBAR_RESP_CMD)
            {
                response = token[Handler.INDEX_ONE];
                Logger.LogMessage(Level.Info, $"Response for LEDBar.ForceColor is {token[Handler.INDEX_ONE]}");
                break;
            }
        }

        if (response.Length > 0)
        {
            result = int.Parse(response);
            ledStatusColor = (LEDSTATUSCOLOR)result;
        }

        // If LEDBar mode is not active, fail
        if (!IsLEDBarMode)
            return false;

        // Use result as index i
        int i = result;

        // Ask the confirmation question
        bool confirmed = HandleLEDBarMode(ref result, ref IsLEDBarMode, ledStatusColor.ToString());

        if (!confirmed)
            return false;   // user selected "No"

        // If still in LED mode, continue with next step
        if (IsLEDBarMode)
            return SetLEDBarForeColor();   // recursion allowed because result increments

        return true; // finished LED cycle successfully
    }

    private bool HandleLEDBarMode(ref int result, ref bool IsLEDBarMode, string ledStatusColor)
    {
        if (!IsLEDBarMode)
            return false;

        string msg;

        bool requiresStatusCheck = (result == 1 || result == 2 || result == 5);

        if (requiresStatusCheck)
        {
            msg = $"Complete LEDBar is {ledStatusColor} and the STATUS LED lights are {ledStatusColor} ? \nPlease Confirm.";
        }
        else
        {
            msg = $"Complete LEDBar is set to {ledStatusColor} ? ";
        }

        if (MessageBox.Show(msg, Handler.LED_CAPTION, MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            result++;

            if (result > 5)
            {
                result = 0;
                IsLEDBarMode = false;
            }

            return true;
        }

        return false;
    }

    public bool SwitchToServiceLevel()
    {
        Logger.LogMessage(Level.Info, Handler.SwitchToServiceLevel);

        // Service challenge command        
        HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceChallange, ServiceLevelParameterNames.ServiceChallangeVal);

        // Wait for response ~Service.Challenge=<value>
        if (!WaitForResponse(ServiceLevelParameterNames.ServiceChallange, ServiceLevelParameterNames.TimeInterval, out string challengeValue))
        {
            Logger.LogMessage(Level.Error, ServiceLevelParameterNames.NoResponseFromDevice);
            return false;
        }

        // service code command
        HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceCodeRequest, ServiceLevelParameterNames.ServiceCode);

        // Wait for expected response: ~Service.Code=1
        if (!WaitForExpectedResponse(ServiceLevelParameterNames.ServiceCodeRequest, ServiceLevelParameterNames.ExpectedServiceCode, ServiceLevelParameterNames.TimeInterval))
        {
            Logger.LogMessage(Level.Error, ServiceLevelParameterNames.ServiceCodeFailure);
            return false;
        }
        else
            Logger.LogMessage(Level.Success, ServiceLevelParameterNames.ServiceCodeSuccess);

        return true;
    }
    private bool SwitchToUserLevel()
    {
        string lockState = null;

        // Switch to User Mode from ServiceLevel
        HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceLevelUserMode, Handler.Nothing);
        HardwareParameters.GetParameter(ServiceLevelParameterNames.ServiceLevelUserMode, out lockState, true);

        if (lockState.Length > 0)
        {
            var lines = lockState.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ServiceLock_Response)
                {
                    string key = token[0];
                    lockState = token[1];
                    Logger.LogMessage(Level.Info, $"Response for Service.Lock is {token[1]}");
                    break;
                }
            }

            if (lockState == ServiceLevelParameterNames.ServiceLockResult)
            {
                Logger.LogMessage(Level.Info, ServiceLevelParameterNames.UserMode);
                return true;
            }
        }
        return false;
    }   

    private List<int> storedKeypadPresses = new List<int>();   // Store key press and release values
       
    private bool KeyPadFunctionality()
    {
        IsLEDBarMode = false;
        HardwareParameters.SetParameter(Handler.KEYPAD_TESTMODE, 1);

        int[] expectedKeys = new int[] { 1, 2, 4, 8, 16 };

        string[] messages = new string[]
        {
        Handler.PRESS_MUTEALARM_BUTTON,
        Handler.PRESS_SELECT_BUTTON,
        Handler.PRESS_DOCK_BUTTON,
        Handler.PRESS_PURGE_BUTTON,
        Handler.PRESS_FLOW_BUTTON
        };

        HashSet<int> capturedKeys = new HashSet<int>();

        Logger.LogMessage(Level.Info, "Starting KeyPadFunctionality test...");

        for (int i = 0; i < expectedKeys.Length; i++)
        {
            Thread.Sleep(500); // Safety delay between buttons

            CustomMessageBox box = new CustomMessageBox(
                Handler.KEYPAD_CAPTION,
                messages[i],
                new string[] { "Yes", "No" });

            box.ShowDialog();

            if (box.SelectedButton != "Yes")
            {
                Logger.LogMessage(Level.Warning,
                    $"User selected NO for: {messages[i]}. Test FAILED.");
                return false;
            }

            //----------------------------------------------------
            // Capture ALL hardware events from the messagebox
            //----------------------------------------------------
            var capturedEvents = box.CapturedValues;

            Logger.LogMessage(Level.Info,
                $"Captured events for '{messages[i]}': {string.Join(",", capturedEvents)}");

            if (capturedEvents.Count == 0)
            {
                Logger.LogMessage(Level.Error,
                    "No hardware key events captured. Test FAILED.");
                return false;
            }

            //----------------------------------------------------
            // PRESS = first non-zero value
            //----------------------------------------------------
            int pressedValue = capturedEvents.FirstOrDefault(v => v > 0);

            if (pressedValue <= 0)
            {
                Logger.LogMessage(Level.Error,
                    $"No valid PRESS detected. Expected {expectedKeys[i]}. Test FAILED.");
                return false;
            }

            //----------------------------------------------------
            // RELEASE = last zero value
            //----------------------------------------------------
            int releaseValue = capturedEvents.LastOrDefault(v => v == 0);

            if (releaseValue != 0)
            {
                Logger.LogMessage(Level.Error,
                    $"Key RELEASE was {releaseValue}. Expected 0. Test FAILED.");
                return false;
            }

            //----------------------------------------------------
            // Validate PRESS
            //----------------------------------------------------
            if (pressedValue != expectedKeys[i])
            {
                Logger.LogMessage(Level.Error,
                    $"Incorrect key press! Expected {expectedKeys[i]} but received {pressedValue}. Test FAILED.");
                return false;
            }

            capturedKeys.Add(pressedValue);

            // Store all raw events if needed for later analysis
            storedKeypadPresses.AddRange(capturedEvents);

            Logger.LogMessage(Level.Info,
                $"Correct PRESS {pressedValue} and RELEASE 0 detected.");
        }

        //--------------------------------------------------------
        // Validate all keys were captured at least once
        //--------------------------------------------------------
        bool allKeysCaptured = AllExpectedKeysCaptured(expectedKeys, capturedKeys);

        if (allKeysCaptured)
        {
            Logger.LogMessage(Level.Info,
                "All expected keys captured successfully. KeyPadFunctionality PASSED.");
        }
        else
        {
            Logger.LogMessage(Level.Error,
                "NOT all expected keys were captured. KeyPadFunctionality FAILED.");
        }

        return allKeysCaptured;
    }

    private bool AllExpectedKeysCaptured(int[] expected, HashSet<int> captured)
    {
        foreach (int key in expected)
        {
            if (!captured.Contains(key))
                return false;
        }
        return true;
    }

    private int GetHardwareKeyRelease()
    {
        bool value = HardwareParameters.GetParameter(Handler.RESPONSE_KEYPROPERTY, out double val);
        //// Logger.LogMessage(Level.Info, $"Raw hardware key release value: {value}");
        return Convert.ToInt32(val);
    }

    private string UpdateLEDValue()
    {
        string response;
        HardwareParameters.SetParameter(Handler.LEDBAR_REQ_CMD, result);
        response = result.ToString();
       
        if (!WaitForLEDExpectedResponse(Handler.LEDBAR_REQ_CMD, response, 3000))
        {
            Logger.LogMessage(Level.Error, "Unexpected Response");
            return "";
        }

        return response;
    }

    private bool WaitForLEDExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    {
        int elapsed = 0;
        int interval = 300;
        string response;
        
        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);

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
        int interval = 100;
        string response;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);
            // Parse and Validate for Service Challenge and Service code values.           
            if (!string.IsNullOrEmpty(response))
            {
                var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    var token = line.Split(Handler.DELIMITER);
                    if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ValidateServiceCode)
                    {
                        response = token.Length > 1 ? token[1] : null;
                        if (response == expectedValue)
                        {
                            Logger.LogMessage(Level.Info, $"Response for Service.Code is {response}");
                            break;
                        }
                    }
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