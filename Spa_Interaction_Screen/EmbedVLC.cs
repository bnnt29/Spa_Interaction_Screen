using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using PlanwerkLichtSpa.Properties;
using System.Diagnostics;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Spa_Interaction_Screen
{
    public partial class EmbedVLC : Form
    {

        private LibVLC libvlc;
        private MainForm main = null;
        private Task? keepVideoalive = null;
        private bool runvideo = false;
        private LibVLCSharp.WinForms.VideoView TVvideoView;
        private PictureBox welcomeqr;
        private String currentlyshowing;

        public EmbedVLC(MainForm f, Screen screen)
        {
            InitializeComponent();
            main = f;
            main.EnterFullscreen(this, screen);
            createElements();

            Core.Initialize();
            libvlc = new LibVLC();
            TVvideoView.MediaPlayer = new MediaPlayer(libvlc);
            TVvideoView.Dock = DockStyle.Fill;

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
            welcomeqr = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)welcomeqr).BeginInit();
            welcomeqr.BackColor = Color.Black;
            welcomeqr.Name = "qrbox";
            welcomeqr.TabIndex = 1;
            welcomeqr.TabStop = false;
            welcomeqr.Size = new Size(424, 332);
            welcomeqr.SizeMode = PictureBoxSizeMode.Zoom;
            Controls.Add(welcomeqr);
            newsession();
        }

        public void quitMedia()
        {
            runvideo = false;
            this.BackgroundImage = null;
            TVvideoView.Hide();
            TVvideoView.MediaPlayer.Media = null;
            currentlyshowing = null;
        }

        public void newsession()
        {
            if (main.generateQRCode(welcomeqr, 29, true, (int)(Constants.Element_width * 1.5), true))
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
            if (currentlyshowing != null && currentlyshowing.Length >= 0 && currentlyshowing.Equals(link))
            {
#if DEBUG
                Debug.Print($"Already showing: {link}");
#endif
                return;
            }
            Debug.Print($"Showing {link}");
            quitMedia();
            if (user)
            {
                welcomeqr.Hide();
            }
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
                    video(link);
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
                    picture(link);
                    break;
                default:
                    Debug.Print($"Couldn't use file format of file: {link}");
                    break;
            }
        }

        private void video(String link)
        {
            if (!File.Exists(link))
            {
                Debug.Print("Video Media not found");
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
            welcomeqr.BringToFront();

        }

        private void picture(String link)
        {
            if (!File.Exists(link))
            {
                Debug.Print("Video Media not found");
            }
            Uri uri = new Uri(link);

            Image image = Image.FromFile(link);

            this.BackgroundImage = image;
            currentlyshowing = link;
            welcomeqr.BringToFront();
        }
    }
}
