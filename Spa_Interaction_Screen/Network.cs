using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using ENTTEC.Devices.MessageFilters;
using System.Diagnostics.Eventing.Reader;
using static Spa_Interaction_Screen.Constants;
using static System.Collections.Specialized.BitVector32;
using System.Text.RegularExpressions;

namespace Spa_Interaction_Screen
{
    public class Network
    {
        private Logger Log;
        private MainForm form;
        private Config config;
        public List<TcpClient> tcpClients = new List<TcpClient>();
        private Task connectClients = null;
        public List<Task> ListeningClients = new List<Task>();
        private TcpClient routerclient = null;
        private Task ListenRouter = null;
        public bool routertelnetinit = false;

        public Network(MainForm f, Config c)
        {
            Log = f.Log;
            form = f;
            config = c;
            StartTCPServer();
        }

        public void changeconfig(Config c)
        {
            config = c;
        }
       
        //Structures 
        public class RequestJson
        {
            public String? destination = null;
            public int? port = null;
            public String type;
            public int Raum;
            public String? label = null;
            public int? id = null;
            public String[]? values = null;
        }

        //Prepare Send
        private static String assembleJsonString(RequestJson req)
        {
            String s = "{";
            s += $"\"type\":{req.type}";
            s += $",\"room\":{req.Raum}";
            if (req.label != null)
            {
                s += $",\"label\":{req.label}";
            }
            if (req.id != null)
            {
                s += $",\"id\":{req.id}";
            }
            if (req.values != null)
            {
                s += $"\",\"values\":[{req.values[0]}";
                for (int i = 1; i < req.values.Length; i++)
                {
                    s += $",{req.values[i]}";
                }
                s += "]";
            }
            s += "}";
            return s;
        }
        private static String assembleQuestionJsonString(RequestJson req)
        {
            String s = "[";
            s += $"\"type\":{req.type}";
            s += $",\"room\":{req.Raum}";
            if (req.label != null)
            {
                s += $",\"label\":{req.label}";
            }
            if (req.id != null)
            {
                s += $",\"id\":{req.id}";
            }
            if (req.values != null)
            {
                s += $"\",\"values\":[{req.values[0]}";
                for (int i = 1; i < req.values.Length; i++)
                {
                    s += $",{req.values[i]}";
                }
                s += "]";
            }
            s += "]";
            return s;
        }

