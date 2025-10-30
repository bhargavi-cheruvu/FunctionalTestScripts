using LogViewManager;
using System;
using System.Threading;
using UniversalBoardTestApp;
using Helper;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;

public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number 
    private static Dictionary<string, int> FanSpeeds = new Dictionary<string, int>();

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
        int interval = 100;
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
                    Logger.LogMessage(Level.Info, $"Response for Service.Code is { token[1]}");
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

        // 2nd iteration with -127 and -127 PWM values.
        result = CheckAndCalculateFANSpeeds(FanParameterNames.FAN1_PWM_2, FanParameterNames.FAN2_PWM_2);

        // 3rd iteration with -255 and -255 PWM values.
        result = CheckAndCalculateFANSpeeds(FanParameterNames.FAN1_PWM_3, FanParameterNames.FAN2_PWM_3);

        if (result.Resp1 == 0 && result.Resp2 == 0)
            return false;

        return true; // need to check this. 
    }

    public (int Resp1, int Resp2) CheckAndCalculateFANSpeeds(int fan1Value, int fan2Value)
    {
        int fan1SpeedVal = 0;
        int fan2SpeedVal = 0;
        // Send to Pump Module
        HardwareParameters.SetParameter(FanParameterNames.FAN1_COMMAND, fan1Value);
        HardwareParameters.SetParameter(FanParameterNames.FAN2_COMMAND, fan2Value);
        Thread.Sleep(FanParameterNames.WAITTIME);               // Wait Time in milliseconds.

        if (fan1Value == FanParameterNames.FAN1_PWM_1 && fan2Value == FanParameterNames.FAN2_PWM_1)
        {
            FanSpeeds.Clear();
            HardwareParameters.GetParameter(FanParameterNames.FAN1_COMMAND, out string content1);
            HardwareParameters.GetParameter(FanParameterNames.FAN2_COMMAND, out string content2);                       
            FanSpeeds = ParseAndValidateFANResponse(content1);
            FanSpeeds = ParseAndValidateFANResponse(content2);

            HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string Response1);
            HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string Response2);
            FanSpeeds = ParseAndValidateFANSpeeds(Response1);
            FanSpeeds = ParseAndValidateFANSpeeds(Response2);
        }
        else if(fan1Value == FanParameterNames.FAN1_PWM_2 && fan2Value == FanParameterNames.FAN2_PWM_2)
        {
            FanSpeeds.Clear();
            HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string Response1);
            HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string Response2);
            FanSpeeds = ParseAndValidateFANSpeeds(Response1);
            FanSpeeds = ParseAndValidateFANSpeeds(Response2);
        }
        else if (fan1Value == FanParameterNames.FAN1_PWM_3 && fan2Value == FanParameterNames.FAN2_PWM_3)
        {
            FanSpeeds.Clear();
            HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string Response1);
            HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string Response2);
            FanSpeeds = ParseAndValidateFANSpeeds(Response1);
            FanSpeeds = ParseAndValidateFANSpeeds(Response2);
        }
        Thread.Sleep(FanParameterNames.WAITTIME_INMILLISECONDS);        // 10 seconds.

        if (FanSpeeds.Count > 0 && FanSpeeds.ContainsKey(FanParameterNames.FAN1_SPEEDFROM_RESP) && FanSpeeds.ContainsKey(FanParameterNames.FAN2_SPEEDFROM_RESP))
        {
            // Calculate the Speed Value. -- check this
            fan1SpeedVal = FanSpeeds[FanParameterNames.FAN1_SPEEDFROM_RESP] * FanParameterNames.SPEED_MULTIPLE_OFFSET;
            fan2SpeedVal = FanSpeeds[FanParameterNames.FAN2_SPEEDFROM_RESP] * FanParameterNames.SPEED_MULTIPLE_OFFSET;
        }
        
        StabilizationFanSpeedValue(fan1SpeedVal+5, fan2SpeedVal+5);           
        return (fan1SpeedVal, fan2SpeedVal);    
    }

    public Dictionary<string, int> ParseAndValidateFANSpeeds(string res)
    {
        FanSpeeds = ExtractResponseBasedonFANSpeed(res);
        return FanSpeeds;
    }

    public Dictionary<string, int> ParseAndValidateFANResponse(string Response)
    {
        FanSpeeds = ExtractResponseBasedonFANNumber(Response, FanParameterNames.FAN1_PWM_1);
        return FanSpeeds;
    }

    private static Dictionary<string, int> ExtractResponseBasedonFANSpeed(string Response)
    {
        string key = string.Empty;
        int Rep = 0;
        if (Response != null)
        {
            var lines = Response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);
           
            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);

                // check for FAN speed Reponse                
                if (token.Length > Handler.INDEX_ZERO && (token[Handler.INDEX_ZERO] == FanParameterNames.FAN1_SPEEDFROM_RESP) ||
                    (token[Handler.INDEX_ZERO] == FanParameterNames.FAN2_SPEEDFROM_RESP))
                {
                    key = token[Handler.INDEX_ZERO];
                    Response = token[Handler.INDEX_ONE];
                    Rep = Convert.ToInt32(Response);
                    if (!FanSpeeds.ContainsKey(key))
                    {
                        FanSpeeds.Add(key, Rep);
                        Logger.LogMessage(Level.Info, $"Response for {key} is { Rep}");
                    }
                }
            }
            return FanSpeeds;
        }
        return null;
    }
    
    private static Dictionary<string,int> ExtractResponseBasedonFANNumber(string Response, int expectedValue)
    {
        string key = string.Empty;
        int Rep = 0;
        if (Response != null)
        {
            var lines = Response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);

                // check for FAN1 / FAN2 Reponse                
                if (token.Length > Handler.INDEX_ZERO && ((token[Handler.INDEX_ZERO] == FanParameterNames.FAN1_COMMAND_RESP) ||
                (token[Handler.INDEX_ZERO] == FanParameterNames.FAN2_COMMAND_RESP)))
                {
                    key = token[Handler.INDEX_ZERO];
                    Response = token[Handler.INDEX_ONE];
                    Rep = Convert.ToInt32(Response);

                    if (Rep == expectedValue)
                    {
                        if (!FanSpeeds.ContainsKey(key))
                        {
                            FanSpeeds.Add(key, Rep);
                            Logger.LogMessage(Level.Info, $"Response for {key} is {Rep}");
                        }
                    }
                }
            }
            return FanSpeeds;
        }
        return null;
    }

    private static void StabilizationFanSpeedValue(int Fan1CalSpeedVal, int Fan2CalSpeedVal)
    {
        Fan1CalSpeedVal += 5;
        Fan2CalSpeedVal += 5;
    }
}