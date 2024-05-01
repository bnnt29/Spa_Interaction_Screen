using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Design;
using static QRCoder.PayloadGenerator;
using static Spa_Interaction_Screen.Constants;

namespace Spa_Interaction_Screen
{
    public class Config
    {
        private Logger Log;

        public bool allread = false;
        public byte[] currentvalues = null;
        public DateTime lastchangetime;

        //current generated wifi password
        public String password = null;

        //Config
        public int Room = -1;
        public int TotalRooms = -1;
        public String[] IPZentrale = null;
        public int PortZentrale = -1;
        public int LocalPort = -1;
        public int StateSendInterval = -1;
        public String ComPort = null;
        public String RestrictedDescription = null;
        public int FadeTime = -1;
        public String[] Wartungspin = null;
        public String GastroUrl = null;
        public String WiFiSSID = null;
        public String[] IPRouter = null;
        public int PortRouter = -1;
        public bool showtime = false;
        public bool showedgetime = false;
        public int edgetimePosition = -1;
        public bool showcolor = false;
        public int Logoposition = -1;
        public bool[] showLogo = null;
        public String LogoFilePath = null;
        public String QRLogoFilePath = null;
        public String AmbienteBackgroundImage = null;
        public String ColorBackgroundImage = null;
        public String MediaBackgroundImage = null;
        public String TimeBackgroundImage = null;
        public String ServiceBackgroundImage = null;
        public String WartungBackgroundImage = null;
        public String SessionEndImage = null;
        public int SessionEndShowTimeLeft = -1;
        public String LogPath = null;
        public int[] Dimmerchannel = null;
        public String[] slidernames = null;
        public byte[] HDMISwitchInterval = null;
        public int HDMISwitchchannel = -1;
        public byte[] ObjectLightInterval = null;
        public int ObjectLightchannel = -1;
        public String Objectname = null;
        public int[][] colorwheelvalues;
        public String[] Typenames = null;
        public int SessionSetting = -1;
        public int DMXSceneSetting = -1;
        public String DMXSceneSettingJson = null;
        public String VolumeSliderName = null;
        public int Volume = -1;
        public String VolumeJson = null;
        public List<Constants.SystemSetting> SystemSettings = null;
        public List<Constants.TCPSetting> TCPSettings = null;
        public List<Constants.SessionSetting> SessionSettings = null;
        public List<Constants.ServicesSetting> ServicesSettings = null;
        public List<Constants.DMXScene> DMXScenes = null;

