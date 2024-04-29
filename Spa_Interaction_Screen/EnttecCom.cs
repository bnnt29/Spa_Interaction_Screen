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
        private MainForm form;
        private Config config;
        private SerialPort port = null;
        private OpenDMX open;

        public EnttecCom(MainForm f, Config c)
        {
            form = f;
            config = c;
            open = new OpenDMX();
            connect();
        }

        public void sendDMX(byte[] channels)
        {
            if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
            {
                Debug.Print("No Enttec USB Device Found");
            }
            byte[] temp = new byte[config.DMXScenes[0].Channelvalues.Length+1];
            Buffer.BlockCopy(channels, 0, temp, 1, channels.Length);
            for(int i = 0; i < temp.Length; i++) 
            {
                OpenDMX.setDmxValue(i, temp[i]);
            }
            //Debug.Print($"Enttec written bytes: {OpenDMX.bytesWritten}");
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
                if (OpenDMX.status == FT_STATUS.FT_DEVICE_NOT_FOUND)
                {
                    //update status
                    Debug.Print("No Enttec USB Device Found");
                    return false;
                }
                else if (OpenDMX.status == FT_STATUS.FT_OK)
                {
                    Debug.Print("Found DMX on USB");
                }
                else
                {
                    Debug.Print("Error Opening Device");
                    return false;
                }

            }
            catch (Exception exp)
            {
                Console.WriteLine(exp);
                Debug.Print("Error Connecting to Enttec USB Device");
                return false;

            }
            return true;
        }
    }
}
