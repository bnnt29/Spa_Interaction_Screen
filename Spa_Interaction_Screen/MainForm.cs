using AudioSwitcher.AudioApi.CoreAudio;
using Cyotek.Windows.Forms;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using static QRCoder.PayloadGenerator.SwissQrCode;
using System.Runtime.InteropServices;
using System.Xml;
using System.Diagnostics.Metrics;
using static Spa_Interaction_Screen.EmbedVLC;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.LinkLabel;
using System.Net.NetworkInformation;
using System.Net;
using System;
using QRCoder.Extensions;

/*TODO:
 * red marked in word
 * resolve System.InvalidOperationException (Invoke oder BeginInvoke kann für ein Steuerelement)
 * resolve fullscreen flicker
 * resolve Progamm not exiting on cross click
 * repair artefacts by change of Media Page
 * Implement all Jsons (sending)
 * Add TCP Json Type security
 * unblock scene via tcp
 * repair monitor setup (start with 1, then connect 1)
 * do not show elements with no / empty ShowText
 * Implement UPD Send/Receive
 * Test TCPButtons
 * Code //TODO's
 * Refactor for performance
 * Spid out DMX
 * Remove left over artefacts
 * 
 */

namespace Spa_Interaction_Screen
{
    public partial class MainForm : Form
    {
        public EmbedVLC vlc;
        public Loading loadscreen;

        System.Windows.Forms.Timer SessionTimer = null;
        private byte[] changeddimmerchannels;
        private CoreAudioDevice defaultPlaybackDevice;
        private int currentPasswordindex = 0;
        private bool passwordstillvalid = true;
        private Label pinfield = null;
        private bool passwordwaswrong = false;
        public int currentState = 3;
        public Screen? mainscreen;
        private bool streaming = false;
        public bool vlcclosed = false;
        private int minutes_received = 0;
        public bool scenelocked = false;
        public DateTime? TimeSessionEnd = null;
        private bool blocknonstreamingmedia = false;
        public bool SessionEndbool = false;
        private EmbedVLC sessionEndVLC = null;
        public bool SessionendNet = false;
        public int timeleftnet = int.MaxValue;
        public bool switchedtotimepage = false;
        public bool RunTask = true;
        public bool consoleshown = false;
        public bool showconsoleonallsites = false;

        private System.Windows.Forms.Timer ButtonColorTimer = new System.Windows.Forms.Timer();
        private List<Buttonfader> timecoloredbuttons = new List<Buttonfader>();

        public Config config;
        public UIHelper helper;
        public Network? net = null;
        public EnttecCom? enttec = null;
        public SerialPort? serialPort1 = null;
        public Logger Log;

        private Task state = null;
        private Task windows = null;
        private Task pinggastro = null;

        private bool exitProgramm = false;
        public bool HandleCreate = false;

