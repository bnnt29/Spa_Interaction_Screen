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
using System.Net.Sockets;
using Test;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;

/*TODO:
 * repair monitor setup (start with 1, then connect 1)
 * Test TCPButtons
 * Refactor for performance
 * Remove left over artefacts
 * repair artefacts by change of Media Page
 * 
 */

namespace Spa_Interaction_Screen
{
    public abstract partial class CForm : Form
    {
        public bool HandleCreate = false;

        public CForm()
        {
            this.HandleCreated += new EventHandler((sender, args) =>
            {
                HandleCreate = true;
            });
            //this.CreateHandle();
            /*if (this.IsHandleCreated)
            {
                HandleCreate = true;
            }*/

            this.FormClosed += OnFormClosed;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        public abstract void OnFormClosed(object sender, EventArgs e);
       
    }
    public partial class MainForm : CForm
    {
        public EmbedVLC vlc;
        public Loading loadscreen;

        System.Windows.Forms.Timer SessionTimer = null;
        System.Windows.Forms.Timer fadingtimer = null;

        private byte[] changeddimmerchannels;
        private CoreAudioDevice defaultPlaybackDevice;
        private int currentPasswordindex = 0;
        private bool passwordstillvalid = true;
        private Label pinfield = null;
        private bool passwordwaswrong = false;
        public static int currentState = 1;
        public Screen? mainscreen;
        private bool streaming = false;
        public bool vlcclosed = false;
        public bool Sessionlocked = false;
        public bool Servicelocked = false;
        public bool Scenelocked = false;
        public DateTime? TimeSessionEnd = null;
        private bool blocknonstreamingmedia = false;
        public bool SessionEndbool = false;
        public EmbedVLC sessionEndVLC = null;
        public bool SessionendNet = false;
        public int timeleftnet = int.MaxValue;
        public bool switchedtotimepage = false;
        public bool RunTask = true;
        public bool showconsoleonallsites = false;
        public bool lastpingpositiv = true;
        public bool lastZentralButtonstate = true;
        public bool endFading = false;
        public bool watingforEmbed = false;
        public bool[] SaunaAmbientButtons = new bool[3];
        public bool volumeinit = false;

        private System.Windows.Forms.Timer ButtonColorTimer = new System.Windows.Forms.Timer();

        public Config config;
        public UIHelper helper;
        public Network? net = null;
        public SerialPort? serialPort1 = null;

        private Task state = null;
        private Task windows = null;
        private Task pinggastro = null;
        private Task pingzentrale = null;
        private Task<PingReply>[] ping = new Task<PingReply>[2];

        private bool exitProgramm = false;

        private delegate object MyNoArgument(); 
        private delegate object MySetupEmbedvlcScreen(MainForm form);
        private delegate object Myswitchgastro(bool reachable);
        private delegate object MyContentchange(bool reachable);
        private delegate object Myperformsecondary(Constants.ServicesSettingfunction ssf);
        private delegate object MyFullscreen(Form f, Screen screen);
        private delegate object Mysetscenelocked(bool x, String txt, Color c);
        private delegate object Mysetservicelocked(bool x, Color c);
        private delegate object Mysetsessionlocked(bool x);

