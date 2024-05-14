using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using static Spa_Interaction_Screen.Constants;
namespace Spa_Interaction_Screen
{
    public class Network
    {
        private MainForm form;
        public List<Socket> tcpSockets = new List<Socket>();
        private Task connectClients = null;
        public List<Task> ListeningClients = new List<Task>();
        private TcpClient routerclient = null;
        private Task ListenRouter = null;
        public bool routertelnetinit = false;

        public Network(MainForm f)
        {
            form = f;
            //connect();
            StartTCPServer();
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
            s += $"\"room\":{req.Raum}";
            s += $",\"type\":\"{req.type}\"";
            if (req.label != null)
            {
                s += $",\"label\":\"{req.label}\"";
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

        private Socket getbestclient(string? dest, int? port)
        {
            Socket? cl = tcpSockets[0];
            if (tcpSockets.Count > 1)
            {
                byte[] ip = new byte[4];
                try
                {
                    if(dest == null || dest.Length <= 0)
                    {
                        for (int i = 0; i < Config.IPZentrale.Length; i++)
                        {
                            ip[i] = Byte.Parse(Config.IPZentrale[i]);
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
                    MainForm.currentState = 8;
                    Logger.Print(ex.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                    return null;
                }

                List<Socket> possibilities = new List<Socket>();
                IPAddress specifiedIP = new IPAddress(ip);
                foreach (Socket client in tcpSockets)
                {
                    if (client != null)
                    {
                        IPEndPoint endpoint = client.RemoteEndPoint as IPEndPoint;
                        if (endpoint.Address.Equals(specifiedIP))
                        {
                            possibilities.Add(client);
                            if (((port != null && port > 0 && endpoint.Port.Equals(port))||(Config.PortZentrale != null && Config.PortZentrale > 0 && endpoint.Port.Equals(Config.PortZentrale))) && client.Connected)
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
                            tcpSockets.Remove(possibilities[i]);
                            i--;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    while (i < tcpSockets.Count && !tcpSockets[i].Connected)
                    {
                        i++;
                    }
                    if (i >= tcpSockets.Count)
                    {
                        return null;
                    }
                    return tcpSockets[i];
                }
                cl = possibilities[0];
            }
            return cl;
        }

        public bool isClientZentrale(Socket client)
        {
            if (Config.IPZentrale != null && client != null && client.Connected)
            {
                return false;
            }
            byte[] ip = new byte[4];
            for (int i = 0; i < Config.IPZentrale.Length; i++)
            {
                ip[i] = Byte.Parse(Config.IPZentrale[i]);
            }
            IPAddress specifiedIP = new IPAddress(ip);
            IPEndPoint endpoint = client.RemoteEndPoint as IPEndPoint;
            if (!endpoint.Address.Equals(specifiedIP))
            {
                return false;
            }
            return true;
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
                    Logger.Print($"{Encoding.ASCII.GetString(receivedBytes)}");
                    if (receivedBytes != null && receivedBytes.Length > 0)
                    {
                        handleReceivedUDP(parse(receivedBytes));
                    }
                    else
                    {
                        Logger.Print("Recaived UDP Paket with no Json Content");
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
                Logger.Print($"Send {req.type} Request to {req.destination}:{req.port} with {req.label}, {req.id}");
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
                Logger.Print(e.Message);
            }
            Logger.Print($"[{req.type}]Bytes send via UDP: {num}");
            Logger.Print(text);
        }
        */

        //TCP

        public bool SendTCPMessage(RequestJson json, Socket? cl)
        {
            if (tcpSockets == null || tcpSockets.Count <= 0)
            {
                return false;
            }
            if (cl == null)
            {
                cl = getbestclient(json.destination, json.port);
            }
            if (cl == null)
            {
                Logger.Print("No connected Clients",Logger.MessageType.TCPSend, Logger.MessageSubType.Notice);
                return false;
            }
            NetworkStream stream = new NetworkStream(cl);
            string text = assembleJsonString(json);
            byte[] send_buffer = Encoding.Default.GetBytes(text);
            Logger.Print(text, Logger.MessageType.TCPSend, Logger.MessageSubType.Information);
            try
            {
                stream.Write(send_buffer, 0, send_buffer.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case ArgumentNullException:
                    case ArgumentOutOfRangeException:
                    case InvalidOperationException:
                    case IOException:
                        MainForm.currentState = 8;
                        Logger.Print(ex.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            return true;
        }

        private void StartTCPServer()
        {
            TcpListener server = new TcpListener(IPAddress.Any, Config.LocalPort);
            server.Start();
            connectClients = Task.Run(() => acceptClientsLoop(this, server));
        }

        private void acceptClientsLoop(Network net, TcpListener server)
        {
            while (net.form.RunTask)   //we wait for a connection
            {
                Socket client = server.AcceptSocket();  //if a connection exists, the server will accept it
                client.DontFragment = true;
                client.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                net.tcpSockets.Add(client);
                net.ListeningClients.Add(Task.Run(async() => listenTCPConnection(client, net, false, ListeningClients.Count)));
                IPEndPoint endpoint = client.RemoteEndPoint as IPEndPoint;
                Logger.Print($"Client with IP:{endpoint.Address} connected", [Logger.MessageType.TCPReceive, Logger.MessageType.TCPSend], Logger.MessageSubType.Information);
            }
        }

        private void connect()
        {
            TcpClient client = new TcpClient();
            byte[] ip = new byte[4];
            for (int i = 0; i < Config.IPZentrale.Length; i++)
            {
                ip[i] = Byte.Parse(Config.IPZentrale[i]);
            }
            Debug.Print("Connecting client");
            IPAddress specifiedIP = new IPAddress(ip);
            try
            {
                client.Connect(specifiedIP, Config.PortZentrale);
            }catch(Exception e)
            {
                Logger.Print(e.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                Logger.Print("Couldn't connect to Zentrale.", Logger.MessageType.TCPSend, Logger.MessageSubType.Notice);
                return;
            }
            tcpSockets.Add(client.Client);
            ListeningClients.Add(Task.Run(async () => listenTCPConnection(client.Client, this, false, ListeningClients.Count)));
            Network.RequestJson request = new Network.RequestJson();
            request.destination = form.ArraytoString(Config.IPZentrale, 4);
            request.port = Config.PortZentrale;
            request.type = "Status";
            request.id = MainForm.currentState;
            request.Raum = Config.Room;
            if (SendTCPMessage(request, null))
            {
                Logger.Print($"Message sent sucessfully", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            }
            if (form.Programmstate != null)
            {
                form.Programmstate.Text = $"Programmstatus: {MainForm.currentState}";
            }
            IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
            Logger.Print($"Client with IP:{endpoint.Address} connected", [Logger.MessageType.TCPReceive, Logger.MessageType.TCPSend], Logger.MessageSubType.Information);
        }

        private async void listenTCPConnection(Socket cl, Network net, bool isTelnet, int index)
        {
            Logger.Print("ListeningTCP", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            int b_read = 0;
            Task<int> receivetask = null;
            //while the client is connected, we look for incoming messages
            do
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                try
                {
                    receivetask = cl.ReceiveAsync(msg, SocketFlags.None);   //the same networkstream reads the message sent by the client
                    b_read = await receivetask;
                }
                catch (ObjectDisposedException ex)
                {
                    cl.Close();
                    break;
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case ArgumentNullException:
                        case ArgumentOutOfRangeException:
                        case InvalidOperationException:
                        case IOException:
                            MainForm.currentState = 8;
                            Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                net.receivedMessage(msg, cl, isTelnet, net, index);
            } while (cl != null && cl.Connected && b_read > 0 && net.form.RunTask);
            net.tcpSockets.Remove(cl);
            cl.Dispose();
        }

        //Telnet
        public bool SendTelnetASKPass()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Logger.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
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
                        MainForm.currentState = 8;
                        Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                        break;
                }
                return false;
            }
            //Logger.Print("pass ?");
            return true;
        }

        public bool SendTelnetSetSSID(String SSID)
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Logger.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
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
                        MainForm.currentState = 8;
                        Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
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
                            MainForm.currentState = 8;
                            Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
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
            Logger.Print($"wifi ssid {SSID}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            return true;
        }

        public bool SendTelnetWakeup()
        {
            if (routerclient == null || !routerclient.Connected)
            {
                Logger.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
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
                        MainForm.currentState = 8;
                        Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
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
                            MainForm.currentState = 8;
                            Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
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
                Logger.Print("Couldnt Send Telnet to router. Check if Router is reachable.", Logger.MessageType.Router, Logger.MessageSubType.Notice);
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
                        MainForm.currentState = 8;
                        Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
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
                            MainForm.currentState = 8;
                            Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
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
            if(routerclient == null || !routerclient.Connected)
            {
                return "";
            }
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
                            MainForm.currentState = 8;
                            Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                String[] splt = m.Split("\n");
                int i = 0;
                for (i = i; i < splt.Length - 1; i++)
                {
                    if (splt[i].Contains("#pass") && splt[i].Contains(':'))
                    {
                        //Logger.Print(splt[i].Split(':')[1].Trim());
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
                Logger.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                if (Config.UseoldWiFicreds())
                {
                    f.helper.setnewPassword();
                }
                return;
            }
            if (!SendTelnetASKPass())
            {
                Logger.Print("Error occured while Asking Router for Password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
            }
            
            Config.Wifipassword = WaitForPass();
            //Logger.Print($"old Password: {Config.password}");
            WaitforRouterClientClose();
            if (!connectrouter(f))
            {
                Logger.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                if (Config.UseoldWiFicreds())
                {
                    f.helper.setnewPassword();
                }
                return;
            }
            if (!SendTelnetPasswordrefresh())
            {
                Logger.Print("Error occured while refreshing router password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                if (Config.UseoldWiFicreds())
                {
                    f.helper.setnewPassword();
                }
                return;
            }
            WaitforRouterClientClose();
            String npw;
            DateTime dateTimestart = DateTime.Now;
            do
            {
                //Logger.Print("Waiting for new Password");
                if (!connectrouter(f))
                {
                    Logger.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                    WaitforRouterClientClose();
                }
                if (!SendTelnetASKPass())
                {
                    Logger.Print("Error occured while Asking Router for Password", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                }
                npw = WaitForPass();
            } while (npw.Equals(Config.Wifipassword) && (DateTime.Now - dateTimestart).TotalSeconds <= Constants.TelnetComTimeout * 1.5);
            WaitforRouterClientClose();
            Config.Wifipassword = npw;
            Logger.Print($"Neues Passwort: {npw}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            f.helper.setnewPassword();
        }

        public void setuprouterssid(MainForm f)
        {
            if (!connectrouter(f))
            {
                Logger.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                if (Config.UseoldWiFicreds())
                {
                    f.helper.setnewPassword();
                }
                return;
            }
            if (!SendTelnetSetSSID($"{Config.WiFiSSID}"))
            {
                Logger.Print("Error occured while Setting new Router SSID", Logger.MessageType.Router, Logger.MessageSubType.Notice);

            }
            WaitforRouterClientClose();
        }

        public void wakeup(MainForm f)
        {
            if (!connectrouter(f))
            {
                Logger.Print("Error occured while connecting to router", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                WaitforRouterClientClose();
                return;
            }
            if (!SendTelnetWakeup())
            {
                Logger.Print("Error occured while tried to wakeup Router", Logger.MessageType.Router, Logger.MessageSubType.Notice);

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
            Logger.Print($"Received Message: {m}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            if (m.Contains("#pass"))
            {
                String[] lines = m.Split("WMedia>");
                bool foundline = false;
                foreach (String line in lines)
                {
                    if (line.Contains('#') && line.Contains("pass") && line.Contains(":"))
                    {
                        String p = line.Split(":")[1].Split("\n\r")[0].Trim();
                        Config.Wifipassword = p;
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
            String ip_address = f.ArraytoString(Config.IPRouter, 4);
            int port_number = Config.PortRouter;
            NetworkStream stream = null;
            String response = null;
            try
            {
                routerclient = new TcpClient();
                routerclient.Connect(ip_address, port_number);

                Logger.Print($"Success connecting to: {ip_address}, port: {port_number}", Logger.MessageType.Router, Logger.MessageSubType.Information);
            }
            catch (Exception e)
            {
                MainForm.currentState = 2;
                Logger.Print(e.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                Logger.Print($"Failed while connecting to: {ip_address}, port: {port_number}", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                routerclient = null;
            }
            if (routerclient == null || !routerclient.Connected)
            {
                Logger.Print("Error when tried to connect to ecler Router via Telnet over TCP.Cannot change Password or name remotely nor display the current password. Trying to Fallback to previous WiFi Credentials", Logger.MessageType.Router, Logger.MessageSubType.Notice);
                return false;
            }
            bool receivedadd = false;
            if(routerclient == null || !routerclient.Connected)
            {
                return false;
            }
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
                            Logger.Print(ex.Message, Logger.MessageType.Router, Logger.MessageSubType.Error);
                            break;
                    }
                }
                Array.Resize(ref msg, b_read);
                String m = parse(msg);
                //Logger.Print(m);
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
                    //Logger.Print("Telnet Terminal: ");
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
            if (!(jsonpartvalid(Json, "type")))
            {
                Logger.Print("Invalid Json received: missing or wrong \"type\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if(!(jsonpartvalid(Json, "room")))
            {
                Logger.Print("Invalid Json received: missing or wrong \"room\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                if (Constants.NetRoomSpecMandatory)
                {
                    return;
                }
            }
            if (!(jsonpartvalid(Json, "id") || jsonpartvalid(Json, "label")))
            {
                Logger.Print("Invalid Json received: missing or wrong \"id\" or \"label\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }

            bool processedjson = false;
            int x = -1;
            if (Json["type"].GetType().Equals(typeof(Int64)))
            {
                processedjson = indexsjsontypeswitch(Json, (Int32)(Int64)(Json["type"]));
            }
            else if (Json["type"].GetType().Equals(typeof(String)))
            {
                if (int.TryParse(((String)Json["type"]).Trim().ToLower(), out x))
                {
                    processedjson = indexsjsontypeswitch(Json, x);
                }
                else
                {
                    switch (((String)Json["type"]).Trim().ToLower())
                    {
                        case "status":
                            Logger.Print("This Paket type does not belog here (Paket: Status). Ignoring it", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            processedjson = true;
                            break;
                        default:
                            for (int i = 0; i < Config.Typenames.Length && !processedjson; i++)
                            {
                                if (Config.Typenames[i] != null && Config.Typenames[i].Trim().ToLower().Equals(((string)Json["type"]).Trim().ToLower()))
                                {
                                    processedjson = indexsjsontypeswitch(Json, i);
                                }
                            }
                            break;
                    }
                }
            }
            if (!processedjson)
            {
                Logger.Print($"Json could not be related to a given \"type\": {((string)(Json["type"]))}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void SystemPacket(Dictionary<String, Object> json, MainForm f)
        {
            int index = -1;
            bool usedidforindex = false;
            if (jsonpartvalid(json, "label"))
            {
                for (int i = 0; i < Config.SystemSettings.Count; i++)
                {
                    Constants.SystemSetting ss = Config.SystemSettings[i];
                    if (ss.JsonText.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
                    {
                        index = i;
                        break;
                    }
                }
            }
            else if(jsonpartvalid(json, "id") && jsonpartvalid(json, "values"))
            {
                index = (Int32)(Int64)json["id"];
            }
            else
            {
                Logger.Print("Invalid \"label\", \"id\" or \"values\" for a System Packet", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
            if(index <=0 && Config.DMXSceneSettingJson.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = Config.SystemSettings.Count;
            }
            if (index <= 0 && Config.VolumeJson.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = Config.SystemSettings.Count + 1;
            }
            if (index <= 0 && ("block").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = Config.SystemSettings.Count + 2;
            }
            if (index <= 0 && ("video").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = Config.SystemSettings.Count + 3;
            }
            if (index <= 0 && ("?").Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
            {
                index = Config.SystemSettings.Count + 4;
            }
            switch (index) 
            {
                case 0:
                    Logger.Print("Message received Working Normally. Nothing changed", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                    break;
                case 1:
                    Logger.Print("Message received: Resetting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.reset();
                    break;
                case 2:
                    Logger.Print("Message received: Restarting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.Restart();
                    break;
                case 3:
                    Logger.Print("Message received: Shutdown", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    f.Shutdown();
                    break;
                case 4:
                    int sceneindex = -1;
                    if (jsonpartvalid(json, "id") && !usedidforindex)
                    {
                        sceneindex = (Int32)(Int64)json["id"];
                    }
                    else if (jsonpartvalid(json, "values"))
                    {
                        try
                        {
                            sceneindex = Int32.Parse(((JArray)json["values"]).ToObject<String[]>()[0]);
                        }
                        catch (FormatException ex)
                        {
                            Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                        }
                        if (sceneindex <= 0)
                        {
                            String name = ((JArray)json["values"]).ToObject<String[]>()[0];
                            for (int i = 0; i < Config.DMXScenes.Count; i++)
                            {
                                if (name.Trim().ToLower().Equals(Config.DMXScenes[i].JsonText.Trim().ToLower()))
                                {
                                    sceneindex = i;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene to).", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    if (sceneindex < 0 || sceneindex >= Config.DMXScenes.Count)
                    {
                        Logger.Print("Index / value outside Scenes Range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        break;
                    }
                    f.Ambiente_Change(Config.DMXScenes[sceneindex], true, false, false);
                    break;
                case 5:
                    int volumevalue = -1;
                    if (jsonpartvalid(json, "id") && !usedidforindex)
                    {
                        volumevalue = (int)json["id"];
                    }
                    else if (jsonpartvalid(json, "values"))
                    {
                        if (((JArray)json["values"])[0] != null && ((JArray)json["values"]).ToObject<String[]>()[0].Length >= 0)
                        {
                            try
                            {
                                sceneindex = Int32.Parse(((JArray)json["values"]).ToObject<String[]>()[0]);
                            }
                            catch (FormatException ex)
                            {
                                Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                            }
                        }
                        else
                        {
                            Logger.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to. Values has to be an Array with Scene id or name in first index", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Logger.Print("Missing necesarry Json key (\"id\" or \"values\", to change valume to", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    if(volumevalue < 0 || volumevalue > 100)
                    {
                        Logger.Print("index / value outside Volume Range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        break;
                    }
                    f.AmbientVolume(volumevalue);
                        break;
                    case 6:
                    if (jsonpartvalid(json, "values"))
                    {
                        if (((JArray)(json["values"])).Count >= 1 && jsonpartvalid(json, "id"))
                        {
                            bool b = (((Int32)(Int64)(json["id"])) % 2 == 0) ? false : true;
                            int[] dex = new int[((JArray)(json["values"])).Count];
                            for(int x =0;x< ((JArray)(json["values"])).Count;x++)
                            {
                                String s = ((JArray)(json["values"])).ToObject<String[]>()[x];
                                dex[x] = -1;
                                if (int.TryParse(s, out dex[x]));
                                if (dex[x] < 0)
                                {
                                    for (int i = Config.Typenames.Length-3; i < 3; i++)
                                    {
                                        if (s.Trim().ToLower().Equals(Config.Typenames[i].Trim().ToLower()))
                                        {
                                            dex[x] = i;
                                            break;
                                        }
                                    }
                                }
                            } 
                            for(int i = 0; i< dex.Length;i++)
                            {
                                int x = dex[i];
                                switch(x)
                                {
                                    case 2:
                                        form.setsessionlocked(b);
                                        break;
                                    case 3:
                                        form.setservicelocked(b, Constants.Warning_color);
                                        break;
                                    case 4:
                                        form.setscenelocked(b, "", Constants.Warning_color);
                                        break;
                                    default:
                                        Logger.Print($"Couldn't match given Arguments to a Block function (Only Supported for the last 3 JsonTypes) for Argmument: {((JArray)(json["values"])).ToObject<String[]>()[i]}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                                        break;
                                }
                            }
                        }
                        else if (((JArray)(json["values"])).Count >= 2)
                        {
                            try
                            {
                                bool p = Boolean.Parse(((JArray)json["values"]).ToObject<String[]>()[0]);
                                if (!p)
                                {
                                    f.setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                                }
                                else
                                {
                                    f.setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                                }
                                int[] dex = new int[((JArray)(json["values"])).Count];
                                for (int x = 0; x < ((JArray)(json["values"])).Count; x++)
                                {
                                    String s = ((JArray)(json["values"])).ToObject<String[]>()[x];
                                    dex[x] = -1;
                                    if (int.TryParse(s, out dex[x])) ;
                                    if (dex[x] < 0)
                                    {
                                        for (int i = Config.Typenames.Length - 3; i < 3; i++)
                                        {
                                            if (s.Trim().ToLower().Equals(Config.Typenames[i].Trim().ToLower()))
                                            {
                                                dex[x] = i;
                                                break;
                                            }
                                        }
                                    }
                                }
                                for (int i = 0; i < dex.Length; i++)
                                {
                                    int x = dex[i];
                                    switch (x)
                                    {
                                        case 2:
                                            form.setsessionlocked(p);
                                            break;
                                        case 3:
                                            form.setservicelocked(p, Constants.Warning_color);
                                            break;
                                        case 4:
                                            form.setscenelocked(p, "", Constants.Warning_color);
                                            break;
                                        default:
                                            Logger.Print($"Couldn't match given Arguments to a Block function (Only Supported for the last 3 JsonTypes) for Argmument: {((JArray)(json["values"])).ToObject<String[]>()[i]}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                                            break;
                                    }
                                }
                            }
                            catch (FormatException ex)
                            {
                                Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                                bool value = false;
                                try
                                {
                                    value = (Int32.Parse(((JArray)json["values"]).ToObject<String[]>()[0]) % 2 == 0) ? false : true;
                                }
                                catch (FormatException fex)
                                {
                                    MainForm.currentState = 2;
                                    Logger.Print(fex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                                }
                                int[] dex = new int[((JArray)(json["values"])).Count];
                                for (int x = 0; x < ((JArray)(json["values"])).Count; x++)
                                {
                                    String s = ((JArray)(json["values"])).ToObject<String[]>()[x];
                                    dex[x] = -1;
                                    if (int.TryParse(s, out dex[x])) ;
                                    if (dex[x] < 0)
                                    {
                                        for (int i = Config.Typenames.Length - 3; i < 3; i++)
                                        {
                                            if (s.Trim().ToLower().Equals(Config.Typenames[i].Trim().ToLower()))
                                            {
                                                dex[x] = i;
                                                break;
                                            }
                                        }
                                    }
                                }
                                for (int i = 0; i < dex.Length; i++)
                                {
                                    int x = dex[i];
                                    switch (x)
                                    {
                                        case 2:
                                            form.setsessionlocked(value);
                                            break;
                                        case 3:
                                            form.setservicelocked(value, Constants.Warning_color);
                                            break;
                                        case 4:
                                            form.setscenelocked(value, "", Constants.Warning_color);
                                            break;
                                        default:
                                            Logger.Print($"Couldn't match given Arguments to a Block function (Only Supported for the last 3 JsonTypes) for Argmument: {((JArray)(json["values"])).ToObject<String[]>()[i]}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Logger.Print("Couldn't match System (block) Packet to given Formats", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Logger.Print("Missing necesarry Json key (\"id\" or \"values\", to change scene block", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    break;
                    case 7:
                    if (jsonpartvalid(json, "values"))
                    {
                        String Path = ((JArray)json["values"]).ToObject<String[]>()[0];
                        Config.finalizePaths(out Path, Path);
                        if (File.Exists(Path))
                        {
                            if (f.vlc != null)
                            {
                                f.Content_Change(false);
                                f.vlc.changeMedia(Path, false);
                                Logger.Print($"Projecting Path {Path}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                            }
                        }
                        else
                        {
                            Logger.Print("No File found for the given Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        }
                    }
                    else
                    {
                        Logger.Print("Missing necesarry Json key (\"values\", to change Media", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    break;
                case 8:
                    String ret = "";
                    foreach(Constants.SystemSetting ss in Config.SystemSettings)
                    {
                        RequestJson j = new RequestJson();
                        j.type = Config.Typenames[0].Trim().ToLower();
                        j.Raum = Config.Room;
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
                    r.Raum = Config.Room;
                    r.type = Config.Typenames[0];
                    r.label = "?";
                    r.values = [ ret ];
                    SendTCPMessage(r, null);
                    break;
                default:
                    Logger.Print("Couldn't recognize \"type\" argument", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    break;
            }
        }

        private void TCPPacket(Dictionary<String, Object> json, MainForm f)
        {
            int currenttcpws = 0;
            foreach(TCPSetting tcp in Config.TCPSettings)
            {
                if(tcp.ShowText != null &&  tcp.ShowText.Length > 0)
                {
                    currenttcpws++;
                }
            }
            if (jsonpartvalid(json, "label"))
            {
                if (((string)json["label"]).Equals("?"))
                {
                    String ret = "";
                    foreach (Constants.TCPSetting ss in Config.TCPSettings)
                    {
                        RequestJson j = new RequestJson();
                        j.type = Config.Typenames[1].Trim().ToLower();
                        j.Raum = Config.Room;
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
                    r.Raum = Config.Room;
                    r.type = Config.Typenames[0];
                    r.label = "?";
                    r.values = [ret];
                    SendTCPMessage(r, null);
                    return;
                }
                if (jsonpartvalid(json, "values") && ((JArray)json["values"]).Count > 2)
                {
                    foreach (Constants.TCPSetting tcp in Config.TCPSettings)
                    {
                        edittcpwithjsonvaluesarraydata(json, f,tcp);
                        form.logout();
                    }
                }
                else
                {
                    Logger.Print("Not enough Arguments provided to edit existing TCP Button (3-4 required)", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
                return;
            }else if(currenttcpws <= Constants.maxtcpws)
            {
                if (jsonpartvalid(json, "values") && ((JArray)json["values"]).Count > 2)
                {
                    Constants.TCPSetting tcp = new Constants.TCPSetting();
                    edittcpwithjsonvaluesarraydata(json, f,tcp);
                    form.logout();
                }
                else
                {
                    Logger.Print("Not enough Arguments provided to create new TCP Button (3-4 required)", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            else
            {
                Logger.Print("Already reached max number of TCP Wartungs Buttons and it was no label given to edit TCP button", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void SessionPacket(Dictionary<String, Object> json, MainForm f)
        {
            if (jsonpartvalid(json, "label") && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.SessionSetting ss in Config.SessionSettings)
                {
                    RequestJson j = new RequestJson();
                    j.type = Config.Typenames[2].Trim().ToLower();
                    j.Raum = Config.Room;
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
                r.Raum = Config.Room;
                r.type = Config.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            if (jsonpartvalid(json, "values"))
            {
                if (((JArray)(json["values"])).Count==1)
                {
                    int time = 0;
                    try
                    {
                        time = Int32.Parse(((JArray)(json["values"])).ToObject<String[]>()[0]);
                    }
                    catch(FormatException ex)
                    {
                        Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                    }
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(time);
                    if (f.timeleftnet >= Config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else if(((JArray)(json["values"])).Count >= 1)
                {
                    Constants.SessionSetting session = new Constants.SessionSetting();
                    session.id = Config.SessionSettings.Count;
                    bool neueSession = true;
                    foreach (Constants.SessionSetting ss in Config.SessionSettings)
                    {
                        if (ss.JsonText.Trim().ToLower().Equals(((string)json["id"]).Trim().ToLower()) || ss.JsonText.Trim().ToLower().Equals(((string)json["label"]).Trim().ToLower()))
                        {
                            session = ss;
                            neueSession = false;
                            break;
                        }
                    }
                    int i = 0;
                    if(((JArray)(json["values"])).Count > 2)
                    {
                        i++;
                    }
                    try
                    {
                        session.should_reset = Boolean.Parse(((JArray)json["values"]).ToObject<String[]>()[i++]);
                    }catch(FormatException ex)
                    {
                        Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                        Logger.Print("First \"values\" argument should be the reset boolean. Aborting creation or edit.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                        return;
                    }
                    if (((JArray)(json["values"])).Count > 2)
                    {
                        session.JsonText = ((JArray)json["values"]).ToObject<String[]>()[0];
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
                    session.ShowText = ((JArray)json["values"]).ToObject<String[]>()[i++];
                    Config.SessionSettings.Add(session); if (!Config.SessionSettings.Contains(session))
                    {
                        Config.SessionSettings.Add(session);
                    }
                }
                else
                {
                    Logger.Print("Couldn't match given Arguments to a form that is useable.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            else
            {
                if (json.ContainsKey("id") && json["id"] != null)
                {
                    f.TimeSessionEnd = DateTime.Now.AddMinutes(((Int64)json["id"]));
                    f.timeleftnet = (Int32)((Int64)json["id"]);
                    if (f.timeleftnet >= Config.SessionEndShowTimeLeft)
                    {
                        f.switchedtotimepage = false;
                    }
                }
                else
                {
                    Logger.Print("Missing necesarry Json key (\"id\", to set time to).", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            
        }

        private void ServicePacket(Dictionary<String, Object> json, MainForm f)
        {
            if (jsonpartvalid(json, "label") && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.ServicesSetting ss in Config.ServicesSettings)
                {
                    RequestJson j = new RequestJson();
                    j.type = Config.Typenames[3].Trim().ToLower();
                    j.Raum = Config.Room;
                    j.id = ss.id;
                    j.label = ss.JsonText;
                    String[] t = new string[3];
                    t[0] = ss.ShowText;
                    t[1] = ((Constants.ServicesSettingfunction)ss).ToString();
                    j.values = t;
                    ret += assembleQuestionJsonString(j);

                }
                RequestJson r = new RequestJson();
                r.Raum = Config.Room;
                r.type = Config.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            if ((jsonpartvalid(json, "id") || jsonpartvalid(json, "label")) && jsonpartvalid(json, "values") && ((JArray)(json["values"])).Count > 0)
            {
                Constants.ServicesSettingfunction ss = new Constants.ServicesSettingfunction();
                ss.id = Config.SessionSettings.Count;
                int x = -1;
                if (jsonpartvalid(json, "id") && ((int)json["id"]) < Config.ServicesSettings.Count)
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
                        Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                    }
                    if (x < 0)
                    {
                        for(int i = 0; i< Config.ServicesSettings.Count;i++)
                        {
                            if (((string)json["label"]).Trim().ToLower().Equals(Config.ServicesSettings[i].JsonText.Trim().ToLower()))
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                }
                if (x >= 0 || x < Config.ServicesSettings.Count)
                {
                    ss = (ServicesSettingfunction)Config.ServicesSettings[x];
                }
                if(jsonpartvalid(json, "label") && ((string)(json["label"])).Length > 0)
                {
                    ss.JsonText = ((string)(json["label"]));
                }
                else if (json.ContainsKey("id") && json["id"] != null)
                {
                    ss.JsonText = ((string)(json["id"]));
                }
                ss.ShowText = ((JArray)(json["values"])).ToObject<String[]>()[0];
                Constants.rawfunctiontext fun = new Constants.rawfunctiontext();
                fun.functionText = ((JArray)(json["values"])).ToObject<String[]>()[1];
                ss.secondary = fun;
                //Not secure, because setupsecondaryfunctionsforServiceButtons can return ServiceSettings without secondary function.
                ss = (ServicesSettingfunction)Config.setupsecondaryfunctionsforServiceButtons(ss);
                if (!Config.ServicesSettings.Contains(ss))
                {
                    Config.ServicesSettings.Add(ss);
                    f.helper.GendynamicServiceButtons();
                }
            }
            else if(jsonpartvalid(json, "values") && ((JArray)(json["values"])).Count > 2)
            {
                Constants.ServicesSettingfunction ss = new Constants.ServicesSettingfunction();
                ss.id = Config.SessionSettings.Count;
                int x = -1;
                try
                {
                    x = Int32.Parse(((JArray)json["values"]).ToObject<String[]>()[0]);
                }
                catch (FormatException ex)
                {
                    Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                }
                if (x < 0)
                {
                    for (int i = 0; i < Config.ServicesSettings.Count; i++)
                    {
                        if (((string[])json["values"])[0].Trim().ToLower().Equals(Config.ServicesSettings[i].JsonText.Trim().ToLower()))
                        {
                            x = i;
                            break;
                        }
                    }
                }
                if (x >= 0 || x < Config.ServicesSettings.Count)
                {
                    ss = (ServicesSettingfunction)Config.ServicesSettings[x];
                }
                ss.ShowText = ((JArray)(json["values"])).ToObject<String[]>()[0];
                Constants.rawfunctiontext fun = new Constants.rawfunctiontext();
                fun.functionText = ((JArray)(json["values"])).ToObject<String[]>()[1];
                ss.JsonText = ((JArray)(json["values"])).ToObject<String[]>()[2];
                ss.secondary = fun;
                //Not secure, because setupsecondaryfunctionsforServiceButtons can return ServiceSettings without secondary function.
                ss = (ServicesSettingfunction)Config.setupsecondaryfunctionsforServiceButtons(ss);
                if (!Config.ServicesSettings.Contains(ss))
                {
                    Config.ServicesSettings.Add(ss);
                    f.helper.GendynamicServiceButtons();
                }
            }
            else
            {
                Logger.Print("Given arguments do not meet the requirements to add or edit a Service Button", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
            }
        }

        private void DMXScenePacket(Dictionary<String, Object> json, MainForm f)
        {
            if (jsonpartvalid(json, "label") && ((string)json["label"]).Equals("?"))
            {
                String ret = "";
                foreach (Constants.DMXScene ss in Config.DMXScenes)
                {
                    RequestJson j = new RequestJson();
                    j.type = Config.Typenames[4].Trim().ToLower();
                    j.Raum = Config.Room;
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
                r.Raum = Config.Room;
                r.type = Config.Typenames[0];
                r.label = "?";
                r.values = [ret];
                SendTCPMessage(r, null);
                return;
            }
            int index = -1;
            if (jsonpartvalid(json, "id"))
            {
                index = (Int32)(Int64)json["id"];
            }
            else if (jsonpartvalid(json, "label"))
            {
                for (int i = 0; i < Config.DMXScenes.Count; i++)
                {
                    if (Config.DMXScenes[i].JsonText.Trim().ToLower().Equals(((String)json["label"]).Trim().ToLower()))
                    {
                        if (index > 0)
                        {
                            Logger.Print("DMXScene Paket label has more than 1 reference in scenes. Taking first found scene", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            continue;
                        }
                        index = i;
                    }
                }
            }
            else
            {
                Logger.Print("DMXScene Paket doesn't contain necesarry Keys (\"id\" or \"label\") to identify the scene to be edited", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if (jsonpartvalid(json, "values"))
            {
                if (Config.DMXScenes.Count > index && index >= 0)
                {
                    Config.DMXScenes[index].ShowText = ((JArray)json["values"]).ToObject<String[]>()[0];
                    String Path = ((JArray)json["values"]).ToObject<String[]>()[1];
                    Config.finalizePaths(out Path, Path);
                    if (File.Exists(Path))
                    {
                        Config.DMXScenes[index].ContentPath = Path;
                        f.helper.GendynamicAmbientButtons();
                        Logger.Print($"Name and Path of Scene {Config.DMXScenes[index].JsonText} updated", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                    }
                    else
                    {
                        Logger.Print($"Only Name of Scene {Config.DMXScenes[index].JsonText} updated, No File found for the given Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }

                }
                else
                {
                    Logger.Print("DMXScene Paket id out of range. Creating new temporary scene. To show scene in UI you have to send another packet to set showname and (optionally) the Content Path", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    Constants.DMXScene scene = new Constants.DMXScene();
                    scene.id = Config.DMXScenes.Count;
                    scene.JsonText = (string)json["type"];
                    bool sucess = false;
                    if (json["values"].GetType() == typeof(byte[]))
                    {
                        try
                        {
                            for (int i = 0; i < ((JArray)json["values"]).Count; i++)
                            {
                                int x = Int32.Parse(((JArray)json["values"]).ToObject<String[]>()[i]);
                                x = Math.Min(255, x);
                                x = Math.Max(0, x);
                                scene.Channelvalues[i] = (byte)x;
                                if (i == ((JArray)json["values"]).Count - 1)
                                {
                                    sucess = true;
                                }
                            }
                        }
                        catch (FormatException fex)
                        {
                            sucess = false;
                            Logger.Print(fex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                        }
                    }
                    else
                    {
                        Logger.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[]), aborting creation.", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                    if (sucess)
                    {
                        Config.DMXScenes.Add(scene);
                        Logger.Print($"Added DMXScene with Json label: {scene.JsonText}", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                    }
                    else
                    {
                        Logger.Print($"Could not Add DMXScene with, because of an error, while Parsing \"values\" Array", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                    }
                }
            }
            else
            {
                if (index >= 0 && index < Config.DMXScenes.Count)
                {
                    f.Ambiente_Change(Config.DMXScenes[index], true, true, false);
                }
                else
                {
                    Logger.Print("DMXScene Paket \"id\" / \"label\" out of range", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                }
            }
            f.SendCurrentSceneOverCom();
        }

        private bool indexsjsontypeswitch(Dictionary<string, object>? Json, int i)
        {
            switch (i)
            {
                case 0:
                    SystemPacket((Dictionary<String, Object>)Json, form);
                    return true;
                case 1:
                    TCPPacket((Dictionary<String, Object>)Json, form);
                    return true;
                case 2:
                    SessionPacket((Dictionary<String, Object>)Json, form);
                    return true;
                case 3:
                    ServicePacket((Dictionary<String, Object>)Json, form);
                    return true;
                case 4:
                    DMXScenePacket((Dictionary<String, Object>)Json, form);
                    return true;
                default:
                    return false;
            }
        }

        private void edittcpwithjsonvaluesarraydata(Dictionary<string, object> json, MainForm f, Constants.TCPSetting tcp)
        {
            String[] jsonvalues = ((JArray)json["values"]).ToObject<String[]>();
            tcp.ShowText = jsonvalues[0];
            tcp.JsonText = jsonvalues[1];
            try
            {
                tcp.id = Int32.Parse(((string[])json["values"])[2]);
            }
            catch (FormatException ex)
            {
                Logger.Print(ex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                Logger.Print("Aborted new TCP creation, because of invalid \"id\"", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                return;
            }
            if (jsonvalues.Length > 3)
            {
                tcp.value = jsonvalues[3];
            }
            Config.TCPSettings.Add(tcp);
            f.logout();
            f.UIControl.SelectTab(f.UIControl.TabCount - 1);
            Logger.Print("Added new TCP Setting to the \"Wartungs\" Page", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
        }

        private String parse(byte[] json)
        {
            String resString;
            try
            {
                resString = System.Text.Encoding.Default.GetString(json);
            }catch(DecoderFallbackException e)
            {
                Logger.Print(e.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                return null;
            }
            return resString;
        }

        public void receivedMessage(byte[] bytes, Socket cl, bool isTelnet, Network net, int index)
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
            if(m.Length <= 0 || !m.Contains('{') || !m.Contains('}'))
            {
                return;
            }
            Logger.Print(m, Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            Dictionary<String, Object> keyValuePairs;
            try
            {
                keyValuePairs = JsonConvert.DeserializeObject<Dictionary<String, Object>>(m);
            }catch(JsonReaderException jex)
            {
                MainForm.currentState = 2;
                Logger.Print(jex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                return;
            }
            catch (JsonSerializationException jex)
            {
                MainForm.currentState = 2;
                Logger.Print(jex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                return;
            }
            catch (JsonException jex)
            {
                MainForm.currentState = 2;
                Logger.Print(jex.Message, Logger.MessageType.TCPReceive, Logger.MessageSubType.Error);
                return;
            }
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

        public bool jsonpartvalid(Dictionary<string, object> json, string s)
        {
            if(!(json.ContainsKey(s) && json[s] != null))
            {
                return false;
            }
            switch (s)
            {
                case "type":
                    return (json[s].GetType().Equals(typeof(Int64)) || (json[s].GetType().Equals(typeof(String)) && ((string)(json[s])).Length > 0));
                case "room":
                    return json[s].GetType().Equals(typeof(Int64)) && ((Int64)(json[s])) >= 0;
                case "id":
                    return json[s].GetType().Equals(typeof(Int64));
                case "label":
                    return json[s].GetType().Equals(typeof(String)) && ((string)(json[s])).Length > 0;
                case "values":
                    return json[s].GetType().Equals(typeof(JArray)) && ((JArray)(json[s])).Count > 0;
                default:
                    return false;
                    break;
            }
        }
    }
}
