using AudioSwitcher.AudioApi.CoreAudio;
using Cyotek.Windows.Forms;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using static QRCoder.PayloadGenerator.SwissQrCode;


namespace Spa_Interaction_Screen
{
    public partial class MainForm : Form
    {
        public EmbedVLC vlc;
        public Loading loadscreen;

        System.Windows.Forms.Timer t = null;
        public DateTime Sessionstart;
        public DateTime Sessionend;
        private byte[] changeddimmerchannels;
        private CoreAudioDevice defaultPlaybackDevice;
        private int currentPasswordindex = 0;
        private bool passwordstillvalid = true;
        private Label pinfield = null;
        private bool passwordwaswrong = false;
        public int currentState = 3;
        public Screen? main;
        private bool streaming = false;
        private bool vlcclosed = false;
        private int minutes_received = 0;

        private System.Windows.Forms.Timer ButtonColorTimer = new System.Windows.Forms.Timer();
        private List<Buttonfader> timecoloredbuttons = new List<Buttonfader>();

        public Config config;
        public UIHelper helper;
        public Network? net = null;
        public EnttecCom? enttec = null;
        public SerialPort? serialPort1 = null;

        private Task dmx = null;
        private Task state = null;
        private Task windows = null;

        private bool exitProgramm = false;
        public bool HandleCreate = false;

        public MainForm()
        {
            this.Hide();
            main = Screen.PrimaryScreen;
            if(main == null) 
            {
                Debug.Print("Could not detect main screen");
                return;
            }
            Constants.recalcsizes(main.Bounds.Size.Width, main.Bounds.Size.Height);
            changeddimmerchannels = new byte[3];
            loadscreen = new Loading(this, main);
            loadscreen.Show();
            loadscreen.Activate();
            loadscreen.updateProgress(10);
            InitializeComponent();
            this.Hide();
            loadscreen.updateProgress(20);
            while ((config == null || !config.allread) && !exitProgramm)
            {
                config = new Config(null);
                loadscreen.Debugtext("Es gibt ein Problem beim lesen der Konfig Datei. (Vielleicht ist sie noch blockiert)", true);
                loadscreen.exitp(true);
            }
            if (exitProgramm)
            {
                Application.Exit();
                return;
            }
            loadscreen.Debugtext("", false);
            loadscreen.exitp(false);
            loadscreen.updateProgress(30);
            this.HandleCreated += new EventHandler((sender, args) =>
            {
                HandleCreate = true;
            });
            if (!Constants.noNet)
            {
                net = new Network(this, config);
            }
            /*Old way of creating Com port
            this.components = new System.ComponentModel.Container();
            serialPort1 = new SerialPort(this.components);
            serialPort1.PortName = config.ComPort;
            serialPort1.BaudRate = 9600;
            */
            if (!Constants.noCOM)
            {
                serialPort1 = new SerialPort(config.ComPort, 9600);
                enttec = new EnttecCom(this, config);
            }
            loadscreen.updateProgress(40);
            helper = new UIHelper(this, config);
            helper.init();
            loadscreen.updateProgress(50);
            SendCurrentSceneOverCom();
        }

        public async void Main_Load(object sender, EventArgs e)
        {
            if (exitProgramm)
            {
                Application.Exit();
                return;
            }
            this.Hide();

            start();
            loadscreen.updateProgress(90);
            await GastronomieWebview.EnsureCoreWebView2Async(null);

            if (vlc != null)
            {
                vlc.showthis();
            }

            setupThreads();
            loadscreen.updateProgress(100);
            this.TopMost = false;
            loadscreen.TopMost = true;
            for(int i = 0; i < UIControl.TabCount; i++)
            {
                UIControl.SelectTab(i);
                Application.DoEvents();
            }


            ButtonColorTimer.Interval = Constants.buttonupdatemillis;
            ButtonColorTimer.Tick += UpdateButtoncolor;
            ButtonColorTimer.Enabled = true;


            UIControl.SelectTab(0);
            this.Show();
            EnterFullscreen(this, main);
            loadscreen.Hide();
            loadscreen.Close();
            loadscreen = null;
            if (exitProgramm)
            {
                Application.Exit();
            }
        }

