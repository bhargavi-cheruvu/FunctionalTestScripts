using LogViewManager;
using System;
using UniversalBoardTestApp;
using Helper;
public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.BackupMemoryTest);
        if (CheckBackupMemoryConnected())
            return true;
        else
            return false;
    }

    private bool CheckBackupMemoryConnected()
    {
        string resp;
        // Backup Memory command
        HardwareParameters.SetParameter(Handler.BACKUPMEMORY_COMMAND, Handler.REQUEST, true);
        HardwareParameters.GetParameter(Handler.BACKUPMEMORY_COMMAND, out resp, true);

        var lines = resp.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == Handler.BACKUPMEMORY_STATUS)
            {
                string key = token[Handler.INDEX_ZERO];
                resp = token[Handler.INDEX_ONE];
                break;
            }
        }

        if (resp == Handler.BackupStatusResponse)
        {
            Logger.LogMessage(Level.Info, resp);
            return true;
        }
        else
        {
            Logger.LogMessage(Level.Error, Handler.BACKUPMEMORY_INVALID_RESPONSE);
            return false;
        }
    }
}