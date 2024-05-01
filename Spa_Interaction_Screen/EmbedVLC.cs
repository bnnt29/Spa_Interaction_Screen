using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Microsoft.VisualBasic.ApplicationServices;
using PlanwerkLichtSpa.Properties;
using System.Diagnostics;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Spa_Interaction_Screen.MainForm;
using static System.Windows.Forms.LinkLabel;

namespace Spa_Interaction_Screen
{
    public partial class EmbedVLC : Form
    {
        private Logger Log;
        private LibVLC libvlc;
        private MainForm main = null;
        private Task? keepVideoalive = null;
        private bool runvideo = false;
        private LibVLCSharp.WinForms.VideoView TVvideoView;
        private PictureBox welcomeqr;
        private String currentlyshowing;
        private Screen scrn;
        private bool HandleCreate = false;
        private bool SessionEnd = false;
        private bool showingconsole = false;

        public RichTextBox ConsoleBox;

        public EmbedVLC(MainForm f, Screen screen, bool Sessionend)
        {
            Log = f.Log;
            this.FormClosed += OnFormClosed;
            SessionEnd = Sessionend;
            InitializeComponent();
            this.HandleCreated += new EventHandler((sender, args) =>
            {
                HandleCreate = true;
            });

            this.main = f;
            scrn = screen;

            Core.Initialize();
            libvlc = new LibVLC();

            main.EnterFullscreen(this, scrn, HandleCreate);
            createElements();

            TVvideoView.MediaPlayer = new MediaPlayer(libvlc);
            TVvideoView.Dock = DockStyle.Fill;
            this.Show();
            this.BringToFront();
            if (SessionEnd)
            {
                f.SessionEnded(this, false);
            }
        }

        public void OnFormClosed(object sender, EventArgs e)
        {
#if DEBUG
            main.vlcclosed = true;
#endif
            main.vlc = null;
        }

        private void createElements()
        {
            // 
            // TVvideoView
            // 
            TVvideoView = new VideoView();
            ((System.ComponentModel.ISupportInitialize)TVvideoView).BeginInit();

            TVvideoView.BackColor = Color.Black;
            TVvideoView.Dock = DockStyle.Fill;
            TVvideoView.Location = new Point(0, 0);
            TVvideoView.MediaPlayer = null;
            TVvideoView.Name = "TVvideoView";
            TVvideoView.Size = new Size(this.Width, this.Height);
            TVvideoView.TabIndex = 1;
            TVvideoView.TabStop = false;
            TVvideoView.Hide();
            Controls.Add(TVvideoView);
            // Make VideoView control
            // 
            // welcomeqr
            // 
            if (SessionEnd)
            {
                Constants.createButton((Constants.windowwidth / 2) - (Constants.Element_width / 2), (Constants.tabheight - Constants.Element_height) - Constants.Element_y_padding, (List<Button>)null, "Mitarbeiter Einstellungen", null, this, null, this.exittorestricted);
            }
            else
            {
                welcomeqr = new PictureBox();
                ((System.ComponentModel.ISupportInitialize)welcomeqr).BeginInit();
                welcomeqr.BackColor = Color.Black;
                welcomeqr.Name = "qrbox";
                welcomeqr.TabIndex = 1;
                welcomeqr.TabStop = false;
                welcomeqr.SizeMode = PictureBoxSizeMode.Zoom;
                Controls.Add(welcomeqr);
                newsession();
            }

            ConsoleBox = new RichTextBox();
            Controls.Add(ConsoleBox);
            ConsoleBox.Size = new Size(this.Width, this.Height);
            ConsoleBox.Location = new Point(0, 0);
            ConsoleBox.ReadOnly = true;
            ConsoleBox.Hide();

        }

        public void exittorestricted(object sender, EventArgs e)
        {
            main.UIControl.SelectTab(main.UIControl.TabCount - 1);
            this.SendToBack();
            this.Hide();
            main.showlogin();
        }

