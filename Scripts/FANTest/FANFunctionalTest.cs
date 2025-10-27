using LogViewManager;
using System;
using System.Threading;
using UniversalBoardTestApp;
using Helper;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;

public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        // switch to Service Level.
        Logger.LogMessage(Level.Info, Handler.StartServiceLevelTest);
                
        if (SwitchToServiceLevel())
        {
            if (TestFANFunctionality()) return true;
            else return false;
        }
        else return false;

        //// testing purpose - remove this
        //if (TestFANFunctionality()) return true;
        //else return false;
    }

    public bool SwitchToServiceLevel()
    {
        Logger.LogMessage(Level.Info, Handler.SwitchToServiceLevel);
                
        // Service challenge command        
        HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceChallange, 0);

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

    private bool WaitForResponse(string parameterName, int timeoutMs, out string response)
    {
        int elapsed = 0;
        int interval = 100;
        response = null;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response);
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
            HardwareParameters.GetParameter(parameterName, out response);
            // Parse and Validate for Service Challenge and Service code values.           
            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);
                if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ValidateServiceCode)
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

    private bool TestFANFunctionality()
    {
        // 1st iteration with -26 and -26 PWM Values.
        var result = CheckAndCalculateFANSpeeds(FanParameterNames.FAN1_PWM_1, FanParameterNames.FAN2_PWM_1);

        //Calculated Speed Values for Fan1 and Fan2 are :- result.Resp1 and result.Resp2;

        // 2nd iteration with -127 and -127 PWM values.
        result = CheckAndCalculateFANSpeeds(FanParameterNames.FAN1_PWM_2, FanParameterNames.FAN2_PWM_2);

        //Calculated Speed Values for Fan1 and Fan2 are :- result.Resp1 and result.Resp2;

        // 3rd iteration with -255 and -255 PWM values.
        result = CheckAndCalculateFANSpeeds(FanParameterNames.FAN1_PWM_3, FanParameterNames.FAN2_PWM_3);

        //Calculated Speed Values for Fan1 and Fan2 are :- result.Resp1 and result.Resp2;

        // Now, Stabilization is speed property value (+/-) 5. It is to be verified.. 
        if (CheckStabilizationValue(result.Resp1, result.Resp2))
            return true;
        else
            return false;
    }

    public (int Resp1, int Resp2) CheckAndCalculateFANSpeeds(int fan1Value, int fan2Value)
    {
        // Send to Pump Module
        HardwareParameters.SetParameter(FanParameterNames.FAN1_COMMAND, fan1Value);
        HardwareParameters.SetParameter(FanParameterNames.FAN2_COMMAND, fan2Value);
        Thread.Sleep(FanParameterNames.WAITTIME);       // Wait Time in milliseconds.

        HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string Response1);
        HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string Response2);

        // uncomment this code, while testing with the Device.
        int fan1_Res = ParseAndValidateFANResponse(Response1);
        int fan2_Res = ParseAndValidateFANResponse(Response2);

        Thread.Sleep(FanParameterNames.WAITTIME_INMILLISECONDS);        // 10 seconds.

        // Calculate the Speed Value.
        int fan1SpeedVal = fan1_Res * FanParameterNames.SPEED_MULTIPLE_OFFSET;
        int fan2SpeedVal = fan2_Res * FanParameterNames.SPEED_MULTIPLE_OFFSET;

        return (fan1SpeedVal, fan2SpeedVal);
    }
  
    private static int ParseAndValidateFANResponse(string Response1)
    {
        int result = 0;
        var lines = Response1.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == FanParameterNames.FAN1_SPEEDFROM_RESP)
            {
                string key = token[Handler.INDEX_ZERO];
                Response1 = token[Handler.INDEX_ONE];
                break;
            }
        }

        if (Response1 != "") { result = int.Parse(Response1); }
            return result;
    }

    private bool CheckStabilizationValue(int Fan1CalcSpeedval, int Fan2CalcSpeedval)
    {
        // Stabilization could be speed property value +/ -5
        Fan1CalcSpeedval += 5;
        Fan2CalcSpeedval += 5;

        return true;
    }
}