using Helper;
using LogViewManager;
using System;
using System.Diagnostics;
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

        if (ComputeModuleResponse())
        {
            return true;
        }
        return false;
    }
    
    public bool ComputeModuleResponse()
    {
        string result = string.Empty;
        Thread.Sleep(2000);
        HardwareParameters.SetParameter("TestDCFAlignment", "");

        const int totalTimeoutMs = 3000; // poll for 1 second total
        const int pollIntervalMs = 50;   // poll every 50ms
        var sw = Stopwatch.StartNew();

        while (sw.ElapsedMilliseconds < totalTimeoutMs)
        {
            // read current response from hardware parameter
            HardwareParameters.GetParameter("TestDCFAlignment", out string resp, true);

            if (!string.IsNullOrEmpty(resp))
            {
                var lines = resp.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    var token = line.Split(Handler.DELIMITER);

                    // Ensure we have at least two tokens before accessing INDEX_ONE
                    if (token.Length > Handler.INDEX_ONE && token[Handler.INDEX_ZERO] == Handler.TestDCFAlignment_STATUS)
                    {
                        resp = token[Handler.INDEX_ONE];
                        if (resp.Contains(Handler.TestDCFAlignmentErrorResponse))
                        {
                            result = resp.Trim();
                            Logger.LogMessage(Level.Info, result);
                            return true;
                        }
                    }

                    if (token.Length > Handler.INDEX_ONE && token[Handler.INDEX_ZERO] == "~TestDCFAlignment")
                    {
                        resp = token[Handler.INDEX_ONE];
                        if (resp.Contains(Handler.TestDCF_MissingTestAdapter))
                        {
                            result = resp.Trim();
                            Logger.LogMessage(Level.Error, result);
                            return false;
                        }
                    }
                }
            }

            if (ScriptHelper.CheckIfProcedureIsCancelled())
                return false;

            Thread.Sleep(pollIntervalMs);
        }

        // timed out after 1 second without matching conditions
        return false;
    }
}