        public MainForm() :base()
        {
            Logger.form = this;
            mainscreen = Screen.PrimaryScreen;
            loadscreen = new Loading(this, mainscreen);
            loadscreen.Show();
            loadscreen.Activate();
            loadscreen.updateProgress(10);
            loadscreen.Debugtext($"Loading Konfig", true);
            while ((config == null || !config.allread) && !exitProgramm)
            {
                config = new Config(null);
                loadscreen.Debugtext("Es gibt ein Problem beim lesen der Konfig Datei. (Vielleicht ist sie noch blockiert)", !config.allread);
                loadscreen.exitp(!config.allread);
            }
            loadscreen.updateProgress(20);
            loadscreen.Debugtext($"Creating Static Objects", true);
            InitializeComponent();
            if (mainscreen == null)
            {
                MainForm.currentState = 3;
                Logger.Print("Could not detect main screen", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
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
            if (!Constants.noNet)
            {
                net = new Network(this, config);
            }
            streaming = false;
            loadscreen.updateProgress(40);
            loadscreen.Debugtext($"Creating Dynamic Objects", true);
            helper = new UIHelper(this, config);
            loadscreen.updateProgress(45);
            helper.init();
            loadscreen.updateProgress(50);
            helper.setConfig(config);
            loadscreen.updateProgress(55);
            loadscreen.Debugtext($"Sending first DMX", true);
            SendCurrentSceneOverCom();
        }

        public async void Main_Load(object sender, EventArgs e)
        {
            this.Hide();
            if (exitProgramm)
            {
                Application.Exit();
                return;
            }
            loadscreen.updateProgress(60);
            loadscreen.Debugtext($"Connecting to Gastro Website", true);
            await GastronomieWebview.EnsureCoreWebView2Async(null);

            GastronomieWebview.CoreWebView2InitializationCompleted += webcontentLoaded;

            loadscreen.updateProgress(80);
            loadscreen.Debugtext($"Initiating Variables", true);
            start();
            loadscreen.updateProgress(85);
            loadscreen.Debugtext($"Finishing UP", true);
            //loadscreen.TopMost = true;

            config.currentvalues[config.HDMISwitchchannel] = 255;
            EnttecCom.sendDMX(config.currentvalues);
            Task.Delay(Constants.sendtimeout*2+1).Wait();
            SendCurrentSceneOverCom();

            loadscreen.updateProgress(90);

            ButtonColorTimer.Interval = Constants.buttonupdatemillis;
            ButtonColorTimer.Tick += ButtonFader.UpdateButtoncolor;
            ButtonColorTimer.Enabled = true;

            loadscreen.updateProgress(99);

            loadscreen.Debugtext($"Preloading Sites", true);
            hidethis();
            Constants.InvokeDelegate<object>([], new MyNoArgument(UIControlSwitcher), this);

            loadscreen.Debugtext($"Switching to Mainview", true);
            Constants.InvokeDelegate<object>([], new MyNoArgument(loadscreensloser), this);
            EnterFullscreen(this, mainscreen);
            showthis();
            if (exitProgramm)
            {
                Application.Exit();
            }
            //Constants.InvokeDelegate<object>([], new MyNoArgument(selectUIControl), this);
            //Constants.InvokeDelegate<object>([], new MyNoArgument(selectUIControl), this);
        }

        public override void OnFormClosed(object sender, EventArgs e)
        {
            if (vlc != null)
            {
                vlc.Close();
            }
            if(loadscreen != null)
            {
                loadscreen.Close();
            }
            UIControl.SelectedIndex = 0;
            SendCurrentSceneOverCom();
            exitProgramm = true;
            RunTask = false;
            if (net != null && net.tcpSockets != null)
            {
                foreach (Socket tcp in net.tcpSockets)
                {
                    tcp.Close();
                }
            }
            Logger.Print("Shutdown Main", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
            Logger.closeStreams();
            OpenDMX.done = true;
            Program.runningParent = false;
            Application.Exit();
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void start()
        {
            EnterFullscreen(this, mainscreen);

            Constants.InvokeDelegate<object>([], new MyNoArgument(removetabs), this);
            loadscreen.updateProgress(60);

            loadscreen.updateProgress(70);
            Task.Run(() => {
                defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                volumeinit = true;
            });

            setupThreads();

            SendCurrentSceneOverCom();

            loadscreen.updateProgress(80);
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
            Constants.InvokeDelegate<object>([], new MyNoArgument(resizeUIControlItems), this);
        }

        private void setupThreads()
        {
            RunTask = false;
            Task.Run(() =>
            {
                if (windows != null)
                {
                    windows.Wait();
                }
                if (pinggastro != null)
                {
                    pinggastro.Wait();
                }
                if (pingzentrale != null)
                {
                    pingzentrale.Wait();
                }
                if (state != null)
                {
                    state.Wait();
                }
                RunTask = true;
                windows = Task.Run(async () => ScreenManagerTaskMethod(this));
                pinggastro = Task.Run(async () => GastroPing(this));
                pingzentrale = Task.Run(async () => ZentralePing(this));
                if (config.StateSendInterval > 0)
                {
                    //TODO
                    //state = Task.Run(async () => sendState(this));
                }
            });
        }

        private async Task ZentralePing(MainForm form)
        {
            int index = 0;
            while (form.RunTask && !Constants.noNet)
            {
                Ping pinger = null;
                byte[] ip = new byte[4];
                try
                {
                    for (int i = 0; i < config.IPZentrale.Length; i++)
                    {
                        ip[i] = Byte.Parse(config.IPZentrale[i]);
                    }
                }
                catch (FormatException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.TCPSend, Logger.MessageSubType.Error);
                }
                IPAddress ZentralIP = new IPAddress(ip);
                try
                {
                    pinger = new Ping();
                    ping[index] = pinger.SendPingAsync(ZentralIP);
                }
                catch (PingException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Extern, Logger.MessageSubType.Error);
                }
                bool zisclient = form.net.tcpSockets.Count > 0;
                if (lastZentralButtonstate != zisclient)
                {
                    lastZentralButtonstate = zisclient;
                    Constants.InvokeDelegate<object>([], new MyNoArgument(delegateswitchzentralenotreachable), form);
                    
                }
                if (lastpingpositiv)
                {
                    await Task.Delay(10000);
                }
                else
                {
                    await Task.Delay(3000);
                }

                index++;
                index %= ping.Length;
                if (ping != null && ping[index] != null)
                {
                    PingReply reply = await ping[index];
                    lastpingpositiv = reply.Status == IPStatus.Success;
                    if (!lastpingpositiv)
                    {
                        Logger.Print($"Zentralen Ping Status:{reply.Status}", Logger.MessageType.TCPSend, Logger.MessageSubType.Information);
                    }
                }
                if (!lastpingpositiv && zisclient)
                {
                    Logger.Print("Die Zentrale ist nicht erreichbar. Es hat sich jedoch jemand Registriert.", Logger.MessageType.Intern, Logger.MessageSubType.Notice);
                }
                if (lastpingpositiv && !zisclient)
                {
                    Logger.Print("Die Zentrale ist erreichbar, hat sich allerdings noch nicht Registriert.", Logger.MessageType.Extern, Logger.MessageSubType.Notice);
                }
                if (!lastpingpositiv && !zisclient)
                {
                    Logger.Print("Die Zentrale ist nicht erreichbar und hat sich nicht Registriert.", [Logger.MessageType.Extern, Logger.MessageType.Intern], Logger.MessageSubType.Notice);
                }
                if (zisclient)
                {
                    bool zentral = false;
                    foreach (Socket tcp in form.net.tcpSockets)
                    {
                        zentral |= net.isClientZentrale(tcp);
                    }
                    if (!lastpingpositiv && zentral)
                    {
                        Logger.Print("Die Zentrale ist nicht erreichbar, hat sich allerdings Registriert.", [Logger.MessageType.Extern, Logger.MessageType.Intern], Logger.MessageSubType.Notice);
                    }
                    if (lastpingpositiv && zentral)
                    {
                        Logger.Print("Die Zentrale ist erreichbar und hat sich Registriert.", [Logger.MessageType.Extern, Logger.MessageType.Intern], Logger.MessageSubType.Information);
                    }
                }
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
                    MainForm.currentState = 4;
                    Logger.Print(wex.Message, Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                    //set flag if there was a timeout or some other issues
                }
                catch (Exception ex)
                {
                    MainForm.currentState = 5;
                    Logger.Print(ex.Message, Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                }
                if (response == null)
                {
                    MainForm.currentState = 5;
                    Logger.Print("No Connection", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }
                if ((int)response.StatusCode > 100 && (int)response.StatusCode < 400)
                {
                    connectionok = true;
                }
                else if ((int)response.StatusCode > 400 && (int)response.StatusCode < 500)
                {
                    Logger.Print("Client Error received, but ignoring it for now", Logger.MessageType.Gastro, Logger.MessageSubType.Notice);
                    connectionok = true;
                }
                else if ((int)response.StatusCode > 500 && (int)response.StatusCode < 600)
                {
                    MainForm.currentState = 5;
                    Logger.Print("Server Error received", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }
                else
                {
                    MainForm.currentState = 5;
                    Logger.Print("unknown Error received", Logger.MessageType.Gastro, Logger.MessageSubType.Error);
                    connectionok = false;
                }

                Constants.InvokeDelegate<object>([connectionok], new Myswitchgastro(delegateswitchgastronotreachable), form);
                
                if (connectionok)
                {
                    await Task.Delay(10000);
                }
                else
                {
                    await Task.Delay(2500);
                }
            }
        }

        private async Task sendState(MainForm form)
        {
            while (form.RunTask && !Constants.noNet)
            {
                Network.RequestJson request = new Network.RequestJson();

                request.destination = ArraytoString(config.IPZentrale, 4);
                request.port = config.PortZentrale;
                request.label = "State";
                request.type = config.Typenames[0];
                request.id = MainForm.currentState;
                request.Raum = config.Room;
                if (net.SendTCPMessage(request, null))
                {
                    Logger.Print($"Message sent sucessfully", Logger.MessageType.TCPSend, Logger.MessageSubType.Information);
                }
                if(Programmstate != null)
                {
                    Programmstate.Text = $"Programmstatus: {MainForm.currentState}";
                }
                await Task.Delay(config.StateSendInterval * 1000);
            }
        }

        private async void ScreenManagerTaskMethod(MainForm form)
        {
            while (form.RunTask)
            {
                switch (SystemInformation.MonitorCount)
                {
                    case 0:
                        MainForm.currentState = 3;
                        Logger.Print("No Screen detected", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
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
                        EnterFullscreen(form, Screen.PrimaryScreen);
                        if (vlc != null)
                        {
                            MainForm.currentState = 0;
                            Logger.Print("No second Screen detected", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                            vlc.hidethis();
                        }
                        break;
                    default:
                        if(vlc == null) 
                        {
                        
                            form.watingforEmbed = true;
                            SetupEmbedvlcScreen(form);
                            if (!form.SessionEndbool)
                            {
                                form.showthis();
                            }
                        }
                        break;
                }
                while (form.watingforEmbed)
                {
                    await Task.Delay(1000);
                }
                await Task.Delay(3000);
            }
        }

        public object delegateswitchzentralenotreachable()
        {
            if(config.ServicesSettings == null)
            {
                return null;
            }
            foreach (Constants.ServicesSetting ss in config.ServicesSettings)
            {
                Button b = ss.ButtonElement;
                if (b == null)
                {
                    continue;
                }
                if (((net.tcpSockets.Count > 0 && lastpingpositiv) || ss.hassecondary) && !Servicelocked)
                {
                    ButtonFader.addcolortimedButton(b, Constants.Buttonshortfadetime, Constants.Button_color, null);
                    b.Enabled = true;
                    b.Click += Service_Request_Handle;
                }
                else
                {
                    ButtonFader.addcolortimedButton(b, Constants.Buttonshortfadetime, Constants.alternative_color, null);
                    b.Click -= Service_Request_Handle;
                    b.Enabled = false;
                }
            }
            if (net.tcpSockets.Count > 0 && lastpingpositiv)
            {
                ZentraleNotReachable.Hide();
            }
            else
            {
                ZentraleNotReachable.Show();
            }
            return null;
        }

        public object delegateswitchgastronotreachable(bool reachable)
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
            return null;
        }

        public object UIControlSwitcher()
        {
            for (int i = UIControl.TabCount-1; i>=0; i--)
            {
                UIControl.SelectTab(i);
                Application.DoEvents();
            }
            return null;
        }

        public object selectUIControl()
        {
            UIControl.SelectTab(0);
            return null;
        }

        public object loadscreensloser()
        {
            loadscreen.Debugtext($"Opening", true);
            loadscreen.updateProgress(100);
            loadscreen.Close();
            return null;
        }

        public void showthis()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegateshowthis), this);
        }

        private object delegateshowthis()
        {
            if(this != null && !this.IsDisposed)
            {
                this.BringToFront();
                this.Show();
            }
            return null;
        }

        public void hidethis()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegatehidethis), this);
        }