        public delegate void MyNoArgument();
        public delegate void MyquitMedia(bool user);
        public delegate void MychangeMedia(String link, bool user);
        public void quitMedia(bool user)
        {
            object[] delegateArray = new object[1];
            delegateArray[0] = user;
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MyquitMedia(delegatequitMedia), delegateArray);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    delegatequitMedia(user);
                }
            }
            else
            {
                try
                {
                    delegatequitMedia(user);
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("QuitMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    this.Invoke(new MyquitMedia(delegatequitMedia), delegateArray);
                }
            }
        }

        private void delegatequitMedia(bool user)
        {
            runvideo = false;
            this.BackgroundImage = null;
            if (TVvideoView != null)
            {
                TVvideoView.Hide();
                TVvideoView.MediaPlayer.Media = null;
            }
            currentlyshowing = null;
            if (user && welcomeqr != null)
            {
                welcomeqr.Hide();
            }
        }

        public void showthis()
        {
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MyNoArgument(delegateshowthis));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("showthis", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    delegateshowthis();
                }
            }
            else
            {
                try
                {
                    delegateshowthis();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("showthis", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    this.Invoke(new MyNoArgument(delegateshowthis));
                }
            }
        }

        private void delegateshowthis()
        {
            if (this != null && !this.IsDisposed)
            {
                this.Show();
            }
        }

        public void hidethis()
        {
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MyNoArgument(delegatehidethis));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("hidethis", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    delegatehidethis();
                }
            }
            else
            {
                try
                {
                    delegatehidethis();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("hidethis", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    this.Invoke(new MyNoArgument(delegatehidethis));
                }
            }
        }

        private void delegatehidethis()
        {
            this.Hide();
        }

        public void newsession()
        {
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MyNoArgument(delegatenewsession));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("newsession", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    delegatenewsession();
                }
            }
            else
            {
                try
                {
                    delegatenewsession();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("newsession", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    this.Invoke(new MyNoArgument(delegatenewsession));
                }
            }
        }

        private void delegatenewsession()
        {
            if (main.generateQRCode(welcomeqr, 29, true, (int)(scrn.Bounds.Width * 0.2), true))
            {
                welcomeqr.Location = new Point(this.Width / 2 - welcomeqr.Size.Width / 2, this.Height / 2 - welcomeqr.Size.Height / 2);
                welcomeqr.BringToFront();
                welcomeqr.Show();
            }
            else
            {
                welcomeqr.Hide();
            }
        }

        public void changeMedia(String link, bool user)
        {
            object[] delegateArray = new object[2];
            delegateArray[0] = link;
            delegateArray[1] = user;
            if (HandleCreate)
            {
                try
                {
                    this.BeginInvoke(new MychangeMedia(delegatechangeMedia), delegateArray);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("changeMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    delegatechangeMedia(link, user);
                }
            }
            else
            {
                try
                {
                    delegatechangeMedia(link, user);
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                    Log.Print("changeMedia", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    this.BeginInvoke(new MychangeMedia(delegatechangeMedia), delegateArray);
                }
            }
        }

        private void delegatechangeMedia(String link, bool user)
        {
            if (currentlyshowing != null && currentlyshowing.Length >= 0 && currentlyshowing.Equals(link))
            {
                if (user && welcomeqr != null)
                {
                    welcomeqr.Hide();
                }
                return;
            }
            quitMedia(user);
            if (link == null || link.Length <= 0)
            {
                return;
            }
            Log.Print($"Showing {link}", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Information);
            int index = link.LastIndexOf('.');
            switch (link.Substring(index))
            {
                case ".mp4":
                case ".3GP":
                case ".ASF":
                case ".AVI":
                case ".FLV":
                case ".MKV":
                case ".MP4":
                case ".Ogg":
                case ".OGM":
                case ".WAV":
                case ".AIFF":
                case ".MXF":
                case ".VOB":
                case ".RM":
                case ".VCD":
                case ".SVCD":
                case ".DVB":
                case ".HEIF":
                case ".AVIF":
                    video(link, user);
                    break;
                case ".bitmap":
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".metafile":
                case ".icon":
                case ".png":
                case ".svg":
                case ".bmp":
                case ".exif":
                case ".tiff":
                    picture(link, user);
                    break;
                default:
                    Log.Print($"Couldn't use file format of file: {link}", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
                    break;
            }
            if (showingconsole)
            {
                ConsoleBox.Show();
                ConsoleBox.BringToFront();
            }
            else
            {
                ConsoleBox.Hide();
                ConsoleBox.SendToBack();
            }
        }

        private void video(String link, bool user)
        {
            if (!File.Exists(link))
            {
                Log.Print("Video Media not found", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
            }
            var uri = new Uri(link);
            // Use command line options as Options for media playback (https://wiki.videolan.org/VLC_command-line_help/)
            Media media = new Media(libvlc, uri, ":input-repeat=65535");
            TVvideoView.MediaPlayer.Play(media);
            currentlyshowing = link;

            TVvideoView.Show();

            runvideo = true;
            /*
            keepVideoalive = new Task(async() =>
            {
                while (true)
                {
                    if (!TVvideoView.MediaPlayer.IsPlaying && runvideo)
                    {
                        TVvideoView.MediaPlayer.Play();
                    }
                }
            });
            */
            //keepVideoalive.Start();
            if (user && welcomeqr != null)
            {
                welcomeqr.BringToFront();
            }

        }

        private void picture(String link, bool user)
        {
            if (!File.Exists(link))
            {
                Log.Print("Video Media not found", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Notice);
            }
            Uri uri = new Uri(link);

            Image image = Image.FromFile(link);

            this.BackgroundImage = image;
            currentlyshowing = link;
            if (user && welcomeqr != null)
            {
                welcomeqr.BringToFront();
            }
        }

        public void toggleConsoleBox(bool show)
        {
            showingconsole = show;
            if (show)
            {
                ConsoleBox.Show();
                ConsoleBox.BringToFront();
            }
            else
            {
                ConsoleBox.Hide();
                ConsoleBox.SendToBack();
            }
        }

        public void SetConsoleText(String text)
        {
            ConsoleBox.Text = text;
        }

        public String GetConsoleText()
        {
            return ConsoleBox.Text;
        }
    }
}