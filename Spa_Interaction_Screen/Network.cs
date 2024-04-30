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

namespace Spa_Interaction_Screen
{
    public class Network
    {
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
                    Debug.Print(ex.Message);
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
                    Debug.Print($"{Encoding.ASCII.GetString(receivedBytes)}");
                    if (receivedBytes != null && receivedBytes.Length > 0)
                    {
                        handleReceivedUDP(parse(receivedBytes));
                    }
                    else
                    {
                        Debug.Print("Recaived UDP Paket with no Json Content");
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
                Debug.Print($"Send {req.type} Request to {req.destination}:{req.port} with {req.label}, {req.id}");
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
                Debug.Print(e.Message);
            }
            Debug.Print($"[{req.type}]Bytes send via UDP: {num}");
            Debug.Print(text);
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
                Debug.Print("No connected Clients");
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
                        Debug.Print(ex.Message);
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
            while (true)   //we wait for a connection
            {
                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it
                net.tcpClients.Add(client);
                net.ListeningClients.Add(Task.Run(() => listenTCPConnection(client, net, false, ListeningClients.Count)));
                IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                Debug.Print($"Client with IP:{endpoint.Address} connected");
            }
        }

        private void listenTCPConnection(TcpClient cl, Network net, bool isTelnet, int index)
        {
            NetworkStream ns = cl.GetStream();
            Debug.Print("ListeningTCP");
            while (cl.Connected)  //while the client is connected, we look for incoming messages
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
                            Debug.Print(ex.Message);
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
                Debug.Print("Couldnt Send Telnet to router. Check if Router is reachable.");
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
                        Debug.Print(ex.Message);
                        break;
                }
                return false;
            }
            //Debug.Print("pass ?");
            return true;
        }

        public bool SendTelnetSetSSID(String SSID)
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Debug.Print("Couldnt Send Telnet to router. Check if Router is reachable.");
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
                        Debug.Print(ex.Message);
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
                            Debug.Print(ex.Message);
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
            Debug.Print($"wifi ssid {SSID}");
            return true;
        }

        public bool SendTelnetPasswordrefresh()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Debug.Print("Couldnt Send Telnet to router. Check if Router is reachable.");
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
                        Debug.Print(ex.Message);
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
                            Debug.Print(ex.Message);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Debug.Print(m);
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
                            Debug.Print(ex.Message);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Debug.Print(m);
                String[] splt = m.Split("\n");
                int i = 0;
                for (i = i; i < splt.Length - 1; i++)
                {
                    if (splt[i].Contains("#pass") && splt[i].Contains(':'))
                    {
                        //Debug.Print(splt[i].Split(':')[1].Trim());
                        return splt[i].Split(':')[1].Trim();
                    }
                }
            }
            return "";
        }
        public void setuprouterssid(MainForm f)
        {
            if (!connectrouter(f))
            {
                Debug.Print("Error occured while connecting to router");
                WaitforRouterClientClose();
                return;
            }
            SendTelnetSetSSID($"{config.WiFiSSID}");
            WaitforRouterClientClose();
        }

        public void setuprouterpassword(MainForm f)
        {
            if (!connectrouter(f))
            {
                Debug.Print("Error occured while connecting to router");
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            SendTelnetASKPass();
            config.password = WaitForPass();
            //Debug.Print($"old Password: {config.password}");
            WaitforRouterClientClose();
            if (!connectrouter(f))
            {
                Debug.Print("Error occured while connecting to router");
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            if (!SendTelnetPasswordrefresh())
            {
                Debug.Print("Error occured while refreshing router password");
                WaitforRouterClientClose();
                f.helper.setnewPassword();
                return;
            }
            WaitforRouterClientClose();
            String npw = "";
            DateTime dateTimestart = DateTime.Now;
            do
            {
                //Debug.Print("Waiting for new Password");
                if (!connectrouter(f))
                {
                    Debug.Print("Error occured while connecting to router");
                    WaitforRouterClientClose();
                }
                SendTelnetASKPass();
                npw = WaitForPass();
            } while (npw.Equals(config.password) && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout * 2);
            WaitforRouterClientClose();
            config.password = npw;
            Debug.Print($"Neues Passwort: {npw}");
            f.helper.setnewPassword();
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
            Debug.Print("handle");
            Debug.Print(m);
            if (m.Contains("#pass"))
            {
                String[] lines = m.Split("WMedia>");
                bool foundline = false;
                foreach (String line in lines)
                {
                    Debug.Print("Something");
                    Debug.Print($"{line}");
                    Debug.Print("Something2");
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

                //Debug.Print($"[Communication] : [EstablishConnection] : Success connecting to : {ip_address}, port: {port_number}");
            }
            catch (Exception e)
            {
                Debug.Print($"[Communication] : [EstablishConnection] : Failed while connecting to : {ip_address}, port: {port_number}");
                Debug.Print(e.Message);
                routerclient = null;
            }
            if (routerclient == null || !routerclient.Connected)
            {
                Debug.Print("Error when tried to connect to ecler Router via Telnet over TCP\nCannot change Password or name remotely nor display the current password.");
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
                            Debug.Print(ex.Message);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Debug.Print(m);
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
                    //Debug.Print("Telnet Terminal: ");
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
            if (!Json.ContainsKey("type"))
            {
                Debug.Print("Invalid Json received: missing \"type\" argument");
                return;
            }
            if(!Json.ContainsKey("room") || (Int64)Json["room"] != config.Room)
            {
                Debug.Print("Invalid Json received: missing or wrong \"room\" argument");
            }
            bool processedjson = false;
            int x = -1;
            if (int.TryParse(((String)Json["type"]).Trim(),out x))
            {
                processedjson = indexsjsontypewitch(Json, x);
            }
            switch (((String)Json["type"]).Trim().ToLower())
            {
                case "status":
                    Debug.Print("This Paket type does not belog here (Paket: Status). Ignoring it");
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
            if (!processedjson)
            {
                Debug.Print("Json could be related to a given \"type\".");
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
                    Debug.Print("Message received Working Normally. Nothing changed");
                    break;
                case 1:
                    Debug.Print("Message received: Resetting");
                    f.reset();
                    break;
                case 2:
                    Debug.Print("Message received: Restarting");
                    f.Restart();
                    break;
                case 3:
                    Debug.Print("Message received: Shutdown");
                    f.Shutdown();
                    break;
                case 4:
                    int sceneindex = -1;
                    if (json.ContainsKey("id"))
                    {

                        sceneindex = (int)json["id"];
                    }
                    else if (json.ContainsKey("values"))
                    {
                       if (((string[])json["values"])[0] != null && ((string[])json["values"])[0].Length >= 0)
                        {
                            try
                            {
                                sceneindex = Int32.Parse(((string[])json["values"])[0]);
                            }
                            catch (FormatException ex)
                            {
                                Debug.Print(ex.Message);
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
                            Debug.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene to). Values has to be an Array with Scene id or name in first index");
                        }
                        if(sceneindex < 0 || sceneindex >= c.DMXScenes.Count)
                        {
                            Debug.Print("Index / value outside Scenes Range");
                            break;
                        }
                        f.Ambiente_Change(c.DMXScenes[sceneindex], true, false, false);
                    }
                    else
                    {
                        Debug.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene to).");
                    }
                    break;
                case 5:
                    int volumevalue = -1;
                    if (json.ContainsKey("id"))
                    {
                        volumevalue = (int)json["id"];
                    }
                    else if (json.ContainsKey("values"))
                    {
                        if (((string[])json["values"])[0] != null && ((string[])json["values"])[0].Length >= 0)
                        {
                            try
                            {
                                sceneindex = Int32.Parse(((string[])json["values"])[0]);
                            }
                            catch (FormatException ex)
                            {
                                Debug.Print(ex.Message);
                            }
                        }
                        else
                        {
                            Debug.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to. Values has to be an Array with Scene id or name in first index");
                        }
                    }
                    else
                    {
                        Debug.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to");
                    }
                    if(volumevalue < 0 || volumevalue > 100)
                    {
                        Debug.Print("index / value outside Volume Range");
                        break;
                    }
                    f.AmbientVolume(volumevalue, null, null);
                        break;
                    case 6:
                    if (json.ContainsKey("id"))
                    {
                        if ((int)json["id"] == 0)
                        {
                            f.setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                        }
                        else
                        {
                            f.setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                        }
                    }else if (json.ContainsKey("values"))
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
                            Debug.Print(ex.Message);
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
                        Debug.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene block");
                    }
                    break;
                    case 7:
                    if (json["values"].GetType() == typeof(string[]))
                    {
                        String Path = ((string[])json["values"])[1];
                        c.finalizePaths(out Path, Path);
                        if (File.Exists(Path))
                        {
                            f.vlc.changeMedia(Path, false);
                            Debug.Print($"Projecting Path {Path}");
                        }
                        else
                        {
                            Debug.Print("No File found for the given Path");
                        }
                    }
                    else
                    {
                        Debug.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[] or string[])");
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
                    Debug.Print("Couldn't recognize \"type\" argument");
                    break;
            }
        }
        private void TCPPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label"))
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
                    Debug.Print("Not enough Arguments provided to edit existing TCP Button (3-4 required)");
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
                    Debug.Print("Not enough Arguments provided to create new TCP Button (3-4 required)");
                }
            }
            else
            {
                Debug.Print("Already reached max number of TCP Wartungs Buttons and it was no label given to edit TCP button");
            }
        }

        private void SessionPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && ((string)json["label"]).Equals("?"))
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
            if (json.ContainsKey("values") && ((string[])(json["values"])).Length>=0)
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
                        Debug.Print(ex.Message);
                    }
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(time);
                    if (f.timeleft >= config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else if(((string[])(json["values"])).Length > 1 && (json.ContainsKey("id") || json.ContainsKey("label")))
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
                        Debug.Print(ex.Message);
                        Debug.Print("First \"values\" argument should be the reset boolean. Aborting creation or edit.");
                        return;
                    }
                    if (((string[])(json["values"])).Length > 2)
                    {
                        session.JsonText = ((string[])json["values"])[0];
                    }
                    else if(neueSession)
                    {
                        if (json.ContainsKey("id"))
                        {
                            session.JsonText = ((string)json["id"]);
                        }
                        else if(json.ContainsKey("label"))
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
                        Debug.Print(ex.Message);
                        Debug.Print("First \"values\" argument should be the reset boolean. Aborting creation or edit.");
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
                    Debug.Print("Couldn't match given Arguments to a form that is useable.");
                }
            }
            else
            {
                if (json.ContainsKey("id"))
                {
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(((Int64)json["id"]));
                    f.timeleft = (Int32)((Int64)json["id"]);
                    if (f.timeleft >= config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else
                {
                    Debug.Print("Missing necesarry Json key (\"id\", to set time to).");
                }
            }
            
        }
        private void ServicePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && ((string)json["label"]).Equals("?"))
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
            if ((json.ContainsKey("id") || (json.ContainsKey("label") && ((string)(json["label"])).Length > 0)) && json.ContainsKey("values") && ((string[])(json["values"])).Length>1)
            {
                Constants.ServicesSettingfunction ss = new Constants.ServicesSettingfunction();
                ss.id = c.SessionSettings.Count;
                int x = -1;
                if (json.ContainsKey("id") && ((int)json["id"])>=0 && ((int)json["id"]) < c.ServicesSettings.Count)
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
                        Debug.Print(ex.Message);
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
                if((json.ContainsKey("label") && ((string)(json["label"])).Length > 0))
                {
                    ss.JsonText = ((string)(json["label"]));
                }
                else if (json.ContainsKey("id")){
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
            else if(json.ContainsKey("values") && ((string[])(json["values"])).Length > 2)
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
                    Debug.Print(ex.Message);
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
                Debug.Print("Given arguments do not meet the requirements to add or edit a Service Button");
            }
        }

        private void DMXScenePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("label") && ((string)json["label"]).Equals("?"))
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
            if (json.ContainsKey("id"))
            {
                index = (int)json["id"];
            }
            else if (json.ContainsKey("label") && ((String)(json["label"])).Length >= 0)
            {
                for (int i = 0; i < c.DMXScenes.Count; i++)
                {
                    if (c.DMXScenes[i].JsonText.Trim().ToLower().Equals(((String)json["label"]).Trim().ToLower()))
                    {
                        if (index > 0)
                        {
                            Debug.Print("DMXScene Paket label has more than 1 reference in scenes. Taking first found scene");
                            continue;
                        }
                        index = i;
                    }
                }
            }
            else
            {
                Debug.Print("DMXScene Paket doesn't contain necesarry Keys (\"id\" or \"label\") to identify the scene to be edited");
                return;
            }
            if (json.ContainsKey("values"))
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
                            Debug.Print($"Name and Path of Scene {c.DMXScenes[index].JsonText} updated");
                        }
                        else
                        {
                            Debug.Print($"Only Name of Scene {c.DMXScenes[index].JsonText} updated, No File found for the given Path");
                        }
                    }
                    else
                    {
                        Debug.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[] or string[])");
                    }
                }
                else
                {
                    Debug.Print("DMXScene Paket id out of range. Creating new temporary scene. To show scene in UI you have to send another packet to set showname and (optionally) the Content Path");
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
                        Debug.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[]), aborting creation.");
                    }
                    c.DMXScenes.Add(scene);
                    Debug.Print($"Added DMXScene with Json label: {scene.JsonText}");
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
                    Debug.Print("DMXScene Paket \"id\" / \"label\" out of range");
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


        private static void edittcpwithjsonvalusarraydata(Dictionary<string, object> json, MainForm f, Config c, Constants.TCPSetting tcp)
        {
            tcp.ShowText = ((string[])json["values"])[0];
            tcp.JsonText = ((string[])json["values"])[1];
            try
            {
                tcp.id = Int32.Parse(((string[])json["values"])[2]);
            }
            catch (FormatException ex)
            {
                Debug.Print(ex.Message);
                Debug.Print("Aborted new TCP creation, because of invalid \"id\"");
                return;
            }
            if (((string[])json["values"]).Length > 3)
            {
                tcp.value = ((string[])json["values"])[3];
            }
            c.TCPSettings.Add(tcp);
            f.logout();
            f.UIControl.SelectTab(f.UIControl.TabCount - 1);
            Debug.Print("Added new TCP Setting to the \"Wartungs\" Page");
        }

        private static String parse(byte[] json)
        {
            String resString;
            try
            {
                resString = System.Text.Encoding.Default.GetString(json);
            }catch(Exception e)
            {
                Debug.Print(e.Message);
                return null;
            }
            return resString;
        }

        public void receivedMessage(byte[] bytes, TcpClient cl, bool isTelnet, Network net, int index)
        {
            if(parse(bytes) == null)
            {
                return;
            }
            String m = parse(bytes);
            m = m.Trim().ToLower();
            Debug.Print(m);
            Dictionary<String, Object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<String, Object>>(m);
            handleReceivedNet(keyValuePairs);

        }
    }
}