        private TcpClient getbestclient(string? dest, int? port)
        {
            TcpClient? cl = tcpClients[0];
            if (tcpClients.Count > 1)
            {
                byte[] ip = new byte[4];
                try
                {
                    if(dest == null || dest.Length <= 0)
                    {
                        for (int i = 0; i < config.IPZentrale.Length; i++)
                        {
                            ip[i] = Byte.Parse(config.IPZentrale[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < dest.Split('.').Length; i++)
                        {
                            ip[i] = Byte.Parse(dest.Split('.')[i]);
                        }
                    }
                    
                }
                catch (FormatException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                }

                List<TcpClient> possibilities = new List<TcpClient>();
                IPAddress specifiedIP = new IPAddress(ip);
                foreach (TcpClient client in tcpClients)
                {
                    if (client != null)
                    {
                        IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                        if (endpoint.Address.Equals(specifiedIP))
                        {
                            possibilities.Add(client);
                            if (((port != null && port > 0 && endpoint.Port.Equals(port))||(config.PortZentrale != null && config.PortZentrale > 0 && endpoint.Port.Equals(config.PortZentrale))) && client.Connected)
                            {
                                return client;
                            }
                        }
                    }
                }
                if (possibilities.Count > 0)
                {
                    for (int i = 0; i < possibilities.Count; i++)
                    {
                        if (!possibilities[i].Connected)
                        {
                            possibilities[i].Close();
                            possibilities.RemoveAt(i);
                            tcpClients.Remove(possibilities[i]);
                            i--;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    while (i < tcpClients.Count && !tcpClients[i].Connected)
                    {
                        i++;
                    }
                    if (i >= tcpClients.Count)
                    {
                        return null;
                    }
                    return tcpClients[i];
                }
                cl = possibilities[0];
            }
            return cl;
        }


        //UDP
        /*
        public Task UDPReceiver()
        {
            Task t = Task.Run(async () =>
            {
                // This constructor arbitrarily assigns the local port number.
                UdpClient udpClient = new UdpClient(Constants.UDPReceivePort);

                //IPEndPoint object will allow us to read datagrams sent from any source.

                // Blocks until a message returns on this socket from a remote host.
                UdpReceiveResult result;
                while (true)
                {
                    result = await udpClient.ReceiveAsync();
                    Byte[] receivedBytes = result.Buffer;
                    Log.Print($"{Encoding.ASCII.GetString(receivedBytes)}");
                    if (receivedBytes != null && receivedBytes.Length > 0)
                    {
                        handleReceivedUDP(parse(receivedBytes));
                    }
                    else
                    {
                        Log.Print("Recaived UDP Paket with no Json Content");
                    }
                }
            });
            t.Start();
            return t;
        }
        public static void UDPSender(RequestJson req, bool debug)
        {
            if (debug)
            {
                Log.Print($"Send {req.type} Request to {req.destination}:{req.port} with {req.label}, {req.id}");
            }
            UdpClient client = new UdpClient();
            IPAddress serverAddr = IPAddress.Parse(req.destination);
            IPEndPoint endPoint = new IPEndPoint(serverAddr, req.port);
            string text = assembleJsonString(req);
            byte[] send_buffer = Encoding.ASCII.GetBytes(text);
            int num = 0;
            try
            {
                num = client.Send(send_buffer, send_buffer.Length, endPoint);
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
            }
            Log.Print($"[{req.type}]Bytes send via UDP: {num}");
            Log.Print(text);
        }
        */

        //TCP
        public bool SendTCPMessage(RequestJson json, TcpClient? cl)
        {
            if (tcpClients == null || tcpClients.Count <= 0)
            {
                return false;
            }
            if (cl == null)
            {
                cl = getbestclient(json.destination, json.port);
            }
            if (cl == null)
            {
                Log.Print("No connected Clients",Logger.MessageType.TCPSend, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream stream = cl.GetStream();
            string text = assembleJsonString(json);
            byte[] send_buffer = Encoding.Default.GetBytes(text);
            try
            {
                stream.Write(send_buffer, 0, send_buffer.Length);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        Log.Print(ex.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            return true;
        }

        private void StartTCPServer()
        {
            TcpListener server = new TcpListener(IPAddress.Any, config.LocalPort);
            server.Start();
            connectClients = Task.Run(() => acceptClientsLoop(this, server));
        }

        private void acceptClientsLoop(Network net, TcpListener server)
        {
            while (net.form.RunTask)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it
                net.tcpClients.Add(client);
                net.ListeningClients.Add(Task.Run(() => listenTCPConnection(client, net, false, ListeningClients.Count)));
                IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                Log.Print($"Client with IP:{endpoint.Address} connected", [Logger.MessageType.TCPReceive, Logger.MessageType.TCPSend], Logger.MessageSubType.Information);
            }
        }

        private void listenTCPConnection(TcpClient cl, Network net, bool isTelnet, int index)
        {
            NetworkStream ns = cl.GetStream();
            Log.Print("ListeningTCP", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            while (cl.Connected && net.form.RunTask)  //while the client is connected, we look for incoming messages
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;
                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                net.receivedMessage(msg, cl, isTelnet, net, index);
            }
            net.tcpClients.Remove(cl);
            cl.Dispose();
        }

        //Telnet
        
        public bool SendTelnetASKPass()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Log.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream ns = routerclient.GetStream();

            Byte[] data = Encoding.Default.GetBytes("pass ?\n");
            try
            {
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            //Log.Print("pass ?");
            return true;
        }

        public bool SendTelnetSetSSID(String SSID)
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Log.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream ns = routerclient.GetStream();

            Byte[] data = Encoding.Default.GetBytes($"wifi ssid {SSID}\n");
            try
            {
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            ns = routerclient.GetStream();
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout)
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;

                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            return true; //true, because we do not really need to read anything and the router commonly closes the connection without answer, when the new SSID equals the old SSID
                    }
                }

                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                String[] splt = m.Split("\n");
                for (int i = 0; i < splt.Length; i++)
                {
                    if (splt[i].Contains("WMedia") || splt[i].Contains("wifi"))
                    {
                        return true;
                    }
                }
            }
            Log.Print($"wifi ssid {SSID}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            return true;
        }

        public bool SendTelnetWakeup()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Log.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream ns = routerclient.GetStream();

            Byte[] data = Encoding.Default.GetBytes($"standby wakeup\n");
            try
            {
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            ns = routerclient.GetStream();
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout)
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;

                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            return true; //true, because we do not really need to read anything and the router commonly closes the connection without answer, when the new SSID equals the old SSID
                    }
                }

                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                String[] splt = m.Split("\n");
                for (int i = 0; i < splt.Length; i++)
                {
                    if (splt[i].Contains("WMedia") || splt[i].Contains("wifi"))
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        public bool SendTelnetPasswordrefresh()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Log.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream ns = routerclient.GetStream();

            Byte[] data = Encoding.Default.GetBytes($"pass refresh\n");
            try
            {
                ns.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            ns = routerclient.GetStream();
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout)
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;

                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Log.Print(m);
                String[] splt = m.Split("\n");
                for (int i = 0; i < splt.Length; i++)
                {
                    if (splt[i].Contains("OK"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public String WaitForPass()
        {
            NetworkStream ns = routerclient.GetStream();
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout)
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;
                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Log.Print(m);
                String[] splt = m.Split("\n");
                int i = 0;
                for (i = i; i < splt.Length - 1; i++)
                {
                    if (splt[i].Contains("#pass") && splt[i].Contains(':'))
                    {
                        //Log.Print(splt[i].Split(':')[1].Trim());
                        return splt[i].Split(':')[1].Trim();
                    }
                }
            }
            return "";
        }

        public void setuprouterpassword(MainForm f)
        {
            if (!connectrouter(f))
            {
                Log.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            if (!SendTelnetASKPass())
            {
                Log.Print("Error occured while Asking Router for Password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
            }
            
            config.password = WaitForPass();
            //Log.Print($"old Password: {config.password}");
            WaitforRouterClientClose();
            if (!connectrouter(f))
            {
                Log.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            if (!SendTelnetPasswordrefresh())
            {
                Log.Print("Error occured while refreshing router password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            WaitforRouterClientClose();
            String npw = "";
            DateTime dateTimestart = DateTime.Now;
            do
            {
                //Log.Print("Waiting for new Password");
                if (!connectrouter(f))
                {
                    Log.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                    WaitforRouterClientClose();
                }
                if (!SendTelnetASKPass())
                {
                    Log.Print("Error occured while Asking Router for Password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                }
                npw = WaitForPass();
            } while (npw.Equals(config.password) && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout * 2);
            WaitforRouterClientClose();
            config.password = npw;
            Log.Print($"Neues Passwort: {npw}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            f.helper.setnewPassword();
        }
        public void setuprouterssid(MainForm f)
        {
            if (!connectrouter(f))
            {
                Log.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                return;
            }
            if (!SendTelnetSetSSID($"{config.WiFiSSID}"))
            {
                Log.Print("Error occured while Setting new Router SSID", Logger.MessageType.Router, Logger.MessageSubType.Notice);

            }
            WaitforRouterClientClose();
        }

        public void wakeup(MainForm f)
        {
            if (!connectrouter(f))
            {
                Log.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                return;
            }
            if (!SendTelnetWakeup())
            {
                Log.Print("Error occured while tried to wakeup Router", Logger.MessageType.Router, Logger.MessageSubType.Notice);

            }
            WaitforRouterClientClose();
        }

        private void WaitforRouterClientClose()
        {
            if(routerclient == null)
            {
                return;
            }
            if (routerclient.Connected)
            {
                routerclient.GetStream().Close();
                routerclient.Close();
            }
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout) ;
            routerclient.Dispose();
        }

        private bool handleReceivedTel(String m)
        {
            Log.Print($"Received Message: {m}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            if (m.Contains("#pass"))
            {
                String[] lines = m.Split("WMedia>");
                bool foundline = false;
                foreach (String line in lines)
                {
                    if (line.Contains('#') && line.Contains("pass") && line.Contains(":"))
                    {
                        String p = line.Split(":")[1].Split("\n\r")[0].Trim();
                        config.password = p;
                        foundline = true;
                        break;
                    }
                }
                if (!foundline)
                {
                    return false;
                }
                form.helper.setnewPassword();
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool connectrouter(MainForm f)
        {
            if (routerclient != null && routerclient.Connected)
            {
                return true;
            }
            String ip_address = f.ArraytoString(config.IPRouter, 4);
            int port_number = config.PortRouter;
            NetworkStream stream = null;
            String response = null;
            try
            {
                routerclient = new TcpClient();
                routerclient.Connect(ip_address, port_number);

                Log.Print($"Success connecting to: {ip_address}, port: {port_number}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            }
            catch (Exception e)
            {
                Log.Print(e.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                Log.Print($"Failed while connecting to: {ip_address}, port: {port_number}", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                routerclient = null;
            }
            if (routerclient == null || !routerclient.Connected)
            {
                Log.Print("Error when tried to connect to ecler Router via Telnet over TCP\nCannot change Password or name remotely nor display the current password.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            bool receivedadd = false;
            NetworkStream ns = routerclient.GetStream();
            DateTime dateTimestart = DateTime.Now;
            while (routerclient.Connected && (DateTime.Now-dateTimestart).TotalSeconds <= Constants.TelnetComTimeout)
            {

                byte[] msg = new byte[1024];     //the messages arrive as byte array
                int b_read = 0;

                try
                {
                    b_read = ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            Log.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Log.Print(m);
                String[] splt= m.Split("\n");
                int i = 0;
                for (i = i; i< splt.Length-1; i++)
                {
                    if (!receivedadd && splt[i].Contains('+'))
                    {
                        receivedadd = true;
                    }
                }
                if (receivedadd && splt.Length > i && splt[i].EndsWith("WMedia>"))
                {
                    //Log.Print("Telnet Terminal: ");
                    return true;
                }
            }
            return false;
        }

        //Received data
        private void handleReceivedNet(Dictionary<String, Object>? Json)
        {
            if (Json == null)
            {
                return;
            }
            if (!(Json.ContainsKey("type") && Json["type"] != null))
            {
                Log.Print("Invalid Json received: missing \"type\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if(!(Json.ContainsKey("room") && Json["room"] != null) || (Int64)Json["room"] != config.Room)
            {
                Log.Print("Invalid Json received: missing or wrong \"room\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
            bool processedjson = false;
            int x = -1;
            if (int.TryParse(((String)Json["type"]).Trim().ToLower(), out x))
            {
                processedjson = indexsjsontypewitch(Json, x);
            }
            else
            {
                switch (((String)Json["type"]).Trim().ToLower())
                {
                    case "status":
                        Log.Print("This Paket type does not belog here (Paket: Status). Ignoring it", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        processedjson = true;
                        break;
                    default:
                        for (int i = 0; i < config.Typenames.Length && !processedjson; i++)
                        {
                            if (config.Typenames[i] != null && config.Typenames[i].Trim().ToLower().Equals(((string)Json["type"]).Trim().ToLower()))
                            {
                                processedjson = indexsjsontypewitch(Json, i);
                            }
                        }
                        break;
                }
            }
            if (!processedjson)
            {
                Log.Print("Json could be related to a given \"type\".", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void SystemPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            int index = -1;
            for (int i = 0; i < c.SystemSettings.Count; i++)
            {
                Constants.SystemSetting ss = c.SystemSettings[i];
                if (ss.JsonText.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
                {
                    index = i; 
                    break;
                }
            }
            if(index <=0 && c.DMXSceneSettingJson.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = c.SystemSettings.Count;
            }
            if (index <= 0 && c.VolumeJson.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = c.SystemSettings.Count + 1;
            }
            if (index <= 0 && ("block").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = c.SystemSettings.Count + 2;
            }
            if (index <= 0 && ("video").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = c.SystemSettings.Count + 3;
            }
            if (index <= 0 && ("?").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = c.SystemSettings.Count + 4;
            }
            switch (index) 
            {
                case 0:
                    Log.Print("Message received Working Normally. Nothing changed", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                    break;
                case 1:
                    Log.Print("Message received: Resetting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.reset();
                    break;
                case 2:
                    Log.Print("Message received: Restarting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.Restart();
                    break;
                case 3:
                    Log.Print("Message received: Shutdown", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.Shutdown();
                    break;
                case 4:
                    int sceneindex = -1;
                    if (json.ContainsKey("id") && json["id"] != null)
                    {
                        sceneindex = (Int32)(Int64)json["id"];
                    }
                    else if (json.ContainsKey("values") && json["values"] != null)
                    {
                       if (((string[])json["values"])[0] != null && ((string[])json["values"])[0].Length >= 0)
                        {
                            try
                            {
                                sceneindex = Int32.Parse(((string[])json["values"])[0]);
                            }
                            catch (FormatException ex)
                            {
                                Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            }
                            if (sceneindex <= 0)
                            {
                                String name = ((string[])json["values"])[0];
                                for (int i = 0; i < c.DMXScenes.Count; i++)
                                {
                                    if (name.Trim().ToLower().Equals(c.DMXScenes[i].JsonText.Trim().ToLower()))
                                    {
                                        sceneindex = i;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene to). Values has to be an Array with Scene id or name in first index", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Log.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene to).", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    if (sceneindex < 0 || sceneindex >= c.DMXScenes.Count)
                    {
                        Log.Print("Index / value outside Scenes Range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        break;
                    }
                    f.Ambiente_Change(c.DMXScenes[sceneindex], true, false, false);
                    break;
                case 5:
                    int volumevalue = -1;
                    if (json.ContainsKey("id") && json["id"] != null)
                    {
                        volumevalue = (int)json["id"];
                    }
                    else if (json.ContainsKey("values") && json["values"] != null)
                    {
                        if (((string[])json["values"])[0] != null && ((string[])json["values"])[0].Length >= 0)
                        {
                            try
                            {
                                sceneindex = Int32.Parse(((string[])json["values"])[0]);
                            }
                            catch (FormatException ex)
                            {
                                Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            }
                        }
                        else
                        {
                            Log.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to. Values has to be an Array with Scene id or name in first index", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Log.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    if(volumevalue < 0 || volumevalue > 100)
                    {
                        Log.Print("index / value outside Volume Range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        break;
                    }
                    f.AmbientVolume(volumevalue, null, null);
                        break;
                    case 6:
                    if (json.ContainsKey("id") && json["id"] != null)
                    {
                        if ((int)json["id"] == 0)
                        {
                            f.setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                        }
                        else
                        {
                            f.setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                        }
                    }else if (json.ContainsKey("values") && json["values"] != null)
                    {
                        try
                        {
                            bool p = Boolean.Parse(((string[])json["values"])[0]);
                            if (!p)
                            {
                                f.setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                            }
                            else
                            {
                                f.setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                            }
                        }
                        catch(FormatException ex){
                            Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            if (((int[])json["values"])[0] == 0)
                            {
                                f.setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                            }
                            else
                            {
                                f.setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                            }
                        }
                    }
                    else
                    {
                        Log.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene block", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    break;
                    case 7:
                    if (json["values"].GetType() == typeof(string[]))
                    {
                        String Path = ((string[])json["values"])[1];
                        c.finalizePaths(out Path, Path);
                        if (File.Exists(Path))
                        {
                            if (f.vlc != null)
                            {
                                f.vlc.changeMedia(Path, false);
                                Log.Print($"Projecting Path {Path}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                            }
                        }
                        else
                        {
                            Log.Print("No File found for the given Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Log.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[] or string[])", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    break;
                case 8:
                    String ret = "";
                    foreach(Constants.SystemSetting ss in c.SystemSettings)
                    {
                        RequestJson j = new RequestJson();
                        j.id = ss.id;
                        j.label = ss.JsonText;
                        String[] t = new string[1];
                        if (ss.value != null && ss.value.Length > 0)
                        {
                            t = new string[2];
                            t[1] = ss.value;
                            j.values = t;
                        }
                        t[0] = ss.ShowText;
                        ret += assembleQuestionJsonString(j);

                    }
                    RequestJson r = new RequestJson();
                    r.Raum = c.Room;
                    r.type = c.Typenames[0];
                    r.label = "?";
                    r.values = [ ret ];
                    SendTCPMessage(r, null);
                    break;
                default:
                    Log.Print("Couldn't recognize \"type\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    break;
            }
        }
        private void TCPPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && json["label"] != null)
            {
                if (((string)json["label"]).Equals("?"))
                {
                    String ret = "";
                    foreach (Constants.TCPSetting ss in c.TCPSettings)
                    {
                        RequestJson j = new RequestJson();
                        j.id = ss.id;
                        j.label = ss.JsonText;
                        String[] t = new string[1];
                        if (ss.value != null && ss.value.Length > 0)
                        {
                            t = new string[2];
                            t[1] = ss.value;
                            j.values = t;
                        }
                        t[0] = ss.ShowText;
                        ret += assembleQuestionJsonString(j);

                    }
                    RequestJson r = new RequestJson();
                    r.Raum = c.Room;
                    r.type = c.Typenames[0];
                    r.label = "?";
                    r.values = [ret];
                    SendTCPMessage(r, null);
                    return;
                }
                if (((string[])json["values"]).Length > 2)
                {
                    foreach (Constants.TCPSetting tcp in c.TCPSettings)
                    {
                        edittcpwithjsonvalusarraydata(json, f, c, tcp);
                    }
                }
                else
                {
                    Log.Print("Not enough Arguments provided to edit existing TCP Button (3-4 required)", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
                return;
            }else if(c.TCPSettings.Count <= Constants.maxtcpws)
            {
                if (((string[])json["values"]).Length > 2)
                {
                    Constants.TCPSetting tcp = new Constants.TCPSetting();
                    edittcpwithjsonvalusarraydata(json, f, c, tcp);
                }
                else
                {
                    Log.Print("Not enough Arguments provided to create new TCP Button (3-4 required)", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            else
            {
                Log.Print("Already reached max number of TCP Wartungs Buttons and it was no label given to edit TCP button", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void SessionPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && json["label"] != null && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.SessionSetting ss in c.SessionSettings)
                {
                    RequestJson j = new RequestJson();
                    j.id = ss.id;
                    j.label = ss.JsonText;
                    String[] t = new string[3];
                    t[0] = ss.ShowText;
                    t[1] = ss.mins.ToString();
                    t[2] = ss.should_reset.ToString();
                    j.values = t;
                    ret += assembleQuestionJsonString(j);

                }
                RequestJson r = new RequestJson();
                r.Raum = c.Room;
                r.type = c.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            if (json.ContainsKey("values") && json["values"] != null && ((string[])(json["values"])).Length>=0)
            {
                if (((string[])(json["values"])).Length==1)
                {
                    int time = 0;
                    try
                    {
                        time = Int32.Parse(((string[])(json["values"]))[0]);
                    }
                    catch(FormatException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                    }
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(time);
                    if (f.timeleftnet >= config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else if(((string[])(json["values"])).Length > 1 && ((json.ContainsKey("id") && json["id"] != null) || (json.ContainsKey("label") && json["label"] != null)))
                {
                    Constants.SessionSetting session = new Constants.SessionSetting();
                    session.id = c.SessionSettings.Count;
                    bool neueSession = true;
                    foreach (Constants.SessionSetting ss in c.SessionSettings)
                    {
                        if (ss.JsonText.Trim().ToLower().Equals(((string)json["id"]).Trim().ToLower()) || ss.JsonText.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
                        {
                            session = ss;
                            neueSession = false;
                            break;
                        }
                    }
                    int i = 0;
                    if(((string[])(json["values"])).Length > 2)
                    {
                        i++;
                    }
                    try
                    {
                        session.should_reset = Boolean.Parse(((string[])json["values"])[i++]);
                    }catch(FormatException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                        Log.Print("First \"values\" argument should be the reset boolean. Aborting creation or edit.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        return;
                    }
                    if (((string[])(json["values"])).Length > 2)
                    {
                        session.JsonText = ((string[])json["values"])[0];
                    }
                    else if(neueSession)
                    {
                        if (json.ContainsKey("id") && json["id"] != null)
                        {
                            session.JsonText = ((string)json["id"]);
                        }
                        else if(json.ContainsKey("label") && json["label"] != null)
                        {
                            session.JsonText = ((string)json["label"]);
                        }
                    }
                    session.ShowText = ((string[])json["values"])[i++];
                    c.SessionSettings.Add(session); if (!c.SessionSettings.Contains(session))
                    {
                        c.SessionSettings.Add(session);
                    }
                }
                else if (((string[])(json["values"])).Length > 2)
                {

                    Constants.SessionSetting session = new Constants.SessionSetting();
                    session.id = c.SessionSettings.Count;
                    foreach (Constants.SessionSetting ss in c.SessionSettings)
                    {
                        if (ss.JsonText.Equals(((string[])json["values"])[0]))
                        {
                            session = ss;
                            break;
                        }
                    }
                    try
                    {
                        session.should_reset = Boolean.Parse(((string[])json["values"])[0]);
                    }
                    catch (FormatException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                        Log.Print("First \"values\" argument should be the reset boolean. Aborting creation or edit.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        return;
                    }
                    session.JsonText = (string)json["id"];
                    session.ShowText = ((string[])json["values"])[1];
                    if (!c.SessionSettings.Contains(session))
                    {
                        c.SessionSettings.Add(session);
                    }
                }
                else
                {
                    Log.Print("Couldn't match given Arguments to a form that is useable.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            else
            {
                if (json.ContainsKey("id") && json["id"] != null)
                {
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(((Int64)json["id"]));
                    f.timeleftnet = (Int32)((Int64)json["id"]);
                    if (f.timeleftnet >= config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else
                {
                    Log.Print("Missing necesarry Json key (\"id\", to set time to).", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            
        }
        private void ServicePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && json["label"] != null && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.ServicesSetting ss in c.ServicesSettings)
                {
                    RequestJson j = new RequestJson();
                    j.id = ss.id;
                    j.label = ss.JsonText;
                    String[] t = new string[3];
                    t[0] = ss.ShowText;
                    t[1] = ((Constants.ServicesSettingfunction)ss).ToString();
                    j.values = t;
                    ret += assembleQuestionJsonString(j);

                }
                RequestJson r = new RequestJson();
                r.Raum = c.Room;
                r.type = c.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            if (((json.ContainsKey("id") && json["id"] != null) || (json.ContainsKey("label") && json["label"] != null && ((string)(json["label"])).Length > 0)) && json.ContainsKey("values") && json["values"] != null && ((string[])(json["values"])).Length>1)
            {
                Constants.ServicesSettingfunction ss = new Constants.ServicesSettingfunction();
                ss.id = c.SessionSettings.Count;
                int x = -1;
                if (json.ContainsKey("id") && json["id"] != null && ((int)json["id"])>=0 && ((int)json["id"]) < c.ServicesSettings.Count)
                {
                    x = ((int)json["id"]);
                }
                else
                {
                    try
                    {
                        x = Int32.Parse(((string)json["label"]));
                    }catch(FormatException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                    }
                    if (x < 0)
                    {
                        for(int i = 0; i< c.ServicesSettings.Count;i++)
                        {
                            if (((string)json["label"]).Trim().ToLower().Equals(c.ServicesSettings[i].JsonText.Trim().ToLower()))
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                }
                if (x >= 0 || x < c.ServicesSettings.Count)
                {
                    ss = (ServicesSettingfunction)c.ServicesSettings[x];
                }
                if((json.ContainsKey("label") && json["label"] != null && ((string)(json["label"])).Length > 0))
                {
                    ss.JsonText = ((string)(json["label"]));
                }
                else if (json.ContainsKey("id") && json["id"] != null)
                {
                    ss.JsonText = ((string)(json["id"]));
                }
                ss.ShowText = ((string[])(json["values"]))[0];
                Constants.rawfunctiontext fun = new Constants.rawfunctiontext();
                fun.functionText = ((string[])(json["values"]))[1];
                ss.function = fun;
                //Not secure, because setupsecondaryfunctionsforServiceButtons can return ServiceSettings without secondary function.
                ss = (ServicesSettingfunction)c.setupsecondaryfunctionsforServiceButtons(ss);
                if (!c.ServicesSettings.Contains(ss))
                {
                    c.ServicesSettings.Add(ss);
                    f.helper.GendynamicServiceButtons();
                }
            }
            else if(json.ContainsKey("values") && json["values"] != null && ((string[])(json["values"])).Length > 2)
            {
                Constants.ServicesSettingfunction ss = new Constants.ServicesSettingfunction();
                ss.id = c.SessionSettings.Count;
                int x = -1;
                try
                {
                    x = Int32.Parse(((string[])json["values"])[0]);
                }
                catch (FormatException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                }
                if (x < 0)
                {
                    for (int i = 0; i < c.ServicesSettings.Count; i++)
                    {
                        if (((string[])json["values"])[0].Trim().ToLower().Equals(c.ServicesSettings[i].JsonText.Trim().ToLower()))
                        {
                            x = i;
                            break;
                        }
                    }
                }
                if (x >= 0 || x < c.ServicesSettings.Count)
                {
                    ss = (ServicesSettingfunction)c.ServicesSettings[x];
                }
                ss.ShowText = ((string[])(json["values"]))[0];
                Constants.rawfunctiontext fun = new Constants.rawfunctiontext();
                fun.functionText = ((string[])(json["values"]))[1];
                ss.JsonText = ((string[])(json["values"]))[2];
                ss.function = fun;
                //Not secure, because setupsecondaryfunctionsforServiceButtons can return ServiceSettings without secondary function.
                ss = (ServicesSettingfunction)c.setupsecondaryfunctionsforServiceButtons(ss);
                if (!c.ServicesSettings.Contains(ss))
                {
                    c.ServicesSettings.Add(ss);
                    f.helper.GendynamicServiceButtons();
                }
            }
            else
            {
                Log.Print("Given arguments do not meet the requirements to add or edit a Service Button", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void DMXScenePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && json["label"] != null && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.DMXScene ss in c.DMXScenes)
                {
                    RequestJson j = new RequestJson();
                    j.id = ss.id;
                    j.label = ss.JsonText;
                    String[] t = new string[2+ss.Channelvalues.Length];
                    t[0] = ss.ShowText;
                    t[1] = ss.ContentPath;

                    for(int i = 0; i + 2 < t.Length && i < ss.Channelvalues.Length; i++)
                    {
                        t[i+2] = ss.Channelvalues[i].ToString();
                    }
                    j.values = t;
                    ret += assembleQuestionJsonString(j);
                }
                RequestJson r = new RequestJson();
                r.Raum = c.Room;
                r.type = c.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            int index = -1;
            if (json.ContainsKey("id") && json["id"] != null)
            {
                index = (int)json["id"];
            }
            else if (json.ContainsKey("label") && json["label"] != null && ((String)(json["label"])).Length >= 0)
            {
                for (int i = 0; i < c.DMXScenes.Count; i++)
                {
                    if (c.DMXScenes[i].JsonText.Trim().ToLower().Equals(((String)json["label"]).Trim().ToLower()))
                    {
                        if (index > 0)
                        {
                            Log.Print("DMXScene Paket label has more than 1 reference in scenes. Taking first found scene", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            continue;
                        }
                        index = i;
                    }
                }
            }
            else
            {
                Log.Print("DMXScene Paket doesn't contain necesarry Keys (\"id\" or \"label\") to identify the scene to be edited", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if (json.ContainsKey("values") && json["values"] != null)
            {
                if (c.DMXScenes.Count > index && index >= 0)
                {
                    if (json["values"].GetType() == typeof(byte[]))
                    {
                        for (int i = 0; i < ((int[])json["values"]).Length; i++)
                        {
                            int x = ((int[])json["values"])[i];
                            x = Math.Min(255, x);
                            x = Math.Max(0, x);
                            c.DMXScenes[index].Channelvalues[i] = (byte)x;
                        }
                    }
                    else if (json["values"].GetType() == typeof(string[]))
                    {
                        c.DMXScenes[index].ShowText = ((string[])json["values"])[0];
                        String Path = ((string[])json["values"])[1];
                        c.finalizePaths(out Path, Path);
                        if (File.Exists(Path))
                        {
                            c.DMXScenes[index].ContentPath = Path;
                            f.helper.GendynamicAmbientButtons();
                            Log.Print($"Name and Path of Scene {c.DMXScenes[index].JsonText} updated", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                        }
                        else
                        {
                            Log.Print($"Only Name of Scene {c.DMXScenes[index].JsonText} updated, No File found for the given Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Log.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[] or string[])", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                }
                else
                {
                    Log.Print("DMXScene Paket id out of range. Creating new temporary scene. To show scene in UI you have to send another packet to set showname and (optionally) the Content Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    Constants.DMXScene scene = new Constants.DMXScene();
                    scene.id = c.DMXScenes.Count;
                    scene.JsonText = (string)json["type"];
                    if (json["values"].GetType() == typeof(byte[]))
                    {
                        for (int i = 0; i < ((int[])json["values"]).Length; i++)
                        {
                            int x = ((int[])json["values"])[i];
                            x = Math.Min(255, x);
                            x = Math.Max(0, x);
                            scene.Channelvalues[i] = (byte)x;
                        }
                    }
                    else
                    {
                        Log.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[]), aborting creation.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    c.DMXScenes.Add(scene);
                    Log.Print($"Added DMXScene with Json label: {scene.JsonText}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                }
            }
            else
            {
                if (index >= 0 && index < c.DMXScenes.Count)
                {
                    //TODO: Implement Update Method to update UI correctly and send data
                    f.Ambiente_Change(c.DMXScenes[index], true, true, false);
                }
                else
                {
                    Log.Print("DMXScene Paket \"id\" / \"label\" out of range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            f.SendCurrentSceneOverCom();
        }


        private bool indexsjsontypewitch(Dictionary<string, object>? Json, int i)
        {
            switch (i)
            {
                case 0:
                    SystemPacket((Dictionary<String, Object>)Json, form, config);
                    return true;
                case 1:
                    TCPPacket((Dictionary<String, Object>)Json, form, config);
                    return true;
                case 2:
                    SessionPacket((Dictionary<String, Object>)Json, form, config);
                    return true;
                case 3:
                    ServicePacket((Dictionary<String, Object>)Json, form, config);
                    return true;
                case 4:
                    DMXScenePacket((Dictionary<String, Object>)Json, form, config);
                    return true;
                default:
                    return false;
            }
        }


        private void edittcpwithjsonvalusarraydata(Dictionary<string, object> json, MainForm f, Config c, Constants.TCPSetting tcp)
        {
            tcp.ShowText = ((string[])json["values"])[0];
            tcp.JsonText = ((string[])json["values"])[1];
            try
            {
                tcp.id = Int32.Parse(((string[])json["values"])[2]);
            }
            catch (FormatException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                Log.Print("Aborted new TCP creation, because of invalid \"id\"", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if (((string[])json["values"]).Length > 3)
            {
                tcp.value = ((string[])json["values"])[3];
            }
            c.TCPSettings.Add(tcp);
            f.logout();
            f.UIControl.SelectTab(f.UIControl.TabCount - 1);
            Log.Print("Added new TCP Setting to the \"Wartungs\" Page", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
        }

        private String parse(byte[] json)
        {
            String resString;
            try
            {
                resString = System.Text.Encoding.Default.GetString(json);
            }catch(Exception e)
            {
                Log.Print(e.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                return null;
            }
            return resString;
        }

        public void receivedMessage(byte[] bytes, TcpClient cl, bool isTelnet, Network net, int index)
        {
            if (parse(bytes) == null)
            {
                return;
            }
            Messageafterparse(parse(bytes));

        }

        public void Messageafterparse(String m)
        {
            m = m.Trim().ToLower();
            Log.Print(m, Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            Dictionary<String, Object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<String, Object>>(m);
            handleReceivedNet(keyValuePairs);
        }

        public String jsonsorter(String m)
        {
            if (!m.Contains(',') || !m.Contains("room") || !m.Contains("type"))
            {
                return "";
            }
            String s = "{";
            String[] ls = m.Split(',');
            foreach (String s2 in ls)
            {
                if (s2.Contains("room"))
                {
                    s += $"{s2},";
                    break;
                }
            }
            foreach (String s2 in ls)
            {
                if (s2.Contains("id"))
                {
                    s += $"{s2},";
                    break;
                }
            }
            if (m.Contains("label"))
            {
                foreach (String s2 in ls)
                {
                    if (s2.Contains("label"))
                    {
                        s += $"{s2},";
                        break;
                    }
                }
            }
            if (m.Contains("id"))
            {
                foreach (String s2 in ls)
                {
                    if (s2.Contains("id"))
                    {
                        s += $"{s2},";
                        break;
                    }
                }
            }
            if (m.Contains("values"))
            {
                for (int i = 0;i<ls.Length;i++)
                {
                    if (ls[i].Contains("id"))
                    {
                        s += $"{ls[i++]}";
                        while (!ls[i].Contains(']'))
                        {
                            s += ls[i++];
                        }
                        break;
                    }
                }
            }
            if (s.EndsWith(','))
            {
                s = s.Substring(0,s.Length-1);
            }
            s += '}';
            return s;
        }
    }
}
