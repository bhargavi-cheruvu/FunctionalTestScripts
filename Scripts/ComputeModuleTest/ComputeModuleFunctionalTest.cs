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
        HardwareParameters.SetParameter("TestDCFAlignment", "");
        HardwareParameters.GetParameter("TestDCFAlignment", out string resp);

        var lines = resp.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == Handler.TestDCFAlignment_STATUS)
            {
                string key = token[Handler.INDEX_ZERO];
                resp = token[Handler.INDEX_ONE];
                break;
            }
        }

        if (resp == Handler.TestDCFAlignmentResponse || resp == Handler.TestDCFAlignmentErrorResponse)
            return true;
        else
        {
            Logger.LogMessage(Level.Error, Handler.TESTDCFAlignment_INVALID_RESPONSE);
            return false;
        }
    }
}