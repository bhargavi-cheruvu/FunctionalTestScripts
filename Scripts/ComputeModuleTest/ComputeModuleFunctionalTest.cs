using Helper;
using LogViewManager;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using UniversalBoardTestApp;
public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.ComputeModuleTest);

        if(ComputeModuleResponse())
        {
            return true;
        }
        return false;
    }

    public bool ComputeModuleResponse()
    {
        bool b = HardwareParameters.SetParameter("TestDCFAlignment", "");
        bool b1 = HardwareParameters.GetParameter("TestDCFAlignment", out string resp1);

        if (b1)
        {
            return true;
        }
        return false;
    }

    //public bool SwitchToServiceLevel()
    //{
    //    Logger.LogMessage(Level.Info, Handler.SwitchToServiceLevel);
    //    string lockState = null;
        
    //    // Service challenge command        
    //    HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceChallange, ServiceLevelParameterNames.ServiceChallangeVal);

    //    // Wait for response ~Service.Challenge=<value>
    //    if (!WaitForResponse(ServiceLevelParameterNames.ServiceChallange, ServiceLevelParameterNames.TimeInterval, out string challengeValue))
    //    {
    //        Logger.LogMessage(Level.Error, ServiceLevelParameterNames.NoResponseFromDevice);
    //        return false;
    //    }

    //    // service code command     
    //    HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceCodeRequest, ServiceLevelParameterNames.ServiceCode);
               
    //    // Wait for expected response: ~Service.Code=1
    //    if (!WaitForExpectedResponse(ServiceLevelParameterNames.ServiceCodeRequest, ServiceLevelParameterNames.ExpectedServiceCode, ServiceLevelParameterNames.TimeInterval))
    //    {
    //        Logger.LogMessage(Level.Error, ServiceLevelParameterNames.ServiceCodeFailure);
    //        return false;
    //    }
    //    else
    //        Logger.LogMessage(Level.Success, ServiceLevelParameterNames.ServiceCodeSuccess);

    //    // Switch to User Mode from ServiceLevel
    //    HardwareParameters.SetParameter(ServiceLevelParameterNames.ServiceLevelUserMode, Handler.Nothing);
    //    HardwareParameters.GetParameter(ServiceLevelParameterNames.ServiceLevelUserMode, out lockState);

    //    if(lockState.Length > 0)
    //    {
    //        var lines = lockState.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

    //        foreach (string line in lines)
    //        {
    //            var token = line.Split(Handler.DELIMITER);
    //            if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ServiceLock_Response)
    //            {
    //                string key = token[0];
    //                lockState = token[1];
    //                Logger.LogMessage(Level.Info, $"Response for Service.Lock is {token[1]}");
    //                break;
    //            }
    //        }

    //        if (lockState == ServiceLevelParameterNames.ServiceLockResult)
    //        {
    //            Logger.LogMessage(Level.Info, ServiceLevelParameterNames.UserMode);
    //        }
    //    }

    //    return true;
    //}

    //private bool WaitForResponse(string parameterName, int timeoutMs, out string response)
    //{
    //    int elapsed = 0;
    //    int interval = 100;
    //    response = null;

    //    while (elapsed < timeoutMs)
    //    {
    //        HardwareParameters.GetParameter(parameterName, out response);
    //        if (!string.IsNullOrEmpty(response))
    //        {
    //            // Parse and Validate for Service Challenge and Service code values.           
    //            var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

    //            foreach (string line in lines)
    //            {
    //                var token = line.Split(Handler.DELIMITER);
    //                if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ServiceChallange_Resp)
    //                {
    //                    string key = token[0];
    //                    response = token[1];
    //                    Logger.LogMessage(Level.Info, $"Response for Service.Challenge is {token[1]}");
    //                    break;
    //                }
    //            }
    //            return true;
    //        }
    //        Thread.Sleep(interval);
    //        elapsed += interval;
    //    }

    //    response = null;
    //    return false;
    //}  

    //private bool WaitForExpectedResponse(string parameterName, string expectedValue, int timeoutMs)
    //{
    //    int elapsed = 0;
    //    int interval = 100;
    //    string response;

    //    while (elapsed < timeoutMs)
    //    {
    //        HardwareParameters.GetParameter(parameterName, out response);
    //        // Parse and Validate for Service Challenge and Service code values.           
    //        var lines = response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

    //        foreach (string line in lines)
    //        {
    //            var token = line.Split(Handler.DELIMITER);
    //            if (token.Length > 0 && token[0] == ServiceLevelParameterNames.ValidateServiceCode)
    //            {
    //                string key = token[0];
    //                response = token[1];
    //                Logger.LogMessage(Level.Info, $"Response for Service.Code is { token[1]}");
    //                break;
    //            }
    //        }
                
    //        if (!string.IsNullOrEmpty(response) && response == expectedValue)
    //            return true;

    //        Thread.Sleep(interval);
    //        elapsed += interval;
    //    }

    //    return false;
    //}    
}