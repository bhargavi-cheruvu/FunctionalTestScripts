using LogViewManager;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using UniversalBoardTestApp;
using ConnectionTester;
using System.Text;
public class Connection
{
    public UsbStreamHandler usbStream = null;
    // Gets called any time the status of the device under test is disconnected
    // Needs to be filled with logic that returns true if device under test is detected and false if not
    private bool EstablishConnection()
    {
        // Standard Methods which can be used:
        // Logger.LogMessage(Level LogMessageType, string TextMessage, bool LastRowErase = false)
        // Sends log message to main window of UBTA (LogView)
        // Level:
        // Level.Info:      black text on white background
        // Level.Warning:   yellow text on white background
        // Level.Error:     red text on white background
        // Level.Success:   green text on white background
        // Level.Failed:    dark red text on red background
        // Level.Passed:    dark green text on green background
        // ScriptHelper.CheckIfTestIsRunning()
        // Returns true if a test script is active
        // ScriptHelper.GetConnectionName()
        // Returns (string) name of currently active connection
        // ScriptHelper.GetConnectionType()
        // Returns (string) type name of currently active connection
        // ScriptHelper.GetFunctionalTest()
        // Returns serialized (FunctionalTest) object from loaded xml config file
        // ScriptHelper.GetEnteredSerialNumbers()
        // Returns (Dictionary<string, string>) object from entered serial numbers when test was started
        // ScriptHelper.GetEnteredRevisionNumbers()
        // Returns (Dictionary<string, string>) object from entered revision numbers when test was started
        // ScriptHelper.GetCountOfPendingTestScripts()
        // Returns (int) number of pending test scripts
        // ScriptHelper.GetNameOfTestCase()
        // Returns (string) name of test case
        // ScriptHelper.GetTestLimit(string LimitFieldName, out double lowerLimit, out double upperLimit)
        // Returns (true) bool and limit details of test limit by LimitFieldName
        return OpenUsbConnection(false);
    }

    // Gets called periodically when status of device under test is connected to make sure device is still available
    // Needs to be filled with logic that returns true if device under test is detected and false if not
    private bool KeepAlive()
    {
        return OpenUsbConnection(true);
    }

    // Gets called when status of device under test changes from connected to disconnected
    // Needs to be filled with everything what is necessary to reconnect to a device (for instance: closing COM Port)
    private bool EndConnection()
    {
        if (usbStream != null)
        {
            usbStream.Close();
            return true;
        }
        return false;
    }

    // Gets called when parameter of device under test is requested (for instance from monitor tab/test script)
    // Value can be returned as string or double
    private bool GetParameter(HardwareParameter parameter, out object value)
    {        
        if (parameter != null && usbStream != null)
        {
            StringBuilder response = ReadUSBResponse(usbStream);
            value = response;
            return true;
        }
        value = string.Empty;
        return false;
    }

    // Gets called when value of parameter from device under test is set to a new value (for instance from debug tab/test script)
    // Value can be set as string or double
    private bool SetParameter(HardwareParameter parameter, object setValue)
    {
        try
        {
            if (parameter != null && setValue != null && usbStream != null)
            {
                string messageToWrite = string.Empty;
                if (parameter.ConversionFormula == "IsNeeded") // Append ~ to the String
                    messageToWrite = string.Format("{0}={1}", "~" + parameter.Name, setValue);
                else if (parameter.ConversionFormula == "None") // Send string without appending ~
                    messageToWrite = string.Format("{0}={1}", parameter.Name, setValue);
                else if(parameter.ConversionFormula == "NotNeeded") // send only Name, No Value required. Ex: Service.Lock
                    messageToWrite = string.Format("{0}", parameter.Name);

                    WriteToUSB(messageToWrite, usbStream);
                    Thread.Sleep(1000); // wait for device to process the command
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static string[] GetRegistrySubKeyNames(string registryPath)
    {
        try
        {
            // Open the registry key
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(registryPath);
            if (registryKey == null)
            {
                Console.WriteLine($"Registry key '{registryPath}' not found.");
                return null;
            }

            // Retrieve subkey names
            string[] subKeyNames = registryKey.GetSubKeyNames();
            registryKey.Close(); // Close the registry key
            return subKeyNames;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing registry: {ex.Message}");
            return null;
        }
    }
    private StringBuilder ReadUSBResponse(Stream usbStream)
    {
        int chunkSize = 1024; // Size of each chunk (1 KB, adjust as needed)
        byte[] readBuffer = new byte[chunkSize];
        int bytesRead;

        // Example of reading data in chunks
        bool isStartLoop = true;
        bool isFirstRequest = true;
        StringBuilder dataBuilder = new StringBuilder();
        while (isStartLoop)
        {
            bytesRead = usbStream.Read(readBuffer, 0, readBuffer.Length);
            if (bytesRead > 0)
            {
                string dataChunk = Encoding.UTF8.GetString(readBuffer, 0, bytesRead);
                dataBuilder.Append(dataChunk);
            }
            if (isFirstRequest)
                Thread.Sleep(1000);

            if (!isFirstRequest && bytesRead <= 0)
            {
                isStartLoop = false;
            }
            isFirstRequest = false;
            //Console.WriteLine("Data chunk received: " + dataChunk);
            //totalMessage += dataChunk;
        }

        return dataBuilder;
    }
    private void WriteToUSB(string message, Stream usbStream)
    {
        if (message[message.Length - 1] != '\n')
            message = message + '\n';

        byte[] messageBuffer = System.Text.Encoding.UTF8.GetBytes(message);

        lock (usbStream)
        {
            usbStream.Write(messageBuffer, 0, message.Length);
        }
    }

    private bool OpenUsbConnection(bool isKeepAlive)
    {
        string[] registrySubKeyNames = GetRegistrySubKeyNames("HARDWARE\\DEVICEMAP\\DIONEX");
        if (registrySubKeyNames == null || registrySubKeyNames.Length == 0)
        {
            //Logger.LogMessage(Level.Warning, "No USB devices found to open the connection.");
            return false;
        }

        string devicePath = @"\\.\" + registrySubKeyNames[0];
        try
        {
            if (isKeepAlive)
            {
                usbStream.Close();
            }
            usbStream = new UsbStreamHandler(devicePath);
            if (usbStream != null && usbStream.IsOpened)
            {
                //Logger.LogMessage(Level.Success, "USB connection is available and opened successfully.");
                return true;
            }
            else
            {
                //Logger.LogMessage(Level.Warning, "USB connection is not available.");
                return false;
            }
        }
        catch (IOException ex)
        {
            //Logger.LogMessage(Level.Failed, $"Error while opening the connection, IO Exception:{ex.Message}.");
            return false;
        }
        catch (UnauthorizedAccessException ex2)
        {
            //Logger.LogMessage(Level.Failed, $"Error while opening the connection, Access Exception:{ex2.Message}.");
            return false;
        }
        catch (Exception ex3)
        {
            //Logger.LogMessage(Level.Failed, $"Error while opening the connection, Exception:{ex3.Message}.");
            return false;
        }
    }
}
