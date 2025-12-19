using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    public class Handler
    {
        public const string TEST_VERSION = "1.0.0.0";
        public const string BackupMemoryTest = "Starting Backup Memory Test Script.";
        public const string BackupStatusResponse = "0,0,1";
        public const string BACKUPMEMORY_TESTDETAIL = "BackupMemoryTestDetail";
        public const string BACKUP_TESTTABLE = "BackupMemoryTestTable";
        public const char DELIMITER = '=';
        public const string REQUEST = "?";
        public const string BACKUPMEMORY_COMMAND= "BackupMemory.Status";
        public const string BACKUPMEMORY_STATUS = "~BackupMemory.Status";
        public const string BACKUPMEMORY_INVALID_RESPONSE = "Invalid Response or Backup memory is Disconnected";
        public const int INDEX_ZERO = 0;
        public const int INDEX_ONE = 1;
        public const char NEWLINE = '\n';
        public const char CARRAIGE_RETURN = '\r';
    }
}
