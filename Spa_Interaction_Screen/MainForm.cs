using AudioSwitcher.AudioApi.CoreAudio;
using Cyotek.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;


namespace Spa_Interaction_Screen
{
    public partial class MainForm : Form
    {
        private EmbedVLC vlc;
        private Loading loadscreen;

        System.Windows.Forms.Timer t = null;
        public DateTime Sessionstart;
        public DateTime Sessionend;
        private byte[] changeddimmerchannels;
        private SerialPort serialPort1;
        private CoreAudioDevice defaultPlaybackDevice;
        private int currentPasswordindex = 0;
        private bool passwordstillvalid = true;
        private Label pinfield = null;
        private Color NumfieldButtonColor = Color.Red;
        private bool passwordwaswrong = false;
        public int currentState = 3;
        public Screen main;
        private bool streaming = false;
        private bool vlcclosed = false;
        private int minutes_received = 0;

        private Config config;
        private UIHelper helper;
        private Network net;

        public MainForm()
        {
            this.Hide();
            changeddimmerchannels = new byte[3];
            main = Screen.PrimaryScreen;
            loadscreen = new Loading(this, main);
            loadscreen.Show();
            loadscreen.updateProgress(10);
            config = new Config(null);
            loadscreen.updateProgress(20);
            net = new Network(this, config);
            serialPort1 = new SerialPort();
            loadscreen.updateProgress(30);
            InitializeComponent();
            loadscreen.updateProgress(40);
            helper = new UIHelper(this, config);
            loadscreen.updateProgress(50);
        }

        public async void Main_Load(object sender, EventArgs e)
        {
            this.Hide();
            start();
            loadscreen.updateProgress(90);
            await GastronomieWebview.EnsureCoreWebView2Async(null);

            loadscreen.updateProgress(100);
            this.Show();
            vlc.Show();
            loadscreen.Hide();

            //TODO: Test the State updater
            await Task.Run(async () =>
            {
                while (true)
                {
                    Network.RequestJson request = new Network.RequestJson();

                    request.destination = ArraytoString(config.IPZentrale, 4);
                    request.port = config.PortZentrale;
                    request.type = "Status";
                    request.id = currentState;
                    request.Raum = config.Room;
                    Network.UDPSender(request, false);
                    await Task.Delay(config.StateSendInterval * 1000);
                }
            });
        }

        private void start()
        {
            EnterFullscreen(this, main);
            if (config.showtime <= 0)
            {
                UIControl.Controls.Remove(TimePage);
            }
            if (config.showcolor <= 0)
            {
                UIControl.Controls.Remove(ColorPage);
            }
            int tabs = 5;
            if (config.showtime > 0)
            {
                tabs++;
            }
            UIControl.ItemSize = new Size(Constants.windowwidth / tabs, Constants.windowheight - Constants.tabheight);

            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

            loadscreen.updateProgress(60);
            Screen TV = null;
            if (Screen.AllScreens.Length > 1)
            {
                for (int i = 0; i < Screen.AllScreens.Length; i++)
                {
                    //Not Safe if Monitor is connected / disconnected during Loop is running
                    if (!Screen.AllScreens[i].Equals(main))
                    {
                        TV = Screen.AllScreens[i];
                        break;
                    }
                }
            }
            if (TV == null)
            {
                //Could be Timing Issue, when TV not yet Registered as  monitor
                Debug.Print("Second Monitor not found");
                currentState = 1;
                vlc.Hide();
                vlc.Dispose();
                vlc = null;
            }
            else
            {
                vlc = new EmbedVLC(this, TV);
            }

            loadscreen.updateProgress(70);
            GastronomieWebview.CoreWebView2InitializationCompleted += webcontentLoaded;

            AmbientVolume(config.Volume, 3, null);
            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], true);

            Sessionstart = DateTime.Now;
            Sessionend = DateTime.Now;
            Sessionend = Sessionend.AddMinutes(config.Sitzungsdauer);

