
using System.Diagnostics;
using static Spa_Interaction_Screen.Constants;
using static Spa_Interaction_Screen.Logger;

namespace Spa_Interaction_Screen
{
    public static class Config
    {
        private static String[]? __preconfiglines;
        private static int currentpreconfigindex = 0;
        private static String[]? __configlines;
        private static int currentconfigindex = 0;

        public static bool allread = false;
        public static byte[] currentvalues = null;
        public static DateTime lastchangetime;

        //current generated wifi password
        public static String Wifipassword = null;

        //Config
        public static int Room = -1;
        public static int TotalRooms = -1;
        public static String[] IPZentrale = null;
        public static int PortZentrale = -1;
        public static int LocalPort = -1;
        public static int StateSendInterval = -1;
        public static String ComPort = null;
        public static String RestrictedDescription = null;
        public static int FadeTime = -1;
        public static String[] Wartungspin = null;
        public static String GastroUrl = null;
        public static String WiFiSSID = null;
        public static String[] IPRouter = null;
        public static int PortRouter = -1;
        public static bool showtime = false;
        public static bool showedgetime = false;
        public static int edgetimePosition = -1;
        public static bool showcolor = false;
        public static int Logoposition = -1;
        public static bool[] showLogo = null;
        public static String LogoFilePath = null;
        public static String QRLogoFilePath = null;
        public static String AmbienteBackgroundImage = null;
        public static String ColorBackgroundImage = null;
        public static String MediaBackgroundImage = null;
        public static String TimeBackgroundImage = null;
        public static String ServiceBackgroundImage = null;
        public static String WartungBackgroundImage = null;
        public static String SessionEndImage = null;
        public static int SessionEndShowTimeLeft = -1;
        public static String LogPath = null;
        public static int[] Dimmerchannel = null;
        public static String[] slidernames = null;
        public static byte[] HDMISwitchInterval = null;
        public static int HDMISwitchchannel = -1;
        public static byte[] ObjectLightInterval = null;
        public static int ObjectLightchannel = -1;
        public static String Objectname = null;
        public static int[][] colorwheelvalues = null;
        public static String[] Typenames = null;
        public static int SessionSetting = -1;
        public static int DMXSceneSetting = -1;
        public static String DMXSceneSettingJson = null;
        public static String VolumeSliderName = null;
        public static int Volume = -1;
        public static String VolumeJson = null;
        public static List<Constants.SystemSetting> SystemSettings = null;
        public static List<Constants.TCPSetting> TCPSettings = null;
        public static List<Constants.SessionSetting> SessionSettings = null;
        public static List<Constants.ServicesSetting> ServicesSettings = null;
        public static List<Constants.DMXScene> DMXScenes = null;