        public Config(Config? c, Logger L)
        {
            Log = L;
            if (!LoadPreConfig())
            {
                Log.Print("Problem in loading PreConfig", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return;
            }
            LoadContentConfig();
        }

        private bool LoadPreConfig()
        {
            if (!File.Exists(Constants.PreConfigPath))
            {
                Log.Print("Config not find PreConfig", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            StreamReader stream = null;
            try
            {
                stream = File.OpenText(Constants.PreConfigPath);
            }
            catch (IOException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Log.Print("Could not open Config File", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            if (stream == null)
            {
                return false;
            }
            String res = null;
            while ((res == null || res.Length <= 0) && !stream.EndOfStream)
            {
                res = stream.ReadLine();
            }
            if (stream.EndOfStream)
            {
                Log.Print("Reached End of file unexpectedly. No Data Read.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return false;
            }
            String[] data = res.Split(Constants.PreConfigDelimiter);
            if (!data[1].Equals(Constants.CurrentVersion))
            {
                Log.Print("Config doesnt have the Correct Version", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return false;
            }
            try
            {
                Constants.DateTimeFormat = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
            }catch(FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            Log.Print($"Loading Preconfig Version: {data[1]}", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            Constants.ContentPath = ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1].ToLower()+"\\";
            try 
            {
                Constants.UDPReceivePort = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            { 
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            Char[] seps = new Char[2];
            seps[0] = ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1].ToCharArray()[0];
            seps[1] = ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1].ToCharArray()[0];
            Constants.Delimiter = seps;
           try
            {
                Constants.maxscenes = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.maxhelps = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.maxtcpws = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.XButtonCount = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.YButtonCount = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.InlineUntilXButtons = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            try
            {
                Constants.noCOM = Boolean.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1].ToLower());
                Constants.noNet = Boolean.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1].ToLower());
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            try
            {
                Constants.EdgeItemposxdist = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.EdgeItemposydist = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.Logoxsize = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
                Constants.Logoysize = Int32.Parse(ReadPreConfig(stream).Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            return true;
        }

        private String ReadPreConfig(StreamReader stream)
        {
            String s = "";
            if(stream == null || stream.EndOfStream)
            {
                return s;
            }
            try
            {
                s = stream.ReadLine();
            }catch (IOException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            return (s!=null)?s:"";
        }

        private void LoadContentConfig()
        {
            String Path = Constants.ContentPath + "ConfigFile.csv";
            Log.Print($"Using {Path} for the MainConfig Path", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            if (!File.Exists(Path))
            {
                Log.Print("Config not find Config", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return;
            }
            StreamReader stream = null;
            try
            {
                stream = File.OpenText(Path);
            }
            catch (IOException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Log.Print("Could not open Config File", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }

            if (stream == null)
            {
                return;
            }
            String res = null;

            while ((res == null || res.Length <= 0) && !stream.EndOfStream)
            {
                res = stream.ReadLine();
            }
            if (stream.EndOfStream)
            {
                Log.Print("Reached End of file unexpectedly. No Data Read.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return;
            }
            String[] fields = stripComments(res.Split(Constants.Delimiter[0]), Constants.Delimiter[1]);
            Log.Print($"Loading CSV Config Version: {fields[0]}", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            if (!fields[0].Equals(Constants.CurrentVersion))
            {
                Log.Print("Config doesnt have the Correct Version", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return;
            }
            bool read_all = true;
            read_all = getcsvFields(stream, ref Room, 0, false, read_all);
            read_all = getcsvFields(stream, ref TotalRooms, 0, false, read_all);
            read_all = getcsvFields(stream, ref IPZentrale, -1, false, read_all);
            read_all = getcsvFields(stream, ref PortZentrale, 0, false, read_all);
            read_all = getcsvFields(stream, ref LocalPort, 0, false, read_all);
            read_all = getcsvFields(stream, ref StateSendInterval, 0, false, read_all);
            read_all = getcsvFields(stream, ref ComPort, 0, false, read_all);
            read_all = getcsvFields(stream, ref RestrictedDescription, 0, false, read_all);
            read_all = getcsvFields(stream, ref FadeTime, 0, false, read_all);
            read_all = getcsvFields(stream, ref Wartungspin, -1, false, read_all);
            read_all = getcsvFields(stream, ref GastroUrl, 0, false, read_all);
            read_all = getcsvFields(stream, ref WiFiSSID, 0, false, read_all);
            WiFiSSID = WiFiSSID.Replace("[Room]", $"{Room}");
            read_all = getcsvFields(stream, ref IPRouter, -1, false, read_all);
            read_all = getcsvFields(stream, ref PortRouter, 0, false, read_all);
            read_all = getcsvFields(stream, ref showtime, 0, false, read_all);
            String[] time = null;
            read_all = getcsvFields(stream, ref time, -1, false, read_all);
            try
            {
                showedgetime = Boolean.Parse(time[0]);
            }
            catch (FormatException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            try
            {
                edgetimePosition = Int32.Parse(time[1]);
            }
            catch (FormatException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(stream, ref showcolor, 0, false, read_all);
            String[] sl = null;
            read_all = getcsvFields(stream, ref sl, -1, false, read_all);
            try
            {
                Logoposition = Int32.Parse(sl[0]);
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            showLogo = new bool[(sl.Length - 1 > 7) ? 7 : sl.Length - 1];
            try
            {
                for (int i = 0; i < showLogo.Length; i++)
                {
                    showLogo[i] = Boolean.Parse(sl[i + 1]);
                }
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(stream, ref LogoFilePath, 0, true, read_all);
            read_all = getcsvFields(stream, ref QRLogoFilePath, 0, true, read_all);
            read_all = getcsvFields(stream, ref AmbienteBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(stream, ref ColorBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(stream, ref MediaBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(stream, ref TimeBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(stream, ref ServiceBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(stream, ref WartungBackgroundImage, 0, true, read_all);
            String[] SessionEnd = null;
            read_all = getcsvFields(stream, ref SessionEnd, -1, true, read_all);
            SessionEndImage = SessionEnd[0];
            try
            {
                SessionEndShowTimeLeft = Int32.Parse(SessionEnd[1]);
            }
            catch (FormatException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(stream, ref LogPath, 0, true, read_all);
            if (!read_all)
            {
                Log.Print("Something went wrong in reading the single Variables (x01)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return;
            }
            finalizePaths(out LogoFilePath, LogoFilePath);
            finalizePaths(out QRLogoFilePath, QRLogoFilePath);
            finalizePaths(out AmbienteBackgroundImage, AmbienteBackgroundImage);
            finalizePaths(out ColorBackgroundImage, ColorBackgroundImage);
            finalizePaths(out MediaBackgroundImage, MediaBackgroundImage);
            finalizePaths(out TimeBackgroundImage, TimeBackgroundImage);
            finalizePaths(out ServiceBackgroundImage, ServiceBackgroundImage);
            finalizePaths(out WartungBackgroundImage, WartungBackgroundImage);
            finalizePaths(out SessionEndImage, SessionEndImage);
            stream.ReadLine();
            String[] Dimmerchannelval1 = null;
            read_all = getcsvFields(stream, ref Dimmerchannelval1, -1, false, read_all);
            String[] Dimmerchannelval2 = null;
            read_all = getcsvFields(stream, ref Dimmerchannelval2, -1, false, read_all);
            try
            {
                Dimmerchannel = new Int32[2] { Int32.Parse(Dimmerchannelval1[0]), Int32.Parse(Dimmerchannelval2[0]) };
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Log.Print("Die in der Konfig angegebene Zahl für die Dimmerchannel ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            slidernames = new String[] { Dimmerchannelval1[1], Dimmerchannelval2[1], null };
            String[] field = null;
            read_all = getcsvFields(stream, ref field, -1, false, read_all);
            try
            {
                HDMISwitchchannel = Int32.Parse(field[0]);
                HDMISwitchInterval = new Byte[2] { Byte.Parse(field[1]), Byte.Parse(field[2]) };
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Log.Print("Die in der Konfig angegebene Zahl für die HDMIchannel ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            field = null;
            read_all = getcsvFields(stream, ref field, -1, false, read_all);
            try
            {
                ObjectLightchannel = Int32.Parse(field[0]);
                Objectname = field[1];
                ObjectLightInterval = new Byte[2] { Byte.Parse(field[2]), Byte.Parse(field[3]) };
            }
            catch (FormatException e)
            {
                Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Log.Print("Die in der Konfig angegebene Zahl für das ObjectLight ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            String[] ReadFields = null;
            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
            colorwheelvalues = new Int32[4][];
            bool atleastonechannel = false;
            for (int i = 0; i < Math.Min(ReadFields.Length, colorwheelvalues.Length); i++)
            {
                int y = 0;
                colorwheelvalues[i] = new Int32[ReadFields[i].Length];
                foreach (String number in ReadFields[i].Split(','))
                {
                    try
                    {
                        colorwheelvalues[i][y++] = Int32.Parse(number.Trim());
                    }
                    catch (FormatException e)
                    {
                        Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                        Log.Print("Die in der Konfig angegebene Zahl für die Colorwheelvalues ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                    }
                    atleastonechannel = true;
                }
                Array.Resize(ref colorwheelvalues[i], y);
            }
            if (!atleastonechannel)
            {
                showcolor = false;
            }
            while (!read_all || ReadFields == null || ReadFields.Length <= 0 || !ReadFields[0].Contains("Json Type:"))
            {
                read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
            }

            if (!read_all)
            {
                Log.Print("Something went wrong in reading the single Variables (x02)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return;
            }
            while (!read_all || ReadFields == null || ReadFields.Length <= 0 || (!ReadFields[0].Contains("Json Type:")))
            {
                read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
            }
            if (!read_all)
            {
                Log.Print("Something went wrong in reading the bunch Variables (x03)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return;
            }
            Typenames = new String[5];
            SystemSettings = new List<SystemSetting>();
            TCPSettings = new List<TCPSetting>();
            SessionSettings = new List<SessionSetting>();
            ServicesSettings = new List<ServicesSetting>();
            DMXScenes = new List<DMXScene>();
            int x = (6 + 5 + 12 + 15 + 5) * 2;
            while (x >= 0)
            {
                if (stream == null || stream.EndOfStream)
                {
                    read_all = true;
                    break;
                }
                if (ReadFields == null || !ReadFields[0].Contains(':'))
                {
                    read_all = getcsvFields(stream, ref ReadFields, -1, true, read_all);
                    continue;
                }

                int Jtype = 0;
                if (ReadFields != null && ReadFields.Length > 1 && ReadFields[0].Contains(":"))
                {
                    try
                    {
                        Jtype = Int32.Parse(ReadFields[0].Split(':')[1].Trim());
                    }
                    catch (FormatException e)
                    {
                        Log.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                switch (Jtype)
                {
                    case 1:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        for (int i = 0; i < 4; i++)
                        {
                            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                            SystemSetting s = new SystemSetting();
                            s.Typename = Typenames[Jtype - 1];
                            s.JsonText = ReadFields[0];
                            s.id = i;
                            SystemSettings.Add(s);
                        }
                        stream.ReadLine();
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        DMXSceneSettingJson = ReadFields[0];
                        DMXSceneSetting = Int32.Parse(ReadFields[1]);

                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        slidernames[2] = ReadFields[0];
                        VolumeJson = ReadFields[1];
                        Volume = Int32.Parse(ReadFields[2]);
                        //stream.ReadLine();
                        //read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        read_all = true;
                        break;
                    case 2:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        while (read_all && ReadFields != null && ReadFields.Length > 2 && !ReadFields[0].Contains("Json Type:"))
                        {
                            TCPSetting s = new TCPSetting();
                            s.Typename = Typenames[Jtype - 1];
                            s.ShowText = ReadFields[0];
                            s.JsonText = ReadFields[1];
                            int ident = -1;
                            try
                            {
                                ident = Int32.Parse((string)ReadFields[2]);
                            }
                            catch (FormatException ex)
                            {
                                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                            }
                            if (ident >= 0)
                            {
                                s.id = ident;
                            }
                            if (ReadFields.Length > 3 && ReadFields[3] != null && ReadFields[3].Length > 0)
                            {
                                s.value = ReadFields[3];
                            }
                            TCPSettings.Add(s);
                            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 3:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        SessionSetting firstss = new SessionSetting();
                        firstss.mins = int.MinValue + 1;
                        firstss.JsonText = "";
                        firstss.ShowText = "";
                        firstss.should_reset = false;
                        SessionSettings.Add(firstss);
                        while (read_all && ReadFields != null && ReadFields.Length >= 2 && !ReadFields[0].Contains("Json Type:"))
                        {
                            SessionSetting si = new SessionSetting();
                            si.Typename = Typenames[Jtype - 1];
                            si.id = SessionSettings.Count;
                            int ident = -1;
                            si.JsonText = ReadFields[0];
                            bool parsed = false;
                            if (si.JsonText.Equals("[all_left]"))
                            {
                                si.mins = int.MaxValue-1;
                            }
                            else
                            {
                                try
                                {
                                    si.mins = Int32.Parse(si.JsonText);
                                }
                                catch (FormatException ex)
                                {
                                    Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                                    continue;
                                }
                            }
                            try
                            {
                                parsed = Boolean.Parse((string)ReadFields[1]);
                            }
                            catch (FormatException ex)
                            {
                                parsed = false;
                                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                            }
                            si.should_reset = parsed;
                            if (ReadFields.Length >= 3)
                            {
                                si.ShowText = ReadFields[2];
                            }
                            SessionSettings.Add(si);
                            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 4:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        while (read_all && ReadFields != null && ReadFields.Length >= 2 && !ReadFields[0].Contains("Json Type:"))
                        {
                            ServicesSettingfunction si = new ServicesSettingfunction();
                            si.Typename = Typenames[Jtype - 1];
                            si.id = ServicesSettings.Count;
                            si.JsonText = ReadFields[0];
                            si.ShowText = ReadFields[1];
                            if (ReadFields.Length > 2 && ReadFields[2] != null && ReadFields[2].Length > 0)
                            {
                                Constants.rawfunctiontext f = new Constants.rawfunctiontext();
                                f.functionText = ReadFields[2];
                                si.function = f;
                            }
                            else
                            {
                                si.function = null;
                            }
                            ServicesSettings.Add(si);
                            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 5:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        while (read_all && ReadFields != null && ReadFields.Length > 2 && !ReadFields[0].Contains("Json Type:"))
                        {
                            DMXScene scene = new DMXScene();
                            scene.Typename = Typenames[Jtype - 1];
                            int i = 0;
                            scene.id = DMXScenes.Count;
                            scene.JsonText = ReadFields[i++];
                            scene.ShowText = ReadFields[i++];
                            scene.Channelvalues = new byte[ReadFields.Length - 2];
                            int rese;
                            if (!int.TryParse(ReadFields[ReadFields.Length - 1], out rese))
                            {
                                scene.ContentPath = ReadFields[ReadFields.Length - 1];
                                finalizePaths(out scene.ContentPath, scene.ContentPath);
                                scene.Channelvalues = new byte[ReadFields.Length - 3];
                            }
                            int y = i;
                            for (i = y; i < scene.Channelvalues.Length; i++)
                            {
                                int save = 0;
                                try
                                {
                                    save = Int32.Parse(ReadFields[i]);
                                }
                                catch (FormatException ex)
                                {
                                    Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                                }
                                save = Math.Min(255, save);
                                save = Math.Max(0, save);
                                scene.Channelvalues[i - y] = (byte)save;
                            }
                            DMXScenes.Add(scene);
                            read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    default:
                        read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        break;
                }
                x--;
            }
            stream.Close();
            if (!read_all)
            {
                return;
            }
            SessionSettings = SessionSettings.OrderBy(x => x.mins).ToList<SessionSetting>();
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
            List<Constants.ServicesSetting> neu = new List<ServicesSetting>();
            foreach (Constants.ServicesSettingfunction sss in ServicesSettings)
            {
                Constants.ServicesSetting temp = setupsecondaryfunctionsforServiceButtons(sss);
                if (temp != null)
                {
                    neu.Add(temp);
                }
            }
            ServicesSettings = neu;
            currentvalues = new byte[DMXScenes[0].Channelvalues.Length];
            allread = true;
        }

        public Constants.ServicesSetting setupsecondaryfunctionsforServiceButtons(Constants.ServicesSettingfunction sss)
        {

            if (sss.function == null || ((Constants.rawfunctiontext)(sss.function)).functionText == null || ((Constants.rawfunctiontext)(sss.function)).functionText.Length <= 0 || !((Constants.rawfunctiontext)(sss.function)).functionText.Contains(',') || ((Constants.rawfunctiontext)(sss.function)).functionText.Split(',').Length < 2)
            {
                ServicesSetting ss = new ServicesSetting();
                ss.id = sss.id;
                ss.ShowText = sss.ShowText;
                ss.JsonText = sss.JsonText;
                return ss;
            }
            bool typefound = false;
            ServicesSettingfunction service = null;
            for (int i = 0; i < Typenames.Length; i++)
            {
                String s = Typenames[i];
                if (s == null || s.Length <= 0)
                {
                    continue;
                }
                String f = ((Constants.rawfunctiontext)(sss.function)).functionText.Split(',')[0].Trim().ToLower();
                if (s.Equals(f))
                {
                    service = getServiceFunctionFromString(i, sss);
                    typefound = true;
                }
            }
            if (!typefound)
            {
                int z = -1;
                try
                {
                    z = Int32.Parse(((Constants.rawfunctiontext)(sss.function)).functionText.Split(',')[0].Trim());
                }
                catch (FormatException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                    return null;
                }
                if (z >= 0 && z < Typenames.Length)
                {
                    service = getServiceFunctionFromString(z, sss);
                }
            }
            if (service == null)
            {
                Constants.ServicesSetting ser = new ServicesSetting();
                ser.id = sss.id;
                ser.ShowText = sss.ShowText;
                ser.JsonText = sss.JsonText;
                return ser;
            }
            service.id = sss.id;
            service.ShowText = sss.ShowText;
            service.JsonText = sss.JsonText;
            service.hassecondary = true;
            return service;
        }

        private ServicesSettingfunction getServiceFunctionFromString(int type, Constants.ServicesSettingfunction s)
        {
            String[] functionspecs = ((Constants.rawfunctiontext)(s.function)).functionText.Split(',');
            configclasses functionclass = null;
            if (functionspecs.Length < 2)
            {
                Log.Print("Not enough Secondary Function arguments", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return null;
            }
            ServicesSettingfunction res = new ServicesSettingfunction();
            res.function = null;
            res.functionclass = Getconfigclasstype(type);
            if (res.functionclass == null)
            {
                return null;
            }
            List<configclasses> typelist = getListfromint(type);
            foreach (configclasses sys in typelist)
            {
                if (sys.JsonText.Equals(functionspecs[1]))
                {
                    res.function = sys;
                    break;
                }
            }
            if (res.function == null)
            {
                int rese = -1;
                if (functionspecs.Length > 0 && int.TryParse(functionspecs[1], out rese))
                {
                    if (rese != 0 && rese >= 0 && rese < getListfromint(type).Count)
                    {
                        res.function = getListfromint(type)[rese];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            try
            {
                if (functionspecs.Length > 2)
                {
                    res.enable = Boolean.Parse(functionspecs[2]);
                }
                else
                {
                    res.enable = false;
                }
                if (functionspecs.Length > 3)
                {
                    res.block = Boolean.Parse(functionspecs[3]);
                }
                else
                {
                    res.block = false;
                }
            }
            catch (FormatException ex)
            {
                Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            if (functionspecs.Length > 4 && functionspecs[4].Length > 0)
            {
                res.value = functionspecs[4];
            }
            else
            {
                res.value = null;
            }
            return res;
        }

        private static Type Getconfigclasstype(int type)
        {
            switch (type)
            {
                case 0:
                    return new SystemSetting().GetType();
                case 1:
                    return new TCPSetting().GetType();
                case 2:
                    return new SessionSetting().GetType();
                case 3:
                    return new ServicesSetting().GetType();
                case 4:
                    return new DMXScene().GetType();
                default:
                    return null;
            }
        }

        private List<configclasses> getListfromint(int x)
        {
            switch (x)
            {
                case 0:
                    return SystemSettings.Cast<configclasses>().ToList();
                case 1:
                    return TCPSettings.Cast<configclasses>().ToList();
                case 2:
                    return SessionSettings.Cast<configclasses>().ToList();
                case 3:
                    return ServicesSettings.Cast<configclasses>().ToList();
                case 4:
                    return DMXScenes.Cast<configclasses>().ToList();
                default:
                    return new List<configclasses>();
            }
        }
        public void finalizePaths(out String? sp, String s)
        {
            if (s != null)
            {
                if (s.Length >= 0)
                {
                    s = $@"{Constants.ContentPath}{s.ToLower()}";
                }
                else
                {
                    s = null;
                }
            }
            sp = s;
        }

        public bool getcsvFields<T>(StreamReader stream, ref T variable, int index, bool canbeempty, bool lasttry)
        {
            variable = default(T); 
            if (stream == null || stream.EndOfStream)
            {
                return false;
            }
            String line = stream.ReadLine();
            if (!lasttry)
            {
                Log.Print("Something went wrong in reading the single Variables (x05)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                stream.Close();
                return false;
            }
            String[] s = stripComments(line.Split(Delimiter[0]), Delimiter[1]);
            if (s == null || s.Length <= 0)
            {
                return canbeempty;
            }
            if (index >= 0 && s.Length >= index)
            {
                try
                {
                    variable = (T)Convert.ChangeType(s[index], typeof(T));
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                    Log.Print("Beim einlesen der Konfig konnte eine Einstellung nicht in den benötigten Typen umgewandelt werden.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
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
                        Log.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                        Log.Print("Beim einlesen der Konfig konnte eine Einstellungsmenge nicht in den benötigten Typen umgewandelt werden.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                    }
                }
                else
                {
                    return false;
                }
            }
            if (EqualityComparer<T>.Default.Equals(variable, default(T)))
            {
                return false;
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
