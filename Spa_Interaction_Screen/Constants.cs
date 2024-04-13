namespace Spa_Interaction_Screen
{
    public static class Constants
    {
        public const int PasswordLength = 12;
        public const String Configpath = @"C:\Users\Berni\Documents\GitHub\Spa_Interaction_Screen\ConfigFile.csv";
        public const String CurrentVersion = "1.0";
        public const int UDPReceivePort = 50100;

        public const String EnterFullscreenText = "Programm in Fullscreen setzen";
        public const String ExitFullscreenText = "Verlasse Fullscreen";

        private static Char[] delimiter = new Char[] { ';', '#' };
        private static Char[] sonderzeichen = new Char[] { '!', '?', '_' };

        public static Char[] Delimiter
        {
            get { return delimiter; }
        }

        public static Char[] Sonderzeichen
        {
            get { return sonderzeichen; }
        }

        //UI
        public const int windowwidth = 1420;
        public const int windowheight = 800;
        public const int XButtonCount = 5;
        public const int YButtonCount = 5;
        public const int tabheight = (int)((double)windowheight * 0.87);
        public const int Element_width = (int)(((double)windowwidth / (XButtonCount + 1)) * 0.87);
        public const int Element_x_padding = (int)((double)windowwidth / (XButtonCount + 1)) - Element_width;
        public const int Element_height = (int)(((double)tabheight / (YButtonCount + 1)) * 0.87);
        public const int Element_y_padding = (int)((double)tabheight / (YButtonCount + 1)) - Element_height;
        public const int InlineUntilXButtons = 5;
        public const int maxscenes = 12;
        public const int maxhelps = 12;
        public const int maxSaunaSettings = 6;
        public static Color selected_color = Color.Green;

        public class SystemSetting
        {
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            public String JsonText = null;
        }
        public class SessionSetting
        {
            public int id = -1;
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
            public int JsonValue = -1;
            public String? ShowText = null;
        }

        public class ServicesSetting
        {
            public int id = -1;
            public int startvalue = -1;//inclusive
            public int endvalue = -1;//inclusive
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


        public static Button createButton(int width, int height, int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, TabPage tp, MainForm form, EventHandler? ev)
        {
            Button b = null;
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
            b.Size = new Size(width, height);
            return b;
        }

        public static void createButton(int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, TabPage tp, MainForm form, EventHandler? ev)
        {
            Button b = new Button();
            createButton(Pos_x, Pos_y, bl, Text, Tag, tp, form, ev, out b);
        }

        public static void createButton(int Pos_x, int Pos_y, List<Button>? bl, String Text, Object? Tag, TabPage tp, MainForm form, EventHandler? ev, out Button? b)
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
            Button b = new Button();
            b.Size = new Size(Constants.Element_width, Constants.Element_height);
            if (TabIndex > 0)
            {
                b.TabIndex = TabIndex;
            }
            b.Tag = Tag;
            b.Text = Text;
            b.UseVisualStyleBackColor = true;
            b.Location = p;
            return b;
        }
    }
}
