using Helper;
using LogViewManager;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Forms;
using UniversalBoardTestApp;

public class Test
{
    [DllImport("GMH3x32E.dll")]
    public static extern Int16 UniversalOpenCom(Int16 ini16COMPortNumber, UInt32 inui32BaudRate, Int16 inui16ConverterType, Int16 ini16Parity, Int16 ini16StoppBits);
    [DllImport("GMH3x32E.dll")]
    public static extern Int16 GMH_CloseCom();  
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_GetType(Int32 inui32StatusCode, byte* outcarrStatusText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern byte GMH_GetUnit(Int16 inui16UnitCode, byte* outcarrUnitText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_Transmit(Int16 ini16DeviceAddress, Int16 ini16TransmitCode, Int16* refi16ptrPriority, double* refdblptrFloatValue, Int32* refi32ptrIntegerValue);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern byte GMH_GetMeasurement(Int16 ini16MeasurementCode, byte* outcarrMeasurementText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_GetErrorMessageRet(Int16 ini16ErrorCode, byte* outcarrErrorText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_GetErrorMessageFL(double indblErrorCode, byte* outcarrErrorText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_SearchForComPorts(Int16* refi16ArrComPortNumbers, out Int16 outi16LenghtOfArray);

    Int16 i16ErrorCode = -1;
    Int16 i16GMHAddress = -1;
    Int16 i16ArrayLength = -1;
    Int16 i16AnzahlDerComPorts = -1;
    Int16 i16ComPortNummer = 1; // change to configure com port number
    Int16[] i16ArrComPortArray = new Int16[255];
    Int32 i32GMHSerialNumber = -1;
    String strGMHTypeString;
    String strErrorMessage;
    String strMeasurementText;
    String strUnitText;

    string str = string.Empty;
    short COMPortVal = 0;

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
        int interval = 300;
        string response;

        while (elapsed < timeoutMs)
        {
            HardwareParameters.GetParameter(parameterName, out response, true);
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
        double presureVal = CheckAndCalculateDegasserPressure();
        GMH_CloseCom();
        if (presureVal > 0)
            return true;
        else
            return false;
    }
    
    private void DisplayErrorMessage(Int16 ini16ErrorCode, Int32 ini32IntegerValue, double indblFloatValue)
    {
        unsafe
        {
            Int16 i16Length;
            byte[] barrText = new byte[1024];
            System.Text.UTF7Encoding enc = new System.Text.UTF7Encoding();
            String strErrorMessage = "OK";
            if (0 > ini16ErrorCode)
            {
                fixed (byte* cptrText = &barrText[0])
                {
                    if (-36 == ini16ErrorCode)
                    {
                        i16Length = GMH_GetErrorMessageFL(indblFloatValue, cptrText);
                    }
                    else
                    {
                        i16Length = GMH_GetErrorMessageRet(ini16ErrorCode, cptrText);
                    }
                }
                strErrorMessage = enc.GetString(barrText, 0, i16Length);
            }
            MessageBox.Show(strErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
    }
    private void ReadCOMPorts()
    {
        this.i16GMHAddress = 1;
        
        Int16 _i16_Priority = 0;
        Int32 i32IntegerValue = 0;
        double dblFloatValue = 0;
        bool bFoundDevice = false;

        unsafe
        {
            fixed (Int16* i16_ptr_Temp = &i16ArrComPortArray[0])
            {
                i16AnzahlDerComPorts = GMH_SearchForComPorts(i16_ptr_Temp, out i16ArrayLength);
            }
        }
        if (i16AnzahlDerComPorts > 0)
        {
            string str = "System has " + i16AnzahlDerComPorts + " COM-Ports";
            for (Int16 _counter = 0; _counter < i16AnzahlDerComPorts; _counter++)
            {
                str = "";
                str = str + "COM" + i16ArrComPortArray[_counter];
                COMPortVal = i16ArrComPortArray[_counter];
            }
        }
        this.i16ComPortNummer = COMPortVal;

        this.i16ErrorCode = UniversalOpenCom(this.i16ComPortNummer, 38400, 10, 0, 0); /*Try GMH 5000*/
        if (this.i16ErrorCode >= 0)
        {
            for (Int32 i32Counter = 0; i32Counter <= 9; i32Counter++)
            {
                this.i16GMHAddress = (Int16)(1 + (10 * i32Counter));
                unsafe
                {
                    this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, 12, &_i16_Priority, &dblFloatValue, &i32IntegerValue);
                }
                if (-34 == this.i16ErrorCode) /*No GMH 5000 detected, use GMH 3000 instead*/
                {
                    GMH_CloseCom();
                    UniversalOpenCom(this.i16ComPortNummer, 4800, 8, 0, 0);
                    unsafe
                    {
                        this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, 12, &_i16_Priority, &dblFloatValue, &i32IntegerValue);
                    }
                }
                if (0 == this.i16ErrorCode)
                {                    
                    bFoundDevice = true;
                    break;
                }
            }
            if (false == bFoundDevice)
            {
                GMH_CloseCom();
                MessageBox.Show("Found no device on selected COM Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        else
        {
            this.DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
        }

        Int16 i16Priority = 0;    
        Int16 i16Length = -1;
        byte[] barrTypeString = new byte[1024];
        System.Text.UTF7Encoding encTextEncoding = new System.Text.UTF7Encoding();


        i16GMHAddress = 1;
        unsafe
        {
            this.i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.IDNummerLesen, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode < 0)
        {
            this.DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
        }
        else
        {
            i32GMHSerialNumber = i32IntegerValue;
            unsafe
            {
                fixed (byte* char_ptr_Temp = &barrTypeString[0])
                {
                    i16Length = GMH_GetType(i32GMHSerialNumber, char_ptr_Temp);
                }
            }
            strGMHTypeString = encTextEncoding.GetString(barrTypeString, 0, i16Length);      
        }
        unsafe
        {
            i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.MessbereichMessartLesen, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode == (Int16)Fehlermeldungen.NegativeQuittung)
        {
            unsafe
            {
                i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.AnzeigeMessartLesen, &i16Priority, &dblFloatValue, &i32IntegerValue);
            }
        }
        if (i16ErrorCode < 0)
        {
            DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
        }
        else
        {
            unsafe
            {
                fixed (byte* char_ptr_Temp = &barrTypeString[0])
                {
                    i16Length = GMH_GetMeasurement((Int16)i32IntegerValue, char_ptr_Temp);
                }
            }
            strMeasurementText = encTextEncoding.GetString(barrTypeString, 0, i16Length);
        }
        unsafe
        {
            i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.MessbereichEinheitLesen, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode == (Int16)Fehlermeldungen.NegativeQuittung)
        {
            unsafe
            {
                i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.AnzeigeeinheitLesen, &i16Priority, &dblFloatValue, &i32IntegerValue);
            }
        }
        if (i16ErrorCode < 0)
        {
            this.DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
        }
        else
        {
            unsafe
            {
                fixed (byte* char_ptr_Temp = &barrTypeString[0])
                {
                    i16Length = GMH_GetUnit((Int16)i32IntegerValue, char_ptr_Temp);
                }
            }
            strUnitText = encTextEncoding.GetString(barrTypeString, 0, i16Length);
        }
    }

    private double ReadCurrentDisplayValue()
    {

        Int16 i16Priority;
        double dblFloatValue;
        Int32 i32IntergerValue;
        unsafe
        {
            this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, (Int16)GHM_TransmitFunktion.AnzeigewertLesen, &i16Priority, &dblFloatValue, &i32IntergerValue);
        }
        if (i16ErrorCode < 0)
        {
            this.DisplayErrorMessage(this.i16ErrorCode, i32IntergerValue, dblFloatValue);
        }
        return dblFloatValue;
    }

    public double CheckAndCalculateDegasserPressure()
    {
        // Send to Pump Module
        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_CMD, Handler.INDEX_ONE);

        // Wait for at least 2 minutes for the pump to react
        Thread.Sleep(DegasserParameterNames.WAITTIME_TWO_MINS);

        // Monitor ~Degasser.Pressure for stability (timeout 1 minute, allowed deviation +/-0.5 mbar)
        const double allowedDeviationMbar = 0.5; // mbar
        const double scalePerCountMbar = 0.01; // 1 count = 0.01 mbar
        int allowedDeviationCounts = (int)Math.Round(allowedDeviationMbar / scalePerCountMbar); // =50 counts

        Logger.LogMessage(Level.Info, "Monitoring degasser pressure for stability...");

        List<int> samples = new List<int>();
        int monitorTimeoutMs = DegasserParameterNames.WAITTIME_ONE_MIN; // 1 minute
        int sampleIntervalMs = 1000; // sample every 1 second
        int elapsed = 0;
        bool stable = false;
        int stableValueCounts = 0;

        while (elapsed < monitorTimeoutMs)
        {
            HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string response, true);
            int valCounts = (int)Math.Round(ExtractDegasserPressureVal(ResponseSafe(response)));
            samples.Add(valCounts);

            // evaluate stability over collected samples
            int min = int.MaxValue, max = int.MinValue;
            foreach (var v in samples)
            {
                if (v < min) min = v;
                if (v > max) max = v;
            }

            if (samples.Count >= 3 && (max - min) <= allowedDeviationCounts)
            {
                stable = true;
                stableValueCounts = (int)Math.Round(Average(samples));
                Logger.LogMessage(Level.Info, $"Pressure stabilized (counts). Min={min}, Max={max}, Avg={stableValueCounts}");
                break;
            }

            Thread.Sleep(sampleIntervalMs);
            elapsed += sampleIntervalMs;
        }

        if (!stable)
        {
            Logger.LogMessage(Level.Error, "Degasser pressure did not stabilize within timeout.");
            return -1;
        }

        // When stable, average property outputs over 30 seconds
        Logger.LogMessage(Level.Info, "Averaging degasser pressure over 30 seconds...");
        samples.Clear();
        int averagingDurationMs = DegasserParameterNames.WAITTIME_THIRTY_SECONDS; // 30 seconds
        elapsed = 0;
        while (elapsed < averagingDurationMs)
        {
            HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string response, true);
            int valCounts = (int)Math.Round(ExtractDegasserPressureVal(ResponseSafe(response)));
            samples.Add(valCounts);
            Thread.Sleep(sampleIntervalMs);
            elapsed += sampleIntervalMs;
        }
        double averageCounts = Average(samples);
        double averageMbar = averageCounts * scalePerCountMbar;
        Logger.LogMessage(Level.Info, $"Averaged degasser pressure: {averageCounts} counts = {averageMbar:F2} mbar");

        // Read external pressure sensor and calculate offset
        ReadCOMPorts(); // ensure COM is open and GMH configured
        double externalMbar = ReadCurrentDisplayValue();
        Logger.LogMessage(Level.Info, $"External sensor reading = {externalMbar:F2} mbar");

        double offsetMbar = externalMbar - averageMbar; // resolution 0.01 mbar
        offsetMbar = Math.Round(offsetMbar, 2);

        // Set Degasser.Pressure.Offset=<offset>
        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, offsetMbar);
        Logger.LogMessage(Level.Info, "Set Degasser pressure offset to " + offsetMbar);

        // Read back offset and pressure from device
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, out string pressureOffsetResp, true);
        double readBackOffset = ExtractDegasserPressureOffsetVal(ResponseSafe(pressureOffsetResp));
        Logger.LogMessage(Level.Info, "Read back Degasser.Pressure.Offset = " + readBackOffset);

        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string pressureResp, true);
        double readBackCounts = ExtractDegasserPressureVal(ResponseSafe(pressureResp));
        double readBackMbar = readBackCounts * scalePerCountMbar;
        Logger.LogMessage(Level.Info, $"Read back Degasser.Pressure = {readBackCounts} counts = {readBackMbar:F2} mbar");

