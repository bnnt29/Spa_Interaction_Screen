using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spa_Interaction_Screen
{
    public static class ButtonFader
    {
        private static List<Buttonfader> timecoloredbuttons = new List<Buttonfader>();

        struct Buttonfader
        {
            public Buttonfader(Button b, DateTime until, Color to, EventHandler eh)
            {
                this.b = b;
                this.eh = eh;
                this.until = until;
                this.to = to;
            }
            public Button b;
            public EventHandler eh;
            public DateTime until;
            public Color to;
        }

        public static bool containsfadingbutton(Button b)
        {
            foreach (Buttonfader bf in timecoloredbuttons)
            {
                if (bf.b == b)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool removefadingbutton(Button b)
        {
            for (int i = 0; i < timecoloredbuttons.Count; i++)
            {
                if (timecoloredbuttons[i].b == b)
                {
                    timecoloredbuttons[i].b.Click += timecoloredbuttons[i].eh;
                    timecoloredbuttons.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static void addcolortimedButton(Button b, long millis, Color to, EventHandler eh)
        {
            DateTime until = DateTime.Now.AddMilliseconds(millis);
            /*
            Buttonfader? bf = null;
            foreach (Buttonfader tb in timecoloredbuttons)
            {
                if (tb.b == b)
                {
                    bf = tb;
                }
            }
            if (bf != null)
            {
                Buttonfader bu = (Buttonfader)bf;
                bu.to = to;
                bu.until = until;
                bu.eh = eh;
                return;
            }
            */
            removefadingbutton(b);
            Buttonfader nbu = new Buttonfader(b, until, to, eh);
            timecoloredbuttons.Add(nbu);
        }

        public static void removeall()
        {
            foreach(Buttonfader bf in timecoloredbuttons)
            {
                bf.b.BackColor = bf.to;
                bf.b.Click += bf.eh;
            }
            timecoloredbuttons = new List<Buttonfader>();
        }

        public static void UpdateButtoncolor(object sender, EventArgs e)
        {
            for (int i = 0; i < timecoloredbuttons.Count; i++)
            {
                DateTime T = DateTime.Now;
                Buttonfader bf = timecoloredbuttons[i];
                if (T >= bf.until)
                {
                    bf.b.BackColor = bf.to;
                    timecoloredbuttons.Remove(bf);
                    bf.b.Click += bf.eh;
                    i--;
                }
                else
                {
                    int r = bf.to.R - bf.b.BackColor.R;
                    int g = bf.to.G - bf.b.BackColor.G;
                    int b = bf.to.B - bf.b.BackColor.B;

                    double steps = bf.until.Millisecond - T.Millisecond;
                    steps /= Constants.buttonupdatemillis;

                    if (steps > 0)
                    {
                        r = (int)Math.Floor(r / steps) + bf.b.BackColor.R;
                        g = (int)Math.Floor(g / steps) + bf.b.BackColor.G;
                        b = (int)Math.Floor(b / steps) + bf.b.BackColor.B;
                    }
                    else
                    {
                        r = bf.b.BackColor.R;
                        g = bf.b.BackColor.G;
                        b = bf.b.BackColor.B;
                    }

                    r = (r >= 0) ? (r <= 255) ? r : 255 : 0;
                    g = (g >= 0) ? (g <= 255) ? g : 255 : 0;
                    b = (b >= 0) ? (b <= 255) ? b : 255 : 0;

                    Color fade = Color.FromArgb(r, g, b);
                    bf.b.BackColor = fade;
                }
            }
        }
    }
}
