using System.Diagnostics;
using static Spa_Interaction_Screen.Constants;

namespace Spa_Interaction_Screen
{
    public class Config
    {
        //current generated wifi password
        public String password = null;

        //Config
        public int Room = -1;
        public int TotalRooms = -1;
        public String[] IPZentrale = null;
        public int PortZentrale = -1;
        public int StateSendInterval = -1;
        public String ComPort = null;
        public String EnttecComPort = null;
        public String[] Wartungspin = null;
        public int Sitzungsdauer = -1;
        public String GastroUrl = null;
        public String WiFiSSID = null;
        public String WiFiAPIPassword = null;
        public String[] IPRouter = null;
        public int PortRouter = -1;
        public int showtime = -1;
        public int showcolor = -1;
        public String PasswordFilePath = null;
        public String LogoFilePath = null;
        public String AmbienteBackgroundFilePath = null;
        public String MediaBackgroundFilePath = null;
        public String TimeBackgroundFilePath = null;
        public String ServiceBackgroundFilePath = null;
        public int[] Dimmerchannel = null;
        public String[] slidernames = null;
        public byte[] HDMISwitchInterval = null;
        public int HDMISwitchchannel = -1;
        public byte[] ObjectLightInterval = null;
        public String Objectname = null;
        public int[][] colorwheelvalues;
        public int SystemSettingsChannel = -1;
        public List<Constants.SystemSetting> SystemSettings = null;
        public int ObjectLightchannel = -1;
        public int SessionSetting = -1;
        public int DMXSceneSetting = -1;
        public int DMXSceneSettingChannel = -1;
        public String DMXSceneSettingJson = null;
        public String VolumeSliderName = null;
        public int Volume = -1;
        public int VolumeChannel = -1;
        public String VolumeJson = null;
        public int StateChannel = -1;
        public List<Constants.SessionSetting> SessionSettings = null;
        public int SessionChannel = -1;
        public List<Constants.ServicesSetting> ServicesSettings = null;
        public int ServicesChannel = -1;
        public List<Constants.DMXScene> DMXScenes = null;
        public int DMXScenesChannel = -1;

