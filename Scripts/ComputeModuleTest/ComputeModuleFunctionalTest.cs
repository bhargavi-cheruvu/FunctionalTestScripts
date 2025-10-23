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
}