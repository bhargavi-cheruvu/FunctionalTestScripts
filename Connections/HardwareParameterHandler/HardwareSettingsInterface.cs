using System.Runtime.Remoting.Messaging;
using UniversalBoardTestApp;

public class Parser
{
    public void Start()
    {
        try
        {
            // HardwareParameter Object with all attributes, which can be used to describe them.
            // HardwareParameter will be sorted to connection, which is stated in the config.xml
            // All but name and connection attributes are optional. You only need to use the attributes you want/need to.

            // Digital I/O
            Add(new HardwareParameter() { Name = BoardParameterName.InitialState, DeviceType = "USB", ConversionFormula = "None" });
            Add(new HardwareParameter() { Name = BoardParameterName.Relay1State, DeviceType = "USB", ConversionFormula = "None" });
            Add(new HardwareParameter() { Name = BoardParameterName.Relay2State, DeviceType = "USB", ConversionFormula = "None" });
            Add(new HardwareParameter() { Name = BoardParameterName.Relay3State, DeviceType = "USB", ConversionFormula = "None" });
            Add(new HardwareParameter() { Name = BoardParameterName.Relay4State, DeviceType = "USB", ConversionFormula = "None" });
            
            // LEDBar
            Add(new HardwareParameter() { Name = BoardParameterName.LedBarForceColor, DeviceType = "USB", ConversionFormula = "IsNeeded" });
           
            // KeyPad
            Add(new HardwareParameter() { Name = BoardParameterName.KeyPadTestMode, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.KeyPad, DeviceType = "USB", ConversionFormula = "IsNeeded" });

            // Service Level
            Add(new HardwareParameter() { Name = BoardParameterName.ServiceLevel, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.ServiceLevelCode, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.ServiceLevelLock, DeviceType = "USB", ConversionFormula = "NotNeeded" });

            // FAN
            Add(new HardwareParameter() { Name = BoardParameterName.Fan1Pwm, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.Fan2Pwm, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.Fan1Speed, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.Fan2Speed, DeviceType = "USB", ConversionFormula = "IsNeeded" });

            // Backup
            Add(new HardwareParameter() { Name = BoardParameterName.BackupMemory, DeviceType = "USB", ConversionFormula = "None" });

            // CAN
            Add(new HardwareParameter() { Name = BoardParameterName.CANE0NmtStop, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.CANE0NmtStart, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.CANE0SdoSW, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.CANE0SdoSB, DeviceType = "USB", ConversionFormula = "IsNeeded" });

            // Degasser            
            Add(new HardwareParameter() { Name = BoardParameterName.Degasser_CMD, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.Degasser_PRESSURE, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.Degasser_PRESSURE_OFFSET, DeviceType = "USB", ConversionFormula = "IsNeeded" });

            // Leak Sensor
            Add(new HardwareParameter() { Name = BoardParameterName.LeakSensorCalibrate, DeviceType = "USB", ConversionFormula = "NotNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.LeakSensorCalibOffset, DeviceType = "USB", ConversionFormula = "IsNeeded" });
            Add(new HardwareParameter() { Name = BoardParameterName.MuteAlarm, DeviceType = "USB", ConversionFormula = "None" });

            // Compute Module
            Add(new HardwareParameter() { Name = BoardParameterName.TestDCFAlignment, DeviceType = "USB", ConversionFormula = "NotNeeded" });//IsNeeded
        }
        catch
        {
            // do nothing
        }
    }

    public void Add(HardwareParameter parameter)
    {
        UniversalBoardTestApp.XmlModels.FunctionalTest functionalTest = ScriptHelper.GetFunctionalTest();
        parameter.Connection = ScriptHelper.GetConnectionName();

        functionalTest.HardwareParameters.listHardwareParameters.Add(parameter);
    }
}

public class BoardParameterName
{
    //Digital I/O
    public const string InitialState = "Initial.State";
    public const string Relay1State = "Relay1.State";
    public const string Relay2State = "Relay2.State";
    public const string Relay3State = "Relay3.State";
    public const string Relay4State = "Relay4.State";

    // LEDBar
    public const string LedBarForceColor = "LedBar.ForceColor";

    // FAN
    public const string Fan1Pwm = "Fan1Pwm";
    public const string Fan2Pwm = "Fan2Pwm";
    public const string Fan1Speed = "Fan1Speed";
    public const string Fan2Speed = "Fan2Speed";

    // KeyPad
    public const string KeyPadTestMode = "Key.TestMode"; // need to check
    public const string KeyPad = "Keys"; // need to check

    //ServiceLevel
    public const string ServiceLevel = "Service.Challenge";
    public const string ServiceLevelCode = "Service.Code";
    public const string ServiceLevelLock = "Service.Lock";

    //Backup
    public const string BackupMemory = "BackupMemory.Status";

    //CAN
    public const string CANE0NmtStop = "CanE0.NmtStop";
    public const string CANE0NmtStart = "CanE0.NmtStart";
    public const string CANE0SdoSW = "CanE0.SdoSW";
    public const string CANE0SdoSB = "CanE0.SdoSB";

    public const string CANLeftConnector = "0x11"; // need to check
    public const string CANRightConnector = "0x12";

    // Degasser
    public const string Degasser_CMD = "Degasser";
    public const string Degasser_PRESSURE = "Degasser.Pressure";    
    public const string Degasser_PRESSURE_OFFSET = "Degasser.Pressure.Offset";    

    //Leak Sensor
    public const string LeakSensorCalibrate = "LeakSensor.Calibrate";
    public const string LeakSensorCalibOffset = "LeakSensor.CalibOffset";
    public const string MuteAlarm = "MuteAlarm";

    //Compute Module
    public const string TestDCFAlignment = "TestDCFAlignment";
}