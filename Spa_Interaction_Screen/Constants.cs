using BrbVideoManager.Controls;
using System.Runtime.CompilerServices;

namespace Spa_Interaction_Screen
{
    public static class Constants
    {
        //Constants
        public const String CurrentVersion = "1.2";
        public static String PreConfigPath = @$"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}\AdminConfig";
        public const String PreConfigGastroURL = @"https://www.lieferando.de/";
        public const Char PreConfigDelimiter = '%';
        public const String EnterFullscreenText = "Programm in Fullscreen setzen";
        public const String ExitFullscreenText = "Verlasse Fullscreen";
        public const int SessionOvertimeBuffer = 0;

        //PreConfig
        public static String ContentPath = null;
        public static int UDPReceivePort = 50100;
        private static Char[] delimiter = new Char[] { ';', '#' };
        public static int maxscenes = 12;
        public static int maxhelps = 12;
        public static int maxtcpws = 5;
        public static int XButtonCount = 5;
        public static int YButtonCount = 5;
        public static int InlineUntilXButtons = 5;
        public static bool noCOM = false;
        public static bool noNet = false;
        public static int EdgeItemposxdist = 50;
        public static int EdgeItemposydist = 50;
        public static int Logoxsize = 50;
        public static int Logoysize = 50;
        public static int buttonupdatemillis = 35;


        //UI
        public static int windowwidth = 1420;
        public static int windowheight = 800;
        public static int controlheight = windowheight - 50;
        public static int tabheight = (int)((double)controlheight * 0.94);
        public static int Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.85);
        public static int Element_x_padding = (int)((double)windowwidth / (XButtonCount + 1)) - Element_width;
        public static int Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.85);
        public static int Element_y_padding = (int)((double)tabheight / (YButtonCount + 1)) - Element_height;
        public static Color Background_color = ColorTranslator.FromHtml("#1C3F5E");
        public static Color alternative_color = ColorTranslator.FromHtml("#617C94"); 
        public static Color selected_color = Color.Turquoise;
        public static Color Text_color = Color.White;
        public static Color Button_color = ColorTranslator.FromHtml("#CDAA39");
        public static Color ButtonText_color = Text_color;
        public static Color NumfieldErrorButton_color = Color.Red;
        public static Font Standart_font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);


        //USB
        public const int waitfordmxanswer = 10;
        public const int waittonextcheck = 100;
        public const int sendtimeout = 32;

        public class SystemSetting : configclasses
        {
            public String? value = null;
        }

        public class TCPSetting : SystemSetting
        {
        }

        public class SessionSetting : configclasses
        {
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public int mins = -1;
            public bool should_reset = false;
        }

        public class ServicesSetting : configclasses
        {
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public bool hassecondary = false;
        }

        public class rawfunctiontext : configclasses
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
            public bool enable;
            public bool block;
            public configclasses? function;
        }

        public class DMXScene : configclasses
        {
            public byte[] Channelvalues = null;
            public String? ContentPath = null;
        }

        public abstract class configclasses
        {
            public int id=-1;
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

        public static Button createButton(int width, int height, int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, Control tp, MainForm? form, EventHandler? ev)
        {
            Button b = null;
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
            b.Size = new Size(width, height);
            return b;
        }

        public static void createButton(int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, Control tp, MainForm? form, EventHandler? ev)
        {
            Button b = new Button();
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
        }

        public static void createButton(int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, Control tp, MainForm? form, EventHandler? ev, out Button? b)
        {
            Point point = new Point(Pos_x, Pos_y);
            b = Constants.createButton((bl != null) ? bl.Count : -1, Text, Tag, point);
            if (bl != null)
            {
                bl.Add(b);
            }
            tp.Controls.Add(b);
            b.Click += ev;
            b.BringToFront();
        }

        public static Button createButton(int TabIndex, String Text, Object? Tag, Point p)
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
            b.Location = p;
            return (Button)b;
        }
    }
}
