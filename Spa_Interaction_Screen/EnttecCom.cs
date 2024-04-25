﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ENTTEC.Devices;
using ENTTEC.Devices.Data;
using System.IO.Ports;
using System.Diagnostics;
using ENTTEC.Devices.MessageHandlers;

namespace Spa_Interaction_Screen
{
    public class EnttecCom
    {
        private MainForm form;
        private Config config;
        private SerialPort port = null;
        private bool messageReceived = false;
        private bool error_while_receiving = false;
        private bool deviceregistered = false;

        public EnttecCom(MainForm f, Config c)
        {
            form = f;
            config = c;
            deviceregistered = connect();
        }


        private async Task waitforusb(SerialDataReceivedEventHandler ev, byte[] data)
        {
            messageReceived = false;
            error_while_receiving = false;
            port.DataReceived += ev;
            port.Write(data, 0, data.Length);
            int i = 0;
            while (!messageReceived&&i<Constants.waitfordmxanswer)
            {
                await Task.Delay(Constants.waittonextcheck).ConfigureAwait(false);
                i++;
            }
            if (i == Constants.waitfordmxanswer)
            {
                error_while_receiving = false;
                Debug.Print($"Couldnt receive an Answer for {i * Constants.waittonextcheck} ms");
            }
            port.DataReceived -= ev;
            error_while_receiving = false;
            messageReceived = false;
        }

        public void sendDMX(byte[] channels)
        {
            if(channels==null || channels.Length<=0 || port==null || !port.IsOpen)
            {
                Debug.Print("Error when tried to Send Enttec Data");
                return;
            }
            Debug.Print($"Sending {channels.Length} + startbyte to enttec DMX sender");
            byte[] temp = new byte[channels.Length+2];
            Buffer.BlockCopy(channels, 0, temp,1, channels.Length);
            temp = DmxUsbProUtils.CreatePacketForDevice(DmxUsbProConstants.SEND_DMX_PACKET_REQUEST_LABEL, temp);
            port.Write(temp, 0, temp.Length);
        }

        public bool isopen()
        {
            return deviceregistered && port.IsOpen;
        }
       
        public bool connect()
        {
            if (port == null)
            {
                try
                {
                    port = new SerialPort(config.EnttecComPort, 9600, Parity.None, 8, StopBits.One);
                }catch(IOException ex)
                {
                    Debug.Print(ex.Message);
                }
            }
            if(!port.IsOpen)
            {
                try
                {
                    port.Open();
                }
                catch (IOException ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print("Error when trying to Open Enttec Port");
                    form.currentState = 1;
                    return false;
                }
            }
            else
            {
                return true;
            }
            return true;
        }

        private void Port_ParamsDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                int bufferSize = 1024;
                Byte[] buffer = new Byte[bufferSize];
                var readData = port.Read(buffer, 0, bufferSize);
                IMessageDeviceHandler<DmxUsbProParamsType> handler = Fabric.Instance.GetDmxUsbProMessageHandler<DmxUsbProParamsType>();
                handler.HandleMessage(buffer);
                
                if (handler.MessageReady)
                {
                    DmxUsbProParamsType message = handler.GetMessage();
                    Console.WriteLine("---DMX USB PRO parameters---");
                    Console.WriteLine($"Firmware: {message.Firmware.FirmwareMSB}.{message.Firmware.FirmwareLSB}");
                    int breakTime = ConvertToUserValue(message.BreakTime);
                    Console.WriteLine($"Break time (microseconds): {breakTime}");
                    int mab = ConvertToUserValue(message.MaBTime);
                    Console.WriteLine($"Mark After break (MAB in microseconds): {mab}");
                    Console.WriteLine($"Packet refresh rate (per second): {message.RefreshRate}");
                    messageReceived = true;
                }
            }
        }

        private void Port_SerialDataReceived(Object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                int bufferSize = 1024;
                Byte[] buffer = new Byte[bufferSize];
                //var readData = port.Read(buffer, 0, bufferSize);
                IMessageDeviceHandler<DmxSerialNumberResponse> handler = Fabric.Instance.GetDmxUsbProMessageHandler<DmxSerialNumberResponse>();
                handler.HandleMessage(buffer);
                if (handler.MessageReady)
                {
                    DmxSerialNumberResponse message = handler.GetMessage();
                    Console.WriteLine("---DMX USB PRO Serialnumber---");
                    int Serial = message.Byte0MSB << (8 * 3) + message.Byte1 << (8 * 2) + message.Byte2 << (8 * 1) + message.Byte3LSB << (8 * 0);
                    Console.WriteLine($"{Serial}");
                    messageReceived = true;
                }
            }
        }

        Int32 ConvertToUserValue(Int32 deviceValue)
        {
            var converted = deviceValue * DmxUsbProConstants
                .DmxMultiplicationCoefficient;

            var result = (Int32)converted;

            return result;
        }
    }
}
