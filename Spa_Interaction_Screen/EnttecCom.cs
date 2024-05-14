using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ENTTEC.Devices;
using ENTTEC.Devices.Data;
using System.IO.Ports;
using System.Diagnostics;
using ENTTEC.Devices.MessageHandlers;
using System.Windows.Forms;
using Test;
using static QRCoder.PayloadGenerator;

namespace Spa_Interaction_Screen
{
    public static class EnttecCom
    {
        private static bool triedtoopen = false;
        public static void sendDMX(byte[] channels)
        {
            if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
            {
                Logger.Print("No Enttec USB Device Found", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
            }
            OpenDMX.setDMXValues(channels);
            //Logger.Print($"Enttec written bytes: {OpenDMX.bytesWritten}", Logger.MessageType.Licht, Logger.MessageSubType.Information);
        }

        public static bool isopen()
        {
            return OpenDMX.status == FT_STATUS.FT_OK && triedtoopen;
        }

        public static FT_STATUS getstate()
        {
            return OpenDMX.status;
        }

        public static bool connect()
        {
            try
            {
                OpenDMX.start();//find and connect to devive (first found if multiple)
                triedtoopen = true;
            }
            catch (Exception exp)
            {
                MainForm.currentState = 6;
                Logger.Print(exp.Message, Logger.MessageType.Licht, Logger.MessageSubType.Error);
                Logger.Print("Error Connecting to Enttec USB Device", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
                triedtoopen = false;
                return false;
            }
            if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
            {
                //update status
                Logger.Print("No Enttec USB Device Found", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
                return false;
            }
            else if (OpenDMX.status == FT_STATUS.FT_OK)
            {
                Logger.Print("Found DMX on USB", Logger.MessageType.Licht, Logger.MessageSubType.Information);
            }
            else
            {
                MainForm.currentState = 6;
                Logger.Print("Error Opening Device", Logger.MessageType.Licht, Logger.MessageSubType.Error);
                return false;
            }
            return true;
        }
    }
}
