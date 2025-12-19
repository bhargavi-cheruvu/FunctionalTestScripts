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
    private static int fanPWMVal = 47;
    private static int lowerFANSpeedLimit;
    private static int upperFANSpeedLimit;
    private static bool IsSecondFANExists = false;

    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        // switch to Service Level.
        Logger.LogMessage(Level.Info, Handler.StartServiceLevelTest);
        bool result = false;

        if (SwitchToServiceLevel())
        {
            result = TestFANFunctionality();
            //for (int i = 0; i < 3; i++)
            //{
            //    // Create entry for database/protocol 
            //    TestTable backuptable = new TestTable(FanParameterNames.FAN_TESTDETAIL, new string[] { Handler.TABLE_ID, FanParameterNames.FAN_TESTDETAIL });
            //    if (result)
            //    {
            //        // Create new Row for BackupTable
            //        backuptable.AddRow((Handler.TABLE_ID, 1), (FanParameterNames.FAN_TESTDETAIL, result));

            //        if (ScriptHelper.CheckIfProcedureIsCancelled()) { return false; }
            //        Thread.Sleep(500);
            //    }
            //}
        }

        SwitchToUserLevel();
        return result;
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

    /// <summary>    
    /// - Runs three PWM cases (-60, -128, -255)
    /// - Validates fan speed ranges as requested
    /// - Detects fan2 existence after the first PWM read (fan2 speed > 10)
    /// - Returns true only if all three cases pass for fan1, and for fan2 if present
    /// </summary>
    private bool TestFANFunctionality()
    {
        // Define the three PWM cases using your existing constants
        var pwmCases = new[]
        {
            new { Fan1Pwm = FanParameterNames.FAN1_PWM_1, Fan2Pwm = FanParameterNames.FAN2_PWM_1, Min = FanParameterNames.FAN_MIN_SPEED_1, Max = FanParameterNames.FAN_MAX_SPEED_1, Comparison = RangeComparison.Inclusive },
            new { Fan1Pwm = FanParameterNames.FAN1_PWM_2, Fan2Pwm = FanParameterNames.FAN2_PWM_2, Min = FanParameterNames.FAN_MIN_SPEED_2, Max = FanParameterNames.FAN_MAX_SPEED_2, Comparison = RangeComparison.Inclusive },
            new { Fan1Pwm = FanParameterNames.FAN1_PWM_3, Fan2Pwm = FanParameterNames.FAN2_PWM_3, Min = FanParameterNames.FAN_MIN_SPEED_3, Max = int.MaxValue, Comparison = RangeComparison.GreaterThan }
        };

        // Track pass/fail per PWM for each fan
        var fan1Passes = new List<bool>();
        var fan2Passes = new List<bool>();

        // Ensure FanSpeeds dictionary is fresh
        FanSpeeds = new Dictionary<string, int>();

        for (int i = 0; i < pwmCases.Length; i++)
        {
            var c = pwmCases[i];
            Logger.LogMessage(Level.Info, $"Starting PWM test case {i + 1}: FAN1 PWM={c.Fan1Pwm}, FAN2 PWM={c.Fan2Pwm}");

            // Apply PWM commands
            HardwareParameters.SetParameter(FanParameterNames.FAN1_COMMAND, c.Fan1Pwm);
            HardwareParameters.SetParameter(FanParameterNames.FAN2_COMMAND, c.Fan2Pwm);

            // Allow command to be processed
            Thread.Sleep(FanParameterNames.WAITTIME);

            // Read back the command responses to ensure PWM was set
            HardwareParameters.GetParameter(FanParameterNames.FAN1_COMMAND, out string cmdResp1, true);
            var cmdMap1 = ParseAndValidateFANResponse(cmdResp1); // will add FAN1 command response into FanSpeeds if matches
            HardwareParameters.GetParameter(FanParameterNames.FAN2_COMMAND, out string cmdResp2, true);
            var cmdMap2 = ParseAndValidateFANResponse(cmdResp2);

            // Wait stabilization time (e.g., 10s)
            Thread.Sleep(FanParameterNames.WAITTIME_INMILLISECONDS);

            // Read speeds
            HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string speedResp1, true);
            var speedMap1 = ParseAndValidateFANSpeeds(speedResp1);

            HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string speedResp2, true);
            var speedMap2 = ParseAndValidateFANSpeeds(speedResp2);

            // Extract numeric speeds if available
            int fan1Speed = ExtractSpeedFromMap(speedMap1, FanParameterNames.FAN1_SPEEDFROM_RESP);
            int fan2Speed = ExtractSpeedFromMap(speedMap2, FanParameterNames.FAN2_SPEEDFROM_RESP);

            // After first PWM (-60) determine if fan2 exists (per requirement: if fan speed > 10 then consider fan2 exist)
            if (i == 0)
            {
                if (fan2Speed > 10)
                {
                    IsSecondFANExists = true;
                    Logger.LogMessage(Level.Info, $"Detected second fan (speed {fan2Speed} > 10).");
                }
                else
                {
                    IsSecondFANExists = false;
                    Logger.LogMessage(Level.Info, $"Second fan not detected (speed {fan2Speed} <= 10).");
                }
            }

            // Validate ranges for fan1
            bool fan1Ok = ValidateSpeedAgainstCase(fan1Speed, c.Min, c.Max, c.Comparison);
            fan1Passes.Add(fan1Ok);
            Logger.LogMessage(fan1Ok ? Level.Success : Level.Error, $"PWM case {i + 1} - FAN1 speed={fan1Speed} => {(fan1Ok ? "PASS" : "FAIL")} (expected {RangeDescription(c)})");

            // Validate ranges for fan2 only if present
            if (IsSecondFANExists)
            {
                bool fan2Ok = ValidateSpeedAgainstCase(fan2Speed, c.Min, c.Max, c.Comparison);
                fan2Passes.Add(fan2Ok);
                Logger.LogMessage(fan2Ok ? Level.Success : Level.Error, $"PWM case {i + 1} - FAN2 speed={fan2Speed} => {(fan2Ok ? "PASS" : "FAIL")} (expected {RangeDescription(c)})");
               // new TestDetail(FanParameterNames.FAN_TESTDETAIL, $"PWM case {i + 1} - FAN2 speed={fan2Speed} => {(fan2Ok ? "PASS" : "FAIL")} (expected {RangeDescription(c)})", fan2Ok);
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, fan2Speed, c.Min, c.Max, fan2Ok);
            }
            else
            {
                // If fan2 doesn't exist, record true so the "all cases pass" for fan2 does not block final result
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, fan2Speed, c.Min, c.Max, fan1Ok);
                fan2Passes.Add(false);
            }

            // small delay before next case
            Thread.Sleep(500);
        }

        // Reset PWM(s) to default
        HardwareParameters.SetParameter(FanParameterNames.FAN1_COMMAND, FanParameterNames.FAN1_PWM_RESETVal);
        HardwareParameters.SetParameter(FanParameterNames.FAN2_COMMAND, FanParameterNames.FAN2_PWM_RESETVal);
        Thread.Sleep(200);

        // Final evaluation
        bool allFan1Passed = fan1Passes.TrueForAll(x => x);
        bool allFan2Passed = fan2Passes.TrueForAll(x => x);        

        if (!allFan1Passed && !allFan2Passed)
        {
            Logger.LogMessage(Level.Error, "FAN1 & FAN2 are not connected or FAN test FAILED for FAN1 & FAN2.");
            new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN1 & FAN2 are not connected or FAN test FAILED for FAN1 & FAN2.", false);
            return false;
        }
        else if (!allFan1Passed)
        {
            Logger.LogMessage(Level.Error, "FAN1 not connected or FAN test FAILED for FAN1.");
            new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN1 not connected or FAN test FAILED for FAN1.", false);
            return false;
        }
        else if (!allFan2Passed)
        {
            Logger.LogMessage(Level.Error, "FAN2 not connected or FAN test FAILED for FAN2.");
            new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN2 not connected or FAN test FAILED for FAN2.", false);
            return false;
        }
       
        if (IsSecondFANExists)
        {
            if (allFan1Passed && allFan2Passed)
            {
                Logger.LogMessage(Level.Success, "FAN test PASSED for both FAN1 and FAN2 (all three PWM cases).");
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN test PASSED for both FAN1 and FAN2 (all three PWM cases).", true);
                return true;
            }            
            else
            {
                Logger.LogMessage(Level.Error, "FAN test FAILED. Not all PWM cases passed for both fans.");
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN test FAILED. Not all PWM cases passed for both fans.", false);
                return false;
            }
        }
        else
        {
            if (allFan1Passed)
            {
                Logger.LogMessage(Level.Success, "FAN test PASSED for FAN1 (all three PWM cases).");
                // Insert the Test details in to the database.
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN test PASSED for FAN1 (all three PWM cases)", true);
                return true;
            }
            else
            {
                Logger.LogMessage(Level.Error, "FAN test FAILED for FAN1.");
                new TestDetail(FanParameterNames.FAN_TESTDETAIL, "FAN test FAILED for FAN1.", false);
                return false;
            }
        }
    }

    /// <summary>
    /// Extract integer speed value from the parsed dictionary.
    /// Returns 0 if not present
    /// </summary>
    private int ExtractSpeedFromMap(Dictionary<string, int> map, string key)
    {
        if (map != null && map.ContainsKey(key))
            return map[key];
        return 0;
    }

    /// <summary>
    /// Validate numeric speed against the case ranges.
    /// Comparison: Inclusive (min..max) or GreaterThan (speed > min-1, using Min value as threshold)
    /// </summary>
    private bool ValidateSpeedAgainstCase(int speed, int min, int max, RangeComparison comparison)
    {
        if (speed <= 0) // no speed reading or zero => fail
            return false;

        if (comparison == RangeComparison.Inclusive)
        {
            return speed >= min && speed <= max;
        }
        else if (comparison == RangeComparison.GreaterThan)
        {
            return speed > (min - 1); // min already set to 241 for >240 case
        }
        return false;
    }

    private string RangeDescription(dynamic c)
    {
        if (c.Comparison == RangeComparison.Inclusive)
            return $"[{c.Min} .. {c.Max}]";
        else
            return $"> {c.Min - 1}";
    }

    // Keep your existing parsing entry points, but ensure they return dictionaries as before
    public (int Resp1, int Resp2) CheckAndCalculateFANSpeeds(int fan1Value, int fan2Value)
    {
        // This method is retained for backward compatibility but now simply reads speeds and returns them
        HardwareParameters.SetParameter(FanParameterNames.FAN1_COMMAND, fan1Value);
        HardwareParameters.SetParameter(FanParameterNames.FAN2_COMMAND, fan2Value);

        Thread.Sleep(FanParameterNames.WAITTIME);

        HardwareParameters.GetParameter(FanParameterNames.FAN1_SPEED, out string Response1, true);
        var map1 = ParseAndValidateFANSpeeds(Response1);
        int fan1SpeedVal = 0;
        if (map1 != null && map1.ContainsKey(FanParameterNames.FAN1_SPEEDFROM_RESP))
            fan1SpeedVal = map1[FanParameterNames.FAN1_SPEEDFROM_RESP] * FanParameterNames.SPEED_MULTIPLE_OFFSET;

        HardwareParameters.GetParameter(FanParameterNames.FAN2_SPEED, out string Response2, true);
        var map2 = ParseAndValidateFANSpeeds(Response2);
        int fan2SpeedVal = 0;
        if (map2 != null && map2.ContainsKey(FanParameterNames.FAN2_SPEEDFROM_RESP))
            fan2SpeedVal = map2[FanParameterNames.FAN2_SPEEDFROM_RESP] * FanParameterNames.SPEED_MULTIPLE_OFFSET;

        Thread.Sleep(FanParameterNames.WAITTIME_INMILLISECONDS);

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
                if (token.Length > Handler.INDEX_ZERO && ((token[Handler.INDEX_ZERO] == FanParameterNames.FAN1_SPEEDFROM_RESP) ||
                    (token[Handler.INDEX_ZERO] == FanParameterNames.FAN2_SPEEDFROM_RESP)))
                {
                    key = token[Handler.INDEX_ZERO];
                    var valueStr = token.Length > 1 ? token[Handler.INDEX_ONE] : "0";
                    if (!int.TryParse(valueStr, out Rep))
                        Rep = 0;

                    // If a second fan reports any speed > 10, flag existence (explicit)
                    if (key == FanParameterNames.FAN2_SPEEDFROM_RESP && Rep > 10)
                        IsSecondFANExists = true;

                    // store/overwrite speed value
                    if (!FanSpeeds.ContainsKey(key))
                        FanSpeeds.Add(key, Rep);
                    else
                        FanSpeeds[key] = Rep;

                    Logger.LogMessage(Level.Info, $"Response for {key} is {Rep}");
                }
            }
            return FanSpeeds;
        }
        return null;
    }

    private static Dictionary<string, int> ExtractResponseBasedonFANNumber(string Response, int expectedValue)
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
                    var valueStr = token.Length > 1 ? token[Handler.INDEX_ONE] : "0";
                    if (!int.TryParse(valueStr, out Rep))
                        Rep = 0;

                    if (Rep == expectedValue)
                    {
                        if (!FanSpeeds.ContainsKey(key))
                        {
                            FanSpeeds.Add(key, Rep);
                            Logger.LogMessage(Level.Info, $"Response for {key} is {Rep}");
                            //new TestDetail(FanParameterNames.FAN_TESTDETAIL, Rep, true);
                        }
                        else
                        {
                            FanSpeeds[key] = Rep;
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
        // Note: previous implementation did not return anything. Kept it no-op.
        Fan1CalSpeedVal += 5;
        Fan2CalSpeedVal += 5;
    }

    private enum RangeComparison
    {
        Inclusive,
        GreaterThan
    }
}
