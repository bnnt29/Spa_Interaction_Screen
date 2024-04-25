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
using ArtNetSharp;

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
        public bool routeranswered = false;

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
        /*
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
        private static String assembleJsonString(RequestJson req)
        {
            String s = "{";
            s += $"\"type\":{req.type}";
            if (req.Raum != null)
            {
                s += $",\"room\":{req.Raum}";
            }
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

        public class RequestJson
        {
            public String destination;
            public int port;
            public String type;
            public int? Raum = null;
            public String? label = null;
            public int? id = null;
            public String[]? values = null;
        }
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
        */
        private void handleReceivedNet(Dictionary<String, Object>? Json)
        {
            if (Json == null)
            {
                return;
            }
            if (!Json.ContainsKey("type"))
            {
                Debug.Print("Invalid Json received via UDP");
            }
            switch ((String)Json["type"])
            {
                case "Status":
                    Debug.Print("Received type Status Paket. It doesnt belog here.");
                    break;
                case "Service":
                    Debug.Print("Received type Service Paket. It doesnt belog here.");
                    break;
                case "Wartung":
                    Debug.Print("Received type Wartung Paket. It doesnt belog here.");
                    break;
                case "Session":
                    SessionPacket((Dictionary<String, Object>)Json, form, config);
                    break;
                case "System":
                    SystemPacket((Dictionary<String, Object>)Json, form, config);
                    break;
                case "DMXScene":
                    DMXScenePacket((Dictionary<String, Object>)Json, form, config);
                    break;
                case "Volume":
                    VolumePacket((Dictionary<String, Object>)Json, form, config);
                    break;
                case "Update":
                    UpdatePacket((Dictionary<String, Object>)Json, form, config);
                    break;
                default:
                    Debug.Print("Couldn't process [\"type\"] argument in Json");
                    break;
            }
        }
        private static void SessionPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            Debug.Print("Not yet implemented");
        }

        private static void SystemPacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            Debug.Print("Not yet implemented");
        }

        private static void DMXScenePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            if (json.ContainsKey("values"))
            {
                int index = -1;
                if (json.ContainsKey("id"))
                {
                    index = (int)json["id"];
                }
                else if (json.ContainsKey("label") && ((String)(json["label"])).Length >= 0)
                {
                    for (int i = 0; i < c.DMXScenes.Count; i++)
                    {
                        if (c.DMXScenes[i].Equals((String)json["label"]))
                        {
                            if (index >= 0)
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
                }
                if (c.DMXScenes.Count > index && index >= 0)
                {
                    if (json["values"].GetType() == typeof(byte[]))
                    {
                        //TODO: Check for correct DMXValues (maybe also when reading config
                        //TODO: Implement Update Method to update UI correctly and send data
                        c.DMXScenes[index].Channelvalues = (byte[])json["values"];
                    }
                    else
                    {
                        Debug.Print("DMXScene Paket has wrong Type for key \"values\" (needed: byte[])");
                    }
                }
                else
                {
                    Debug.Print("DMXScene Paket id out of range");
                }
            }
            else
            {
                int index = -1;
                if (json.ContainsKey("id"))
                {
                    index = (int)(long)json["id"];

                }
                else if (json.ContainsKey("label"))
                {
                    for (int i = 0; i < c.DMXScenes.Count; i++)
                    {
                        if (c.DMXScenes[i].Equals((String)json["label"]))
                        {
                            if (index >= 0)
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
                }
                if (index >= 0 && index < c.DMXScenes.Count)
                {
                    //TODO: Implement Update Method to update UI correctly and send data
                    f.Ambiente_Change(c.DMXScenes[index], true, true);
                }
                else
                {
                    Debug.Print("DMXScene Paket id out of range");
                }
            }
        }
        private static void VolumePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            Debug.Print("Not yet implemented");
        }

        private static void UpdatePacket(Dictionary<String, Object> json, MainForm f, Config c)
        {
            Debug.Print("Not yet implemented");
        }

        private static String parse(byte[] json)
        {
            String jsonString = "";
            try
            {
                jsonString = System.Text.Encoding.Default.GetString(json);
            }catch(Exception e)
            {
                Debug.Print(e.Message);
                return null;
            }
            Debug.Print(jsonString);
            return jsonString;
        }

        public void receivedMessage(byte[] bytes, TcpClient cl, bool isTelnet, Network net)
        {
            if(parse(bytes) == null)
            {
                return;
            }
            String m = parse(bytes);
            m= m.Trim().ToLower();
            if (isTelnet)
            {
                if (m.Contains("WMedia"))
                {
                    net.routeranswered = true;
                    Debug.Print("Received Router answer");
                }
                handleReceivedTel(m);
            }else
            {
                Dictionary<String, Object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<String, Object>>(m);
                handleReceivedNet(keyValuePairs);
            }
        }

        private void handleReceivedTel(String m)
        {
            Debug.Print(m);
            if (m.Contains("#pass") && m.Contains("OK"))
            {
                String[] lines = m.Split("WMedia>");
                foreach(String line in lines)
                {
                    if (line.StartsWith('#') && line.Contains("pass") && line.Contains(":"))
                    {
                        String p = line.Split(":")[1].Split("OK")[0];
                        config.password = p;
                        break;
                    }
                }
                form.helper.setnewPassword();
            }
        }

        public void setuprouter(MainForm f)
        {
            if (!connectrouter(f))
            {
                return;
            }
            while(!routeranswered)
            {

            }
            SendTelnet($@"wifi ssid {config.WiFiSSID}");
            while (!routeranswered)
            {

            }
            routerclient.Close();
            connectrouter(f);
            while (!routeranswered)
            {

            }
            String pass = "";
            Random rnd = new Random();
            for(int i =0; i<8; i++)
            {
                pass += $"{rnd.Next(9)}";
            }
            SendTelnet($@"pass {pass}");
            Debug.Print("Router Set");
            while (!routeranswered)
            {

            }
            routerclient.Close();
            connectrouter(f);
            while (!routeranswered)
            {

            }
            SendTelnet($@"pass ?");
            while (!routeranswered)
            {

            }
        }

        private bool connectrouter(MainForm f)
        {
            if(routerclient != null && routerclient.Connected)
            {
                return false;
            }
            String ip_address = f.ArraytoString(config.IPRouter, 4);
            int port_number = config.PortRouter;
            NetworkStream stream = null;
            String response = null;
            try
            {
                routerclient = new TcpClient();
                routerclient.Connect(ip_address, port_number);

                Debug.Print($"[Communication] : [EstablishConnection] : Success connecting to : {ip_address}, port: {port_number}");
            }
            catch (Exception e)
            {
                Debug.Print($"[Communication] : [EstablishConnection] : Failed while connecting to : {ip_address}, port: {port_number}");
                Debug.Print(e.Message);
                routerclient = null;
            }
            if(routerclient == null || !routerclient.Connected)
            {
                Debug.Print("Error when tried to connect to Entec Router via Telnet over TCP\nCannot change Password or name remotely nor display the current password.");
                return false;
            }
                ListenRouter = Task.Run(() => listenTCPConnection(routerclient, this, true));
            return true;

        }
        public bool SendTelnet(String command)
        {
            if (command == null || routerclient == null || !routerclient.Connected)
            {
                Debug.Print("Couldnt Send Telnet to router. Check if Router is reachable.");
                return false;
            }
            NetworkStream ns = routerclient.GetStream();

            Byte[] data = Encoding.Default.GetBytes(command + "\n");
            try
            {
                ns.Write(data, 0, data.Length);
                routeranswered = false;
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

            Debug.Print($"Sent: {command}");
            return true;
        }

        /*
        public async void SendTelnet(String command, MainForm f)
        {
            String ip_address = f.ArraytoString(config.IPRouter, 4);
            int port_number = config.PortRouter;
            NetworkStream stream = null;
            Debug.Print($"Telnet: {command}");
            TcpClient client = null;
            String response = null;
            if (stream == null)
            {
                return;
            }
            StreamWriter writer = new StreamWriter(stream);
            // Send command
            Byte[] data = System.Text.ASCIIEncoding.ASCII.GetBytes(command+"\n");
            while (!stream.CanWrite)
            {

            }
            writer.Write(data);
            stream.Write(data, 0, data.Length);
            //stream.Write(data, 0, data.Length);
#if DEBUG
            Debug.Print($"Sent: {command}");
#endif
            while (!stream.CanRead)
            {

            }
            // Receive response
            if (stream.CanRead)
            {
                response = ReadMessage(stream);
#if DEBUG
                Debug.Print($"Received: {response}");
#endif
            }
            if (stream.CanRead)
            {
                response = ReadMessage(stream);
#if DEBUG
                Debug.Print($"Received: {response}");
#endif
            }
            if (stream.CanRead)
            {
                response = ReadMessage(stream);
#if DEBUG
                Debug.Print($"Received: {response}");
#endif
            }
        }
        */

        public static string ReadMessage(NetworkStream stream)
        {
            // Receive response
            Byte[] responseData = new byte[256];
            Int32 numberOfBytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = Encoding.Default.GetString(responseData, 0, numberOfBytesRead);

            if (response == "SEND_COMMAND_AGAIN")
            {
#if DEBUG
                Debug.WriteLine("[ReadMessage] : Error Retreiving data. Send command again.");
#endif
            }
            return response;
        }

        public bool SendTCPMessage(RequestJson json, TcpClient? cl)
        {
            if(tcpClients == null || tcpClients.Count <= 0)
            {
                return false;
            }
            if (cl == null)
            {
                cl = getbestclient();
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
            }catch(Exception ex)
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

        private TcpClient getbestclient()
        {
            TcpClient? cl = tcpClients[0];
            if (tcpClients.Count > 1)
            {
                byte[] ip = new byte[4];
                try
                {
                    for (int i = 0; i < config.IPZentrale.Length; i++)
                    {
                        ip[i] = Byte.Parse(config.IPZentrale[i]);
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
                    while (i<tcpClients.Count && !tcpClients[i].Connected)
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
                net.ListeningClients.Add(Task.Run(() => listenTCPConnection(client, net, false)));
                IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                Debug.Print($"Client with IP:{endpoint.Address} connected");
            }
        }

        private void listenTCPConnection(TcpClient cl, Network net, bool isTelnet)
        {
            NetworkStream ns = cl.GetStream();
            Debug.Print("ListeningTCP");
            while (cl.Connected)  //while the client is connected, we look for incoming messages
            {
                byte[] msg = new byte[1024];     //the messages arrive as byte array
                try
                {
                    ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
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
                net.receivedMessage(msg, cl, isTelnet, net);
            }
            net.tcpClients.Remove(cl);
        }
    }
}
