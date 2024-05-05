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
    public class EnttecCom
    {
        private MainForm form;
        private Config config;
        private SerialPort port = null;

        public EnttecCom(MainForm f, Config c)
        {
            form = f;
            config = c;
            connect();
        }

        public void sendDMX(byte[] channels)
        {
            if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
            {
                Logger.Print("No Enttec USB Device Found", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
            }
            byte[] temp = new byte[config.DMXScenes[0].Channelvalues.Length+1];
            Buffer.BlockCopy(channels, 0, temp, 0, channels.Length);
            for(int i = 0; i < temp.Length; i++) 
            {
                OpenDMX.setDmxValue(i, temp[i]);
            }
            //Log.Print($"Enttec written bytes: {OpenDMX.bytesWritten}");
        }

        public bool isopen()
        {
            return OpenDMX.status == FT_STATUS.FT_OK;
        }

        public FT_STATUS getstate()
        {
            return OpenDMX.status;
        }

        public bool connect()
        {
            try
            {
                OpenDMX.start();//find and connect to devive (first found if multiple)
            }
            catch (Exception exp)
            {
                MainForm.currentState = 5;
                Logger.Print(exp.Message, Logger.MessageType.Licht, Logger.MessageSubType.Error);
                Logger.Print("Error Connecting to Enttec USB Device", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
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
                MainForm.currentState = 5;
                Logger.Print("Error Opening Device", Logger.MessageType.Licht, Logger.MessageSubType.Error);
                return false;
            }
            return true;
        }
    }
}