        private object delegatehidethis()
        {
            this.Hide();
            return null;
        }

        public object resizeUIControlItems()
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
            if (Logger.consoleshown)
            {
                tabs++;
            }
            if (Constants.showButtonTester)
            {
                tabs++;
            }
            UIControl.ItemSize = new Size((Constants.windowwidth - tabs) / tabs, UIControl.ItemSize.Height);
            return null;
        }

        public object removetabs()
        {
            if (!config.showtime)
            {
                UIControl.Controls.Remove(TimePage);
            }
            if (!config.showcolor)
            {
                UIControl.Controls.Remove(ColorPage);
            }
            return null;
        }

        private void SetupEmbedvlcScreen(MainForm form)
        {
            Constants.InvokeDelegate<object>([form], new MySetupEmbedvlcScreen(delegateSetupEmbedvlcScreen), form);
        }

        private object delegateSetupEmbedvlcScreen(MainForm form)
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
                    MainForm.currentState = 0;
                    Logger.Print("Different Variables for the same thing Stated different Results.\n Therefore second Monitor couldn't be initialized.", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
                }
                if (TV == null)
                {
                    //TODO
                    //Could be Timing Issue, when TV not yet Registered as  monitor
                    MainForm.currentState = 3;
                    Logger.Print("Second Monitor not found", Logger.MessageType.VideoProjektion, Logger.MessageSubType.Error);
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
            form.watingforEmbed = false;
            return null;
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
            if (clock != null)
            {
                clock.Text = $"{Hours}:{Minutes}";
                clock.Location = new Point((Constants.windowwidth / 2) - (clock.Size.Width / 2), clock.Location.Y);
            }
            if (helper.globaltimelabels != null)
            {
                foreach (Label l in helper.globaltimelabels)
                {
                    l.Text = $"{Hours}:{Minutes}";
                    l.BackColor = Color.Transparent;
                    l.Font = Constants.Time_font;
                    l.Show();
                    helper.SetEdgePosition(l, config.edgetimePosition);
                }
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
                                int x = 4 + ((config.showcolor) ? 1 : 0);
                                UIControl.SelectTab(x);
                            }
                            Content_Change(false);
                            if (vlc != null)
                            {
                                vlc.changeMedia(config.SessionEndImage, false);
                            }
                            switchedtotimepage = true;
                        }
                        TVSettingsAmbienteButton.Hide();
                        TVSettingsStreamingButton.Hide();
                        MediaPageAmbientVolumeSlider.Location = new Point(MediaPageAmbientVolumeSlider.Location.X, Constants.tabheight / 3);
                        helper.SetupLabelofTrackbar(MediaPageAmbientVolumeSlider, TVSettingsVolumeColorSliderDescribtion, config.slidernames[((int)MediaPageAmbientVolumeSlider.Tag) - 1]);
                    }
                }
                else
                {
                    double z = 2.3;
                    if (config.Wifipassword == null || config.Wifipassword.Length <= 0)
                    {
                        z = 1;
                    }
                    helper.CreateMediaControllingElemets(z);
                }
                if (Sessionlocked)
                {
                    Logger.Print($"Session Timer was locked, release it via tcp or in the restricted area. Current overtime: {timeleftnotclamped}; Locked Time: {timeleftnet}", [Logger.MessageType.Benutzeroberfläche, Logger.MessageType.Hauptprogramm], Logger.MessageSubType.Notice);

                    timeleftnotclamped = timeleftclamped = timeleftnet; 
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
                if (vlc != null && !streaming)
                {
                    vlc.changeMedia(config.DMXScenes[config.DMXSceneSetting].ContentPath, false);
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
                        if (ButtonFader.containsfadingbutton(b))
                        {
                            ButtonFader.removefadingbutton(b);
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
                ButtonFader.addcolortimedButton(((Button)sender), Constants.Buttonshortfadetime, Constants.Button_color, Numberfield_Click);
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
                if (e.TabPageIndex != UIControl.TabCount - 1 || !(e.TabPageIndex == UIControl.TabCount - 2 && Logger.consoleshown))
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
            if (Logger.consoleshown && !showconsoleonallsites)
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
            EnterFullscreen(this, mainscreen);
            ((Button)sender).Click -= Programm_Enter_Handler;
            ((Button)sender).Click += Programm_Exit_Handler;
            ((Button)sender).Text = Constants.ExitFullscreenText;
        }

        public void EnterFullscreen(CForm f, Screen screen)
        {
            Constants.InvokeDelegate<object>([f, screen], new MyFullscreen(delegateEnterFullscreen), f);
        }

        public object delegateEnterFullscreen(Form f, Screen screen)
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
            return null;
        }

        public void Ambiente_Change_Handler(object sender, EventArgs e)
        {
            Ambiente_Change(((Constants.DMXScene?)(((Button)(sender)).Tag)), false, true, false);
            //SendCurrentSceneOverCom();
        }

        public void Ambiente_Sauna_Handler(object sender, EventArgs e)
        {
            bool on = !SaunaAmbientButtons[(int)((Button)(sender)).Tag];
            SaunaAmbientButtons[(int)((Button)(sender)).Tag] = on;
            helper.selectButton(((Button)(sender)), on, Constants.selected_color);
            SendCurrentSceneOverCom();
        }

        public void Ambiente_Change(Constants.DMXScene? scene, bool force, bool user, bool keepMedia)
        {
            if (Scenelocked)
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
            //Logger.Print($"{vlc != null},{scene.ContentPath != null},{scene.ContentPath.Length > 2},{!streaming},{!vlcclosed}");
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

        public void SendCurrentSceneOverCom_Handle(object sender, EventArgs e) 
        {
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
            if (!EnttecCom.isopen())
            {
                con = Task.Run(() =>
                {
                    if (!EnttecCom.connect())
                    {
                        MainForm.currentState = 5;
                        Logger.Print("Error when trying to Communicate with Enttec Port", Logger.MessageType.Licht, Logger.MessageSubType.Error);
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
            if (!Scenelocked)
            {
                for(int i = 0; i < SaunaAmbientButtons.Length; i++)
                {
                    tempchannelvalues[Constants.SaunaChanneloffset+i] = (SaunaAmbientButtons[i]) ? (byte)255 : (byte)0;
                }
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
                    config.currentvalues[config.HDMISwitchchannel] = (streaming) ? (byte)255 : (byte)0;
                    tempchannelvalues[config.HDMISwitchchannel] = (streaming) ? (byte)255 : (byte)0;
                }
            }
            byte[] fade = new byte[tempchannelvalues.Length];
            //Logger.Print($"Time:{DateTime.Now}-{config.lastchangetime}={(DateTime.Now - config.lastchangetime).TotalMilliseconds}");
            double millis = (DateTime.Now - config.lastchangetime).TotalMilliseconds;
            if (millis <= config.FadeTime && config.currentvalues != null)
            {
                bool x = false;
                double fadesteps = (config.FadeTime - millis) / (double)Constants.sendtimeout;
                for (int i = 0; i < tempchannelvalues.Length; i++)
                {
                    if (fadesteps == 0)
                    {
                        break;
                    }
                    fade[i] = (byte)(config.currentvalues[i] + ((double)(tempchannelvalues[i] - config.currentvalues[i]) / fadesteps));
                    if(((double)(tempchannelvalues[i] - config.currentvalues[i]) != 0)){
                        x = true;
                    }
                }
                if (fadingtimer != null && !fadingtimer.Enabled)
                {
                    fadingtimer.Stop();
                    fadingtimer.Dispose();
                    fadingtimer = null;
                }
                if ((fadingtimer == null || !fadingtimer.Enabled) && x)
                {
                    fadingtimer = new System.Windows.Forms.Timer();
                    fadingtimer.Tick += SendCurrentSceneOverCom_Handle;
                    fadingtimer.Interval = Constants.sendtimeout - 1;
                    fadingtimer.Start();
                }
            }
            else
            {
                if (fadingtimer != null && fadingtimer.Enabled)
                {
                    fadingtimer.Stop();
                    fadingtimer.Dispose();
                    fadingtimer = null;
                }
                fade = tempchannelvalues;
            }
            config.currentvalues = fade;
            //Logger.Print($"Trying to send the following DMX Data:0,{fade[0]},{fade[1]},{fade[2]},{fade[3]},{fade[4]},{fade[5]},{fade[6]},{fade[7]},{fade[8]},{fade[9]},{fade[10]},{fade[11]},{fade[12]},{fade[13]},{fade[14]}");
            Task.Run(async () => {
                if (con != null)
                {
                    con.Wait();
                    con.Dispose();
                    con = null;
                }
                if (!noserial && EnttecCom.isopen())
                {
                    EnttecCom.sendDMX(fade);
                }
                else
                {
                    MainForm.currentState = 5;
                    Logger.Print("unable to open Enttec Port", Logger.MessageType.Licht, Logger.MessageSubType.Error);
                }
            });
        }

        public void Ambiente_Design_Handler(object sender, EventArgs e)
        {
            if ((int?)((Button)(sender)).Tag == null)
            {
                MainForm.currentState = 0;
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
                    MainForm.currentState = 1;
                    return false;
                }
            }
            SendCurrentSceneOverCom();
            return true;
        }

        public void Content_Change_Handler(object sender, EventArgs e)
        {
            Content_Change(!streaming);
        }

        public void Content_Change(bool SwitchToStream)
        {
            Constants.InvokeDelegate<object>([SwitchToStream], new MyContentchange(delegateContent_Change), this);
        }

        public object delegateContent_Change(bool SwitchToStream)
        {
            helper.selectButton(TVSettingsAmbienteButton, !SwitchToStream, Constants.selected_color);
            helper.selectButton(TVSettingsStreamingButton, SwitchToStream, Constants.selected_color);
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
            }
            SendCurrentSceneOverCom();
            return null;
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
            request.type = config.Typenames[1];
            request.Raum = config.Room;
            request.label = ((Constants.TCPSetting)((((Button)(sender)).Tag))).JsonText;
            request.id = ((Constants.TCPSetting)((((Button)(sender)).Tag))).id;
            //request.values = new String[] { ((Constants.SystemSetting)((Button)(sender)).Tag).value };
            if (net.SendTCPMessage(request, null))
            {
                Logger.Print($"Message sent sucessfully", Logger.MessageType.TCPSend, Logger.MessageSubType.Information);
            }
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= Wartung_Request_Handle;
            ButtonFader.addcolortimedButton(((Button)sender), Constants.ButtonLongfadetime, Constants.Button_color, Wartung_Request_Handle);
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
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= Service_Request_Handle; bool zisclient = net.tcpSockets.Count > 0 && lastpingpositiv;
            if ((zisclient || ((Constants.ServicesSetting)((Button)sender).Tag).hassecondary) && !Servicelocked)
            {
                ButtonFader.addcolortimedButton(((Button)sender), Constants.ButtonLongfadetime, Constants.Button_color, Service_Request_Handle);
            }
            Constants.ServicesSetting s = (Constants.ServicesSetting)((Button)(sender)).Tag;
            if (s != null && s.hassecondary)
            {
                Constants.ServicesSettingfunction sf = (Constants.ServicesSettingfunction)((Button)(sender)).Tag;
                if (sf.functionclass != null && sf.secondary != null && sf.functionclass.IsSubclassOf(typeof(Constants.configclasses)))
                {
                    Task.Run(async () => performsecondary(sf));
                }
                if(sf.canceling)
                {
                    return;
                }
            }
            
            if (Constants.noNet)
            {
                return;
            }
            Network.RequestJson request = new Network.RequestJson();

            request.destination = ArraytoString(config.IPZentrale, 4);
            request.port = config.PortZentrale;
            request.type = config.Typenames[3];
            request.Raum = config.Room;
            request.label = s.JsonText;
            request.id = s.id;
            if (net.SendTCPMessage(request, null))
            {
                Logger.Print($"Message sent sucessfully", Logger.MessageType.TCPSend, Logger.MessageSubType.Information);
            }
        }

        private void performsecondary(Constants.ServicesSettingfunction ssf)
        {
            Task wait = Task.Delay(ssf.delay);
            Constants.InvokeDelegate<object>([ssf], new Myperformsecondary(delegateperformsecondary), this);
        }

        private object delegateperformsecondary(Constants.ServicesSettingfunction ssf)
        {
            if (ssf == null || ssf.functionclass == null || ssf.secondary == null)
            {
                return null;
            }
            if (ssf.functionclass.Equals(new Constants.SystemSetting().GetType()))
            {
                if (ssf.enable)
                {
                    int index = -1;
                    for (int i = 0; i < config.SystemSettings.Count; i++)
                    {
                        Constants.SystemSetting ss = config.SystemSettings[i];
                        if (ss.JsonText.Trim().ToLower().Equals(ssf.JsonText.Trim().ToLower()))
                        {
                            index = i;
                            break;
                        }
                    }
                    switch (index)
                    {
                        case 0:
                            Logger.Print("Message received Working Normally. Nothing changed", Logger.MessageType.TCPReceive, Logger.MessageSubType.Information);
                            break;
                        case 1:
                            Logger.Print("Message received: Resetting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            reset();
                            break;
                        case 2:
                            Logger.Print("Message received: Restarting", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            Restart();
                            break;
                        case 3:
                            Logger.Print("Message received: Shutdown", Logger.MessageType.TCPReceive, Logger.MessageSubType.Notice);
                            Shutdown();
                            break;
                    }
                }
            }
            else if (ssf.functionclass.Equals(new Constants.TCPSetting().GetType()))
            {
                if (ssf.enable)
                {
                    if (ssf.secondary.ButtonElement != null)
                    {
                        ssf.secondary.ButtonElement.PerformClick();
                    }
                }
            }
            else if (ssf.functionclass.Equals(new Constants.SessionSetting().GetType()))
            {
                if (ssf.enable)
                {
                    TimeSessionEnd = DateTime.Now.AddMinutes(((Constants.SessionSetting)(ssf.secondary)).mins);
                    timeleftnet = ((Constants.SessionSetting)(ssf.secondary)).mins;
                    if (timeleftnet >= config.SessionEndShowTimeLeft)
                    {
                        switchedtotimepage = false;
                    }
                    if (ssf.block)
                    {
                        setsessionlocked(ssf.enable);
                    }
                }
            }
            else if (ssf.functionclass.Equals(new Constants.ServicesSetting().GetType()))
            {
                if (ssf.enable)
                {
                    if (ssf.secondary.ButtonElement != null)
                    {
                        ssf.secondary.ButtonElement.PerformClick();
                    }
                    if (ssf.block)
                    {
                        setservicelocked(ssf.enable, Constants.Warning_color);
                    }
                }
            }
            else if (ssf.functionclass.Equals(new Constants.DMXScene().GetType()))
            {
                if (ssf.enable)
                {
                    Content_Change(false);
                    Ambiente_Change(((Constants.DMXScene)(ssf.secondary)), true, true, false);
                    if (ssf.block)
                    {
                        setscenelocked(ssf.enable, Constants.scenelockedinfo, Constants.Warning_color);
                    }
                }
                else
                {
                    if (ssf.block)
                    {
                        setscenelocked(ssf.enable, Constants.scenelockedinfo, Constants.Warning_color);
                    }
                    Ambiente_Change(config.DMXScenes[0], true, true, false);
                }

            }
            else
            {
                MainForm.currentState = 7;
                Logger.Print("Json Type of secondary Service Button function could not be realted to a Json Type.", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
            }
            if (ssf.toggle)
            {
                ssf.enable = !ssf.enable;
            }
            return null;
        }

        public void temp(object sender, EventArgs e)
        {
            object hi = sender;
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
#if !DEBUG
            if (volumeinit)
            {
                defaultPlaybackDevice.Volume = Decimal.ToDouble(Value);
            }
#endif
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
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    return;
                }
                if ((int)((ColorSlider.ColorSlider)(sender)).Value == null)
                {
                    MainForm.currentState = 0;
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

        public void reset()
        {
            Constants.InvokeDelegate<object>([],new MyNoArgument(delegatereset), this);
        }

        public object delegatereset()
        {
            Logger.Print("Performing Reset", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
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
            CloseConsole(null, null);
            Logger.setCurrentlyshowing(Byte.MaxValue);
            logout();
            Config c = new Config(config);
            loadscreen.updateProgress(40);
            config = c;
            if (!Constants.noNet)
            {
                net.changeconfig(c);
            }
            SessionendNet = false;
            setsessionlocked(false);
            setservicelocked(false, Constants.Text_color);
            setscenelocked(false, "", Constants.Text_color);
            Sessionlocked = false;
            Servicelocked = false;
            Scenelocked = false;
            timeleftnet = int.MaxValue;
            TimeSessionEnd = null;
            blocknonstreamingmedia = false;
            SessionEndbool = false;
            sessionEndVLC = null;
            switchedtotimepage = false;
            Logger.consoleshown = false;
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
            Logger.Print("Reset finisched", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
            return null;
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
            if(p == null)
            {
                return false;
            }
            if (config.Wifipassword == null || config.Wifipassword.Length <= 0)
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
            QRCodeData qrCodeData = qrCodegen.CreateQrCode($"WIFI:T:WPA;S:{config.WiFiSSID};P:{config.Wifipassword};;", QRCodeGenerator.ECCLevel.H, true);
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
            ((ColorSlider.ColorSlider)(colorWheelElement.Tag)).Value = 100;
        }

        public void setscenelocked(bool x, String txt, Color c)
        {
            Constants.InvokeDelegate<object>([x,txt,c],new Mysetscenelocked(delegatesetscenelocked), this);
        }

        public object delegatesetscenelocked(bool x, String txt, Color c)
        {
            Scenelocked = x;
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
                    resetscenelockbutton.Hide();
                    l.Hide();
                    helper.SetEdgePosition(l, 3);
                    l.Location = new Point((Constants.windowwidth / 2) - (l.Size.Width / 2), l.Location.Y);
                }
            }
            foreach (Button b in helper.AmbientePageButtons)
            {
                b.Enabled = !x;
                if (x)
                {
                    ButtonFader.addcolortimedButton(b, Constants.Buttonshortfadetime, Constants.alternative_color, null);
                }
                else
                {
                    ButtonFader.addcolortimedButton(b, Constants.Buttonshortfadetime, Constants.Button_color, null);
                }
            }
            if (TVSettingsAmbienteButton != null)
            {
                TVSettingsAmbienteButton.Enabled = !x;
                if (x)
                {
                    ButtonFader.addcolortimedButton(TVSettingsAmbienteButton, Constants.Buttonshortfadetime, Constants.alternative_color, null);
                }
                else
                {
                    ButtonFader.addcolortimedButton(TVSettingsAmbienteButton, Constants.Buttonshortfadetime, Constants.Button_color, null);
                }
            }
            if (TVSettingsStreamingButton != null)
            {
                TVSettingsStreamingButton.Enabled = !x;
                if (x)
                {
                    ButtonFader.addcolortimedButton(TVSettingsStreamingButton, Constants.Buttonshortfadetime, Constants.alternative_color, null);
                }
                else
                {
                    ButtonFader.addcolortimedButton(TVSettingsStreamingButton, Constants.Buttonshortfadetime, Constants.Button_color, null);
                }
            }
            if (resetcolorbutton != null)
            {
                resetcolorbutton.Enabled = !x;
                if (x)
                {
                    ButtonFader.addcolortimedButton(resetcolorbutton, Constants.Buttonshortfadetime, Constants.alternative_color, null);
                }
                else
                {
                    ButtonFader.addcolortimedButton(resetcolorbutton, Constants.Buttonshortfadetime, Constants.Button_color, null);
                }
            }
            if (Dimmer1ColorSlider != null)
            {
                Dimmer1ColorSlider.Enabled = !x;
            }
            if (Dimmer2ColorSlider != null)
            {
                Dimmer2ColorSlider.Enabled = !x;
            }
            if (colorWheelElement != null)
            {
                colorWheelElement.Enabled = !x;
            }
            if (((ColorSlider.ColorSlider)colorWheelElement.Tag) != null)
            {
                ((ColorSlider.ColorSlider)colorWheelElement.Tag).Enabled = !x;
            }
            return null;
        }

        public void setservicelocked(bool x,Color c)
        {
            Constants.InvokeDelegate<object>([x,c], new Mysetservicelocked(delegatesetservicelocked), this);
        }

        public object delegatesetservicelocked(bool x, Color c)
        {
            Servicelocked = x;
            Servicelockedlabel.ForeColor = c;
            if (x)
            {
                Servicelockedlabel.Show();
                helper.SetEdgePosition(Servicelockedlabel, 3);
                Servicelockedlabel.Location = new Point((Constants.windowwidth / 2) - (Servicelockedlabel.Size.Width / 2), Servicelockedlabel.Location.Y);
                Servicelockedlabel.BringToFront();
                resetServicelockbutton.Show();
            }
            else
            {
                resetServicelockbutton.Hide();
                Servicelockedlabel.Hide();
                helper.SetEdgePosition(Servicelockedlabel, 3);
                Servicelockedlabel.Location = new Point((Constants.windowwidth / 2) - (Servicelockedlabel.Size.Width / 2), Servicelockedlabel.Location.Y);
            }
            delegateswitchzentralenotreachable();
            return null;
        }

        public void setsessionlocked(bool x)
        {
            Constants.InvokeDelegate<object>([x], new Mysetsessionlocked(delegatesetsessionlocked), this);
        }

        public object delegatesetsessionlocked(bool x)
        {
            Sessionlocked = x;
            if (x)
            {
                resetSessionlockbutton.Show();
            }
            else
            {
                resetSessionlockbutton.Hide();
            }
            return null;
        }

        public void resetscenelock_Handler(object sender, EventArgs e)
        {
            resetscenelock();
        }

        public void resetscenelock()
        {
            setscenelocked(false, "", Constants.Text_color);
        }

        public void resetServicelock_Handler(object sender, EventArgs e)
        {
            resetServicelock();
        }

        public void resetServicelock()
        {
            setservicelocked(false, Constants.Text_color);
        }

        public void resettimelock_Handler(object sender, EventArgs e)
        {
            resettimelock();
        }

        public void resettimelock()
        {
            setsessionlocked(false);
        }

        public void SessionEnded(EmbedVLC evlc, bool fromevent)
        {
            sessionEndVLC = evlc;
            SessionEndbool = true;
            if (fromevent)
            {
                UIControl.SelectTab(UIControl.TabCount - 1);
            }
            Scenelocked = false;
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
            Logger.setCurrentlyshowing(Byte.MaxValue);
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
            if (Logger.getCurrentlyshowing() >= Byte.MaxValue)
            {
                return;
            }
            helper.removeConsolePage();
            if(sender == null)
            {
                return;
            }
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
                if (vlc != null && Logger.ConsoleBox != null)
                {
                    bool scroll = true;
                    if (Logger.ConsoleBox.SelectionStart == Logger.ConsoleBox.Text.Length)
                    {
                        scroll = false;
                    }
                    Logger.ConsoleBox.Text += line;
                    Logger.ConsoleBox.Text += "\n\r";
                    if (scroll)
                    {
                        Logger.ConsoleBox.SelectionStart = Logger.ConsoleBox.Text.Length;
                        Logger.ConsoleTextscroll.Value = Logger.ConsoleBox.SelectionStart * -1 + Logger.ConsoleTextscroll.Maximum;
                        Logger.ConsoleBox.ScrollToCaret();
                    }
                    if (TextRenderer.MeasureText(Logger.ConsoleBox.Text, Logger.ConsoleBox.Font).Height > Logger.ConsoleBox.Size.Height)
                    {
                        Logger.ConsoleTextscroll.Maximum = Logger.ConsoleBox.Text.Length;
                        Logger.ConsoleTextscroll.Show();
                    }
                    else
                    {
                        Logger.ConsoleTextscroll.Hide();
                    }
                    return Logger.ConsoleBox.Text;
                }
                return line;
            }
            return "";
        }

        public void SetConsoleText(String Text)
        {
            if (Text != null && Text.Length > 0)
            {
                if (Logger.ConsoleBox != null)
                {
                    Logger.ConsoleBox.Clear();
                    Logger.ConsoleBox.Text = Text;
                    Logger.ConsoleBox.SelectionStart = Logger.ConsoleBox.Text.Length;
                    Logger.ConsoleBox.ScrollToCaret();
                    if (TextRenderer.MeasureText(Logger.ConsoleBox.Text, Logger.ConsoleBox.Font).Height > Logger.ConsoleBox.Size.Height)
                    {
                        Logger.ConsoleTextscroll.Maximum = Logger.ConsoleBox.Text.Length;
                        Logger.ConsoleTextscroll.Value = Logger.ConsoleBox.SelectionStart * -1 + Logger.ConsoleTextscroll.Maximum;
                        Logger.ConsoleTextscroll.Show();
                    }
                    else
                    {
#if DEBUG
                        Logger.ConsoleTextscroll.Show();
#else
                        Logger.ConsoleTextscroll.Hide();
#endif
                    }
                }
            }
        }

        public void ClearConsole()
        {
            if(vlc != null && Logger.ConsoleBox != null)
            {
                Logger.ConsoleBox.Text = "";
                Logger.ConsoleBox.Clear();
            }
        }

        public void comboconsoleItemchanged(object sender, EventArgs e)
        {
            if(Logger.consoletype == null)
            {
                return;
            }
            int? Index = 0;
            if ((Constants.ComboItem)Logger.consoletype.SelectedItem == null)
            {
                return;
            }
            else
            {
                Index = ((Constants.ComboItem)Logger.consoletype.SelectedItem).ID;
            }
            int? SubIndex = null;
            if (Logger.consolesubtype == null)
            {
                return;
            }
            else
            {
                if((Constants.ComboItem)Logger.consolesubtype.SelectedItem == null)
                {
                    SubIndex = null;
                }
                else
                {
                    SubIndex = ((Constants.ComboItem)Logger.consolesubtype.SelectedItem).ID;
                }
            }
            ClearConsole();
            if(Logger.consoletype != null)
            {
                if(SubIndex != null)
                {
                    SetConsoleText(Logger.GetConsoleText((Logger.MessageType)Index, (Logger.MessageSubType)SubIndex));
                }
                else
                {
                    SetConsoleText(Logger.GetConsoleText((Logger.MessageType)Index));
                }
            }
            
            Logger.setCurrentlyshowing((byte)Index);
        }

        public void sendTCPfromconsole(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Constants.alternative_color;
            ((Button)sender).Click -= sendTCPfromconsole;
            ButtonFader.addcolortimedButton(((Button)sender), Constants.Buttonshortfadetime, Constants.Button_color, sendTCPfromconsole);
            net.Messageafterparse(CreateMessagString());
        }

        public void consolescroll(object sender, EventArgs e)
        {
            Logger.ConsoleBox.SelectionStart = (((int)((ColorSlider.ColorSlider)(sender)).Value) - (int)((ColorSlider.ColorSlider)(sender)).Maximum) * -1;
            Logger.ConsoleBox.ScrollToCaret();
        }

        public void TCPMessage_Change_handler(object sender, EventArgs e)
        {
            if (Messagepreview == null || Messagepreview.Tag == null)
            {
                return;
            }
            int posx, posy = 0;
            helper.GetDynamicPosition(4, ((Point)Messagepreview.Tag).X, out posx, out posy, 0, ((Point)Messagepreview.Tag).Y, false);
            Messagepreview.Text = CreateMessagString();
            Messagepreview.Location = new Point((posx + Constants.Element_width / 2) - (Messagepreview.Size.Width / 2), posy);
        }

        public String CreateMessagString()
        {
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
            if(tcptype.SelectedIndex<0 || tcptype.SelectedIndex >= config.Typenames.Length)
            {
                return "Select Type";
            }
            p += config.Typenames[tcptype.SelectedIndex];
            p += '"';
            if (CommandboxLabel.Text.Length > 0)
            {
                p += ',';
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
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                }
            }
            if (id >= 0)
            {
                p += ',';
                p += '"';
                p += "id";
                p += '"';
                p += ':';
                p += id;
            }
            if (Commandboxvalues.Text.Length > 0)
            {
                p += ',';
                p += '"';
                p += "values";
                p += '"';
                p += ":[";
                foreach (String s in Commandboxvalues.Text.Split(','))
                {
                    int parse = 0;
                    if (!s.Contains('"') && !Int32.TryParse(s, out parse))
                    {
                        p+= '"';
                    }
                    p += s.Trim();
                    if (!s.Contains('"') && !Int32.TryParse(s, out parse))
                    {
                        p += '"';
                    }
                    p += ',';
                }
                if (p.EndsWith(','))
                {
                    p = p.Substring(0, p.Length - 1);
                }
                p += ']';
            }
            p += '}';
            return p;
        }

        public void CommandId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(e.KeyChar == 8 || (e.KeyChar >= 48 && e.KeyChar <= 57) || e.KeyChar == 46))
            {
                e.Handled = true;
            }

        }

    }
}
