﻿using Helper;
using LogViewManager;
using System.Collections.Generic;
using System.IO;
using System.Management;
using UniversalBoardTestApp;

public class Test
{
    // Version of the script. Gets displayed in database/protocol
    private const string TestVersion = Handler.TEST_VERSION; // Version Number
    public bool Start()
    {
        if (ScriptHelper.CheckIfProcedureIsCancelled())
            return false;

        Logger.LogMessage(Level.Info, Handler.USBHubTest);
        var usbDrivesInfo = GetUSBDrivesInformation();

        if (usbDrivesInfo.Count == Handler.INDEX_ZERO)
        {
            Logger.LogMessage(Level.Info, Handler.NO_USB_DRIVES);
            return false;
        }
        else if (usbDrivesInfo.Count == Handler.INDEX_ONE)
        {
            Logger.LogMessage(Level.Info, Handler.ONE_USB_DETECTED);
            return false;
        }
        else if (usbDrivesInfo.Count == Handler.INDEX_TWO)
        {
            Logger.LogMessage(Level.Info, Handler.TWO_USB_DETECTED);
            return false;
        }
        else if (usbDrivesInfo.Count == Handler.INDEX_THREE)
        {
            Logger.LogMessage(Level.Success, Handler.THREE_USB_DETECTED);
            return true;
        }

        return true;
    }

    public class USBInfo
    {
        public string DriveLetter;
        public string VolumeLabel;
        public string SerialNumber;
        public string DeviceID;
        public string PNPDeviceID;
        public string Description;
        public long TotalSize;
        public long Freespace;
    }

    public List<USBInfo> GetUSBDrivesInformation()
    {
        var usbDrives = new List<USBInfo>();

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.Removable && drive.IsReady)
            {
                //logical drive to its device using WMI
                string driveLetter = drive.Name.Replace(Handler.OLD_VALUE, Handler.NEW_VALUE);

                string query = Handler.WIN32_LOGICALDISKTOPARTITION;
                var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject obj in searcher.Get())
                {
                    string dependent = obj[Handler.DEPENDENT]?.ToString() ?? Handler.NEW_VALUE;
                    string antecedent = obj[Handler.ANTECEDENT]?.ToString() ?? Handler.NEW_VALUE;

                    if (dependent.Contains(driveLetter))
                    {
                        string PartitionId = antecedent.Split(Handler.DELIMITER)[Handler.INDEX_ONE].Trim('"');
                        string diskQry = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID=\"{PartitionId}\"}} WHERE AssocClass = Win32_DiskDriveToDiskPartition";
                        using (var diskSearcher = new ManagementObjectSearcher(diskQry))
                        {
                            foreach (ManagementObject disk in diskSearcher.Get())
                            {
                                string pnpDeviceId = disk[Handler.PNPDEVICEID]?.ToString();
                                string deviceId = disk[Handler.DEVICEID]?.ToString();
                                string serial = GetUSBSerialFromPNP(pnpDeviceId);
                                string description = disk[Handler.MODEL]?.ToString() ?? Handler.NEW_VALUE;

                                usbDrives.Add(new USBInfo
                                {
                                    DriveLetter = drive.Name,
                                    VolumeLabel = drive.VolumeLabel,
                                    SerialNumber = serial,
                                    DeviceID = deviceId,
                                    PNPDeviceID = pnpDeviceId,
                                    Description = description,
                                    TotalSize = drive.TotalSize,
                                    Freespace = drive.TotalFreeSpace
                                });
                            }
                        }
                    }
                }
            }
        }
        return usbDrives;
    }

    private string GetUSBSerialFromPNP(string pnpDeviceId)
    {
        if (string.IsNullOrEmpty(pnpDeviceId)) return Handler.UNKNOWN;

        var tokens = pnpDeviceId.Split(Handler.PATH_DELIMITER); // last token denotes serial number for USB storage devices.
        return tokens.Length >= Handler.INDEX_THREE ? tokens[Handler.INDEX_TWO] : Handler.UNKNOWN;
    }
}