        public Config(Config? c)
        {
            if (!File.Exists(Constants.Configpath))
            {
                Debug.Print("Config not Found ");
                return;
            }
            StreamReader stream = null;
            try
            {
                stream = File.OpenText(Constants.Configpath);
            }
            catch (IOException ex)
            {
                Debug.Print(ex.Message);
                Debug.Print("Could not open Config File");
            }

            if (stream == null)
            {
                return;
            }
            String res = stream.ReadLine();

            while ((res == null || res.Length <= 0) && !stream.EndOfStream)
            {
                res = stream.ReadLine();
            }
            if (stream.EndOfStream)
            {
                Debug.Print("Reached End of file unexpectedly. No Data Read.");
                stream.Close();
                return;
            }
            String[] fields = stripComments(res.Split(Constants.Delimiter[0]), Constants.Delimiter[1]);
            Debug.Print($"CSV Version: {fields[0]}");
            if (!fields[0].Equals(Constants.CurrentVersion))
            {
                Debug.Print("Config doesnt have the Correct Version");
                stream.Close();
                return;
            }
            bool read_all = true;
            read_all = getcsvFields(stream, ref Room, 0);
            read_all = getcsvFields(stream, ref TotalRooms, 0);
            read_all = getcsvFields(stream, ref IPZentrale, -1);
            read_all = getcsvFields(stream, ref PortZentrale, 0);
            read_all = getcsvFields(stream, ref StateSendInterval, 0);
            read_all = getcsvFields(stream, ref ComPort, 0);
            read_all = getcsvFields(stream, ref EnttecComPort, 0);
            read_all = getcsvFields(stream, ref Wartungspin, -1);
            read_all = getcsvFields(stream, ref Sitzungsdauer, 0);
            read_all = getcsvFields(stream, ref GastroUrl, 0);
            read_all = getcsvFields(stream, ref WiFiSSID, 0);
            read_all = getcsvFields(stream, ref WiFiAPIPassword, 0);
            read_all = getcsvFields(stream, ref IPRouter, -1);
            read_all = getcsvFields(stream, ref PortRouter, 0);
            read_all = getcsvFields(stream, ref showtime, 0);
            read_all = getcsvFields(stream, ref showcolor, 0);
            read_all = getcsvFields(stream, ref PasswordFilePath, 0);
            read_all = getcsvFields(stream, ref LogoFilePath, 0);
            read_all = getcsvFields(stream, ref AmbienteBackgroundFilePath, 0);
            read_all = getcsvFields(stream, ref MediaBackgroundFilePath, 0);
            read_all = getcsvFields(stream, ref TimeBackgroundFilePath, 0);
            read_all = getcsvFields(stream, ref ServiceBackgroundFilePath, 0);
            finalizePaths(out PasswordFilePath, PasswordFilePath);
            finalizePaths(out LogoFilePath, LogoFilePath);
            finalizePaths(out AmbienteBackgroundFilePath, AmbienteBackgroundFilePath);
            finalizePaths(out MediaBackgroundFilePath, MediaBackgroundFilePath);
            finalizePaths(out TimeBackgroundFilePath, TimeBackgroundFilePath);
            finalizePaths(out ServiceBackgroundFilePath, ServiceBackgroundFilePath);
            stream.ReadLine();
            String[] Dimmerchannelval1 = null;
            read_all = getcsvFields(stream, ref Dimmerchannelval1, -1);
            String[] Dimmerchannelval2 = null;
            read_all = getcsvFields(stream, ref Dimmerchannelval2, -1);
            Dimmerchannel = new Int32[2] { Int32.Parse(Dimmerchannelval1[0]), Int32.Parse(Dimmerchannelval2[0]) };
            slidernames = new String[] { Dimmerchannelval1[1], Dimmerchannelval2[1], null };
            String[] field = null;
            read_all = getcsvFields(stream, ref field, -1);
            try
            {
                HDMISwitchchannel = Int32.Parse(field[0]);
                HDMISwitchInterval = new Byte[2] { Byte.Parse(field[1]), Byte.Parse(field[2]) };
            }
            catch (FormatException e)
            {
                Debug.Print(e.Message);
                Debug.Print("Die in der Konfig angegebene Zahl für die Sauna ist fehlerhaft.");
            }
            field = null;
            read_all = getcsvFields(stream, ref field, -1);
            try
            {
                ObjectLightchannel = Int32.Parse(field[0]);
                Objectname = field[1];
                ObjectLightInterval = new Byte[2] { Byte.Parse(field[2]), Byte.Parse(field[3]) };
            }
            catch (FormatException e)
            {
                Debug.Print(e.Message);
                Debug.Print("Die in der Konfig angegebene Zahl für die Sauna ist fehlerhaft.");
            }
            String[] ReadFields = null;
            read_all = getcsvFields(stream, ref ReadFields, -1);
            colorwheelvalues = new Int32[3][];
            bool atleastonechannel = false;
            for (int i = 0; i < ReadFields.Length; i++)
            {
                int x = 0;
                colorwheelvalues[i] = new Int32[ReadFields[i].Length];
                foreach (String number in ReadFields[i].Split(','))
                {
                    try
                    {
                        colorwheelvalues[i][x++] = Int32.Parse(number.Trim());
                    }
                    catch (FormatException e)
                    {
                        Debug.Print(e.Message);
                        Debug.Print("Die in der Konfig angegebene Zahl für die Colorwheelvalues ist fehlerhaft.");
                    }
                    atleastonechannel = true;
                }
                Array.Resize(ref colorwheelvalues[i], x);
            }
            if (!atleastonechannel)
            {
                showcolor = 0;
            }
            read_all = getcsvFields(stream, ref ReadFields, -1);
            while (ReadFields == null || !ReadFields[0].Equals("System"))
            {
                read_all = getcsvFields(stream, ref ReadFields, -1);
            }
            SystemSettingsChannel = Int32.Parse(ReadFields[1]);
            SystemSettings = new List<SystemSetting>();
            for (int i = 0; i < 4; i++)
            {
                read_all = getcsvFields(stream, ref ReadFields, -1);
                SystemSetting s = new SystemSetting();
                s.startvalue = Int32.Parse(ReadFields[0]);
                s.endvalue = Int32.Parse(ReadFields[1]);
                s.JsonText = ReadFields[2];
                SystemSettings.Add(s);
            }

            stream.ReadLine();
            read_all = getcsvFields(stream, ref ReadFields, -1);
            DMXSceneSettingJson = ReadFields[0];
            DMXSceneSetting = Int32.Parse(ReadFields[1]);
            DMXSceneSettingChannel = Int32.Parse(ReadFields[2]);

            read_all = getcsvFields(stream, ref ReadFields, -1);
            slidernames[2] = ReadFields[0];
            VolumeJson = ReadFields[1];
            Volume = Int32.Parse(ReadFields[2]);
            VolumeChannel = Int32.Parse(ReadFields[3]);

            stream.ReadLine();
            if (!read_all)
            {
                Debug.Print("Something went wrong in reading the single Variables");
                stream.Close();
                return;
            }
            while (ReadFields == null || (!ReadFields[0].Equals("Session") && !ReadFields[0].Equals("Services") && !ReadFields[0].Equals("DMXScenes")))
            {
                read_all = getcsvFields(stream, ref ReadFields, -1);
            }
            if (!read_all)
            {
                Debug.Print("Something went wrong in reading the bunch Variables");
                stream.Close();
                return;
            }
            SessionSettings = new List<SessionSetting>();
            ServicesSettings = new List<ServicesSetting>();
            DMXScenes = new List<DMXScene>();
            while (read_all)
            {
                switch (ReadFields[0])
                {
                    case "Session":
                        if (SessionChannel < 0 && ReadFields.Length > 1)
                        {
                            SessionChannel = Int32.Parse(ReadFields[1]);
                        }
                        read_all = getcsvFields(stream, ref ReadFields, -1);
                        while (ReadFields == null || (!ReadFields[0].Equals("Session") && !ReadFields[0].Equals("Services") && !ReadFields[0].Equals("DMXScenes")))
                        {
                            SessionSetting si = new SessionSetting();
                            si.id = SessionSettings.Count;
                            si.startvalue = Int32.Parse(ReadFields[0]);
                            si.endvalue = Int32.Parse(ReadFields[1]);
                            si.JsonValue = Int32.Parse(ReadFields[2]);
                            if (ReadFields.Length > 3)
                            {
                                si.ShowText = ReadFields[3];
                            }
                            SessionSettings.Add(si);
                            read_all = getcsvFields(stream, ref ReadFields, -1);
                            if (!read_all)
                            {
                                Debug.Print($"Something went wrong in reading the Session Variables: {SessionSettings.Count}");
                                break;
                            }
                        }
                        break;
                    case "Services":
                        if (ServicesChannel < 0 && ReadFields.Length > 1)
                        {
                            ServicesChannel = Int32.Parse(ReadFields[1]);
                        }
                        read_all = getcsvFields(stream, ref ReadFields, -1);
                        while (ReadFields == null || (!ReadFields[0].Equals("Session") && !ReadFields[0].Equals("Services") && !ReadFields[0].Equals("DMXScenes")))
                        {
                            ServicesSetting si = new ServicesSetting();
                            si.id = ServicesSettings.Count;
                            int ind = 0;
                            if (ReadFields.Length > 3)
                            {
                                si.startvalue = Int32.Parse(ReadFields[ind++]);
                                si.endvalue = Int32.Parse(ReadFields[ind++]);
                            }
                            si.ShowText = ReadFields[ind++];
                            si.JsonText = ReadFields[ind++];
                            ServicesSettings.Add(si);
                            read_all = getcsvFields(stream, ref ReadFields, -1);
                            if (!read_all)
                            {
                                Debug.Print($"Something went wrong in reading the Services Variables: {ServicesSettings.Count}");
                                break;
                            }
                        }
                        break;
                    case "DMXScenes":
                        read_all = getcsvFields(stream, ref ReadFields, -1);
                        while (ReadFields == null || (!ReadFields[0].Equals("Session") && !ReadFields[0].Equals("Services") && !ReadFields[0].Equals("DMXScenes")))
                        {
                            DMXScene scene = new DMXScene();
                            int i = 0;
                            scene.id = DMXScenes.Count;
                            scene.ShowText = ReadFields[i++];
                            scene.JsonText = ReadFields[i++];
                            scene.Channelvalues = new byte[ReadFields.Length - 2];
                            int rese;
                            if (!int.TryParse(ReadFields[ReadFields.Length - 1], out rese))
                            {
                                scene.ContentPath = ReadFields[ReadFields.Length - 1];
                                finalizePaths(out scene.ContentPath, scene.ContentPath);
                                scene.Channelvalues = new byte[ReadFields.Length - 3];
                            }
                            int x = i;
                            for (i = i; i < scene.Channelvalues.Length; i++)
                            {
                                if (Int32.Parse(ReadFields[i]) > 255 || Int32.Parse(ReadFields[i]) < 0)
                                {
                                    scene.Channelvalues[i - x] = 0;
                                }
                                else
                                {
                                    scene.Channelvalues[i - x] = Byte.Parse(ReadFields[i]);
                                }
                            }
                            DMXScenes.Add(scene);
                            read_all = getcsvFields(stream, ref ReadFields, -1);
                            if (!read_all)
                            {
                                Debug.Print($"Something went wrong in reading the DMXScenes Variables: {DMXScenes.Count}");
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                read_all = getcsvFields(stream, ref ReadFields, -1);
            }
            while (DMXScenes.Count < 2)
            {
                DMXScene scene = new DMXScene();
                scene.ShowText = $"Generated {DMXScenes.Count}";
                scene.JsonText = $"Generated {DMXScenes.Count}";
                scene.ContentPath = "";
                if (DMXScenes.Count > 0)
                {
                    scene.Channelvalues = new byte[DMXScenes[0].Channelvalues.Length - 3];
                }
                else
                {
                    scene.Channelvalues = new byte[Math.Max(Math.Max(Dimmerchannel[0], Dimmerchannel[1]), Math.Max(Dimmerchannel[1], ObjectLightchannel))];
                }
                for (int i = 0; i < scene.Channelvalues.Length - 1; i++)
                {
                    scene.Channelvalues[i] = 0;
                }
                DMXScenes.Add(scene);
            }
            stream.Close();
        }

        private void finalizePaths(out String? sp, String s)
        {
            if (s != null)
            {
                if (s.Length >= 0)
                {
                    s = $@"{s.ToLower()}";
                }
                else
                {
                    s = null;
                }
            }
            sp = s;
        }

        public bool getcsvFields<T>(StreamReader stream, ref T variable, int index)
        {
            dynamic dynamicVariable = variable;
            if (stream == null || stream.EndOfStream)
            {
                return false;
            }
            String[] s = stripComments(stream.ReadLine().Split(Delimiter[0]), Delimiter[1]);
            if (s == null || s.Length <= 0)
            {
                return false;
            }
            if (index >= 0 && s.Length >= index)
            {
                try
                {
                    variable = (T)Convert.ChangeType(s[index], typeof(T));
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print("Beim einlesen der Konfig konnte eine Einstellung nicht in den benötigten Typen umgewandelt werden.");
                }
            }
            else
            {
                if (typeof(T).IsArray)
                {
                    try
                    {
                        variable = (T)(object)Convert.ChangeType(s, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                        Debug.Print("Beim einlesen der Konfig konnte eine Einstellungsmenge nicht in den benötigten Typen umgewandelt werden.");
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public String[] stripComments(String[] field, Char CommandDelimiters)
        {
            String[] res = new String[field.Length];
            int x = 0;
            foreach (String s in field)
            {
                if (s != null && !s.StartsWith(CommandDelimiters) && !s.Equals("") && s.Length > 0)
                {
                    res[x++] = s;
                }
            }
            Array.Resize(ref res, x);
            return res;
        }
    }
}