        private void setupThreads()
        {
            dmx = Task.Run(() =>
            {
                while (true)
                {
                    SendCurrentSceneOverCom();
                    Application.DoEvents();
                }
            });
            windows = Task.Run(() =>
            {
                while (true)
                {
                    while (Screen.AllScreens.Length < 2)
                    {
                        if (Screen.AllScreens.Length == 0)
                        {
                            Debug.Print("No Screen detected");
                            this.Hide();
                        }
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(500);
                    while (Screen.AllScreens.Length >= 2)
                    {
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(500);
                }
            });

            //TODO: Test the State updater
            state = Task.Run(() =>
            {
                while (true && !Constants.noNet)
                {
                    Network.RequestJson request = new Network.RequestJson();

                    request.destination = ArraytoString(config.IPZentrale, 4);
                    request.port = config.PortZentrale;
                    request.type = "Status";
                    request.id = currentState;
                    request.Raum = config.Room;
                    if (net.SendTCPMessage(request, null))
                    {
                        Debug.Print($"Message sent sucessfully");
                    }
                    Thread.Sleep(config.StateSendInterval * 1000);
                }
            });
        }

        private void start()
        {
            EnterFullscreen(this, main);
            loadscreen.updateProgress(60);
            if (!config.showtime)
            {
                UIControl.Controls.Remove(TimePage);
            }
            if (!config.showcolor)
            {
                UIControl.Controls.Remove(ColorPage);
            }
            int tabs = 5; 
            if (config.showcolor)
            {
                tabs++;
            }
            if (config.showtime)
            {
                tabs++;
            }
            UIControl.ItemSize = new Size((Constants.windowwidth-tabs) / tabs, UIControl.ItemSize.Height);

            loadscreen.updateProgress(70);
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

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
                if (vlc != null)
                {
                    vlc.hidethis();
                    vlc.Dispose();
                    vlc = null;
                }
            }
            else
            {
                vlc = new EmbedVLC(this, TV);
            }

            GastronomieWebview.CoreWebView2InitializationCompleted += webcontentLoaded;

            AmbientVolume(config.Volume, 3, null);
            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], true, false);
            //SendCurrentSceneOverCom();

            Sessionstart = DateTime.Now;
            Sessionend = DateTime.Now;
            Sessionend = Sessionend.AddMinutes(config.Sitzungsdauer);

