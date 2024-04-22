using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Spa_Interaction_Screen
{
    public class Network
    {
        private static MainForm form;
        private static Config config;

        public Network(MainForm f, Config c)
        {
            form = f;
            config = c;
        }

        public void changeconfig(Config c)
        {
            config = c;
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
            public int[]? values = null;
        }

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

        private void handleReceivedUDP(Dictionary<String, Object>? Json)
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
                    index = (int)json["id"];

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
                    if (index >= 0 && index < c.DMXScenes.Count)
                    {
                        //TODO: Implement Update Method to update UI correctly and send data
                        c.DMXSceneSetting = index;
                    }
                    else
                    {
                        Debug.Print("DMXScene Paket id out of range");
                    }
                }
                else
                {
                    Debug.Print("DMXScene Paket doesn't contain necesarry Keys (\"id\" or \"label\") to identify the scene to be edited");
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

        private static Dictionary<String, Object>? parse(byte[] json)
        {
            var jsonString = System.Text.Encoding.Default.GetString(json);
            Dictionary<String, Object>? values = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonString);

            return values;
        }
        public static String SendTelnet(String command, MainForm f)
        {
            String ip_address = f.ArraytoString(config.IPRouter, 4);
            int port_number = config.PortRouter;
            NetworkStream stream = null;
            Debug.Print("Telnet1");
            TcpClient client = null;
            String response = null;
            try
            {
                client = new TcpClient(ip_address, port_number);
#if DEBUG
                Debug.WriteLine("[Communication] : [EstablishConnection] : Success connecting to : {0}, port: {1}", ip_address, port_number);
#endif
                stream = client.GetStream();
            }
            catch (Exception e)
            {

                Debug.WriteLine("[Communication] : [EstablishConnection] : Failed while connecting to : {0}, port: {1}", ip_address, port_number);
                Debug.Print(e.Message);
            }
            if (stream == null)
            {
                return "";
            }
            // Send command
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(command);
            stream.Write(data, 0, data.Length);
#if DEBUG
            Debug.Write("Sent : {0}", command);
#endif
            /*
            // Receive response
            response = ReadMessage(stream);
#if DEBUG
            Debug.WriteLine("Received : {0}", response);
#endif
            */
            client.Close();
            return response;
        }
        public static string ReadMessage(NetworkStream stream)
        {
            // Receive response
            Byte[] responseData = new byte[256];
            Int32 numberOfBytesRead = stream.Read(responseData, 0, responseData.Length);
            string response = System.Text.Encoding.ASCII.GetString(responseData, 0, numberOfBytesRead);

            if (response == "SEND_COMMAND_AGAIN")
            {
#if DEBUG
                Debug.WriteLine("[ReadMessage] : Error Retreiving data. Send command again.");
#endif
            }
            return response;
        }
    }
}
