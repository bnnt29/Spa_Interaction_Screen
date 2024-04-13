using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMiracle.LibTiff.Classic;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using static System.Windows.Forms.DataFormats;

namespace Spa_Interaction_Screen
{
    public partial class EmbedVLC : Form
    {

        private LibVLC libvlc;
        private Task? keepVideoalive = null;
        private bool runvideo = false;
        private LibVLCSharp.WinForms.VideoView TVvideoView;
        private PictureBox TVPictureView;
        private String currentlyshowing;

        public EmbedVLC(MainForm f, Screen screen)
        {
            InitializeComponent();

            f.EnterFullscreen(this, screen);
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
            // pictureBox1
            // 
            TVPictureView = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)TVPictureView).BeginInit();

            TVPictureView.BackColor = Color.Black;
            TVPictureView.Dock = DockStyle.Fill;
            TVPictureView.Location = new Point(0, 0);
            TVPictureView.SizeMode = PictureBoxSizeMode.Zoom;
            TVPictureView.Name = "pictureBox1";
            TVPictureView.Size = new Size(this.Width, this.Height);
            TVPictureView.TabIndex = 1;
            TVPictureView.TabStop = false;
            TVPictureView.Hide();
            Controls.Add(TVPictureView);
        }

        public void quitMedia()
        {
            runvideo = false;
            TVPictureView.Hide();
            TVvideoView.Hide();
            TVPictureView.Image = null;
            TVvideoView.MediaPlayer.Media = null;
            currentlyshowing = null;
        }

        public void changeMedia(String link)
        {
            if (currentlyshowing != null && currentlyshowing.Length>=0 && currentlyshowing.Equals(link))
            {
                Debug.Print($"Already showing: {link}");
                return;
            }
            Debug.Print($"Showing {link}");
            quitMedia();
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
            TVvideoView.BringToFront();

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

        }

        private void picture(String link) 
        {
            if (!File.Exists(link))
            {
                Debug.Print("Video Media not found");
            }
            Uri uri = new Uri(link);

            Image image = Image.FromFile(link);

            TVPictureView.Image = image;
            currentlyshowing = link;

            TVPictureView.Show();
            TVPictureView.BringToFront();
        }
    }
}