            loadscreen.updateProgress(80);
            if (config.showtime)
            {
                t = new System.Windows.Forms.Timer();
                t.Interval = 1000;
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
            if(clock != null)
            {
                clock.Text = $"{Hours}:{Minutes}";
                clock.Location = new Point((Constants.windowwidth / 2) - (clock.Size.Width / 2), clock.Location.Y);
            }
            if (timer != null)
            {
                timer.Text = config.SessionSettings[minutes_received].ShowText;
                timer.Location = new Point((Constants.windowwidth / 2) - (timer.Size.Width / 2), timer.Location.Y);
            }
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
                pinfield.Font = Constants.Standart_font;
                pinfield.ForeColor = Constants.Text_color;
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
                    helper.selectButton(b, false, Constants.NumfieldErrorButton_color);
                }
                RestrictedAreaDescribtion.ForeColor = Constants.Text_color;
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
                        if (containsfadingbutton(b))
                        {
                            removefadingbutton(b);
                        }
                        helper.selectButton(b, true, Constants.NumfieldErrorButton_color);
                    }
                    RestrictedAreaDescribtion.ForeColor = Constants.Text_color;
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
            else
            {
                ((Button)sender).BackColor = Constants.alternative_color;
                ((Button)sender).Click -= Numberfield_Click;
                addcolortimedButton(((Button)sender), 250, Constants.Button_color, Numberfield_Click);
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
            if (passwordwaswrong)
            {
                foreach (Button b in helper.WartungPageButtons)
                {
                    helper.selectButton(b, false, Constants.NumfieldErrorButton_color);
                }
                RestrictedAreaDescribtion.ForeColor = Constants.Text_color;
                passwordwaswrong = !passwordwaswrong;
            }
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
            Ambiente_Change(((Constants.DMXScene?)(((Button)(sender)).Tag)), false, true);
            //SendCurrentSceneOverCom();
        }
        public void Ambiente_Change(Constants.DMXScene? scene, bool force, bool user)
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
                    vlc.changeMedia(scene.ContentPath, user);
                    vlc.showthis();
                }
                else
                {
                    vlc.quitMedia();
                }

            }
            //SendCurrentSceneOverCom();
        }

        private void SendCurrentSceneOverCom()
        {
            if (Constants.noCOM)
            {
                return;
            }
            bool noserial = false;
            Task con = null;
            if (!enttec.isopen())
            {
                con = Task.Run(() =>
                {
                    /*if (!serialPort1.IsOpen)
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
                            noserial = true;
                        }
                    }*/
                    if (!enttec.connect())
                    {
                        Debug.Print("Error when trying to Communicate with Enttec Port");
                        currentState = 1;
                    }
                });
                
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
            if (config.HDMISwitchchannel >= 0 && config.HDMISwitchchannel < tempchannelvalues.Length)
            {
                tempchannelvalues[config.HDMISwitchchannel] = (!streaming) ? (byte)255 : (byte)0;
            }
            for (int i = 0; i < tempchannelvalues.Length; i++)
            {
                tempchannelvalues[i] = (tempchannelvalues[i] >= 255) ? (byte)254 : tempchannelvalues[i];
            }
            tempchannelvalues[0] = 255;
            byte[] fade = new byte[tempchannelvalues.Length];
            int millis = DateTime.Now.Millisecond - config.lastchangetime.Millisecond;
            if(millis <= config.FadeTime && config.prevscene != null)
            {
                double fadesteps = (config.FadeTime - millis) / Constants.sendtimeout;
                for (int i = 0; i < tempchannelvalues.Length; i++)
                {
                    if(fadesteps == 0)
                    {
                        break;
                    }
                    fade[i] += (byte)((double)(config.prevscene[i] - tempchannelvalues[i])/fadesteps);  
                }
            }
            config.prevscene = fade;
            if(con != null)
            {
                con.Wait();
            }
            if (serialPort1.IsOpen)
            {
                //serialPort1.Write(tempchannelvalues, 0, tempchannelvalues.Length);
            }
            else
            {
                currentState = 1;
                //Debug.Print("unable to open Serial Port");
            }
            if (enttec.isopen())
            {
                enttec.sendDMX(fade);
            }
            else
            {
                currentState = 1;
                Debug.Print("unable to open Serial Port");
            }
            Thread.Sleep((int)(Constants.sendtimeout));
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

            //SendCurrentSceneOverCom();
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
                if (vlc != null)
                {
                    vlc.quitMedia();
                }
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
                Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], true, true);
                //SendCurrentSceneOverCom();
            }
        }

        public void Service_Request_Handle(object sender, EventArgs e)
        {
            if (Constants.noNet)
            {
                return;
            }
            Network.RequestJson request = new Network.RequestJson();

            request.destination = ArraytoString(config.IPZentrale, 4);
            request.port = config.PortZentrale;
            request.type = "Service";
            request.Raum = config.Room;
            request.label = ((String?)((Button)(sender)).Tag);
            if (net.SendTCPMessage(request, null))
            {
                Debug.Print($"Message sent sucessfully");
            }
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= Service_Request_Handle;
            addcolortimedButton(((Button)sender), 1000, Constants.Button_color, Service_Request_Handle);
        }

        public void closePlayer_Handler(object sender, EventArgs e)
        {
            closePlayer(false);
            ((Button)(sender)).Click -= closePlayer_Handler;
            ((Button)(sender)).Click += OpenPlayer_Handler;
            ((Button)(sender)).Text = "Öffne den Player";
        }

        public void closePlayer(bool screenissue)
        {
            if (vlc != null)
            {
                vlc.quitMedia();
                vlc.hidethis();
            }
            vlcclosed = true;
            if(screenissue) 
            { 
                foreach (Button b in helper.RestrictedPageButtons)
                {
                    if (b != null && ((String)b.Tag).Length > 0 && ((String)b.Tag).Equals("VLCClose"))
                    {
                        b.Hide();
                    }
                }
            }
        }


        public void OpenPlayer_Handler(object sender, EventArgs e)
        {
            OpenPlayer();
            ((Button)(sender)).Click -= OpenPlayer_Handler;
            ((Button)(sender)).Click += closePlayer_Handler;
            ((Button)(sender)).Text = "Schließe den Player";
        }

        public void OpenPlayer()
        {
            if (vlc != null)
            {
                vlc.showthis();
            }
            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], false, false);
            //SendCurrentSceneOverCom();
            vlcclosed = false;
            foreach (Button b in helper.RestrictedPageButtons)
            {
                if (b != null && ((String)b.Tag).Length > 0 && ((String)b.Tag).Equals("VLCClose"))
                {
                    b.Show();
                }
            }
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
            if (helper.FormColorSlides == null)
            {
                return;
            }
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
                changeddimmerchannels[index] = (byte)(int)((float)((float)((ColorSlider.ColorSlider)sender).Value/100.0)*255);
            }
            //SendCurrentSceneOverCom();
        }

        public void ColorChanged_Handler(object sender, EventArgs e)
        {
            ColorWheel colorWheel = (ColorWheel)sender;
            if (colorWheel != null)
            {
                if (colorWheelElement.Color.R != 0)
                {
                    foreach (int value in config.colorwheelvalues[0])
                    {
                        if (config.DMXScenes[2].Channelvalues.Length > value)
                        {
                            config.DMXScenes[2].Channelvalues[value] = colorWheelElement.Color.R;
                        }
                    }
                }
                if (colorWheelElement.Color.G != 0)
                {
                    foreach (int value in config.colorwheelvalues[1])
                    {
                        if (config.DMXScenes[2].Channelvalues.Length > value)
                        {
                            config.DMXScenes[2].Channelvalues[value] = colorWheelElement.Color.G;
                        }
                    }
                }
                if (colorWheelElement.Color.B != 0)
                {
                    foreach (int value in config.colorwheelvalues[2])
                    {
                        if (config.DMXScenes[2].Channelvalues.Length > value)
                        {
                            config.DMXScenes[2].Channelvalues[value] = colorWheelElement.Color.B;
                        }
                    }
                }
                Ambiente_Change(config.DMXScenes[2], true, true);
            }
        }

        public void reset_Handler(object sender, EventArgs e)
        {
            reset();
        }

        public void reset()
        {
            this.Hide();
            loadscreen = new Loading(this, main);
            loadscreen.Show();
            loadscreen.updateProgress(20);
            logout();
            Config c = new Config(config);
            loadscreen.updateProgress(40);
            config = c;
            if (!Constants.noNet)
            {
                net.changeconfig(c);
            }
            helper.setConfig(c);
            AmbientVolume(config.Volume, 3, null);
            loadscreen.updateProgress(50);
            start();
            UIControl.SelectTab(0);
            loadscreen.updateProgress(100);
            this.Show();
            vlc.showthis();
            loadscreen.Hide();
            loadscreen.Close();
            loadscreen = null;
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
        public bool generateQRCode(PictureBox p, int pixelsize, bool quietzone, int size, bool inv)
        {
            QRCodeGenerator qrCodegen = new QRCodeGenerator();
            QRCodeData qrCodeData = qrCodegen.CreateQrCode($"WIFI:S:{config.WiFiSSID};T:WPA;P:{config.password};;", QRCodeGenerator.ECCLevel.H, true);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = null;
            Color a = Color.White;
            Color b = Color.Black;
            if (config.QRLogoFilePath != null && config.QRLogoFilePath.Length >= 0)
            {
                qrCodeImage = qrCode.GetGraphic(pixelsize, (!inv) ? a : b, (inv) ? a : b, (Bitmap)Bitmap.FromFile(config.QRLogoFilePath), 25, 1, quietzone, Color.Transparent);
            }
            else
            {
                qrCodeImage = qrCode.GetGraphic(pixelsize, (!inv) ? a : b, (inv) ? a : b, quietzone);
            }

            p.Size = new Size(size, size);
            if(qrCodeImage == null)
            {
                return false;
            }
            p.Image = qrCodeImage;
            return true;
        }

        public void NewSession_Handler(object sender, EventArgs e)
        {
            NewSession();
        }

        public void NewSession()
        {
            reset();
        }

        public void ExitProgramm(object sender, EventArgs e)
        {
            exitProgramm = true;
            if (dmx != null)
            {
                dmx.Dispose();
            }
            if (state != null)
            {
                state.Dispose();
            }
            if (windows != null)
            {
                windows.Dispose();
            }
            Application.Exit();
        }

        public void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = UIControl.TabPages[e.Index];
            Rectangle paddedBounds = e.Bounds;
            paddedBounds.Inflate(0, 2);
            paddedBounds.Offset(0, 2);
            e.Graphics.FillRectangle(new SolidBrush((e.State == DrawItemState.Selected) ? Constants.Background_color : Constants.alternative_color), paddedBounds);

            paddedBounds = e.Bounds;
            int yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
            paddedBounds.Offset(1, yOffset);
            TextRenderer.DrawText(e.Graphics, page.Text, e.Font, paddedBounds, page.ForeColor);
        }

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
        public bool containsfadingbutton(Button b)
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

        public bool removefadingbutton(Button b)
        {
            for (int i = 0;i < timecoloredbuttons.Count;i++)
            {
                if (timecoloredbuttons[i].b == b)
                {
                    timecoloredbuttons.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void addcolortimedButton(Button b, long millis, Color to, EventHandler eh)
        {
            DateTime until = DateTime.Now.AddMilliseconds(millis);
            Buttonfader bf = new Buttonfader(b, until, to, eh);
            Debug.Print($"TargetColor: {to.R} {to.G} {to.B}");
            timecoloredbuttons.Add(bf);
        }

        public void UpdateButtoncolor(object sender, EventArgs e)
        {
            for(int i = 0;i < timecoloredbuttons.Count;i++)
            {
                Buttonfader bf = timecoloredbuttons[i];
                if (DateTime.Now >= bf.until)
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

                    double steps = bf.until.Millisecond - DateTime.Now.Millisecond;
                    steps /= Constants.buttonupdatemillis;

                    if(steps > 0)
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
