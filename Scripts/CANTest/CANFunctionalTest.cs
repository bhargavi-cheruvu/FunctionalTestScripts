﻿using LogViewManager;
using System;
using System.Threading;
using UniversalBoardTestApp;
using Helper;
public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    private byte LEFTNODE = 0x11;
    private byte RIGHTNODE = 0x12;
    
    public bool Start()
    {
        try
        {
            if (ScriptHelper.CheckIfProcedureIsCancelled())
                return false;

            //check Left CAN connector.
            if (!TestLeftCANConnectorNode())//LEFTNODE))
            {
                Logger.LogMessage(Level.Error, CANNodeParameters.LEFTCAN_CONNECTOR_FAILED);
                return false;
            }
            else
            {
                Logger.LogMessage(Level.Info, CANNodeParameters.LEFTCAN_CONNECTOR_PASSED);
            }

            if (!TestRightCANConnectorNode())//RIGHTNODE))
            {
                Logger.LogMessage(Level.Error, CANNodeParameters.RIGHTCAN_CONNECTOR_FAILED);
                return false;
            }
            else
            {
                Logger.LogMessage(Level.Info, CANNodeParameters.RIGHTCAN_CONNECTOR_PASSED);
            }

            Logger.LogMessage(Level.Passed, CANNodeParameters.CAN_TEST_COMPLETED);           
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogMessage(Level.Error, ex.ToString());            
            return false;
        }       
    }

    public bool TestLeftCANConnectorNode()//byte nodeId)
    {
        SendNmtStop(0x11);
        Wait(10.0); // increase the time to 2.0 from 1.0

        SendNmtStart(0x11);
        if (!WaitReady(60.0)) return false; // increase the time to 10.0 from 5.0
                
        //    if (!SendSdoSW(0x611, 0x2014, 0x01, 0x0c00)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    if (!SendSdoSB(0x611, 0x2011, 0x01, 0xFF)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    Logger.LogMessage(Level.Warning, CANNodeParameters.POWER_DOWN);
        //    if (!SendSdoSB(0x611, 0x6200, 0x01, 0x01)) return false;
       
        //    if (!SendSdoSW(0x612, 0x2014, 0x01, 0x0c00)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    if (!SendSdoSB(0x612, 0x2011, 0x01, 0xFF)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    Logger.LogMessage(Level.Warning, CANNodeParameters.POWER_DOWN);
        //    if (!SendSdoSB(0x612, 0x6200, 0x01, 0x01)) return false;
        
        //if (!WaitReconnect(CANNodeParameters.WaitForReconnectinSeconds)) return false;

        //Logger.LogMessage(Level.Info, CANNodeParameters.CONNECTION_REESTABLISHED);
        //SendNmtStop(nodeId);
        //Wait(1.0);

        //if(!OnDeviceReebootGoto()) return false;

        //Logger.LogMessage(Level.Info, CANNodeParameters.WAIT_FOR_BOOTPROCESS);
        //if(!WaitReady(CANNodeParameters.WaitForReconnectinSeconds)) return false;

        return true;
    }

    public bool TestRightCANConnectorNode()
    {
        SendNmtStop(0x11);
        Wait(10.0); // increase the time to 2.0 from 1.0

        SendNmtStop(0x12);
        Wait(10.0); // increase the time to 2.0 from 1.0

        SendNmtStart(0x12);
        if (!WaitReady(60.0)) return false; // increase the time to 10.0 from 5.0

        //    if (!SendSdoSW(0x611, 0x2014, 0x01, 0x0c00)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    if (!SendSdoSB(0x611, 0x2011, 0x01, 0xFF)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    Logger.LogMessage(Level.Warning, CANNodeParameters.POWER_DOWN);
        //    if (!SendSdoSB(0x611, 0x6200, 0x01, 0x01)) return false;
      
        //    if (!SendSdoSW(0x612, 0x2014, 0x01, 0x0c00)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    if (!SendSdoSB(0x612, 0x2011, 0x01, 0xFF)) return false;
        //    if (!WaitReady(5.0)) return false;

        //    Logger.LogMessage(Level.Warning, CANNodeParameters.POWER_DOWN);
        //    if (!SendSdoSB(0x612, 0x6200, 0x01, 0x01)) return false;
    
        //if (!WaitReconnect(CANNodeParameters.WaitForReconnectinSeconds)) return false;

        //Logger.LogMessage(Level.Info, CANNodeParameters.CONNECTION_REESTABLISHED);
        //SendNmtStop(nodeId);
        //Wait(1.0);

        //if(!OnDeviceReebootGoto()) return false;

        //Logger.LogMessage(Level.Info, CANNodeParameters.WAIT_FOR_BOOTPROCESS);
        //if(!WaitReady(CANNodeParameters.WaitForReconnectinSeconds)) return false;

        return true;
    }

    private void SendNmtStop(byte nodeId)
    {
        HardwareParameters.SetParameter(CANNodeParameters.LeftCAN_CMD_NMTSTOP, nodeId);
        Logger.LogMessage(Level.Info, $"NMT Start sent to Node {nodeId:X2}");
    }

    private void SendNmtStart(byte nodeId)
    {
        HardwareParameters.SetParameter(CANNodeParameters.LeftCAN_CMD_NMTSTART, nodeId);
        Logger.LogMessage(Level.Info, $"NMT Start sent to Node {nodeId:X2}");
    }
    private bool SendSdoSW(int nodeId, ushort index, byte subIndex, int Val)
    {
        Logger.LogMessage(Level.Info, $"SDO WriteWord to Node {nodeId:X2}: {index:X4}:{subIndex:X2} = {Val:X4}");
        return true;
    }

    private bool SendSdoSB(int nodeId, ushort index, byte subIndex, byte Val)
    {
        Logger.LogMessage(Level.Info, $"SDO WriteByte to Node {nodeId:X2}: {index:X4}:{subIndex:X2} = {Val:X2}");
        return true;
    }

    private void Wait(double seconds)
    {
        Thread.Sleep((int)(seconds * 1000));
    }

    private bool WaitReady(double seconds)
    {
        Logger.LogMessage(Level.Info, $"Waiting for device ready for up to {seconds} seconds...");
        Thread.Sleep((int)(seconds * 1000)); // Wait Time in milliseconds
        return true;
    }
    private bool WaitReconnect(double seconds)
    {
        Logger.LogMessage(Level.Info, $"Waiting for Reconnection (USB loss detection) for up to {seconds} seconds...");
        Thread.Sleep((int)(seconds * 1000)); // Wait Time in milliseconds
        return true;
    }

    private bool OnDeviceReebootGoto()
    {
        // check if previous USB disconnection was set or not.
        Logger.LogMessage(Level.Info, CANNodeParameters.WAIT_FOR_DEVICEREBOOT);
        return true;
    }
}