        public MainForm()
        {
            Log = new Logger();
            this.FormClosed += OnFormClosed;
            mainscreen = Screen.PrimaryScreen;
            loadscreen = new Loading(this, mainscreen);
            loadscreen.Show();
            loadscreen.Activate();
            loadscreen.updateProgress(10);
            while ((config == null || !config.allread) && !exitProgramm)
            {
                config = new Config(null, Log);
                loadscreen.Debugtext("Es gibt ein Problem beim lesen der Konfig Datei. (Vielleicht ist sie noch blockiert)", !config.allread);
                loadscreen.exitp(!config.allread);
            }
            loadscreen.updateProgress(25);
            InitializeComponent();
            if (mainscreen == null)
            {
                Log.Print("Could not detect main screen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                return;
            }
            Constants.recalcsizes(mainscreen.Bounds.Size.Width, mainscreen.Bounds.Size.Height);
            changeddimmerchannels = new byte[3];
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
            loadscreen.updateProgress(45);
            helper.init();
            loadscreen.updateProgress(50);
            helper.setConfig(config);
            loadscreen.updateProgress(55);
            SendCurrentSceneOverCom();
        }

        public void OnFormClosed(object sender, EventArgs e)
        {
            RunTask = false;
        }

        public async void Main_Load(object sender, EventArgs e)
        {
            this.Hide();
            if (exitProgramm)
            {
                Application.Exit();
                return;
            }
            loadscreen.updateProgress(58);

            loadscreen.updateProgress(88);
            await GastronomieWebview.EnsureCoreWebView2Async(null);
            for (int i = 0; i < UIControl.TabCount; i++)
            {
                UIControl.SelectTab(i);
                Application.DoEvents();
            }
            start();

            loadscreen.updateProgress(93);
            if (vlc != null)
            {
                vlc.showthis();
            }
            loadscreen.updateProgress(95);
            this.TopMost = false;
            loadscreen.TopMost = true;


            ButtonColorTimer.Interval = Constants.buttonupdatemillis;
            ButtonColorTimer.Tick += UpdateButtoncolor;
            ButtonColorTimer.Enabled = true;

            GastronomieWebview.CoreWebView2InitializationCompleted += webcontentLoaded;

            loadscreen.updateProgress(100);
            UIControl.SelectTab(0);
            this.Show();
            EnterFullscreen(this, mainscreen, HandleCreate);
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
            RunTask = false;
            if (windows != null)
            {
                windows.Wait();
            }
            if (pinggastro != null)
            {
                pinggastro.Wait();
            }
            if (state != null)
            {
                state.Wait();
            }
            RunTask = true;
            windows = Task.Run(async () => ScreenManagerTaskMethod(this));
            pinggastro = Task.Run(async () => GastroPing(this));
            if (config.StateSendInterval > 0)
            {
                state = Task.Run(async () => sendState(this));
            }
        }

        private async Task GastroPing(MainForm form)
        {
            HttpWebResponse response = null;
            bool connectionok = false;
            while (form.RunTask && !Constants.noNet)
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(config.GastroUrl);
                request.AllowAutoRedirect = true; // find out if this site is up and don't follow a redirector
                request.Method = "HEAD";
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                    // do something with response.Headers to find out information about the request
                }
                catch (WebException wex)
                {
                    Log.Print(wex.Message, Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                    //set flag if there was a timeout or some other issues
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                }
                if (response == null)
                {
                    Log.Print("No Connection", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }
                if ((int)response.StatusCode > 100 && (int)response.StatusCode < 400)
                {
                    connectionok = true;
                }
                else if ((int)response.StatusCode > 400 && (int)response.StatusCode < 500)
                {
                    Log.Print("Client Error received, but ignoring it for now", Logger.MessageType.Gastro, Logger.MessageSubType.Notice);
                    connectionok = true;
                }
                else if ((int)response.StatusCode > 500 && (int)response.StatusCode < 600)
                {
                    Log.Print("Server Error received", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }
                else
                {
                    Log.Print("unknown Error received", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }
                object[] delegateArray = new object[1];
                delegateArray[0] = connectionok;
                if (HandleCreate)
                {
                    try
                    {
                        this.Invoke(new Myswitchgastro(delegateswitchgastronotreachable), delegateArray);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                        Log.Print("switchgastronotreachable", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                        delegateswitchgastronotreachable(connectionok);
                    }
                }
                else
                {
                    try
                    {
                        delegateswitchgastronotreachable(connectionok);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                        Log.Print("switchgastronotreachable", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                        this.Invoke(new Myswitchgastro(delegateswitchgastronotreachable), delegateArray);
                    }
                }
                await Task.Delay(10000);
            }
        }

        private async Task sendState(MainForm form)
        {
            while (form.RunTask && !Constants.noNet)
            {
                Network.RequestJson request = new Network.RequestJson();

                request.destination = ArraytoString(config.IPZentrale, 4);
                request.port = config.PortZentrale;
                request.type = "Status";
                request.id = currentState;
                request.Raum = config.Room;
                if (net.SendTCPMessage(request, null))
                {
                    Log.Print($"Message sent sucessfully", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                }
                await Task.Delay(config.StateSendInterval * 1000);
            }
        }

        public delegate void Myswitchgastro(bool reachable);
        public void delegateswitchgastronotreachable(bool reachable)
        {
            if (reachable)
            {
                GastronomieWebview.Show();
                GastroEx.Hide();
                GastroExDescription.Hide();
            }
            else
            {
                GastronomieWebview.Hide();
                GastroEx.Show();
                GastroExDescription.Show();
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
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("showthis", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
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
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("showthis", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    this.Invoke(new MyNoArgument(delegateshowthis));
                }
            }
        }

        private void delegateshowthis()
        {
            if(this != null && !this.IsDisposed)
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
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("hidethis", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
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
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("hidethis", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    this.Invoke(new MyNoArgument(delegatehidethis));
                }
            }
        }

        private void delegatehidethis()
        {
            this.Hide();
        }


        private async void ScreenManagerTaskMethod(MainForm form)
        {
            while (form.RunTask)
            {
                switch (SystemInformation.MonitorCount)
                {
                    case 0:
                        Log.Print("No Screen detected", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                        form.hidethis(); 
                        if (vlc != null)
                        {
                            vlc.hidethis();
                        }
                        break;
                    case 1:
                        if (!form.SessionEndbool)
                        {
                            form.showthis();
                        }
                        EnterFullscreen(form, Screen.PrimaryScreen, HandleCreate); 
                        if (vlc != null)
                        {
                            Log.Print("No secind Screen detected", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                            vlc.hidethis();
                        }
                        break;
                    default:
                        SetupEmbedvlcScreen(form);
                        if (!form.SessionEndbool)
                        {
                            form.showthis();
                        }
                        break;
                }
                await Task.Delay(3000);
            }
        }


        public delegate void MySetupEmbedvlcScreen(MainForm form);
        private void SetupEmbedvlcScreen(MainForm form)
        {
            object[] delegateArray = new object[1];
            delegateArray[0] = form;
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MySetupEmbedvlcScreen(delegateSetupEmbedvlcScreen), delegateArray);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("SetupEmbedvlcScreen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegateSetupEmbedvlcScreen(form);
                }
            }
            else
            {
                try
                {
                    delegateSetupEmbedvlcScreen(form);
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("SetupEmbedvlcScreen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    this.Invoke(new MySetupEmbedvlcScreen(delegateSetupEmbedvlcScreen), delegateArray);
                }
            }
        }

        private void delegateSetupEmbedvlcScreen(MainForm form)
        {
            if (SystemInformation.MonitorCount > 1)
            { 
                Screen TV = null;
                Size bounds = new Size(0, 0);
                if (Screen.AllScreens.Length > 1)
                {
                    for (int i = 0; i < Screen.AllScreens.Length; i++)
                    {
                        //Not Safe if Monitor is connected / disconnected during Loop is running
                        if (!Screen.AllScreens[i].Equals(mainscreen))
                        {
                            if (TV != null)
                            {
                                if (Screen.AllScreens[i].Bounds.Size.Width > Screen.AllScreens[i].Bounds.Size.Height && Screen.AllScreens[i].Bounds.Size.Width * Screen.AllScreens[i].Bounds.Size.Height > bounds.Width * bounds.Height)
                                {
                                    TV = Screen.AllScreens[i];
                                    bounds = Screen.AllScreens[i].Bounds.Size;
                                }
                            }
                            else
                            {
                                TV = Screen.AllScreens[i];
                                bounds = Screen.AllScreens[i].Bounds.Size;
                            }
                        }
                    }
                }
                else
                {
                    Log.Print("Different Variables for the same thing Stated different Results.\n Therefore second Monitor couldn't be initialized.", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                }
                if (TV == null)
                {
                    //Could be Timing Issue, when TV not yet Registered as  monitor
                    Log.Print("Second Monitor not found", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
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
                    if (!vlcclosed)
                    {
                        if (vlc == null)
                        {
                            vlc = new EmbedVLC(form, TV, false);
                            AmbientVolume(config.Volume, 3, null);
                            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], true, false, false);
                        }
                        vlc.showthis();
                    }
                }
            }
        }

        private void start()
        {
            EnterFullscreen(this, mainscreen, HandleCreate);

            loadscreen.updateProgress(60);
            if (!config.showtime)
            {
                UIControl.Controls.Remove(TimePage);
            }
            if (!config.showcolor)
            {
                UIControl.Controls.Remove(ColorPage);
            }
            resizeUIControlItems();

            loadscreen.updateProgress(70);
            defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;

            setupThreads();

            SendCurrentSceneOverCom();

            loadscreen.updateProgress(80);
            if (config.showtime)
            {
                if (SessionTimer != null)
                {
                    SessionTimer.Stop();
                    SessionTimer.Enabled = false;
                    SessionTimer.Dispose();
                }
                SessionTimer = new System.Windows.Forms.Timer();
                SessionTimer.Interval = 1000;
                SessionTimer.Tick += timer_tick;
                SessionTimer.Enabled = true;
            }


#if !DEBUG
            minutes_received = config.SessionSettings.Count - 1;
#endif
        }

        public void resizeUIControlItems()
        {
            int tabs = 5;
            if (config.showcolor)
            {
                tabs++;
            }
            if (config.showtime)
            {
                tabs++;
            }
            if (consoleshown)
            {
                tabs++;
            }
            UIControl.ItemSize = new Size((Constants.windowwidth - tabs) / tabs, UIControl.ItemSize.Height);
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
            if (TimeSessionEnd != null)
            {
                int timeleftnotclamped = (int)Math.Ceiling(((TimeSpan)(TimeSessionEnd - DateTime.Now)).TotalMinutes);
                int timeleftclamped = Math.Max(timeleftnotclamped, 0);
                if (timeleftnet <= config.SessionEndShowTimeLeft)
                {
                    if (!streaming)
                    {
                        /*
                        if (vlc != null && !SessionEndbool)
                        {
                            vlc.changeMedia(config.SessionEndImage, false);
                        }
                        blocknonstreamingmedia = true;
                        */
                        if (!switchedtotimepage)
                        {
                            if (config.showtime)
                            {
                                Content_Change(false);
                                int x = 4 + ((config.showcolor) ? 1 : 0);
                                UIControl.SelectTab(x);
                                if (vlc != null)
                                {
                                    vlc.changeMedia(config.SessionEndImage, false);
                                }
                            }
                        }
                        TVSettingsAmbienteButton.Hide();
                        TVSettingsStreamingButton.Hide();
                        MediaPageAmbientVolumeSlider.Location = new Point(MediaPageAmbientVolumeSlider.Location.X, Constants.tabheight / 3);
                        helper.SetupLabelofTrackbar(MediaPageAmbientVolumeSlider, TVSettingsVolumeColorSliderDescribtion, config.slidernames[((int)MediaPageAmbientVolumeSlider.Tag) - 1]);
                        switchedtotimepage = true;
                    }
                }
                else
                {
                    double z = 2.3;
                    if (config.password == null || config.password.Length <= 0)
                    {
                        z = 1;
                    }
                    helper.CreateMediaControllingElemets(z);
                }
                if (timeleftnotclamped <= 0)
                {
                    if (timeleftnotclamped <= Constants.SessionOvertimeBuffer * -1 && !SessionEndbool)
                    {
                        foreach (Label l in helper.globaltimelabels)
                        {
                            l.Hide();
                        }
                        EndSession();
                        return;
                    }
                }
                Constants.SessionSetting Settingstoapply = null;
                bool wasbigger = false;
                for (int i = 0; i < config.SessionSettings.Count; i++)
                {
                    if (config.SessionSettings[i].mins < timeleftnet)
                    {
                        if (i < config.SessionSettings.Count - 1)
                        {
                            Settingstoapply = config.SessionSettings[i + 1];
                        }
                        else
                        {
                            Settingstoapply = null;
                        }
                    }
                }
                if (Settingstoapply == null)
                {
                    Settingstoapply = config.SessionSettings[config.SessionSettings.Count - 1];
                }
                if (Settingstoapply != null)
                {
                    if (Settingstoapply.should_reset)
                    {
                        reset();
                        return;
                    }
                    String s = Settingstoapply.ShowText;
                    String min_left = timeleftclamped.ToString();
                    s = s.Replace("[id]", min_left);
                    if (s != null && s.Length > 0)
                    {
                        foreach (Label l in helper.globaltimelabels)
                        {
                            l.Text = s;
                            l.BackColor = Color.Transparent;
                            l.Font = Constants.Time_font;
                            l.Show();
                            helper.SetEdgePosition(l, config.edgetimePosition);
                        }
                    }
                    if (timer != null)
                    {
                        timer.Text = s;
                        timer.Font = Constants.Time_font;
                        timer.Location = new Point((Constants.windowwidth / 2) - (timer.Size.Width / 2), timer.Location.Y);
                    }
                }
            }
            else
            {
                if (vlc != null)
                {
                    vlc.changeMedia(config.DMXScenes[config.DMXSceneSetting].ContentPath,false);
                }
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
                    helper.selectButton(b, false, Constants.Warning_color);
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
                        helper.selectButton(b, true, Constants.Warning_color);
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

        public void Login(object sender, EventArgs e)
        {
            helper.removeWartungPageElements();
            helper.createRestrictedPageElements();
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
            if (!SessionEndbool)
            {
                UIControl.SelectTab(0);
            }
            logout();
        }

        public void Tab_Changed_Handler(object sender, TabControlCancelEventArgs e)
        {
            if (passwordwaswrong)
            {
                foreach (Button b in helper.WartungPageButtons)
                {
                    helper.selectButton(b, false, Constants.Warning_color);
                }
                RestrictedAreaDescribtion.ForeColor = Constants.Text_color;
                passwordwaswrong = !passwordwaswrong;
            }
            if (SessionEndbool)
            {
                if (e.TabPageIndex != UIControl.TabCount - 1 || !(e.TabPageIndex == UIControl.TabCount - 2 && consoleshown))
                {
                    e.Cancel = true;
                    if (sessionEndVLC != null)
                    {
                        sessionEndVLC.Show();
                        sessionEndVLC.BringToFront();
                    }
                    SessionEnded(sessionEndVLC, true);
                }
            }
#if !DEBUG
            logout();
#endif
            if (consoleshown && !showconsoleonallsites)
            {
                vlc.toggleConsoleBox(e.TabPageIndex == UIControl.TabCount - 1);
            }
        }

        public void logout()
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
            EnterFullscreen(this, mainscreen, HandleCreate);
            ((Button)sender).Click -= Programm_Enter_Handler;
            ((Button)sender).Click += Programm_Exit_Handler;
            ((Button)sender).Text = Constants.ExitFullscreenText;
        }


        public delegate void MyFullscreen(Form f, Screen screen);

        public void EnterFullscreen(Form f, Screen screen, bool handle)
        {
            object[] delegateArray = new object[2];
            delegateArray[0] = f;
            delegateArray[1] = screen;
            //The check for the Handle Create can fail when the questioning Form is EmbedVLC.
            if (handle)
            {
                try
                {
                    f.Invoke(new MyFullscreen(delegateEnterFullscreen), delegateArray);
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("EnterFullscreen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegateEnterFullscreen(f, screen);
                }
            }
            else
            {
                try
                {
                    delegateEnterFullscreen(f,screen);
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("EnterFullscreen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    f.Invoke(new MyFullscreen(delegateEnterFullscreen), delegateArray);
                }
            }
        }

        public void delegateEnterFullscreen(Form f, Screen screen)
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
            Ambiente_Change(((Constants.DMXScene?)(((Button)(sender)).Tag)), false, true, false);
            //SendCurrentSceneOverCom();
        }
        public void Ambiente_Change(Constants.DMXScene? scene, bool force, bool user, bool keepMedia)
        {
            if (scenelocked)
            {
                return;
            }
            if (scene == null)
            {
                return;
            }
            int index = config.DMXScenes.IndexOf(scene);
            if (config.DMXSceneSetting == index && !force)
            {
                helper.setActiveDMXScene(0, force);
                scene = config.DMXScenes[0];
            }
            else
            {
                helper.setActiveDMXScene(index, force);
            }
            //Log.Print($"{vlc != null},{scene.ContentPath != null},{scene.ContentPath.Length > 2},{!streaming},{!vlcclosed}");
            if (vlc != null && !keepMedia)
            {
                if (scene.ContentPath != null && scene.ContentPath.Length > 2 && !streaming && !vlcclosed)
                {
                    if (!blocknonstreamingmedia && !SessionEndbool)
                    {
                        vlc.changeMedia(scene.ContentPath, user);
                    }
                    vlc.showthis();
                }
                else
                {
                    vlc.quitMedia(user);
                }

            }
            SendCurrentSceneOverCom();
        }

        public async void SendCurrentSceneOverCom()
        {
            if (Constants.noCOM)
            {
                return;
            }
            bool noserial = true;
            Task con = null;
            serialPort1 = new SerialPort();
            if (!enttec.isopen())
            {
                con = Task.Run(() =>
                {
                    if (!enttec.connect())
                    {
                        Log.Print("Error when trying to Communicate with Enttec Port", Logger.MessageType.Licht, Logger.MessageSubType.Error);
                        currentState = 1;
                    }
                    else
                    {
                        noserial = false;
                    }
                });
            }
            else
            {
                noserial = false;
            }
            byte[] tempchannelvalues = (byte[])config.DMXScenes[config.DMXSceneSetting].Channelvalues.Clone();
            if (!scenelocked)
            {
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

            }
            for (int i = 0; i < tempchannelvalues.Length; i++)
            {
                tempchannelvalues[i] = (byte)Math.Min(tempchannelvalues[i], (byte)254);
                tempchannelvalues[i] = (byte)Math.Max(tempchannelvalues[i], (byte)0);
            }
            tempchannelvalues[0] = 255;
            byte[] fade = new byte[tempchannelvalues.Length];
            //Log.Print($"Time:{DateTime.Now}-{config.lastchangetime}={(DateTime.Now - config.lastchangetime).TotalMilliseconds}");
            double millis = (DateTime.Now - config.lastchangetime).TotalMilliseconds;
            if (millis <= config.FadeTime && config.currentvalues != null)
            {
                double fadesteps = (config.FadeTime - millis) / (double)Constants.sendtimeout;
                for (int i = 0; i < tempchannelvalues.Length; i++)
                {
                    if (fadesteps == 0)
                    {
                        break;
                    }
                    fade[i] = (byte)(config.currentvalues[i] + Math.Round((double)(tempchannelvalues[i] - config.currentvalues[i]) / fadesteps));
                }
            }
            else
            {
                fade = tempchannelvalues;
            }
            config.currentvalues = fade;
            //Log.Print($"Trying to send the following DMX Data:0,{fade[0]},{fade[1]},{fade[2]},{fade[3]},{fade[4]},{fade[5]},{fade[6]},{fade[7]},{fade[8]},{fade[9]},{fade[10]},{fade[11]},{fade[12]},{fade[13]},{fade[14]}");
            if (con != null)
            {
                con.Wait();
            }
            if (!noserial && enttec.isopen())
            {
                enttec.sendDMX(fade);
            }
            else
            {
                currentState = 1;
                Log.Print("unable to open Enttec Port", Logger.MessageType.Licht, Logger.MessageSubType.Error);
            }
        }

        public void Ambiente_Design_Handler(object sender, EventArgs e)
        {
            if ((int?)((Button)(sender)).Tag == null)
            {
                currentState = 1;
                return;
            }
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
            changeddimmerchannels[2] = config.ObjectLightInterval[(int)((Button)(sender)).Tag];
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
            SendCurrentSceneOverCom();
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
            Content_Change(SwitchToStream);
        }

        private void Content_Change(bool SwitchToStream)
        {
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
                    vlc.quitMedia(false);
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
                Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], true, true, false);
                SendCurrentSceneOverCom();
            }
        }

        public void Wartung_Request_Handle(object sender, EventArgs e)
        {
            if (Constants.noNet)
            {
                return;
            }
            Network.RequestJson request = new Network.RequestJson();

            request.destination = ArraytoString(config.IPZentrale, 4);
            request.port = config.PortZentrale;
            request.type = "System";
            request.Raum = config.Room;
            request.label = ((Constants.SystemSetting)((Button)(sender)).Tag).JsonText;
            request.values = new String[] { ((Constants.SystemSetting)((Button)(sender)).Tag).value };
            if (net.SendTCPMessage(request, null))
            {
                Log.Print($"Message sent sucessfully", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            }
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= Wartung_Request_Handle;
            addcolortimedButton(((Button)sender), 1000, Constants.Button_color, Wartung_Request_Handle);
        }

        public void EndSession_Handler(object sender, EventArgs e)
        {
            EndSession();
        }

        public void EndSession()
        {
            EmbedVLC evlc =new EmbedVLC(this, mainscreen, true);
            if(evlc != null)
            {
                evlc.changeMedia(config.SessionEndImage, true);
            }
            if (vlc != null)
            {
                vlc.changeMedia(null, true);
            }
            blocknonstreamingmedia = true;
        }

        public void Service_Request_Handle(object sender, EventArgs e)
        {
            Constants.ServicesSetting s = (Constants.ServicesSetting)((Button)(sender)).Tag;
            if (s != null && s.hassecondary)
            {
                Constants.ServicesSettingfunction sf = (Constants.ServicesSettingfunction)((Button)(sender)).Tag;
                if (sf.functionclass != null && sf.function != null && sf.functionclass.IsSubclassOf(typeof(Constants.configclasses)))
                {
                    performsecondary(sf);
                }
            }
            if (Constants.noNet)
            {
                return;
            }
            Network.RequestJson request = new Network.RequestJson();

            request.destination = ArraytoString(config.IPZentrale, 4);
            request.port = config.PortZentrale;
            request.type = "Service";
            request.Raum = config.Room;
            request.label = s.JsonText;
            if (net.SendTCPMessage(request, null))
            {
                Log.Print($"Message sent sucessfully", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
            }
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= Service_Request_Handle;
            addcolortimedButton(((Button)sender), 1000, Constants.Button_color, Service_Request_Handle);
        }

        private void performsecondary(Constants.ServicesSettingfunction ssf)
        {
            if(ssf == null || ssf.functionclass == null || ssf.function == null || !ssf.functionclass.Equals(ssf.function.GetType()))
            {
                return;
            }
            if (ssf.functionclass.Equals(new Constants.SystemSetting().GetType()))
            {
                //TODO
            }
            else if (ssf.functionclass.Equals(new Constants.TCPSetting().GetType()))
            {
                //TODO
            }
            else if (ssf.functionclass.Equals(new Constants.SessionSetting().GetType()))
            {
                //TODO
            }
            else if (ssf.functionclass.Equals(new Constants.ServicesSetting().GetType()))
            {
                //TODO
            }
            else if (ssf.functionclass.Equals(new Constants.DMXScene().GetType()))
            {
                if (ssf.enable)
                {
                    Ambiente_Change(((Constants.DMXScene)(ssf.function)), true, true, false);
                    if (ssf.block)
                    {
                        setscenelocked(true, Constants.scenelockedinfo, Constants.Warning_color);
                    }
                }
                else
                {
                    if (ssf.block)
                    {

                        setscenelocked(false, Constants.scenelockedinfo, Constants.Warning_color);
                    }
                    Ambiente_Change(config.DMXScenes[0], true, true, false);
                }
               
            }
            else
            {
                //TODO
            }
        }

        public void closePlayer_Handler(object sender, EventArgs e)
        {
            closePlayer(false);
            ((Button)(sender)).Click -= closePlayer_Handler;
            ((Button)(sender)).Click += OpenPlayer_Handler;
            ((Button)(sender)).Text = "Öffne den Player";
            helper.selectButton(((Button)(sender)), false, Constants.selected_color);
        }

        public void closePlayer(bool screenissue)
        {
            if (vlc != null)
            {
                vlc.quitMedia(false);
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
            helper.selectButton(((Button)(sender)), true, Constants.selected_color);
        }

        public void OpenPlayer()
        {
            if (vlc != null)
            {
                vlc.showthis();
            }
            Ambiente_Change(config.DMXScenes[config.DMXSceneSetting], false, false, false);
            //SendCurrentSceneOverCom();
            vlcclosed = false;
            foreach (Button b in helper.RestrictedPageButtons)
            {
                if (b != null && b.Tag != null && b.Tag.GetType().Equals(typeof(String)) && ((String)b.Tag).Length > 0 && ((String)b.Tag).Equals("VLCClose"))
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
                int index;
                try
                {
                    index = Int32.Parse(((ColorSlider.ColorSlider)(sender)).Name);
                }
                catch (FormatException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    return;
                }
                if (scenelocked)
                {

                    ((ColorSlider.ColorSlider)(sender)).ValueChanged -= Dimmer_Change;
                    try
                    {
                        ((ColorSlider.ColorSlider)(sender)).Value = (int)((float)((float)(config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.Dimmerchannel[index]]) / (float)255.0) *100);
                    }
                    catch(Exception ex)
                    {
                        Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    }
                    ((ColorSlider.ColorSlider)(sender)).ValueChanged += Dimmer_Change;
                    return;
                }   
                if ((int)((ColorSlider.ColorSlider)(sender)).Value == null)
                {
                    currentState = 1;
                    return;
                }
                changeddimmerchannels[index] = (byte)(int)((float)((float)((ColorSlider.ColorSlider)sender).Value/100.0)*255);
            }
            SendCurrentSceneOverCom();
        }

        public void ColorChanged_Handler(object sender, EventArgs e)
        {
            if(sender.Equals(colorWheelElement))
            {
                Log.Print("Equal", Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Information);
                if (((ColorSlider.ColorSlider)colorWheelElement.Tag).Value < 15)
                {
                    ((ColorSlider.ColorSlider)colorWheelElement.Tag).Value = 20;
                }
            }
            Constants.RGBW c = RGBtoRGBW(colorWheelElement.Color);
            foreach (int value in config.colorwheelvalues[0])
            {
                if (config.DMXScenes[2].Channelvalues.Length > value && value>=0)
                {
                    config.DMXScenes[2].Channelvalues[value] = (byte)(double)(c.R * ((ColorSlider.ColorSlider)colorWheelElement.Tag).Value * new decimal(0.01));
                }
            }
            foreach (int value in config.colorwheelvalues[1])
            {
                if (config.DMXScenes[2].Channelvalues.Length > value && value >= 0)
                {
                    config.DMXScenes[2].Channelvalues[value] = (byte)(double)(c.G * ((ColorSlider.ColorSlider)colorWheelElement.Tag).Value * new decimal(0.01));
                }
            }
            foreach (int value in config.colorwheelvalues[2])
            {
                if (config.DMXScenes[2].Channelvalues.Length > value && value >= 0)
                {
                    config.DMXScenes[2].Channelvalues[value] = (byte)(double)(c.B * ((ColorSlider.ColorSlider)colorWheelElement.Tag).Value * new decimal(0.01));
                }
            }
            foreach (int value in config.colorwheelvalues[3])
            {
                if (config.DMXScenes[2].Channelvalues.Length > value && value >= 0)
                {
                    config.DMXScenes[2].Channelvalues[value] = (byte)(double)(c.W * ((ColorSlider.ColorSlider)colorWheelElement.Tag).Value * new decimal(0.01));
                }
            }
            Ambiente_Change(config.DMXScenes[2], true, true, true);
        }


        public void reset_Handler(object sender, EventArgs e)
        {
            reset();
        }


        public delegate void MyNoArgument();
        public void reset()
        {
            if (HandleCreate)
            {
                try
                {
                    this.BeginInvoke(new MyNoArgument(delegatereset));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("reset", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegatereset();
                }
            }
            else
            {
                try
                {
                    delegatereset();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("reset", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    this.BeginInvoke(new MyNoArgument(delegatereset));
                }
            }
        }

        public void delegatereset()
        {
            Log.Print("Performing Reset", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
            if (SessionTimer != null)
            {
                SessionTimer.Stop();
                SessionTimer.Enabled = false;
                SessionTimer.Dispose();
            }
            RunTask = false;
            this.Hide();
            loadscreen = new Loading(this, mainscreen);
            loadscreen.Show();
            loadscreen.updateProgress(20);
            logout();
            Config c = new Config(config, Log);
            loadscreen.updateProgress(40);
            config = c;
            if (!Constants.noNet)
            {
                net.changeconfig(c);
            }
            SessionendNet = false;
            scenelocked = false;
            timeleftnet = int.MaxValue;
            TimeSessionEnd = null;
            blocknonstreamingmedia = false;
            SessionEndbool = false;
            sessionEndVLC = null;
            switchedtotimepage = false;
            consoleshown = false;
            helper.setConfig(c);
            AmbientVolume(config.Volume, 3, null);
            loadscreen.updateProgress(50);
            start();
            UIControl.SelectTab(0);
            loadscreen.updateProgress(100);
            this.Show();
            if (vlc != null)
            {
                vlc.showthis();
            }
            loadscreen.Hide();
            loadscreen.Close();
            Log.Print("Reset finisched", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
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
            if (config.password == null || config.password.Length <= 0)
            {
                p.Image = null;
                p.Hide();
                return false;
            }
            else
            {
                p.Show();
            }
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
                p.Image = Image.FromFile("QRplaceholderstillcreating.png");
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

        public Constants.RGBW RGBtoRGBW(Color c)
        {
            //Get the maximum between R, G, and B
            float tM = Math.Max(c.R, Math.Max(c.G, c.B));

            //If the maximum value is 0, immediately return pure black.
            if (tM == 0)
            { 
                return new Constants.RGBW(c, 0, 0, 0, 0); 
            }

            //This section serves to figure out what the color with 100% hue is

            float[] values = new float[4];
            values[0] = 255.0f / tM;
            values[1] = c.R * values[0];
            values[2] = c.G * values[0];
            values[3] = c.B * values[0];

            //This calculates the Whiteness (not strictly speaking Luminance) of the color
            float Luminance = ((Math.Max(values[1], Math.Max(values[2], values[3])) + Math.Min(values[1], Math.Min(values[2], values[3]))) / 2.0f - 127.5f) * (255.0f / 127.5f) / values[0];

            //Calculate the output values
            values[0] = Convert.ToInt32(Luminance);
            values[1] = Convert.ToInt32(c.R - Luminance);
            values[2] = Convert.ToInt32(c.G - Luminance);
            values[3] = Convert.ToInt32(c.B - Luminance);

            //Trim them so that they are all between 0 and 255
            for (int i=0;i<values.Length;i++) 
            {
                values[i] = Math.Max(values[i], 0);
                values[i] = Math.Min(values[i], 255);
            }
            return new Constants.RGBW(c, (byte)values[1], (byte)values[2], (byte)values[3], (byte)values[0]);
        }

        public void resetcolorwheel(object sender, EventArgs e)
        {
            colorWheelElement.ColorChanged -= ColorChanged_Handler;
            colorWheelElement.Color = Color.White;
            colorWheelElement.ColorChanged += ColorChanged_Handler;
            ((ColorSlider.ColorSlider)(colorWheelElement.Tag)).ValueChanged -= ColorChanged_Handler;
            ((ColorSlider.ColorSlider)(colorWheelElement.Tag)).Value = 100;
            ((ColorSlider.ColorSlider)(colorWheelElement.Tag)).ValueChanged += ColorChanged_Handler;
        }

        public void setscenelocked(bool x, String txt, Color c)
        {
            scenelocked = x;
            foreach (Label l in helper.globalinformationlabels)
            {
                l.Text = txt;
                l.ForeColor = c;
                if (x)
                {
                    l.Show();
                    helper.SetEdgePosition(l, 3);
                    l.Location = new Point((Constants.windowwidth/2)-(l.Size.Width/2), l.Location.Y);
                    l.BringToFront();
                    resetscenelockbutton.Show();
                }
                else
                {
                    l.Hide();
                    helper.SetEdgePosition(l, 3);
                    l.Location = new Point((Constants.windowwidth / 2) - (l.Size.Width / 2), l.Location.Y);
                }
            }
            foreach (Button b in helper.AmbientePageButtons)
            {
                b.Enabled = false;
            }
            colorWheelElement.Enabled = false;
            ((ColorSlider.ColorSlider)colorWheelElement.Tag).Enabled = false;
        }

        public void resetscenelock_Handler(object sender, EventArgs e)
        {
            resetscenelock();
        }

        public void resetscenelock()
        {
            resetscenelockbutton.Hide();
            setscenelocked(false, "", Constants.Text_color);

            foreach (Button b in helper.AmbientePageButtons)
            {
                b.Enabled = true;
            }
            colorWheelElement.Enabled = true;
            ((ColorSlider.ColorSlider)colorWheelElement.Tag).Enabled = true;
        }

        public void SessionEnded(EmbedVLC evlc, bool fromevent)
        {
            sessionEndVLC = evlc;
            SessionEndbool = true;
            if (fromevent)
            {
                UIControl.SelectTab(UIControl.TabCount - 1);
            }
            scenelocked = false;
            Ambiente_Change(config.DMXScenes[0], true, true, false);
            this.hidethis();
        }

        public void showlogin()
        {
            if (SessionEndbool)
            {
                this.showthis();
                UIControl.SelectTab(UIControl.TabCount - 1);
            }
        }

        public void Shutdown()
        {
#if DEBUG
            Process.Start("shutdown.exe", "-s -t 100 -c \"Remote Shutdown received\"");
            reset();
            Task.Delay(20000).Wait();
            Process.Start("shutdown.exe", "-a");
#else
            Process.Start("shutdown.exe", "-s -f -t 00 -c \"Remote Shutdown received\"");
#endif
        }

        public void Restart()
        {
#if DEBUG
            Process.Start("shutdown.exe", "-r -t 100 -c \"Remote Restart received\"");
            reset();
            Task.Delay(20000).Wait();
            Process.Start("shutdown.exe", "-a");
#else
            Process.Start("shutdown.exe", "-r -f -t 00 -c \"Remote Shutdown received\"");
#endif
        }

        public void ShowConsole(object sender, EventArgs e)
        {
            Log.setCurrentlyshowing(Byte.MaxValue, this);
            helper.createConsolePage();
            if (showconsoleonallsites)
            {
                vlc.toggleConsoleBox(true);
            }
            else
            {
                vlc.toggleConsoleBox(false);
            }
            if(UIControl != null)
            {
                UIControl.SelectTab(UIControl.TabCount - 1);
            }
            ((Button)(((Button)(sender)).Tag)).Show();
            ((Button)(sender)).Click -= ShowConsole;
            ((Button)(sender)).Click += CloseConsole;
            ((Button)(sender)).Text = "Schließe Konsole";
            helper.selectButton(((Button)(sender)), true, Constants.selected_color);
        }

        public void CloseConsole(object sender, EventArgs e)
        {
            helper.removeConsolePage();
            ((Button)(((Button)(sender)).Tag)).Hide();
            ((Button)(sender)).Click -= CloseConsole;
            ((Button)(sender)).Click += ShowConsole;
            ((Button)(sender)).Text = "Öffne Konsole";
            helper.selectButton(((Button)(sender)), false, Constants.selected_color);
        }

        public void ShowConsoleOnallSites(object sender, EventArgs e)
        {
            showconsoleonallsites = true;
            vlc.toggleConsoleBox(true);
            ((Button)(sender)).Click -= ShowConsoleOnallSites;
            ((Button)(sender)).Click += ShowConsoleNotOnallSites;
            ((Button)(sender)).Text = "Zeige Konsole nicht immer";
            helper.selectButton(((Button)(sender)), true, Constants.selected_color);
        }

        public void ShowConsoleNotOnallSites(object sender, EventArgs e)
        {
            showconsoleonallsites = false;
            vlc.toggleConsoleBox(false);
            ((Button)(sender)).Click -= ShowConsoleNotOnallSites;
            ((Button)(sender)).Click += ShowConsoleOnallSites;
            ((Button)(sender)).Text = "Zeige Konsole immer";
            helper.selectButton(((Button)(sender)), false, Constants.selected_color);
        }

        public String AddConsoleLine(String line)
        {
            if(line != null && line.Length > 0)
            {
                if (vlc != null && vlc.ConsoleBox != null)
                {
                    vlc.ConsoleBox.Text += line;
                    vlc.ConsoleBox.Text += "\n\r";
                    return vlc.ConsoleBox.Text;
                }
                return line;
            }
            return "";
        }

        public void SetConsoleText(String Text)
        {
            if (Text != null && Text.Length > 0)
            {
                if (vlc != null && vlc.ConsoleBox != null)
                {
                    vlc.ConsoleBox.Clear();
                    vlc.ConsoleBox.Text += Text;
                }
            }
        }

        public void ClearConsole()
        {
            if(vlc != null && vlc.ConsoleBox != null)
            {
                vlc.ConsoleBox.Text = "";
                vlc.ConsoleBox.Clear();
            }
        }

        public void comboconsoleItemchanged(object sender, EventArgs e)
        {
            int Index = ((Constants.ComboItem)((ComboBox)sender).SelectedItem).ID;
            ClearConsole();
            SetConsoleText(Log.GetConsoleText((Logger.MessageType)Index, null));
            
            Log.setCurrentlyshowing((byte)Index, this);
        }


        /*
            public ComboBox tcptype;
            public TextBox CommandboxLabel;
            public NumericUpDown Commandboxid;
            public TextBox Commandboxvalues;
         */
        public void sendTCPfromconsole(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Constants.alternative_color;
            addcolortimedButton(((Button)sender), 250, Constants.Button_color, Numberfield_Click);

            String p = "{"; 
            p += '"';
            p += "room";
            p += '"';
            p += ':';
            p += config.Room;
            p += ',';
            p += '"';
            p += "type";
            p += '"';
            p += ':';
            p += '"';
            p += config.Typenames[tcptype.SelectedIndex];
            p += '"';
            if (CommandboxLabel.Text.Length > 0)
            {
                p += ", ";
                p += '"';
                p += "label";
                p += '"';
                p += ":";
                p += '"';
                p += CommandboxLabel.Text;
                p += '"';
            }
            int id = -1;
            if (Commandboxid.Text != null && Commandboxid.Text.Length > 0)
            {
                try
                {
                    id = Int32.Parse(Commandboxid.Text);
                }
                catch (FormatException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                }
            }
            if (id >= 0)
            {
                p += ", ";
                p += '"';
                p += "id";
                p += '"';
                p += ':';
                p += id;
            }
            if (Commandboxvalues.Text.Length > 0)
            {
                p += ", ";
                p+='"';
                p +="values";
                p +='"';
                p +=":[";
                foreach (String s in Commandboxvalues.Text.Split(','))
                {
                    p += '"';
                    p += s.Trim();
                    p += '"';
                    p += ',';
                }
                if (p.EndsWith(','))
                {
                    p = p.Substring(0, p.Length - 1);
                }
                p += ']';
            }
            p += '}';
            net.Messageafterparse(p);
        }
    }
}
