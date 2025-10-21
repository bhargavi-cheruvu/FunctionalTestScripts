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
            if (TestDegasserFunctionality()) return true;
            else return false;
        }
        else return false;
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

    private bool TestDegasserFunctionality()
    {        
        int presureVal = CheckAndCalculateDegasserPressure();

        // Now, Stabilization is speed property value (+/-) 0.5. It is to be verified.. 
        if (CheckStabilizationPressureValue(presureVal))
            return true;
        else
            return false;
    }

    public int CheckAndCalculateDegasserPressure()
    {
        // Send to Pump Module
        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_CMD, Handler.INDEX_ONE);
        Thread.Sleep(DegasserParameterNames.WAITTIME_THREE_MINS); //wait for atleast 2 min's
        
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string response1);

        Thread.Sleep(DegasserParameterNames.WAITTIME_ONE_MIN); // monitor for 1 min

        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string response2);
        Thread.Sleep(DegasserParameterNames.WAITTIME_THIRTY_SECONDS); // outputs over 30 seconds.

        var reply = ParseAndValidateDegasserResponse(response1, response2);

        // Read out external pressure sensor
        // <external_value>
        int externalValue = reply.externalVal;// needs to be read from barometer
        int Value = reply.Val;

        // Calculate <offset> = <external_value> - <value>
        int Offset = externalValue - Value;   // Resolution: 0.01 mbar

        // Set the Degasser.Pressure.Offset=<offset>
        //Degasser.Pressure.Offset
        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, Offset);

        // Get the Degasser.Pressure.Offset=<offset>
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, out string pressureOffset);

        // Get the ~Degasser.Pressure=<value>
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string pressureVal);

        // Need to check <external_value> is within limits

        //Check <value> against range 8000..15000 cts.

        //Stabilization: pressure +/- 0.5 mbar (to be verified)
        // Limits: pressure +/ -0.5 mbar(to be verified)

        // Convert pressureVal to integer and Stabilize
        int pVal = Convert.ToInt32(pressureVal);

        return pVal;
        //return (externalValue, Value);
    }
    public (int externalVal, int Val) ParseAndValidateDegasserResponse(string Response1, string Response2)
    {
        Response1 = ExtractDegasserPressureVal(Response1, DegasserParameterNames.DEGASSER_PRESSURE); // need to check
        Response2 = ExtractDegasserPressureVal(Response2, DegasserParameterNames.DEGASSER_PRESSURE);

        if (Response1 == string.Empty || Response2 == string.Empty)
            return (0, 0);

        int eValue = Convert.ToInt32(Response1);
        int Value = Convert.ToInt32(Response2);
        return (eValue, Value);
    }

    private static string ExtractDegasserPressureVal(string Response, string expectedValue)
    {
        if (Response != null)     
        {
            var lines = Response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                var token = line.Split(Handler.DELIMITER);

                // check for Degasser pressure Reponse                
                if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == DegasserParameterNames.DEGASSER_PRESSURE) //need to check
                {
                    string key = token[Handler.INDEX_ZERO];
                    Response = token[Handler.INDEX_ONE];

                    if (!string.IsNullOrEmpty(Response) && Response == expectedValue) return Response;
                }
            }
        }        
        return string.Empty;
    }

    private bool CheckStabilizationPressureValue(double degasserPressureVal)
    {
        // Stabilization could be speed property value +/- 0.5
        degasserPressureVal += 0.5;
      
        return true;
    }
}