        public static void initconfig()
        {
        __preconfiglines = null;
        currentpreconfigindex = 0;
        __configlines = null;
        currentconfigindex = 0;
        if (!Config.LoadPreConfig())
            {
                MainForm.currentState = 6;
                Logger.Print("Problem in loading PreConfig", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return;
            }
            Config.LoadContentConfig();
        }

        public static bool LoadPreConfig()
        {
            if (!File.Exists(Constants.PreConfigPath))
            {
                MainForm.currentState = 6;
                Logger.Print($"Could not find PreConfig: {Constants.PreConfigPath}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            using (FileStream fstream = Logger.CreateStream(Constants.PreConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fstream != null && fstream.CanRead)
                {
                    using (StreamReader stream = new StreamReader(fstream))
                    {
                        if (stream == null || stream.EndOfStream)
                        {
                            MainForm.currentState = 6;
                            Logger.Print("Could not establish Config FileStream", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                            return false;
                        }
                        try
                        {
                            __preconfiglines = stream.ReadToEnd().Split("\r\n");
                        }
                        catch (Exception ex)
                        {
                            Logger.Print(ex.Message, MessageType.Konfig, MessageSubType.Error);
                            Logger.Print("Could not read Konfig file", MessageType.Konfig, MessageSubType.Notice);
                        }
                        stream.Close();
                        stream.Dispose();
                    }
                }
                else
                {
                    MainForm.currentState = 6;
                    Logger.Print("Opened PreConfig, but cannot Read it.", MessageType.Konfig, MessageSubType.Error);
                }
                if (fstream != null)
                {
                    fstream.Close();
                    fstream.Dispose();
                }
            }
            if(__preconfiglines == null || __preconfiglines.Length <= 0)
            {
                Logger.Print("Could not read Konfig file", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return false;
            }
            String[] data = __preconfiglines[currentpreconfigindex++].Split(Constants.PreConfigDelimiter);
            if (!data[1].Equals(Constants.CurrentVersion))
            {
                Logger.Print("PreConfig doesnt have the Correct Version", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return false;
            }
            try
            {
                Constants.DateTimeFormat = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            Logger.Print($"Loading Preconfig Version: {data[1]}", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            Constants.ContentPath = @$"{ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToLower()}";
            try
            {
                Constants.UDPReceivePort = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            Char[] seps = new Char[3];
            seps[0] = ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToCharArray()[0];
            seps[1] = ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToCharArray()[0];
            seps[2] = ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToCharArray()[0];
            Constants.Delimiter = seps;
            try
            {
                Constants.maxscenes = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.maxhelps = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.maxtcpws = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.XButtonCount = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.YButtonCount = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.InlineUntilXButtons = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            try
            {
                Constants.noCOM = Boolean.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToLower());
                Constants.noNet = Boolean.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToLower());
                Constants.showdirectlypotentiallyfalsewificreds = Boolean.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1].ToLower());
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            try
            {
                Constants.EdgeItemposxdist = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.EdgeItemposydist = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.Logoxsize = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
                Constants.Logoysize = Int32.Parse(ReadPreConfig().Split(Constants.PreConfigDelimiter)[1]);
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return false;
            }
            return true;
        }

        private static String ReadPreConfig()
        {
            String s = "";
            if (currentpreconfigindex< __preconfiglines.Length)
            {
                s = __preconfiglines[currentpreconfigindex++];
            }
            return (s!=null)?s:"";
        }

        public static void LoadContentConfig()
        {
            String FilePath = "ConfigFile.csv";
            finalizePaths(out FilePath, FilePath);
            Logger.Print($"Using {FilePath} for the MainConfig Path", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            if (!File.Exists(FilePath))
            {
                MainForm.currentState = 6;
                Logger.Print($"Could not find Config: {FilePath}", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                return;
            }

            bool read_all = true;
            using (FileStream fstream = Logger.CreateStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fstream != null && fstream.CanRead)
                {
                    using (StreamReader stream = new StreamReader(fstream))
                    {
                        if (stream == null || stream.EndOfStream)
                        {
                            MainForm.currentState = 6;
                            Logger.Print("Could not establish Config FileStream", Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                            return;
                        }
                        try
                        {
                            __configlines = stream.ReadToEnd().Split("\r\n");
                        }
                        catch (Exception ex)
                        {
                            Logger.Print(ex.Message, MessageType.Konfig, MessageSubType.Error);
                            Logger.Print("Could not read Konfig file", MessageType.Konfig, MessageSubType.Notice);
                        }
                        stream.Close();
                        stream.Dispose();
                    }
                }
                else
                {
                    MainForm.currentState = 6;
                    Logger.Print("Opened Config, but cannot Read it.", MessageType.Konfig, MessageSubType.Error);

                }
                if (fstream != null)
                {
                    fstream.Close();
                    fstream.Dispose();
                }
            }

            String[] fields = stripComments(__configlines[currentconfigindex++].Split(Constants.Delimiter[0]), Constants.Delimiter[1], Constants.Delimiter[2]);
            Logger.Print($"Loading CSV Config Version: {fields[0]}", Logger.MessageType.Konfig, Logger.MessageSubType.Information);
            if (!fields[0].Equals(Constants.CurrentVersion))
            {
                Logger.Print("Config doesnt have the Correct Version", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return;
            }
            read_all = getcsvFields(ref Room, 0, false, read_all);
            read_all = getcsvFields(ref TotalRooms, 0, false, read_all);
            read_all = getcsvFields(ref IPZentrale, -1, false, read_all);
            read_all = getcsvFields(ref PortZentrale, 0, false, read_all);
            read_all = getcsvFields(ref LocalPort, 0, false, read_all);
            read_all = getcsvFields(ref StateSendInterval, 0, false, read_all);
            read_all = getcsvFields(ref ComPort, 0, false, read_all);
            read_all = getcsvFields(ref RestrictedDescription, 0, false, read_all);
            read_all = getcsvFields(ref FadeTime, 0, false, read_all);
            read_all = getcsvFields(ref Wartungspin, -1, false, read_all);
            read_all = getcsvFields(ref GastroUrl, 0, false, read_all);
            read_all = getcsvFields(ref WiFiSSID, 0, false, read_all);
            WiFiSSID = WiFiSSID.Replace("[Room]", $"{Room}");
            read_all = getcsvFields(ref IPRouter, -1, false, read_all);
            read_all = getcsvFields(ref PortRouter, 0, false, read_all);
            read_all = getcsvFields(ref showtime, 0, false, read_all);
            String[] time = null;
            read_all = getcsvFields(ref time, -1, false, read_all);
            try
            {
                showedgetime = Boolean.Parse(time[0]);
            }
            catch (FormatException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            try
            {
                edgetimePosition = Int32.Parse(time[1]);
            }
            catch (FormatException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(ref showcolor, 0, false, read_all);
            String[] sl = null;
            read_all = getcsvFields(ref sl, -1, false, read_all);
            try
            {
                Logoposition = Int32.Parse(sl[0]);
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
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
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(ref LogoFilePath, 0, true, read_all);
            read_all = getcsvFields(ref QRLogoFilePath, 0, true, read_all);
            read_all = getcsvFields(ref AmbienteBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(ref ColorBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(ref MediaBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(ref TimeBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(ref ServiceBackgroundImage, 0, true, read_all);
            read_all = getcsvFields(ref WartungBackgroundImage, 0, true, read_all);
            String[] SessionEnd = null;
            read_all = getcsvFields(ref SessionEnd, -1, true, read_all);
            SessionEndImage = SessionEnd[0];
            try
            {
                SessionEndShowTimeLeft = Int32.Parse(SessionEnd[1]);
            }
            catch (FormatException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            read_all = getcsvFields(ref LogPath, 0, true, read_all);
            if (!read_all)
            {
                Logger.Print("Something went wrong in reading the single Variables (x01)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
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
            LogPath = LogPath.Replace("[Date]", $"{DateTime.Now.Year}.{DateTime.Now.Month}.{DateTime.Now.Day}");
            LogPath = LogPath.Replace("[Time]", $"{DateTime.Now.Hour}-{DateTime.Now.Minute}");
            LogPath = LogPath.Replace('<', '-').Replace('>', '-').Replace(':', '-').Replace('"', '-');
            LogPath = LogPath.Replace('/', '_').Replace('\\', '_').Replace('|', '_').Replace('?', '_').Replace('*', '_');
            finalizePaths(out LogPath, LogPath);
            Debug.Print(LogPath);
            Logger.InitLogfromBackup();
            currentconfigindex++;
            String[] Dimmerchannelval1 = null;
            read_all = getcsvFields(ref Dimmerchannelval1, -1, false, read_all);
            String[] Dimmerchannelval2 = null;
            read_all = getcsvFields(ref Dimmerchannelval2, -1, false, read_all);
            try
            {
                Dimmerchannel = new Int32[2] { Int32.Parse(Dimmerchannelval1[0]), Int32.Parse(Dimmerchannelval2[0]) };
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print("Die in der Konfig angegebene Zahl für die Dimmerchannel ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            slidernames = new String[] { Dimmerchannelval1[1], Dimmerchannelval2[1], null };
            String[] field = null;
            read_all = getcsvFields(ref field, -1, false, read_all);
            try
            {
                HDMISwitchchannel = Int32.Parse(field[0]);
                HDMISwitchInterval = new Byte[2] { Byte.Parse(field[1]), Byte.Parse(field[2]) };
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print("Die in der Konfig angegebene Zahl für die HDMIchannel ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            field = null;
            read_all = getcsvFields(ref field, -1, false, read_all);
            try
            {
                ObjectLightchannel = Int32.Parse(field[0]);
                Objectname = field[1];
                ObjectLightInterval = new Byte[2] { Byte.Parse(field[2]), Byte.Parse(field[3]) };
            }
            catch (FormatException e)
            {
                MainForm.currentState = 7;
                Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                Logger.Print("Die in der Konfig angegebene Zahl für das ObjectLight ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
            }
            String[] ReadFields = null;
            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
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
                        MainForm.currentState = 7;
                        Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                        Logger.Print("Die in der Konfig angegebene Zahl für die Colorwheelvalues ist fehlerhaft.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
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
                read_all = getcsvFields(ref ReadFields, -1, false, read_all);
            }

            if (!read_all)
            {
                Logger.Print("Something went wrong in reading the single Variables (x02)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return;
            }
            while (!read_all || ReadFields == null || ReadFields.Length <= 0 || (!ReadFields[0].Contains("Json Type:")))
            {
                read_all = getcsvFields(ref ReadFields, -1, false, read_all);
            }
            if (!read_all)
            {
                Logger.Print("Something went wrong in reading the bunch Variables (x03)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
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
                if (currentconfigindex>=__configlines.Length)
                {
                    read_all = true;
                    break;
                }
                if (ReadFields == null || !ReadFields[0].Contains(':'))
                {
                    read_all = getcsvFields(ref ReadFields, -1, true, read_all);
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
                        MainForm.currentState = 7;
                        Logger.Print(e.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
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
                            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                            SystemSetting s = new SystemSetting();
                            s.Typename = Typenames[Jtype - 1];
                            s.JsonText = ReadFields[0];
                            s.id = i;
                            SystemSettings.Add(s);
                        }
                        currentconfigindex++;
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        DMXSceneSettingJson = ReadFields[0];
                        DMXSceneSetting = Int32.Parse(ReadFields[1]);

                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        slidernames[2] = ReadFields[0];
                        VolumeJson = ReadFields[1];
                        Volume = Int32.Parse(ReadFields[2]);
                        //stream.ReadLine();
                        //read_all = getcsvFields(stream, ref ReadFields, -1, false, read_all);
                        read_all = true;
                        break;
                    case 2:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
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
                                MainForm.currentState = 7;
                                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
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
                            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 3:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
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
                                si.mins = int.MaxValue - 1;
                            }
                            else
                            {
                                try
                                {
                                    si.mins = Int32.Parse(si.JsonText);
                                }
                                catch (FormatException ex)
                                {
                                    MainForm.currentState = 7;
                                    Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
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
                                MainForm.currentState = 7;
                                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                            }
                            si.should_reset = parsed;
                            if (ReadFields.Length >= 3)
                            {
                                si.ShowText = ReadFields[2];
                            }
                            SessionSettings.Add(si);
                            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 4:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
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
                                si.secondary = f;
                            }
                            else
                            {
                                si.secondary = null;
                            }
                            ServicesSettings.Add(si);
                            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    case 5:
                        Typenames[Jtype - 1] = ReadFields[1].Trim().ToLower();
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
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
                                    MainForm.currentState = 7;
                                    Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                                }
                                save = Math.Min(255, save);
                                save = Math.Max(0, save);
                                scene.Channelvalues[i - y] = (byte)save;
                            }
                            DMXScenes.Add(scene);
                            read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        }
                        read_all = true;
                        break;
                    default:
                        read_all = getcsvFields(ref ReadFields, -1, false, read_all);
                        break;
                }
                x--;
            }
            if (!read_all)
            {
                return;
            }
            SessionSettings = SessionSettings.OrderBy(x => x.mins).ToList<SessionSetting>();
            int MaxChannels = Math.Max(Math.Max(Dimmerchannel[0], Dimmerchannel[1]), Math.Max(Dimmerchannel[1], ObjectLightchannel));
            for (int i = 0; i < colorwheelvalues.Length; i++)
            {
                for (int j = 0; j < colorwheelvalues[i].Length; j++)
                {
                    MaxChannels = Math.Max(MaxChannels, colorwheelvalues[i][j]);
                }
            }
            for (int i = 0; i < DMXScenes.Count; i++)
            {
                MaxChannels = Math.Max(MaxChannels, DMXScenes[i].Channelvalues.Length);
            }
            foreach (DMXScene scene in DMXScenes)
            {
                if (scene.Channelvalues.Length < MaxChannels)
                {
                    byte[] tmp = new byte[MaxChannels];
                    Buffer.BlockCopy(scene.Channelvalues, 0, tmp, 0, scene.Channelvalues.Length);
                    scene.Channelvalues = tmp;
                }
            }
            while (DMXScenes.Count < 4)
            {
                DMXScene scene = new DMXScene();
                scene.ShowText = $"Generated_{DMXScenes.Count}";
                scene.JsonText = $"Generated_{DMXScenes.Count}";
                scene.ContentPath = "";
                if (DMXScenes.Count > 0)
                {
                    scene.Channelvalues = new byte[DMXScenes[0].Channelvalues.Length - 4];
                }
                else
                {
                    scene.Channelvalues = new byte[MaxChannels];
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
            currentvalues = new byte[MaxChannels];
            allread = true;
        }

        public static Constants.ServicesSetting setupsecondaryfunctionsforServiceButtons(Constants.ServicesSettingfunction sss)
        {

            if (sss.secondary == null || ((Constants.rawfunctiontext)(sss.secondary)).functionText == null || ((Constants.rawfunctiontext)(sss.secondary)).functionText.Length <= 0 || !((Constants.rawfunctiontext)(sss.secondary)).functionText.Contains(',') || ((Constants.rawfunctiontext)(sss.secondary)).functionText.Split(',').Length < 2)
            {
                return (ServicesSetting)sss;
            }
            bool typefound = false;
            String f = ((Constants.rawfunctiontext)(sss.secondary)).functionText.Split(',')[0].Trim().ToLower();
            for (int i = 0; i < Typenames.Length; i++)
            {
                String s = Typenames[i];
                if (s == null || s.Length <= 0)
                {
                    continue;
                }
                if (s.Equals(f))
                {
                    getServiceFunctionFromString(i, sss);
                    typefound = true;
                    break;
                }
            }
            if (!typefound)
            {
                int z = -1;
                try
                {
                    z = Int32.Parse(((Constants.rawfunctiontext)(sss.secondary)).functionText.Split(',')[0].Trim());
                }
                catch (FormatException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                    return null;
                }
                if (z >= 0 && z < Typenames.Length)
                {
                    getServiceFunctionFromString(z, sss);
                }
            }
            if (sss == null)
            {
                return sss;
            }
            sss.hassecondary = true;
            Logger.Print($"Added secondary: {sss.ToString()} to Service Button: {sss.ShowText}", MessageType.Konfig, MessageSubType.Information);
            return sss;
        }

        private static ServicesSettingfunction getServiceFunctionFromString(int type, Constants.ServicesSettingfunction s)
        {
            Debug.Print(((Constants.rawfunctiontext)(s.secondary)).functionText);
            String[] functionspecs = ((Constants.rawfunctiontext)(s.secondary)).functionText.Split(',');
            Configclasses functionclass = null;
            if (functionspecs.Length < 2)
            {
                Logger.Print("Not enough Secondary Function arguments", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                return null;
            }
            s.secondary = null;
            s.functionclass = Getconfigclasstype(type);
            if (s.functionclass == null)
            {
                return null;
            }
            List<Configclasses> typelist = getListfromint(type);
            foreach (Configclasses sys in typelist)
            {
                if (sys.JsonText.Equals(functionspecs[1]))
                {
                    s.secondary = sys;
                    break;
                }
            }
            if (s.secondary == null)
            {
                int rese = -1;
                if (functionspecs.Length > 0 && int.TryParse(functionspecs[1], out rese))
                {
                    if (rese != 0 && rese >= 0 && rese < getListfromint(type).Count)
                    {
                        s.secondary = getListfromint(type)[rese];
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
            int index = 2;
            try
            {
                if (functionspecs.Length > index)
                {
                    s.enable = Boolean.Parse(functionspecs[index++]);
                }
                else
                {
                    s.enable = false;
                }
                if (functionspecs.Length > index)
                {
                    s.block = Boolean.Parse(functionspecs[index++]);
                }
                else
                {
                    s.block = false;
                }
                if (functionspecs.Length > index)
                {
                    s.canceling = Boolean.Parse(functionspecs[index++]);
                }
                else
                {
                    s.canceling = false;
                }
                if (functionspecs.Length > index)
                {
                    s.toggle = Boolean.Parse(functionspecs[index++]);
                }
                else
                {
                    s.toggle = false;
                }
            }
            catch (FormatException ex)
            {
                MainForm.currentState = 7;
                Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
            }
            if (functionspecs.Length > index)
            {
                s.value = functionspecs[index++];
            }
            else
            {
                s.value = null;
            }
            try
            {
                if (functionspecs.Length > index && functionspecs[index].Length > 0)
                {
                    s.delay = Int32.Parse(functionspecs[index++]);
                }
                else
                {
                    s.delay = 0;
                }
            }catch (FormatException ex)
            {
                Logger.Print(ex.Message, Logger.MessageType.Konfig, MessageSubType.Error);
            }
            return s;
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

        private static List<Configclasses> getListfromint(int x)
        {
            switch (x)
            {
                case 0:
                    return SystemSettings.Cast<Configclasses>().ToList();
                case 1:
                    return TCPSettings.Cast<Configclasses>().ToList();
                case 2:
                    return SessionSettings.Cast<Configclasses>().ToList();
                case 3:
                    return ServicesSettings.Cast<Configclasses>().ToList();
                case 4:
                    return DMXScenes.Cast<Configclasses>().ToList();
                default:
                    return new List<Configclasses>();
            }
        }

        public static void finalizePaths(out String? sp, String s)
        {
            if (s != null)
            {
                if (s.Length >= 0)
                {
                    s = Path.Combine(Constants.ContentPath, s.ToLower());
                }
                else
                {
                    s = null;
                }
            }
            sp = s;
        }

        public static bool getcsvFields<T>(ref T variable, int index, bool canbeempty, bool lasttry)
        {
            variable = default(T); 
            String line = __configlines[currentconfigindex++];
            if (!lasttry)
            {
               Logger.Print("Something went wrong in reading the single Variables (x05)", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
               return false;
            }
            String[] s = stripComments(line.Split(Delimiter[0]), Delimiter[1], Delimiter[2]);
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
                catch (FormatException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                    Logger.Print("Beim einlesen der Konfig konnte eine Einstellung nicht in den benötigten Typen umgewandelt werden.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
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
                    catch (FormatException ex)
                    {
                        MainForm.currentState = 7;
                        Logger.Print(ex.Message, Logger.MessageType.Konfig, Logger.MessageSubType.Error);
                        Logger.Print("Beim einlesen der Konfig konnte eine Einstellungsmenge nicht in den benötigten Typen umgewandelt werden.", Logger.MessageType.Konfig, Logger.MessageSubType.Notice);
                    }
                }
                else
                {
                    return false;
                }
            }
            if (EqualityComparer<T>.Default.Equals(variable, default(T)) && !default(T).GetType().Equals(typeof(Boolean)))
            {
                return false;
            }
            return true;
        }

        public static String[] stripComments(String[] field, Char CommandDelimiters, Char EmptyDelimiter)
        {
            String[] res = new String[field.Length];
            int x = 0;
            foreach (String s in field)
            {
                if (s != null && !s.StartsWith(CommandDelimiters) && !s.Equals("") && s.Length > 0)
                {
                    if(!s.Equals(EmptyDelimiter.ToString()))
                    {
                        res[x++] = s;
                    }
                    else
                    {
                        res[x++] = null;
                    }
                }
            }
            Array.Resize(ref res, x);
            return res;
        }

        public static bool UseoldWiFicreds()
        {
            FileStream tmp = null;
            if (!File.Exists(Constants.WifiCreds))
            {
                Logger.Print("Old WiFi Credentials are non existent", MessageType.Konfig, MessageSubType.Error);
                return false;
            }
            StreamReader stream = null;
            File.SetAttributes(Constants.WifiCreds, FileAttributes.Normal);
            FileStream fstream = Logger.CreateStream(Constants.WifiCreds, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fstream != null && fstream.CanRead)
            {
                stream = new StreamReader(fstream);
            }
            else
            {
                Logger.Print("Opened WiFi Credentials, but cannot Read it.", MessageType.Konfig, MessageSubType.Error);
            }
            String line;
            bool foundssid = false;
            bool foundpassword = false;
            while (stream != null &&!stream.EndOfStream)
            {
                line = stream.ReadLine();
                if(line != null && line.Length > 0 && line.Contains(Constants.PreConfigDelimiter))
                {
                    if (line.Contains("SSID"))
                    {
                        WiFiSSID = line.Split(Constants.PreConfigDelimiter)[1].Trim();
                        foundssid = true;
                    }
                    else if (line.Contains("Password")){
                        Wifipassword = line.Split(Constants.PreConfigDelimiter)[1].Trim();
                        foundpassword = true;
                    }
                    else
                    {
                        Logger.Print("Found unknown WiFi Credentials Keys in WiFi Credentials File", MessageType.Konfig, MessageSubType.Notice);
                    }
                }
                else
                {
                    Logger.Print("Could not read old WiFi Credentials", MessageType.Konfig, MessageSubType.Error);
                    stream.Close();
                    return false;
                }
            }
            if(!foundssid || !foundpassword)
            {
                Logger.Print("The showen WiFi Credentials could be wrong, since not every needed Information could be found in the File", MessageType.Konfig, MessageSubType.Error);
            }
            stream.Close();
            return true;
        }

        public static void updateWificreds()
        {
            if (WiFiSSID == null || WiFiSSID.Length <= 0 || Wifipassword == null || Wifipassword.Length <= 0)
            {
                return;
            }
            try
            {
                File.Create(Constants.WifiCreds).Close();
            }catch(IOException ex)
            {
                Logger.Print(ex.Message, MessageType.Konfig, MessageSubType.Error);
            }
            if (!File.Exists(Constants.WifiCreds))
            {
                Logger.Print($"Filesystem Error cannot find file: {Constants.WifiCreds}", MessageType.Konfig, MessageSubType.Error);
                return;
            }
            File.SetAttributes(Constants.WifiCreds, FileAttributes.Normal);
            StreamWriter stream = null;
            FileStream fstream = Logger.CreateStream(Constants.WifiCreds, FileMode.Append, FileAccess.Write, FileShare.Read);
            if (fstream != null && fstream.CanWrite)
            {
                stream = new StreamWriter(fstream);
            }
            else
            {
                Logger.Print("Opened WiFi Credentials, but cannot Read it.", MessageType.Konfig, MessageSubType.Error);
            }
            if (stream != null)
            {
                stream.WriteLine($"SSID{Constants.PreConfigDelimiter}{WiFiSSID}");
                stream.WriteLine($"Password{Constants.PreConfigDelimiter}{Wifipassword}");
                stream.Flush();
            }
            if(stream != null)
            {
                stream.Close();
            }
        }
    }
}
