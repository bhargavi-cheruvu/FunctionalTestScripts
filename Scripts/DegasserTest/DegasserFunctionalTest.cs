using Helper;
using LogViewManager;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UniversalBoardTestApp;

public class Test
{
    [DllImport("GMH3x32E.dll")]
    public static extern Int16 UniversalOpenCom(Int16 ini16COMPortNumber, UInt32 inui32BaudRate, Int16 inui16ConverterType, Int16 ini16Parity, Int16 ini16StoppBits);
    [DllImport("GMH3x32E.dll")]
    public static extern Int16 GMH_CloseCom();
    [DllImport("GMH3x32E.dll")]
    public static extern UInt32 GetAdditionalDelay();
    [DllImport("GMH3x32E.dll")]
    public static extern UInt32 SetAdditionalDelay(UInt32 inui32AdditionalDelay);
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
    unsafe public static extern Int16 GMH_GetStatusMessage(UInt32 inui32StatusCode, byte* outcarrStatusText);
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_GetErrorMessageFL(double indblErrorCode, byte* outcarrErrorText);
    [DllImport("GMH3x32E.dll")]
    public static extern Int16 GMH_GetVersionNumber();
    [DllImport("GMH3x32E.dll")]
    unsafe public static extern Int16 GMH_ReadLogger(Int16 ini16Adresse, UInt32 inui32FileNumber, UInt32* ini32ptrNumberOfData, double* inOADateptrStartDate, double* outdblarrLoggerData);
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

    private void ReadCOMPorts()
    {
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
        }
    }

    private double ReadCurrentDisplayValue()
    {
        Int16 i16Priority;
        double dblFloatValue;
        Int32 i32IntergerValue;
        i16GMHAddress = 1;

        // Read COM Port
        this.i16ErrorCode = UniversalOpenCom(this.i16ComPortNummer, 38400, 10, 0, 0); /*Try GMH 5000*/
        unsafe
        {
            //this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, (Int16)GHM_TransmitFunktion.AnzeigewertLesen, &i16Priority, &dblFloatValue, &i32IntergerValue);
            this.i16ErrorCode = GMH_Transmit(this.i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadDisplayUnit, &i16Priority, &dblFloatValue, &i32IntergerValue);
        }
        if(i16ErrorCode < 0)
        {
            return 0.0;
        }

        return dblFloatValue;      
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

        //  var reply = ParseAndValidateDegasserResponse(response1, response2);

        //Interact with the GMH3x32.dll and read the vaccum meter value (external value).

        // Read COMPORT
        ReadCOMPorts();

        // Read Device Type
        ReadDeviceType();

        // Read out external pressure sensor
        // <external_value>
        double externalValue = ReadCurrentDisplayValue(); //reply.externalVal;// needs to be read from barometer
        double Value = Convert.ToDouble(response2);    //reply.Val;

        // Calculate <offset> = <external_value> - <value>
        double Offset = externalValue - Value;   // Resolution: 0.01 mbar

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

    private void ReadDeviceType()
    {
        Int16 i16Priority = 0;
        double dblFloatValue = 0;
        Int32 i32IntegerValue = 0;
        Int16 i16Length = -1;
        byte[] barrTypeString = new byte[1024];
        System.Text.UTF7Encoding encTextEncoding = new System.Text.UTF7Encoding();

        i16GMHAddress = 1;
        unsafe
        {
            this.i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadIDNumber, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode < 0)
        {
           // this.DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
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
            i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadMeasurementRangeAndType, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode == -38) // (Int16)Fehlermeldungen.NegativeQuittung)
        {
            unsafe
            {
                i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadDisplayMeasurementType, &i16Priority, &dblFloatValue, &i32IntegerValue);
            }
        }
        if (i16ErrorCode < 0)
        {
            //DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
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
            i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadMeasurementRangeUnit, &i16Priority, &dblFloatValue, &i32IntegerValue);
        }
        if (i16ErrorCode == -38) //(Int16)Fehlermeldungen.NegativeQuittung)
        {
            unsafe
            {
                i16ErrorCode = GMH_Transmit(i16GMHAddress, (Int16)GHM_TransmitFunktion.ReadDisplayUnit, &i16Priority, &dblFloatValue, &i32IntegerValue);
            }
        }
        if (i16ErrorCode < 0)
        {
            //this.DisplayErrorMessage(this.i16ErrorCode, i32IntegerValue, dblFloatValue);
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
                if (token.Length > Handler.INDEX_ZERO && token[Handler.INDEX_ZERO] == DegasserParameterNames.DEGASSER_PRESSURE_VAL) //need to check
                {
                    string key = token[Handler.INDEX_ZERO];
                    Response = token[Handler.INDEX_ONE];

                    return Response;
                   // if (!string.IsNullOrEmpty(Response) && Response == expectedValue) return Response;
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