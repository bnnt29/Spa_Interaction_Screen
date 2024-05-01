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

namespace Spa_Interaction_Screen
{
    public class EnttecCom
    {
        private Logger Log;
        private MainForm form;
        private Config config;
        private SerialPort port = null;
        private OpenDMX open;

        public EnttecCom(MainForm f, Config c)
        {
            Log = f.Log;
            form = f;
            config = c;
            open = new OpenDMX(f.Log);
            connect();
        }

        public void sendDMX(byte[] channels)
        {
            if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
            {
                Log.Print("No Enttec USB Device Found", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
            }
            byte[] temp = new byte[config.DMXScenes[0].Channelvalues.Length+1];
            Buffer.BlockCopy(channels, 0, temp, 1, channels.Length);
            for(int i = 0; i < temp.Length; i++) 
            {
                open.setDmxValue(i, temp[i]);
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
                open.start();//find and connect to devive (first found if multiple)
                if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
                {
                    //update status
                    Log.Print("No Enttec USB Device Found", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
                    return false;
                }
                else if (OpenDMX.status == FT_STATUS.FT_OK)
                {
                    Log.Print("Found DMX on USB", Logger.MessageType.Licht, Logger.MessageSubType.Information);
                }
                else
                {
                    Log.Print("Error Opening Device", Logger.MessageType.Licht, Logger.MessageSubType.Error);
                    return false;
                }

            }
            catch (Exception exp)
            {
                Log.Print(exp.Message, Logger.MessageType.Licht, Logger.MessageSubType.Error);
                Log.Print("Error Connecting to Enttec USB Device", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
                return false;

            }
            return true;
        }
    }
}
