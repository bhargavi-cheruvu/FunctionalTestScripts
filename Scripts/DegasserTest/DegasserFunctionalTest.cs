using Helper;
using LogViewManager;
using System;
using System.Collections.Generic;
using System.Linq;
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
    bool bFoundDevice = false;
    double offsetMbar = 0.0;

    // pressure conversion
    private const double AllowedDeviationMbar =  0.5; //2.5;
    private const double ScalePerCountMbar = 0.01;
    private const int MinCounts = 8000;
    private const int MaxCounts = 15000;

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
        {
            new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS, "Degasser Test Passed.", true);
            return true;
        }
        else
        {
            new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS, "Degasser Test Failed.", false);
            return false;
        }
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
       // bool bFoundDevice = false;

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
              //  MessageBox.Show(str, "COM PORT Number for GMH 3100 series", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                  //  MessageBox.Show(" GMH_Transmit for GMH 5000 is " + this.i16ErrorCode, "GMH 5000 Series ");
                }
                if (-34 == this.i16ErrorCode) /*No GMH 5000 detected, use GMH 3000 instead*/
                {
                    GMH_CloseCom();
                    UniversalOpenCom(this.i16ComPortNummer, 4800, 8, 0, 0);
                    unsafe
                    {
                        this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, 12, &_i16_Priority, &dblFloatValue, &i32IntegerValue);
                      //  MessageBox.Show(" GMH_Transmit for GMH 3000 is " + this.i16ErrorCode, "GMH 3000 series ");
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
                Logger.LogMessage(Level.Error, "Found no device on selected COM Port");
                MessageBox.Show("Found no device on selected COM Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
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
        PrepareDegasser();

        WaitForPumpReaction();

       double stableCounts = WaitForPressureStability();
        if (stableCounts < 0) return -1;

        double averagedCounts = AveragePressureOverTime(30000);
        if (averagedCounts < 0) return -1;

        double averageMbar = averagedCounts * ScalePerCountMbar;

        double externalMbar = ReadExternalSensor();

        if (externalMbar < 0) return -1;

        //double 
            offsetMbar = CalculateAndApplyOffset(averageMbar, externalMbar);

        if (!VerifyOffsetAndPressure(externalMbar))
            return -1;

        if (!VerifyCountsRange())
            return -1;

        return averagedCounts;
    }
   
    private void PrepareDegasser()
    {
        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_CMD, Handler.INDEX_ONE);
        Logger.LogMessage(Level.Info, "~Degasser=1 (sent) - Send to pump module (preparation)");
    }
    private void WaitForPumpReaction()
    {
        Logger.LogMessage(Level.Info, "Waiting minimum 2 minutes for pump reaction...");
        Thread.Sleep(DegasserParameterNames.WAITTIME_TWO_MINS);
    }
    private double WaitForPressureStability()
    {
        Logger.LogMessage(Level.Info,
            $"Monitoring ~Degasser.Pressure for stability. Timeout: 1 minute, allowed deviation: {AllowedDeviationMbar} mbar");

        int allowedDeviationCounts = (int)Math.Round(AllowedDeviationMbar / ScalePerCountMbar);

        const int sampleIntervalMs = 1000;
        const int windowSize = 10;
        const int minSamples = 3;

        int timeoutMs = DegasserParameterNames.WAITTIME_ONE_MIN;
        int elapsed = 0;

        var window = new Queue<int>();

        while (elapsed < timeoutMs)
        {
            int counts = ReadPressureCounts();

            if (counts > 0)
            {
                window.Enqueue(counts);
                if (window.Count > windowSize) window.Dequeue();

                if (window.Count >= minSamples && IsStableWindow(window, allowedDeviationCounts, out int avg))
                {
                    Logger.LogMessage(Level.Info,
                        $"Pressure stabilized. WindowAvg={avg}");
                    return avg;
                }
            }
            else
            {
                Logger.LogMessage(Level.Info, "Invalid degasser reading ignored.");
            }

            Thread.Sleep(sampleIntervalMs);
            elapsed += sampleIntervalMs;
        }

        Logger.LogMessage(Level.Error, "Degasser pressure did not stabilize within timeout.");
        new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS, "Degasser pressure did not stabilize within timeout.", false);
        return -1;
    }

    private int ReadPressureCounts()
    {
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string resp, true);
        int Val = (int)Math.Round(ExtractDegasserPressureVal(ResponseSafe(resp)));
       // Logger.LogMessage(Level.Info, $"Read ~Degasser.Pressure = {Val} counts"); //added
        return (int)Math.Round(ExtractDegasserPressureVal(ResponseSafe(resp)));
    }

    private bool IsStableWindow(Queue<int> window, int allowedDiff, out int avg)
    {
        int min = window.Min();
        int max = window.Max();
        int diff = max - min;

        Logger.LogMessage(Level.Info,
            $"Window snapshot: Count={window.Count}, Min={min}, Max={max}, Diff={diff}");

        if (diff <= allowedDiff)
        {
            avg = (int)Math.Round(Average(window.ToList()));
            return true;
        }

        avg = 0;
        return false;
    }
    private double AveragePressureOverTime(int durationMs)
    {
        Logger.LogMessage(Level.Info, "Averaging ~Degasser.Pressure over 30 seconds...");

        const int sampleIntervalMs = 1000;
        var samples = new List<int>();
        int elapsed = 0;

        while (elapsed < durationMs)
        {
            int counts = ReadPressureCounts();
            if (counts > 0) samples.Add(counts);

            Thread.Sleep(sampleIntervalMs);
            elapsed += sampleIntervalMs;
        }

        if (samples.Count == 0)
        {
            Logger.LogMessage(Level.Error, "No valid degasser readings during averaging.");
            new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS,
                "No valid readings during averaging.", false);
            return -1;
        }

        return Average(samples);
    }
    private double ReadExternalSensor()
    {
        ReadCOMPorts();
        if (bFoundDevice)
        {
            double ext = ReadCurrentDisplayValue();
            Logger.LogMessage(Level.Info, $"External sensor reading = {ext:F2} mbar");
            return ext;
        }
        else return -1;
    }

    private double CalculateAndApplyOffset(double averageMbar, double externalMbar)
    {
        double offset = Math.Round(externalMbar - averageMbar, 2);
        Logger.LogMessage(Level.Info, $"Calculated offset = {offset:F2} mbar");

        HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, offset, true);
        Logger.LogMessage(Level.Info, $"Set ~Degasser.Pressure.Offset = {offset:F2}");

        return offset;
    }
   

    private bool VerifyOffsetAndPressure(double externalMbar)
    {    
        //HardwareParameters.SetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, offsetMbar);

        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE_OFFSET, out string offsetResp, true);
        double readBackOffset = ExtractDegasserPressureOffsetVal(ResponseSafe(offsetResp));

        // read back pressure
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string pressureResp, true);
        double countsDbl = ExtractDegasserPressureVal(ResponseSafe(pressureResp));
        int counts = (int)Math.Round(countsDbl);
        double mbar = counts * ScalePerCountMbar;
                 
        double deltaMbar = 0.0;
        // check deviation             
        deltaMbar = mbar - Math.Abs(offsetMbar);
        double diff = 0.0;
        //if(offsetMbar < 0)
        //    diff = (externalMbar - deltaMbar);
        //else
            diff = deltaMbar - externalMbar;

        if (diff > AllowedDeviationMbar)
        {
            Logger.LogMessage(Level.Error,
                $"Degasser ({deltaMbar:F2}) does not match external ({externalMbar:F2}) within ±{AllowedDeviationMbar} mbar.");
            new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS, "Mismatch with external sensor.", false);
            return false;
        }

        Logger.LogMessage(Level.Info,
            $"Degasser ({deltaMbar:F2}) matches external ({externalMbar:F2}) within ±{AllowedDeviationMbar} mbar.");
        return true;
    }
    private bool VerifyCountsRange()
    {
        HardwareParameters.GetParameter(DegasserParameterNames.DEGASSER_PRESSURE, out string resp, true);
        int counts = (int)Math.Round(ExtractDegasserPressureVal(ResponseSafe(resp)));

        if (counts < MinCounts || counts > MaxCounts)
        {
            Logger.LogMessage(Level.Error,
                $"Degasser pressure {counts} out of allowed range {MinCounts}..{MaxCounts}.");
            new TestDetail(DegasserParameterNames.DEGASSER_TESTDETAILS,
                "Degasser pressure out of allowed range.", false);
            return false;
        }
        else
        {
            Logger.LogMessage(Level.Info, $"Degasser {counts} are within the allowed range {MinCounts} .. {MaxCounts}.");
        }

        return true;
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
                if (token.Length > Handler.INDEX_ONE)
                {
                    string valStr = token[Handler.INDEX_ONE];
                    if (double.TryParse(valStr, out double Value))
                    {
                        // Response is in counts
                        return Value;
                    }
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
                if (token.Length > Handler.INDEX_ONE)
                {
                    string valStr = token[Handler.INDEX_ONE];
                    if (double.TryParse(valStr, out double Value))
                    {
                        return Value; // offset in mbar (expected)
                    }
                }
            }
        }
        return 0.0;
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