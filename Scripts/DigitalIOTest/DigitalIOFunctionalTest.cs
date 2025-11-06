using LogViewManager;
using Helper;
using System;
using System.Collections.Generic;
using System.Threading;
using UniversalBoardTestApp;

public class Test
{
    // Version of the script. Gets displayed in database/protocol
    public const string TestVersion = Handler.TEST_VERSION; // Version Number
    public ResponsefromDevice response = new ResponsefromDevice();
    public string OriginalResponseFromDevice = null;

    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, RequestToDevice.DigiIOTest);
        Thread.Sleep(RequestToDevice.RelayTestDelayMilliseconds);  // 50 millisecs

        // Initialize all relays to 0
        SetAllRelays(RequestToDevice.InitialRelayValue);
        Thread.Sleep(RequestToDevice.RelayResetDelayMilliseconds);      // 1000 millisecs 

        if (!TestRelayCycle(HardwareParameterNames.Relay1State)) return false;
        if (!TestRelayCycle(HardwareParameterNames.Relay2State)) return false;
        if (!TestRelayCycle(HardwareParameterNames.Relay3State)) return false;
        if (!TestRelayCycle(HardwareParameterNames.Relay4State)) return false;

        return true;
    }

    private void SetAllRelays(int state)
    {
        string response;
        HardwareParameters.SetParameter(HardwareParameterNames.Relay1State, state);
        HardwareParameters.GetParameter(HardwareParameterNames.Relay1State, out response, true);

        HardwareParameters.SetParameter(HardwareParameterNames.Relay2State, state);
        HardwareParameters.GetParameter(HardwareParameterNames.Relay2State, out response, true);

        HardwareParameters.SetParameter(HardwareParameterNames.Relay3State, state);
        HardwareParameters.GetParameter(HardwareParameterNames.Relay3State, out response, true);

        HardwareParameters.SetParameter(HardwareParameterNames.Relay4State, state);
        HardwareParameters.GetParameter(HardwareParameterNames.Relay4State, out response, true);


        //check the initial relay responses.
        if (!TryGetParameter(HardwareParameterNames.Relay1State, out string Val)) return;

        if (Val.Length >= 1)
            OriginalResponseFromDevice = Val;
        string[] reponseString = RetreiveReponseBasedOnSetOrReset(HardwareParameterNames.InitialState, 0);
        if (!ValidateParsedResponse(HardwareParameterNames.InitialState, reponseString))
            return;
    }
  
    private bool TryGetParameter(string parameterName, out string value)
    {
        HardwareParameters.GetParameter(parameterName, out value, true);
        if (!string.IsNullOrEmpty(value))
        {          
            return true;
        }

        Logger.LogMessage(Level.Error, Handler.MISSING_ADAPTER);
        return false;
    }

    private bool TestRelayCycle(string relayParameter)
    {
        HardwareParameters.SetParameter(relayParameter, 1);
        if (!TryGetParameter(relayParameter, out string Val)) return false;

        if (Val.Length >= 1)
            OriginalResponseFromDevice = Val;
        string[] reponseString = RetreiveReponseBasedOnSetOrReset(relayParameter, 1);
        if (!ValidateParsedResponse(relayParameter, reponseString))
            return false;

        HardwareParameters.SetParameter(relayParameter, 0);
        if (!TryGetParameter(relayParameter, out string finalValue)) return false;

        if (finalValue.Length >= 1)
            OriginalResponseFromDevice = finalValue;
        reponseString = RetreiveReponseBasedOnSetOrReset(relayParameter, 0);
        if (!ValidateParsedResponse(relayParameter, reponseString))
            return false;       

        return true;
    }

    private string[] RetreiveReponseBasedOnSetOrReset(string relayParameter, int SetOrReset)
    {
        string[] reponseString = new string[ResponsefromDevice.SIZE];
        switch (relayParameter)
        {
            case HardwareParameterNames.InitialState:
                reponseString = response.InitialRelayResponse;
                break;
            case HardwareParameterNames.Relay1State:
                if (SetOrReset != ResponsefromDevice.ZERO)
                    reponseString = response.relay1ExpectedRespforSetState;
                else                    
                    reponseString = response.relay1ExpectedRespforReSetState;
                break;
            case HardwareParameterNames.Relay2State:
                if (SetOrReset != ResponsefromDevice.ZERO)
                    reponseString = response.relay2ExpectedRespforSetState;
                else
                    reponseString = response.relay2ExpectedRespforReSetState;
                break;
            case HardwareParameterNames.Relay3State:
                if (SetOrReset != ResponsefromDevice.ZERO)
                    reponseString = response.relay3ExpectedRespforSetState;
                else
                    reponseString = response.relay3ExpectedRespforReSetState;
                break;
            case HardwareParameterNames.Relay4State:
                if (SetOrReset != ResponsefromDevice.ZERO)
                    reponseString = response.relay4ExpectedRespforSetState;
                else
                    reponseString = response.relay4ExpectedRespforReSetState;
                break;
            default:                
                break;
        }

        return reponseString;
    }

    private bool ValidateParsedResponse(string actualResponse, string[] expectedEntries)
    {
        Dictionary<string, string> parsed = ParseResponse(OriginalResponseFromDevice);
        bool isValid = true;

        foreach (string expected in expectedEntries)
        {
            var parts = expected.Split(Handler.DELIMITER);
            if (parts.Length != Handler.TOKEN_LENGTH)
            {                
                continue;
            }

            string key = parts[0].Trim();
            string expectedValue = parts[1].Trim();

            if (parsed.ContainsKey(key))
            {
                if (parsed[key].Equals(expectedValue))
                {
                    Logger.LogMessage(Level.Info, $"Response from device is {key}: {expectedValue}");
                    isValid = true; continue;                    
                }               
                else if (!parsed[key].Equals(expectedValue))
                {
                    Logger.LogMessage(Level.Error, Handler.MISSING_ADAPTER);
                    isValid = false;
                    continue;
                }
            }
        }

        if (isValid)
            Logger.LogMessage(Level.Success, Handler.VALIDATION_SUCCESSFUL);
        else
            Logger.LogMessage(Level.Error, Handler.VALIDATION_FAILED);

        return isValid;
    }

    private Dictionary<string, string> ParseResponse(string response)
    {
        var result = new Dictionary<string, string>();
        var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length == Handler.TOKEN_LENGTH)
            {
                string key = token[0].Trim();
                string value = token[1].Trim();
                result[key] = value;
            }
        }
        return result;
    }
}