            loadscreen.updateProgress(80);
            if (config.showtime > 0)
            {
                t = new System.Windows.Forms.Timer();
                t.Interval = 500;
                t.Tick += timer_tick;
                t.Enabled = true;
            }
#if !DEBUG
            minutes_received = config.SessionSettings.Count - 1;
#endif 
        }

        public void timer_tick(object sender, EventArgs e)
        {
            String Minutes = "";
            if (DateTime.Now.TimeOfDay.Minutes.ToString().Length <= 1)
            {
                Minutes = "0";
                Minutes += DateTime.Now.TimeOfDay.Minutes.ToString();
            }
            else
            {
                Minutes += DateTime.Now.TimeOfDay.Minutes.ToString();
            }
            String Hours = "";
            if (DateTime.Now.TimeOfDay.Hours.ToString().Length <= 1)
            {
                Hours = "0";
                Hours += DateTime.Now.TimeOfDay.Hours.ToString();
            }
            else
            {
                Hours += DateTime.Now.TimeOfDay.Hours.ToString();
            }
            clock.Text = $"{Hours}:{Minutes}";
            clock.Location = new Point((Constants.windowwidth / 2) - (clock.Size.Width / 2), clock.Location.Y);
            timer.Text = config.SessionSettings[minutes_received].ShowText;
            timer.Location = new Point((Constants.windowwidth / 2) - (timer.Size.Width / 2), timer.Location.Y);
            /*
            timer.Text = $"{((DateTime.Now.Subtract(Sessionstart).Minutes - config.Sitzungsdauer) * (-1)).ToString()}";
            timer.Location = new Point((Constants.windowwidth / 2) - (timer.Size.Width / 2), timer.Location.Y);
            /*
            if ((DateTime.Now.Subtract(Sessionstart).Minutes - config.Sitzungsdauer) > 0)
            {
                timer.ForeColor = Color.Red;
            }
            Minutes = "";
            if (Sessionend.TimeOfDay.Minutes.ToString().Length <= 1)
            {
                Minutes = "0";
                Minutes += Sessionend.TimeOfDay.Minutes.ToString();
            }
            else
            {
                Minutes += Sessionend.TimeOfDay.Minutes.ToString();
            }
            Hours = "";
            if (Sessionend.TimeOfDay.Hours.ToString().Length <= 1)
            {
                Hours = "0";
                Hours += Sessionend.TimeOfDay.Hours.ToString();
            }
            else
            {
                Hours += Sessionend.TimeOfDay.Hours.ToString();
            }
            sessionEnd.Text = $"{Hours} : {Minutes}";
            sessionEnd.Location = new Point((Constants.windowwidth / 2) - (sessionEnd.Size.Width / 2), sessionEnd.Location.Y);
            */
        }

        public void webcontentLoaded(object sender, EventArgs e)
        {
            UIControl.SelectTab(0);
        }

        public void Numberfield_Click(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            Button clicked = (Button)sender;
            if (clicked == null || clicked.Tag == null)
            {
                return;
            }
            int num = (int)clicked.Tag;
            if (num != Int32.Parse(config.Wartungspin[currentPasswordindex++]))
            {
                passwordstillvalid = false;
            }
            int Pos_x, Pos_y = 0;
            helper.GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 0.4, false);
            if (pinfield == null)
            {
                pinfield = new Label();
                pinfield.AutoSize = true;
                pinfield.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 0);
                pinfield.ForeColor = SystemColors.ControlLightLight;
                pinfield.Name = "PinField";
                WartungPage.Controls.Add(pinfield);
                pinfield.Show();
            }
            pinfield.Text = "";
            for (int i = 0; i < currentPasswordindex; i++)
            {
                pinfield.Text += "* ";
            }
            pinfield.Location = new Point((Constants.windowwidth - pinfield.Size.Width) / 2, Pos_y);
            if (passwordwaswrong)
            {
                foreach (Button b in helper.WartungPageButtons)
                {
                    helper.selectButton(b, false, NumfieldButtonColor);
                }
                RestrictedAreaDescribtion.ForeColor = SystemColors.ControlLightLight;
                passwordwaswrong = !passwordwaswrong;
            }
            if (currentPasswordindex == 6)
            {
                if (passwordstillvalid)
                {
                    helper.removeWartungPageElements();
                    helper.createRestrictedPageElements();
                }
                else
                {
                    foreach (Button b in helper.WartungPageButtons)
                    {
                        helper.selectButton(b, true, NumfieldButtonColor);
                    }
                    RestrictedAreaDescribtion.ForeColor = Color.Red;
                    passwordwaswrong = true;
                }
                if (pinfield != null)
                {
                    pinfield.Tag = -1;
                    pinfield.Hide();
                    pinfield.Text = "";
                    WartungPage.Controls.Remove(pinfield);
                    pinfield = null;
                }
                currentPasswordindex = 0;
                passwordstillvalid = true;
            }
        }

        public void Programm_Exit_Handler(object sender, EventArgs e)
        {
            Programm_Exit();
            ((Button)sender).Click -= Programm_Exit_Handler;
            ((Button)sender).Click += Programm_Enter_Handler;
            ((Button)sender).Text = Constants.EnterFullscreenText;
        }

        private void Programm_Exit()
        {
            this.WindowState = FormWindowState.Normal;
            this.ControlBox = true;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        public void logoutbutton_Handler(object sender, EventArgs e)
        {
            UIControl.SelectTab(0);
            logout();
        }

        public void logoutTab_Handler(object sender, EventArgs e)
        {
#if !DEBUG
            logout();
#endif
        }

        private void logout()
        {
            helper.removeRestrictedPageElements();
            helper.createWartungPageElements();
            if (pinfield != null)
            {
                pinfield.Tag = -1;
                pinfield.Hide();
                pinfield.Text = "";
                WartungPage.Controls.Remove(pinfield);
                pinfield = null;
            }
            currentPasswordindex = 0;
            passwordstillvalid = true;
            currentPasswordindex = 0;
            passwordstillvalid = true;
        }

        public void Programm_Enter_Handler(object sender, EventArgs e)
        {
            EnterFullscreen(this, main);
            ((Button)sender).Click -= Programm_Enter_Handler;
            ((Button)sender).Click += Programm_Exit_Handler;
            ((Button)sender).Text = Constants.ExitFullscreenText;
        }

        public void EnterFullscreen(Form f, Screen screen)
        {
#if !DEBUG
            f.TopMost = true;
            f.ControlBox = false;
            f.FormBorderStyle = FormBorderStyle.None;
#else
            f.TopMost = false;
            f.ControlBox = true;
#endif
            f.WindowState = FormWindowState.Normal;
            f.WindowState = FormWindowState.Maximized;
            //Set fullscreen
            f.Size = screen.Bounds.Size;
            f.Location = screen.Bounds.Location;
        }

        public void Ambiente_Change_Handler(object sender, EventArgs e)
        {
            Ambiente_Change(((Constants.DMXScene?)(((Button)(sender)).Tag)), false);

        }

        public void Ambiente_Change(Constants.DMXScene? scene, bool force)
        {
            if (scene == null)
            {
                return;
            }
            int index = config.DMXScenes.IndexOf(scene);
            if (config.DMXSceneSetting == index && !force)
            {
                helper.setActiveDMXScene(0);
                scene = config.DMXScenes[0];
            }
            else
            {
                helper.setActiveDMXScene(index);
            }
            if (vlc != null)
            {
                if (scene.ContentPath != null && scene.ContentPath.Length > 2 && !streaming && !vlcclosed)
                {
                    vlc.changeMedia(scene.ContentPath);
                    vlc.Show();
                }
                else
                {
                    vlc.quitMedia();
                }

            }
            SendCurrentSceneOverCom();
        }

        private void SendCurrentSceneOverCom()
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print("Error when trying to Open Com Port");
                    currentState = 1;
                }
            }
            byte[] tempchannelvalues = (byte[])config.DMXScenes[config.DMXSceneSetting].Channelvalues.Clone();
            if (config.Dimmerchannel[0] >= 0 && config.Dimmerchannel[0] < tempchannelvalues.Length)
            {
                tempchannelvalues[config.Dimmerchannel[0]] = changeddimmerchannels[0];
            }
            if (config.Dimmerchannel[1] >= 0 && config.Dimmerchannel[1] < tempchannelvalues.Length)
            {
                tempchannelvalues[config.Dimmerchannel[1]] = changeddimmerchannels[1];
            }
            if (config.ObjectLightchannel >= 0 && config.ObjectLightchannel < tempchannelvalues.Length)
            {
                tempchannelvalues[config.ObjectLightchannel] = changeddimmerchannels[2];
            }
            foreach (int value in config.colorwheelvalues[0])
            {
                if (tempchannelvalues.Length > value)
                {
                    tempchannelvalues[value] = colorWheelElement.Color.R;
                }
            }
            foreach (int value in config.colorwheelvalues[1])
            {
                if (tempchannelvalues.Length > value)
                {
                    tempchannelvalues[value] = colorWheelElement.Color.G;
                }
            }
            foreach (int value in config.colorwheelvalues[2])
            {
                if (tempchannelvalues.Length > value)
                {
                    tempchannelvalues[value] = colorWheelElement.Color.B;
                }
            }
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(tempchannelvalues, 0, config.DMXScenes[config.DMXSceneSetting].Channelvalues.Length);
                serialPort1.Close();
            }
            else
            {
                currentState = 1;
                System.Console.WriteLine("unable to open Serial Port");
            }
        }

        public void Ambiente_Design_Handler(object sender, EventArgs e)
        {
            if (((int?)(((Button)(sender)).Tag)) == 0)
            {
                ((Button)(sender)).Tag = 1;
                helper.selectButton((Button)sender, true, Constants.selected_color);
            }
            else
            {
                ((Button)(sender)).Tag = 0;
                helper.selectButton((Button)sender, false, Constants.selected_color);
            }
            if ((int?)((Button)(sender)).Tag == null)
            {
                currentState = 1;
                return;
            }
            changeddimmerchannels[2] = config.ObjectLightInterval[Byte.Parse($"{((Button)(sender)).Tag}")];

            SendCurrentSceneOverCom();
        }

        private bool changeChannelInAllScenes(byte value, int channel)
        {
            if (channel < 0)
            {
                return false;
            }
            for (int i = 0; i < config.DMXScenes.Count; i++)
            {
                byte[] ba = config.DMXScenes[i].Channelvalues;
                if (ba != null && ba.Length > channel)
                {
                    ba[channel] = value;
                }
                else
                {
                    currentState = 1;
                    return false;
                }
            }
            return true;
        }

        public void Content_Change_Handler(object sender, EventArgs e)
        {
            bool SwitchToStream = false;
            if ((bool)TVSettingsAmbienteButton.Tag)
            {
                SwitchToStream = TVSettingsAmbienteButton.Name.Equals("AmbientVideo");
            }
            else
            {
                SwitchToStream = !TVSettingsAmbienteButton.Name.Equals("AmbientVideo");
            }
            helper.selectButton(TVSettingsAmbienteButton, !SwitchToStream, Constants.selected_color);
            helper.selectButton(TVSettingsStreamingButton, SwitchToStream, Constants.selected_color);
            TVSettingsAmbienteButton.Tag = !SwitchToStream;
            TVSettingsStreamingButton.Tag = SwitchToStream;
            if (SwitchToStream)
            {
                foreach (ColorSlider.ColorSlider slider in helper.FormColorSlides)
                {
                    if (slider != null && ((int?)slider.Tag) == 3)
                    {
                        slider.Hide();
                    }
                }
                AmbientelautstärkeColorSliderDescribtion.Hide();
                TVSettingsVolumeColorSliderDescribtion.Hide();
                streaming = true;
                vlc.quitMedia();
            }
            else
            {
                foreach (ColorSlider.ColorSlider slider in helper.FormColorSlides)
                {
                    if (slider != null && ((int?)slider.Tag) == 3)
                    {
                        slider.Show();
                    }
                }
                AmbientelautstärkeColorSliderDescribtion.Show();
                TVSettingsVolumeColorSliderDescribtion.Show();
                streaming = false;
                Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], false);
            }
        }

        public void Service_Request_Handle(object sender, EventArgs e)
        {
            Network.RequestJson request = new Network.RequestJson();

            request.destination = ArraytoString(config.IPZentrale, 4);
            request.port = config.PortZentrale;
            request.type = "Service";
            request.Raum = config.Room;
            request.label = ((String?)((Button)(sender)).Tag);
            Network.UDPSender(request, true);
        }

        public void closePlayer(object sender, EventArgs e)
        {
            vlc.quitMedia();
            vlc.Hide();
            ((Button)(sender)).Click -= closePlayer;
            ((Button)(sender)).Click += OpenPlayer;
            ((Button)(sender)).Text = "Öffne den Player";
            vlcclosed = true;
        }

        public void OpenPlayer(object sender, EventArgs e)
        {
            vlc.Show();
            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], false);
            ((Button)(sender)).Click -= OpenPlayer;
            ((Button)(sender)).Click += closePlayer;
            ((Button)(sender)).Text = "Schließe den Player";
            vlcclosed = false;
        }

        public void AmbientVolume_Handler(object sender, EventArgs e)
        {
            if (!((ColorSlider.ColorSlider)(sender)).ContainsFocus)
            {
                return;
            }
            AmbientVolume(((ColorSlider.ColorSlider)(sender)).Value, ((int?)((ColorSlider.ColorSlider)(sender)).Tag), sender);
        }

        public void AmbientVolume(Decimal Value, int? tag, object? sender)
        {
            defaultPlaybackDevice.Volume = Decimal.ToDouble(Value);
            foreach (ColorSlider.ColorSlider slider in helper.FormColorSlides)
            {
                if (slider != null && ((int?)(slider.Tag)) == tag && !slider.Equals((ColorSlider.ColorSlider)(sender)))
                {
                    slider.ValueChanged -= AmbientVolume_Handler;
                    slider.Value = Value;
                    slider.ValueChanged += AmbientVolume_Handler;
                }
            }
        }

        public void Dimmer_Change(object sender, EventArgs e)
        {
            if (((ColorSlider.ColorSlider)(sender)).Name != null && ((ColorSlider.ColorSlider)(sender)).Name.Length >= 0)
            {
                int index = 0;
                try
                {
                    index = Int32.Parse(((ColorSlider.ColorSlider)(sender)).Name);
                }
                catch (FormatException ex)
                {
                    Debug.Print(ex.Message);
                    return;
                }
                if ((int)((ColorSlider.ColorSlider)(sender)).Value == null)
                {
                    currentState = 1;
                    return;
                }
                changeddimmerchannels[index] = (byte)((ColorSlider.ColorSlider)(sender)).Value;
            }
            SendCurrentSceneOverCom();
        }

        public void ColorChanged_Handler(object sender, EventArgs e)
        {
            ColorWheel colorWheel = (ColorWheel)sender;
            if (colorWheel != null)
            {
                Color c = colorWheel.Color;
                //ColorPage.BackColor = c;
                SendCurrentSceneOverCom();
            }
        }

        public void reset(object sender, EventArgs e)
        {
            this.Hide();
            loadscreen.Show();
            loadscreen.updateProgress(20);
            logout();
            Config c = new Config(config);
            config = c;
            net.changeconfig(c);
            helper.setConfig(c);
            AmbientVolume(config.Volume, 3, null);
            start();
            UIControl.SelectTab(0);
            loadscreen.updateProgress(100);
            this.Show();
            vlc.Show();
            loadscreen.Hide();
        }

        public String ArraytoString(String[] array, int until)
        {
            String r = "";
            for (int i = 0; i < array.Length && i < until; i++)
            {
                r += array[i];
                if (i + 1 < array.Length && i + 1 < until)
                {
                    r += ".";
                }
            }
            return r;
        }
    }
}
