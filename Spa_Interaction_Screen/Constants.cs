﻿using BrbVideoManager.Controls;
using System.Runtime.CompilerServices;

namespace Spa_Interaction_Screen
{
    public static class Constants
    {
        //Constants
        public const String CurrentVersion = "1.1";
        public static String PreConfigPath = @$"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}\AdminConfig";
        public const String PreConfigGastroURL = @"https://www.lieferando.de/";
        public const Char PreConfigDelimiter = '%';
        public const String EnterFullscreenText = "Programm in Fullscreen setzen";
        public const String ExitFullscreenText = "Verlasse Fullscreen";

        //PreConfig
        public static String ContentPath = null;
        public static int PasswordLength = 12;
        public static int UDPReceivePort = 50100;
        private static Char[] delimiter = new Char[] { ';', '#' };
        private static Char[] sonderzeichen = new Char[] { '!', '?', '_' };
        public static int maxscenes = 12;
        public static int maxhelps = 12;
        public static int XButtonCount = 5;
        public static int YButtonCount = 5;
        public static int InlineUntilXButtons = 5;
        public static bool noCOM = false;
        public static bool noNet = false;
        public static int Logoposxdist = 50;
        public static int Logoposydist = 50;
        public static int Logoxsize = 50;
        public static int Logoysize = 50;


        //UI
        public static int windowwidth = 1420;
        public static int windowheight = 800;
        public static int tabheight = (int)((double)windowheight * 0.87);
        public static int Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.87);
        public static int Element_x_padding = (int)((double)windowwidth / (XButtonCount + 1)) - Element_width;
        public static int Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.87);
        public static int Element_y_padding = (int)((double)tabheight / (YButtonCount + 1)) - Element_height;
        public static Color selected_color = Color.Green;
        public static Color Background = Color.Blue;
        public static Color Text = Color.Black;
        public static Color Button = Color.Yellow;
        public static Color ButtonText = Color.Black;
        public static Color NumfieldErrorButtonColor = Color.Red;


        //USB
        public const int waitfordmxanswer = 10;
        public const int waittonextcheck = 100;
        public const int sendtimeout = 32;

        public class SystemSetting
        {
            public String JsonText = null;
        }

        public class SessionSetting
        {
            public int id = -1;
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public int JsonValue = -1;
            public String? ShowText = null;
        }

        public class ServicesSetting
        {
            public int id = -1;
            /*
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            */
            public String JsonText = null;
            public String ShowText = null;
        }
        public class DMXScene
        {
            public int id = -1;
            public byte[] Channelvalues = null;
            public String ShowText = null;
            public String JsonText = null;
            public String? ContentPath = null;
            public Button ButtonElement = null;
        }

        public static Char[] Delimiter
        {
            get { return delimiter; }
            set { delimiter = value; }
        }

        public static Char[] Sonderzeichen
        {
            get { return sonderzeichen; }
            set { sonderzeichen = value;}
        }

        public static void recalcsizes(int Hight, int Width)
        {
            windowwidth = Hight;
            windowheight = Width;
            tabheight = (int)((double)windowheight * 0.87);
            Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.87);
            Element_x_padding = (int)((double)windowwidth / (XButtonCount + 1)) - Element_width;
            Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.87);
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

        public static Button createButton(int TabIndex, String Text, object? Tag, Point p)
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
            b.BorderWidth = 5;
            b.BorderDownColor = Constants.Text;
            b.BackColor = Constants.Button;
            b.AutoEllipsis = true;
            b.UseVisualStyleBackColor = true;
            b.Location = p;
            return (Button)b;
        }
    }
}
