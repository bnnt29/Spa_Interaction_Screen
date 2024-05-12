using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Reflection.Metadata.Ecma335;

namespace Spa_Interaction_Screen
{
    public partial class EmbedVLC : CForm
    {
        private LibVLC libvlc;
        private MainForm main = null;
        private LibVLCSharp.WinForms.VideoView TVvideoView;
        private PictureBox welcomeqr;
        private String currentlyshowing;
        private Screen scrn;
        private bool SessionEnd = false;
        private bool showingconsole = false;

        private delegate object MyNoArgument();
        private delegate object MyquitMedia(bool user);
        private delegate object MychangeMedia(String link, bool user);

        public EmbedVLC(MainForm f, Screen screen, bool Sessionend) : base()
        {
            this.FormClosed += OnFormClosed;
            SessionEnd = Sessionend;
            InitializeComponent();

            this.main = f;
            scrn = screen;

            Core.Initialize();
            libvlc = new LibVLC();

            main.EnterFullscreen(this, scrn);
            createElements();

            TVvideoView.MediaPlayer = new MediaPlayer(libvlc);
            TVvideoView.Dock = DockStyle.Fill;
            this.Show();
            this.BringToFront();
            if (SessionEnd)
            {
                f.SessionEnded(this, false);
            }
            if (Constants.Unternehmensname != null && Constants.Unternehmensname.Length > 0)
            {
                this.Text = Constants.Unternehmensname;
            }
            SetIcon();
        }

        public override void OnFormClosed(object sender, EventArgs e)
        {
            if(main != null) 
            {
                if (main.vlc != null && main.vlc.Equals(this))
                {
#if DEBUG
                    main.vlcclosed = true;
#endif
                    main.vlc.Dispose();
                    main.vlc = null;
                }
                else if(main.vlc != null && main.sessionEndVLC != null && main.sessionEndVLC.Equals(this))
                {
                    main.sessionEndVLC.Dispose();
                    main.sessionEndVLC = null;
                }
            }
            this.hidethis();
            Logger.Print("Shutdown EmbedVLC", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
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

                Logger.ConsoleBox = new RichTextBox();
                Controls.Add(Logger.ConsoleBox);
                Logger.ConsoleBox.Size = new Size(this.Width, this.Height);
                Logger.ConsoleBox.Location = new Point(0, 0);
                Logger.ConsoleBox.ReadOnly = true;
                Logger.ConsoleBox.Hide();
            }

        }

        public void exittorestricted(object sender, EventArgs e)
        {
            //TODO
            if (main!=null)
            {
                if (main.UIControl != null)
                {
                    main.UIControl.SelectTab(main.UIControl.TabPages.IndexOf(main.WartungPage));
                }
                main.showlogin();
            }
            this.SendToBack();
            this.Hide();
        }

        public void quitMedia(bool user)
        {
            Constants.InvokeDelegate<object>([user], new MyquitMedia(delegatequitMedia), this, Logger.MessageType.VideoProjection);
        }

        private object delegatequitMedia(bool user)
        {
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
            return null;
        }

        public void showthis()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegateshowthis), this, Logger.MessageType.VideoProjection);
        }

        public object delegateshowthis()
        {
            if (this != null && !this.IsDisposed)
            {
                this.Show();
            }
            return null;
        }

        public void hidethis()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegatehidethis), this, Logger.MessageType.VideoProjection);
        }

        private object delegatehidethis()
        {
            this.Hide();
            return null;
        }

        public void newsession()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegatenewsession), this, Logger.MessageType.VideoProjection);
        }

        private object delegatenewsession()
        {
            if (main.generateQRCode(welcomeqr, 29, true, (int)(scrn.Bounds.Width * 0.2), true))
            {
                welcomeqr.Location = new Point((this.Width / 2) - welcomeqr.Size.Width / 2, this.Height / 2 - welcomeqr.Size.Height / 2);
                welcomeqr.BringToFront();
                welcomeqr.Show();
            }
            else
            {

                welcomeqr.Hide();
            }
            return null;
        }

        public void changeMedia(String link, bool user)
        {
            if (currentlyshowing != null && currentlyshowing.Length >= 0 && currentlyshowing.Equals(link))
            {
                if (!user || welcomeqr == null)
                {
                    return;
                }
            }
            Constants.InvokeDelegate<object>([link, user], new MychangeMedia(delegatechangeMedia), this, Logger.MessageType.VideoProjection);
        }

        private object delegatechangeMedia(String link, bool user)
        {
            if (currentlyshowing != null && currentlyshowing.Length >= 0 && currentlyshowing.Equals(link))
            {
                if (user && welcomeqr != null)
                {
                    welcomeqr.Hide();
                }
                return null;
            }
            quitMedia(user);
            if (link == null || link.Length <= 0)
            {
                return null;
            }
            Logger.Print($"Showing {link}", Logger.MessageType.VideoProjection, Logger.MessageSubType.Information);
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
                    Logger.Print($"Couldn't use file format of file: {link}", Logger.MessageType.VideoProjection, Logger.MessageSubType.Notice);
                    break;
            }
            if (user && welcomeqr != null)
            {
                welcomeqr.Hide();
            }
            toggleConsoleBox(showingconsole);
            return null;
        }

        private void video(String link, bool user)
        {
            if (!File.Exists(link))
            {
                Logger.Print("Video Media not found", Logger.MessageType.VideoProjection, Logger.MessageSubType.Notice);
            }
            var uri = new Uri(link);
            // Use command line options as Options for media playback (https://wiki.videolan.org/VLC_command-line_help/)
            Media media = new Media(libvlc, uri, ":input-repeat=65535");
            TVvideoView.MediaPlayer.Play(media);
            currentlyshowing = link;

            TVvideoView.Show();
            if (user && welcomeqr != null)
            {
                welcomeqr.BringToFront();
            }

        }

        private void picture(String link, bool user)
        {
            if (!File.Exists(link))
            {
                Logger.Print("Video Media not found", Logger.MessageType.VideoProjection, Logger.MessageSubType.Notice);
            }
            Uri uri = new Uri(link);

            Image image = Image.FromFile(link);

            this.BackgroundImage = image;
            currentlyshowing = link;
        }

        public void toggleConsoleBox(bool show)
        {
            showingconsole = show;
            if(Logger.ConsoleBox != null)
            {
                if (show)
                {
                    Logger.ConsoleBox.Show();
                    Logger.ConsoleBox.BringToFront();
                }
                else
                {
                    Logger.ConsoleBox.Hide();
                }
            }
        }

        public void SetConsoleText(String text)
        {
            Logger.ConsoleBox.Text = text;
        }

        public String GetConsoleText()
        {
            return Logger.ConsoleBox.Text;
        }
    }
}