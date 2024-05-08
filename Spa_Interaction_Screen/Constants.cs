using BrbVideoManager.Controls;

namespace Spa_Interaction_Screen
{
    public static class Constants
    {
        //Constants
        public const String CurrentVersion = "1.3";
        public static String PreConfigPath = @$"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}\AdminConfig";
        public static String BackupLOGPath = @$"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}\BLOG";
        public static String WifiCreds = @$"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}\WiFi.creds";
        public const String PreConfigGastroURL = @"https://www.lieferando.de/";
        public const Char PreConfigDelimiter = '%';
        public const String EnterFullscreenText = "Programm in Fullscreen setzen";
        public const String ExitFullscreenText = "Verlasse Fullscreen";
        public const int SessionOvertimeBuffer = 2;
        public const String scenelockedinfo = "Szenen Auswahl gesperrt. Bitte durch das Personal wieder freischalten lassen.";
        public const int TelnetComTimeout = 90;
        public const String ServiceNotReachableText = "Service Ruf nicht möglich";
        public const int Buttonshortfadetime = 150;
        public const int ButtonLongfadetime = 1000;
        public const bool NetRoomSpecMandatory = false;
        public const bool showButtonTester = true;

        //PreConfig
        public static String ContentPath = null;
        public static int UDPReceivePort = 50100;
        public static String Unternehmensname = null;
        private static Char[] delimiter = new Char[] { ';', '#', '!' };
        public static int maxscenes = 12;
        public static int maxhelps = 12;
        public static int maxtcpws = 5;
        public static int XButtonCount = 5;
        public static int YButtonCount = 5;
        public static int InlineUntilXButtons = 5;
        public static bool noCOM = false;
        public static bool noNet = false;
        public static bool showdirectlypotentiallyfalsewificreds;
        public static int EdgeItemposxdist = 50;
        public static int EdgeItemposydist = 50;
        public static int Logoxsize = 50;
        public static int Logoysize = 50;
        public static int buttonupdatemillis = 40;
        public static int DateTimeFormat = 0;