        // Check: must match external_value within limits (±0.5 mbar)
        double diff = Math.Abs(externalMbar - readBackMbar);
        if (diff > allowedDeviationMbar)
        {
            Logger.LogMessage(Level.Error, $"Degasser pressure ({readBackMbar:F2} mbar) does not match external sensor ({externalMbar:F2} mbar) within ±{allowedDeviationMbar} mbar. Diff={diff:F2} mbar");
            return -1;
        }

        // Check <value> against range 8000..15000 cts.
        const int minCounts = 8000;
        const int maxCounts = 15000;
        if (readBackCounts < minCounts || readBackCounts > maxCounts)
        {
            Logger.LogMessage(Level.Error, $"Degasser pressure counts {readBackCounts} out of allowed range {minCounts}..{maxCounts}.");
            return -1;
        }

        // Return the averaged pressure in counts as success (>0)
        return averageCounts;
    }
       
    private static double ExtractDegasserPressureVal(string Response)
    {
        if (string.IsNullOrEmpty(Response))
            return 0.0;

        var lines = Response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);

            // check for Degasser pressure Reponse                
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == DegasserParameterNames.DEGASSER_PRESSURE_VAL)
            {
                string valStr = token[Handler.INDEX_ONE];
                if (double.TryParse(valStr, out double Value))
                {
                    // Response is in counts
                    return Value;
                }
            }
        }
        return 0.0;
    }

    private static double ExtractDegasserPressureOffsetVal(string Response)
    {
        if (string.IsNullOrEmpty(Response))
            return 0.0;

        var lines = Response.Split(new[] { Handler.NEWLINE, Handler.CARRAIGE_RETURN }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            var token = line.Split(Handler.DELIMITER);
           
            if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == DegasserParameterNames.DEGASSER_PRESSURE_OFFSET_VAL)
            {
                string valStr = token[Handler.INDEX_ONE];
                if (double.TryParse(valStr, out double Value))
                {
                    return Value; // offset in mbar (expected)
                }
            }
        }
        return 0.0;
    }

    private double CheckStabilizationPressureValue(double degasserPressureVal)
    {
        // Not used for new algorithm; kept for backward compatibility
        // Stabilization could be speed property value +/- 0.5
        degasserPressureVal += 0.5;      
        return degasserPressureVal;
    }

    private static string ResponseSafe(string resp)
    {
        return resp ?? string.Empty;
    }

    private static double Average(List<int> samples)
    {
        if (samples == null || samples.Count == 0) return 0.0;
        double sum = 0;
        foreach (var s in samples) sum += s;
        return sum / samples.Count;
    }
}