        //UI
        public static int windowwidth = 1420;
        public static int windowheight = 800;
        public static int controlheight = windowheight - 50;
        public static int tabheight = (int)((double)controlheight * 0.94);
        public static int Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.85);
        public static int Element_x_padding = (int)(((double)windowwidth / (XButtonCount + 1)) - Element_width)/2;
        public static int Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.85);
        public static int Element_y_padding = (int)(((double)tabheight / (YButtonCount + 1)) - Element_height)/2;
        public static Color Background_color = ColorTranslator.FromHtml("#1C3F5E");
        public static Color selected_color = ColorTranslator.FromHtml("#CDAA39");
        public static Color alternative_color = ColorTranslator.FromHtml("#617C94"); 
        public static Color Text_color = Color.White;
        public static Color Button_color = alternative_color;
        public static Color ButtonText_color = Text_color;
        public static Color Warning_color = Color.Red;
        public static Font Standart_font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static Font Time_font = new Font("Segoe UI", 28F, FontStyle.Regular, GraphicsUnit.Point, 0);

        //USB
        public const int waitfordmxanswer = 10;
        public const int waittonextcheck = 100;
        public const int sendtimeout = 30;

        public class SystemSetting : Configclasses
        {
            public String? value = null;
        }

        public class TCPSetting : SystemSetting
        {
        }

        public class SessionSetting : Configclasses
        {
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public int mins = -1;
            public bool should_reset = false;
        }

        public class ServicesSetting : Configclasses
        {
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public bool hassecondary = false;
        }

        public class rawfunctiontext : Configclasses
        {
            public String functionText;
        }

        public class ServicesSettingfunction : ServicesSetting
        {
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public Type functionclass;
            public String? value = null;
            public int delay = 0;
            public bool enable;
            public bool block;
            public bool canceling;
            public bool toggle;
            public Configclasses? secondary;
            public override string ToString()
            {
                String s = "[";
                if(hassecondary && secondary != null)
                {
                    if(secondary.Typename!= null && secondary.Typename.Length > 0)
                    {
                        s += secondary.Typename;
                        s += ",";
                    }
                    if (secondary.JsonText != null && secondary.JsonText.Length > 0)
                    {
                        s += secondary.JsonText;
                        s += ",";
                    }
                }
                s += enable.ToString();
                s += ",";
                s += block.ToString();
                s += ",";
                s += canceling.ToString();
                s += ",";
                s += toggle.ToString();
                if (value  != null)
                {
                    s += ",";
                    s += value.ToString();
                }
                if (delay <= 0)
                {
                    s += ",";
                    s += delay.ToString();
                }
                s += "]";
                return s;
            }
        }

        public class DMXScene : Configclasses
        {
            public byte[] Channelvalues = null;
            public String? ContentPath = null;
        }

        public abstract class Configclasses
        {
            public int id=-1;
            public string Typename = null;
            public String? ShowText = null;
            public String JsonText = null;
            public Button? ButtonElement = null;
        }

        public class RGBW
        {
            public RGBW(Color c, byte R, byte G, byte B, byte W) 
            { 
                this.color = c;
                this.R = R;
                this.G = G;
                this.B = B;
                this.W = W;
            }
            public Color color;
            public byte R;
            public byte G;
            public byte B;
            public byte W;
        }

        public class ComboItem
        {
            public int? ID { get; set; }
            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        public static Char[] Delimiter
        {
            get { return delimiter; }
            set { delimiter = value; }
        }

        public static void recalcsizes(int Hight, int Width)
        {
            windowwidth = Hight;
            windowheight = Width;

            controlheight = windowheight - 100;
            tabheight = (int)((double)controlheight * 0.98);
            Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.88);
            Element_x_padding = (int)((double)windowwidth / (XButtonCount + 1)) - Element_width;
            Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.88);
            Element_y_padding = (int)((double)tabheight / (YButtonCount + 1)) - Element_height;
        }

        public static Button createButton<T>(int width, int height, int? Pos_x, int? Pos_y, List<T>? bl, String Text, Object? Tag, Control? tp, MainForm? form, EventHandler? ev) where T : Button
        {
            Button b = null;
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
            b.Size = new Size(width, height);
            return b;
        }

        public static void createButton<T>(int? Pos_x, int? Pos_y, List<T>? bl, String Text, Object? Tag, Control? tp, MainForm? form, EventHandler? ev) where T : Button
        {
            Button b = new Button();
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
        }

        public static void createButton<T>(int? Pos_x, int? Pos_y, List<T>? bl, String Text, Object? Tag, Control? tp, MainForm? form, EventHandler? ev, out Button? b) where T : Button
        {
            Point? point = null;
            if(Pos_x == Pos_y && Pos_x == null)
            {
                point = null;
            }
            else
            {
                if (Pos_x == null)
                {
                    Pos_x = 0;
                }
                if (Pos_y == null)
                {
                    Pos_y = 0;
                }
                point = new Point((int)Pos_x, (int)Pos_y);
            }
            b = Constants.createButton((bl != null) ? bl.Count : -1, Text, Tag, point);
            if (bl != null)
            {
                bl.Add((T)b);
            }
            if(tp != null)
            {
                tp.Controls.Add(b);
            }
            b.Click += ev;
            b.BringToFront();
        }

        public static Button createButton(int TabIndex, String Text, Object? Tag, Point? p)
        {
            RoundedButton b = new BrbVideoManager.Controls.RoundedButton();
            b.Size = new Size(Constants.Element_width, Constants.Element_height);
            if (TabIndex > 0)
            {
                b.TabIndex = TabIndex;
            }
            b.Tag = Tag;
            b.Text = Text;
            b.BorderRadius = (Constants.Element_width>Constants.Element_height)? Constants.Element_width: Constants.Element_height;
            b.BorderWidth = 0;
            b.BackColor = Constants.Button_color;
            b.AutoEllipsis = true;
            b.UseVisualStyleBackColor = true;
            if(p!= null)
            {
                b.Location = (Point)p;
            }
            return (Button)b;
        }

        public static ReturnType InvokeDelegate<ReturnType>(object[]? args, Delegate Mydelegate, CForm form)
        {
            if (form == null)
            {
                return default(ReturnType);
            }
            if (form.HandleCreate)
            {
                try
                {
                    return (ReturnType)form.Invoke(Mydelegate, args);
                }
                catch (InvalidOperationException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Logger.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    try
                    {
                        return (ReturnType)Mydelegate.DynamicInvoke(args);
                    }
                    catch (InvalidOperationException ex2)
                    {
                        MainForm.currentState = 7;
                        Logger.Print(ex2.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                        Logger.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    }
                }
            }
            else
            {
                try
                {
                    return (ReturnType)Mydelegate.DynamicInvoke(args);
                }
                catch (InvalidOperationException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Logger.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    try
                    {
                        return (ReturnType)form.Invoke(Mydelegate, args);
                    }
                    catch (InvalidOperationException ex2)
                    {
                        MainForm.currentState = 7;
                        Logger.Print(ex2.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                        Logger.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    }
                }
            }
            return default(ReturnType);
        }
    }
}
