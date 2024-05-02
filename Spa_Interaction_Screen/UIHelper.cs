﻿
using QRCoder;
using Spa_Interaction_Screen;
using System.Diagnostics;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using System.Management;

namespace Spa_Interaction_Screen
{
    public class UIHelper
    {
        private Logger Log = null;
        private MainForm form = null;
        private Config config = null;

        public List<TabPage> tabs = new List<TabPage>();

        public List<ColorSlider.ColorSlider> FormColorSlides = new List<ColorSlider.ColorSlider>();
        public List<PictureBox> globalLogos = new List<PictureBox>();
        public List<Label> globaltimelabels = new List<Label>();
        public List<Label> globalinformationlabels = new List<Label>();
        public List<Control> AllElements = new List<Control>();

        public List<Button> AmbientePageButtons = new List<Button>();
        public List<Button> AmbientePagedynamicButtons = new List<Button>();
        public List<Label> AmbientePageLabel = new List<Label>();
        public List<ColorSlider.ColorSlider> AmbientePageColorSlide = new List<ColorSlider.ColorSlider>();

        public List<Button> MediaPageButtons = new List<Button>();
        public List<Label> MediaPageLabel = new List<Label>();

        public List<Button> ServicePageButtons = new List<Button>();

        public List<Button> WartungPageButtons = new List<Button>();
        public List<Label> WartungPageLabel = new List<Label>();

        public List<Button> RestrictedPageButtons = new List<Button>();

        public List<Control> ConsoleElements = new List<Control>();

        private const string USB = "USB";
        private bool checkforUSBKeyboard = true;
        private const double Restrictedyoffset = 0.5;
        private int Restrictedycoord = (Constants.Element_height + Constants.Element_y_padding) + (int)((Constants.Element_height + Constants.Element_y_padding) * Restrictedyoffset);
        private int Restrictedstarty = (Constants.Element_height + Constants.Element_y_padding);

        public UIHelper(MainForm f, Config c)
        {
            Log = f.Log;
            form = f;

            form.BackColor = Constants.alternative_color;
            form.ForeColor = Constants.alternative_color;

            form.GastronomieWebview.BackColor = Constants.Background_color;

            form.UIControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            form.UIControl.DrawItem += form.tabControl_DrawItem;

            SetupElementsLists();

#if DEBUG
            this.createdebugUI();
#endif
            if (c == null)
            {
                return;
            }
            config = c;
        }

        public void init()
        {
            createAmbientePageElements();

            createMediaPageElements();

            createServicePageElements();

            createGastroPageElements();

            createRestrictedPageElements();
            removeRestrictedPageElements();

            createWartungPageElements();
        }



        private ColorSlider.ColorSlider createColorSlide(int max)
        {
            ColorSlider.ColorSlider slide = new ColorSlider.ColorSlider();
            slide.BackColor = Constants.Background_color;
            slide.ElapsedInnerColor = Color.Green;
            slide.ElapsedPenColorBottom = Color.Green;
            slide.ElapsedPenColorTop = Color.Green;

            slide.BarPenColorBottom = Color.White;
            slide.BarPenColorTop = Color.White;
            slide.BarInnerColor = Color.White;

            slide.ThumbInnerColor = Constants.Button_color;
            slide.ThumbOuterColor = Color.White;
            slide.ThumbPenColor = Constants.Button_color;

            slide.TickColor = Constants.Text_color;

            slide.BorderRoundRectSize = new Size(8, 8);
            slide.Maximum = new decimal(max);
            slide.Minimum = new decimal(0);
            slide.Orientation = Orientation.Horizontal;
            slide.ScaleDivisions = new decimal(1);
            slide.ScaleSubDivisions = new decimal(5);
            slide.ShowDivisionsText = false;
            slide.ShowSmallScale = false;
            slide.LargeChange = new decimal(4.1701);
            slide.SmallChange = new decimal(4.1701);
            slide.TabIndex = 2;
            slide.ThumbRoundRectSize = new Size(22, 22);
            slide.ThumbSize = new Size(30, 30);
            slide.TickAdd = 0F;
            slide.TickDivide = 0F;
            slide.TickStyle = TickStyle.BottomRight;
            slide.Value = new decimal(0);
            slide.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            slide.ForeColor = Constants.Text_color;
            slide.ShowDivisionsText = true;
            slide.ShowSmallScale = true;
            return slide;
        }

        private void SetupElementsLists()
        {
            //Form Lists
            {
                tabs = new List<TabPage>();
                tabs.Add(form.AmbientePage);
                tabs.Add(form.ColorPage);
                tabs.Add(form.GastronomiePage);
                tabs.Add(form.ServicePage);
                tabs.Add(form.MediaPage);
                tabs.Add(form.TimePage);
                tabs.Add(form.WartungPage);
                FormColorSlides = new List<ColorSlider.ColorSlider>();
                globaltimelabels = new List<Label>();
                globalLogos = new List<PictureBox>();
                globalinformationlabels = new List<Label>();
                AllElements = new List<Control>();
            }

            //Ambiente Lists
            {
                AmbientePageButtons = new List<Button>();
                AmbientePagedynamicButtons = new List<Button>();

                AmbientePageLabel = new List<Label>();
                AmbientePageLabel.Add((Label)form.Dimmer1ColorSliderDescribtion);
                AmbientePageLabel.Add((Label)form.Dimmer2ColorSliderDescribtion);
                AmbientePageLabel.Add((Label)form.AmbientelautstärkeColorSliderDescribtion);
            }

            //Media
            {
                MediaPageButtons = new List<Button>();

                MediaPageLabel = new List<Label>();
                MediaPageLabel.Add((Label)form.WiFiSSIDTitle);
                MediaPageLabel.Add((Label)form.WiFiSSIDLabel);
                MediaPageLabel.Add((Label)form.WiFiPasswordTitle);
                MediaPageLabel.Add((Label)form.WiFiPasswortLabel);
                MediaPageLabel.Add((Label)form.TVSettingsTitle);
                MediaPageLabel.Add((Label)form.TVSettingsVolumeColorSliderDescribtion);
            }

            //Service
            {
                ServicePageButtons = new List<Button>();
            }

            //Wartung
            {
                WartungPageButtons = new List<Button>();

                WartungPageLabel = new List<Label>();
                WartungPageLabel.Add((Label)form.WartungCodeField);
                WartungPageLabel.Add((Label)form.RestrictedAreaDescribtion);
            }

            //Restricted
            {
                RestrictedPageButtons = new List<Button>();
            }

            //Console
            {
                ConsoleElements = new List<Control>();
            }
        }

        public void createAmbientePageElements()
        {
            int Pos_x, Pos_y;

            GetDynamicPosition(3, 1, out Pos_x, out Pos_y, 0, 2, false);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, config.Objectname, (int)0, form.AmbientePage, form, form.Ambiente_Design_Handler);

            ColorSlider.ColorSlider newslider = null;
            GetDynamicPosition(3, 0, out Pos_x, out Pos_y, 0, 2, false);
            newslider = createColorSlide(100);
            form.AmbientePage.Controls.Add(newslider);
            newslider.Size = new Size(Constants.Element_width, Constants.Element_height);
            newslider.Location = new Point(Pos_x, Pos_y);
            newslider.Tag = 1;
            newslider.Name = "0";
            newslider.TabIndex = AmbientePageButtons.Count + 1;
            //newslider.Value = config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.Dimmerchannel[0]];
            FormColorSlides.Add(newslider);
            form.Dimmer1ColorSliderDescribtion = new Label();
            form.Dimmer1ColorSliderDescribtion.AutoSize = true;
            form.Dimmer1ColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.AmbientePage.Controls.Add(form.Dimmer1ColorSliderDescribtion);
            newslider.ValueChanged += form.Dimmer_Change;

            GetDynamicPosition(3, 2, out Pos_x, out Pos_y, 0, 2, false);
            newslider = createColorSlide(100);
            form.AmbientePage.Controls.Add(newslider);
            newslider.Size = new Size(Constants.Element_width, Constants.Element_height);
            newslider.Location = new Point(Pos_x, Pos_y);
            newslider.Tag = 2;
            newslider.Name = "1";
            newslider.TabIndex = AmbientePageButtons.Count + 1;
            //newslider.Value = config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.Dimmerchannel[1]];
            FormColorSlides.Add(newslider);
            form.Dimmer2ColorSliderDescribtion = new Label();
            form.Dimmer2ColorSliderDescribtion.AutoSize = true;
            form.Dimmer2ColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.AmbientePage.Controls.Add(form.Dimmer2ColorSliderDescribtion);
            newslider.ValueChanged += form.Dimmer_Change;

            GetDynamicPosition(2, 0, out Pos_x, out Pos_y, 0, 3, false);
            newslider = createColorSlide(100);
            form.AmbientePage.Controls.Add(newslider);
            newslider.Size = new Size(Constants.Element_width * 2, Constants.Element_height);
            newslider.Location = new Point(Pos_x, Pos_y);
            newslider.TabIndex = AmbientePageButtons.Count + 1;
            newslider.Tag = 3;
            newslider.ValueChanged += form.AmbientVolume_Handler;

            /*if (config.Volume >= newslider.Minimum && config.Volume <= newslider.Maximum)
            {
                newslider.Value = config.Volume;
            }*/

            // 
            // AmbientelautstärkeColorSliderDescribtion
            // 

            form.AmbientelautstärkeColorSliderDescribtion = new Label();
            form.AmbientelautstärkeColorSliderDescribtion.AutoSize = true;
            form.AmbientelautstärkeColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.AmbientePage.Controls.Add(form.AmbientelautstärkeColorSliderDescribtion);
            FormColorSlides.Add(newslider);
        }

        public void createColorPageElements()
        {
            int Posx, Posy;
            GetDynamicPosition(1, 0, out Posx, out Posy, 0, 0, false);
            Label label = new Label();
            label.AutoSize = true;
            label.ForeColor = Constants.Text_color;
            label.Text = "Ambientebeleuchtungsfarbe";
            form.ColorPage.Controls.Add(label);
            label.Location = new Point((Constants.windowwidth / 2) - (label.Size.Width / 2), Posy - 15);
            ColorSlider.ColorSlider newslider = null;
            newslider = createColorSlide(100);

            form.colorWheelElement = new Cyotek.Windows.Forms.ColorWheel();
            form.colorWheelElement.Size = new Size((int)(Constants.windowwidth / 3), (int)(Constants.windowwidth / 3));
            form.ColorPage.Controls.Add(form.colorWheelElement);
            form.colorWheelElement.Location = new Point((Constants.windowwidth / 2) - (form.colorWheelElement.Size.Width / 2), ((Constants.tabheight - (label.Location.Y + label.Size.Height)) / 2) - (form.colorWheelElement.Size.Height / 2) + (label.Location.Y + label.Size.Height) - 15);
            form.colorWheelElement.ColorChanged += form.ColorChanged_Handler;
            form.colorWheelElement.Tag = (ColorSlider.ColorSlider)newslider;
            form.ColorPage.BackColor = Constants.Background_color;


            newslider.Size = new Size(Constants.Element_width * 2, (int)(form.colorWheelElement.Size.Height * 0.75));
            newslider.Location = new Point(form.colorWheelElement.Size.Width + form.colorWheelElement.Location.X, (Constants.tabheight / 2) - (newslider.Size.Height / 2));
            newslider.Orientation = Orientation.Vertical;
            newslider.Value = 100;
            form.ColorPage.Controls.Add(newslider);
            newslider.ValueChanged += form.ColorChanged_Handler;

            GetDynamicPosition(5, 0, out Posx, out Posy, 0, 0, false);
            Constants.createButton<Button>(Constants.Element_width, Constants.Element_height, Posx, (Constants.tabheight / 2) - (Constants.Element_height / 2), null, "Zurrücksetzen", null, form.ColorPage, form, form.resetcolorwheel);
        }

        public void createGastroPageElements()
        {

            Constants.createButton<Button>((int)(Constants.windowwidth / 2) - (Constants.Element_width / 2), (Constants.tabheight / 2) - (Constants.Element_height / 2), null, config.ServicesSettings[1].ShowText, (Constants.ServicesSetting)config.ServicesSettings[1], form.GastronomiePage, form, form.Service_Request_Handle, out form.GastroEx);
            form.GastroEx.Hide();

            form.GastroExDescription = new Label();
            form.GastroExDescription.AutoSize = true;
            form.GastroExDescription.ForeColor = Constants.Text_color;
            form.GastroExDescription.Text = "Leider ist die Bestellseite zur Zeit nicht verfügbar.\nBitte melden Sie sich bei dem Service Personal,\nfalls Sie etwas bestellen möchten.";
            form.GastronomiePage.Controls.Add(form.GastroExDescription);
            form.GastroExDescription.Location = new Point((Constants.windowwidth / 2) - (form.GastroExDescription.Size.Width / 2), (int)(Constants.tabheight / 4) - (form.GastroExDescription.Height / 2));
            form.GastroExDescription.Hide();
        }

        public void createMediaPageElements()
        {
            int Pos_x, Pos_y = 0;

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0.5, 1, false);
            form.WiFiQRCodePicturebox.Location = new Point(Pos_x, Pos_y);

            form.WiFiSSIDTitle.Text = "Wlan Name:";
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 1.5, false);
            form.WiFiSSIDTitle.Location = new Point(Pos_x, Pos_y);

            form.WiFiPasswordTitle.Text = "Wlan Passwort:";
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 2.5, false);
            form.WiFiPasswordTitle.Location = new Point(Pos_x, Pos_y);

            CreateMediaControllingElemets(5);
        }

        public void CreateMediaControllingElemets(double widthelements)
        {
            int Elementsinwidth = (int)widthelements;
            widthelements -= Elementsinwidth;
            if (form.TVSettingsAmbienteButton != null)
            {
                Button but = form.TVSettingsAmbienteButton;
                if(but.Parent != null)
                {
                    but.Parent.Controls.Remove(but);
                }
                but.Hide();
                form.TVSettingsAmbienteButton = null;
                MediaPageButtons.Remove(but);
                but.Dispose();
            }
            if (form.TVSettingsStreamingButton != null)
            {
                Button but = form.TVSettingsStreamingButton;
                if(but.Parent != null)
                {
                    but.Parent.Controls.Remove(but);
                }
                but.Hide();
                form.TVSettingsStreamingButton = null;
                MediaPageButtons.Remove(but);
                but.Dispose();
            }
            if (form.MediaPageAmbientVolumeSlider != null)
            {
                ColorSlider.ColorSlider but = form.MediaPageAmbientVolumeSlider;
                if (but.Parent != null)
                {
                    but.Parent.Controls.Remove(but);
                }
                but.Hide();
                form.MediaPageAmbientVolumeSlider = null;
                FormColorSlides.Remove(but);
                but.Dispose();
            }
            if (form.TVSettingsVolumeColorSliderDescribtion != null)
            {
                Label but = form.TVSettingsVolumeColorSliderDescribtion;
                if (but.Parent != null)
                {
                    but.Parent.Controls.Remove(but);
                }
                but.Hide();
                form.TVSettingsVolumeColorSliderDescribtion = null;
                but.Dispose();
            }
            int Pos_x, Pos_y = 0;
            int sizex = Constants.Element_width * 2; 
            int sizey = (int)(Constants.Element_height * 0.75);
            GetDynamicPositionsize(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 0.8, false, sizex, sizey);
            form.TVSettingsAmbienteButton = Constants.createButton(sizex, sizey, Pos_x, Pos_y, MediaPageButtons, "Ambiente Video", true, form.MediaPage, form, form.Content_Change_Handler);
            selectButton(form.TVSettingsAmbienteButton, true, Constants.selected_color);
            form.TVSettingsAmbienteButton.Name = "AmbientVideo";

            GetDynamicPositionsize(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 1.8, false, sizex, sizey);
            form.TVSettingsStreamingButton = Constants.createButton(sizex, sizey, Pos_x, Pos_y, MediaPageButtons, "Streaming Video", false, form.MediaPage, form, form.Content_Change_Handler);
            form.TVSettingsStreamingButton.Name = "StreamingVideo";

            SetupLabelofButton(form.TVSettingsAmbienteButton, form.TVSettingsTitle, "Video Einstellungen:");

            form.MediaPageAmbientVolumeSlider = null;
            GetDynamicPositionsize(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 2.8, false, sizex, sizey);
            form.MediaPageAmbientVolumeSlider = createColorSlide(100);
            form.MediaPageAmbientVolumeSlider.Location = new Point(Pos_x, Pos_y);
            form.MediaPageAmbientVolumeSlider.Size = new Size(sizex, sizey);
            form.MediaPageAmbientVolumeSlider.Tag = 3;
            form.MediaPage.Controls.Add(form.MediaPageAmbientVolumeSlider);
            FormColorSlides.Add(form.MediaPageAmbientVolumeSlider);
            form.MediaPageAmbientVolumeSlider.ValueChanged += form.AmbientVolume_Handler;
            /*if (config.Volume >= newslider.Minimum && config.Volume <= newslider.Maximum)
            {
                newslider.Value = config.Volume;
            }*/

            form.TVSettingsVolumeColorSliderDescribtion = new Label();
            form.TVSettingsVolumeColorSliderDescribtion.AutoSize = true;
            form.TVSettingsVolumeColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.MediaPage.Controls.Add(form.TVSettingsVolumeColorSliderDescribtion);
        }

        public void createTimePageElements()
        {
            int Pos_x, Pos_y = 0;

            form.TimePage.BackColor = Constants.Background_color;

            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 0, true);
            form.clock = new Label();
            form.clock.AutoSize = true;
            form.clock.Font = new Font("Segoe UI", 120F, FontStyle.Regular, GraphicsUnit.Point, 0);
            form.clock.ForeColor = Constants.Text_color;
            form.clock.Name = "Clock";
            form.clock.TabIndex = 1;
            form.clock.BackColor = Color.Transparent;
            //form.clock.Text = "00:00";
            form.TimePage.Controls.Add(form.clock);
            form.clock.Location = new Point((Constants.windowwidth / 2) - (form.clock.Size.Width / 2), Pos_y);
            /*
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 2, true);
            Label label = new Label();
            label.AutoSize = true;
            label.Font = new Font("Segoe UI", 40F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label.ForeColor = SystemColors.ControlLightLight;
            label.Name = "TimerDescribtion";
            //label.Text = "Verbleibende Zeit:";
            label.TabIndex = 2;
            label.BackColor = Color.Transparent;
            form.TimePage.Controls.Add(label);
            label.Location = new Point((int)((Constants.windowwidth / 2) - (label.Size.Width/2)), Pos_y);
            */
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0.5, 2, true);
            form.timer = new Label();
            form.timer.AutoSize = true;
            form.timer.Font = Constants.Standart_font;
            form.timer.ForeColor = Constants.Text_color;
            form.timer.Name = "Timer";
            //form.clock.Text = "00";
            form.timer.TabIndex = 3;
            form.timer.BackColor = Color.Transparent;
            form.TimePage.Controls.Add(form.timer);
            form.timer.Location = new Point((Constants.windowwidth / 2) - (form.timer.Size.Width / 2), Pos_y);
            /*
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0.5, 4.5, true);
            form.sessionEnd = new Label();
            form.sessionEnd.AutoSize = true;
            form.sessionEnd.Font = new Font("Segoe UI", 25F, FontStyle.Italic, GraphicsUnit.Point, 0);
            form.sessionEnd.ForeColor = SystemColors.ControlLightLight;
            form.sessionEnd.Name = "TimerEnd";
            form.sessionEnd.TabIndex = 4;
            form.sessionEnd.BackColor = Color.Transparent;
            form.clock.Text = "00:00";
            form.TimePage.Controls.Add(form.sessionEnd);
            form.sessionEnd.Location = new Point((Constants.windowwidth / 2) - (form.sessionEnd.Size.Width / 2), Pos_y);
            */
        }

        public void createServicePageElements()
        {
            int Pos_x, Pos_y = 0;
            //form.HowCanIHelpYouDescribtion.Text = config.ServiceStrings[0][0];
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 3, false);
            form.HowCanIHelpYouDescribtion.Location = new Point(Pos_x, Pos_y - Constants.Element_y_padding - form.HowCanIHelpYouDescribtion.Size.Height);
        }

        public void createWartungPageElements()
        {
            int Pos_x, Pos_y = 0;
            /*
            form.WartungCodeField.Text = "Pin eingeben:";
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 0, false);
            form.WartungCodeField.Location = new Point(Constants.windowwidth / 2 - form.WartungCodeField.Size.Width / 2, Pos_y-15);
            form.WartungCodeField.Show();
            */
            form.RestrictedAreaDescribtion.Text = config.RestrictedDescription;
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, -0.1, false);
            form.RestrictedAreaDescribtion.Location = new Point(Constants.windowwidth / 2 - form.RestrictedAreaTitle.Size.Width / 2, Pos_y);
            form.RestrictedAreaDescribtion.Show();

            int pad_padding = ((int)1.2 * Constants.Element_y_padding);

            int startx = Constants.windowwidth / 2 - ((3 * Constants.Element_height + 3 * pad_padding) / 2);
            int starty = Constants.windowheight / 2 - ((4 * Constants.Element_height + 4 * pad_padding) / 2);

            for (int i = 0; i < 3; i++)
            {
                for (int e = 0; e < 3; e++)
                {
                    Constants.createButton(Constants.Element_height, Constants.Element_height, startx + (e * (Constants.Element_height + pad_padding)), starty + (i * (Constants.Element_height + pad_padding)), WartungPageButtons, $"{3 * i + e + 1}", 3 * i + e + 1, form.WartungPage, form, form.Numberfield_Click);
                }
            }
            Constants.createButton(Constants.Element_height, Constants.Element_height, startx + (1 * (Constants.Element_height + pad_padding)), starty + (3 * (Constants.Element_height + pad_padding)), WartungPageButtons, $"{0}", 0, form.WartungPage, form, form.Numberfield_Click);
#if DEBUG
            Constants.createButton(Constants.Element_width, Constants.Element_height,0, Constants.Element_height, WartungPageButtons, "Login", "Login", form.WartungPage, form, form.Login);
#endif
        }

        public void removeWartungPageElements()
        {
            form.WartungCodeField.Hide();
            form.RestrictedAreaDescribtion.Hide();

            while (WartungPageButtons.Count > 0)
            {
                Button rem = WartungPageButtons[0];
                rem.Tag = -1;
                rem.Hide();
                form.WartungPage.Controls.Remove(rem);
                WartungPageButtons.Remove(rem);
            }
        }

        public void createRestrictedPageElements()
        {
            int Pos_x, Pos_y = 0;
            form.RestrictedAreaTitle.Text = "Zugriff nur für Mitarbeiter";
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, -0.1, false);
            form.RestrictedAreaTitle.Location = new Point(Constants.windowwidth / 2 - form.RestrictedAreaTitle.Size.Width / 2, Pos_y);
            form.RestrictedAreaTitle.Show();

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Szenen Auswahl freischlten", null, form.WartungPage, form, form.resetscenelock_Handler, out form.resetscenelockbutton);
            if (form.scenelocked)
            {
                form.resetscenelockbutton.Show();
            }
            else
            {
                form.resetscenelockbutton.Hide();
            }

            int wartungs = Math.Min(config.TCPSettings.Count, Constants.maxtcpws);
            for (int i = 0; i < wartungs; i++)
            {
                GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
                Button but = null;
                Constants.createButton(Pos_x, Restrictedstarty + i * Restrictedycoord, RestrictedPageButtons, config.TCPSettings[i].ShowText, config.TCPSettings[i], form.WartungPage, form, form.Wartung_Request_Handle, out but);
                config.TCPSettings[i].ButtonElement = but;
            }

            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "Starte eine neue Session", "SessionStart", form.WartungPage, form, form.NewSession_Handler);

            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Beende die aktuelle Session", "SessionEnd", form.WartungPage, form, form.EndSession_Handler);

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, Constants.ExitFullscreenText, "ToggleFullscreen", form.WartungPage, form, form.Programm_Exit_Handler);

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Programm zurücksetzen", "Reset", form.WartungPage, form, form.reset_Handler);
            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            if (form.vlcclosed)
            {
                Constants.createButton(Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Öffne den Player", "VLCClose", form.WartungPage, form, form.OpenPlayer_Handler);
            }
            else
            {
                Constants.createButton(Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Schließe den Player", "VLCClose", form.WartungPage, form, form.closePlayer_Handler);
            }

            GetDynamicPosition(5, 4, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "Ausloggen", "Logout", form.WartungPage, form, form.logoutbutton_Handler);
            GetDynamicPosition(5, 4, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Button bu = null;
            if (form.showconsoleonallsites)
            {
                Constants.createButton(Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Zeige Konsole nicht immer", "ConsoleEverywhere", form.WartungPage, form, form.ShowConsoleNotOnallSites, out bu);
            }
            else
            {
                Constants.createButton(Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Zeige Konsole immer", "ConsoleEverywhere", form.WartungPage, form, form.ShowConsoleOnallSites, out bu);
            }
            if (form.consoleshown)
            {
                Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Schließe Konsole", bu, form.WartungPage, form, form.CloseConsole);
            }
            else
            {
                Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Öffne Konsole", bu, form.WartungPage, form, form.ShowConsole);
                bu.Hide();
            }
            setConfigRestricted(config);
        }

        public void removeRestrictedPageElements()
        {
            form.RestrictedAreaTitle.Hide();

            while (RestrictedPageButtons.Count > 0)
            {
                Button rem = RestrictedPageButtons[0];
                rem.Tag = -1;
                rem.Hide();
                form.WartungPage.Controls.Remove(rem);
                RestrictedPageButtons.Remove(rem);
            }
        }

        private void createdebugUI()
        {
            foreach (TabPage control in form.UIControl.TabPages)
            {
                Constants.createButton<Button>(0, 0, null, Constants.ExitFullscreenText, "DebugExitButton", control, form, form.Programm_Exit_Handler);
            }
        }

        public void GetDynamicPosition(int TotalButtonCount, int CurrentButtonIndex, out int pos_X, out int pos_Y, double widthOffsetinButtons, double heightOffsetinButtons, bool useDoubleRow)
        {
            GetDynamicPositionsize(TotalButtonCount, CurrentButtonIndex, out pos_X,out pos_Y,widthOffsetinButtons, heightOffsetinButtons, useDoubleRow, Constants.Element_width, Constants.Element_height);
        }

        public void GetDynamicPositionsize(int TotalButtonCount, int CurrentButtonIndex, out int pos_X, out int pos_Y, double widthOffsetinButtons, double heightOffsetinButtons, bool useDoubleRow, int width, int height)
        {
            int xpadding = (int)((double)Constants.windowwidth / (Constants.XButtonCount + 1)) - Constants.Element_width;
            int ypadding = (int)((double)Constants.tabheight / (Constants.YButtonCount + 1)) - Constants.Element_height;
            int mod1, mod2 = pos_X = 0;

            if (TotalButtonCount > Constants.InlineUntilXButtons)
            {

                mod1 = (TotalButtonCount % 2 == 0) ? TotalButtonCount / 2 : (CurrentButtonIndex >= TotalButtonCount / 2 + 1) ? TotalButtonCount / 2 + 1 : TotalButtonCount / 2 + 1;
                mod2 = (TotalButtonCount % 2 == 0) ? TotalButtonCount / 2 : (CurrentButtonIndex >= TotalButtonCount / 2 + 1) ? TotalButtonCount / 2 : TotalButtonCount / 2 + 1;
            }
            else
            {
                mod1 = mod2 = TotalButtonCount;
            }

            int offsetX = (CurrentButtonIndex % mod1) * (width + xpadding);

            pos_X = (Constants.windowwidth / 2) - (((width + xpadding) / 2) * mod2);
            pos_X += offsetX;

            offsetX = (int)(widthOffsetinButtons * (width + xpadding));
            pos_X += offsetX;

            mod1 = mod2 = pos_Y = 0;

            if (TotalButtonCount > Constants.InlineUntilXButtons)
            {
                if (TotalButtonCount % 2 == 0 && CurrentButtonIndex >= TotalButtonCount / 2)
                {
                    mod1 = 1;
                }
                else if (CurrentButtonIndex >= TotalButtonCount / 2 + 1)
                {
                    mod1 = 1;
                }
                mod2 = Constants.YButtonCount;
            }
            else
            {
                if (useDoubleRow)
                {
                    mod2 = Constants.YButtonCount - 1;
                }
                else
                {
                    mod2 = Constants.YButtonCount;
                }
            }

            int offsetY = mod1 * (height + ypadding);
            pos_Y = (Constants.tabheight / 2) - (((height + ypadding) / 2) * (mod2));
            pos_Y += offsetY;

            offsetY = (int)(heightOffsetinButtons * (height + ypadding));
            pos_Y += offsetY;
        }

        public void SetupLabelofTrackbar(ColorSlider.ColorSlider b, Label l, String Text)
        {
            l.Name = Text;
            l.Text = Text;
            int posx = (int)(b.Location.X + (b.Size.Width / 2 - l.Size.Width / 2));
            int posy = b.Location.Y + b.Size.Height;
            l.Location = new Point(posx, posy);
        }
        public void SetupLabelofButton(Button b, Label l, String Text)
        {
            l.Name = Text;
            l.Text = Text;
            int posx = (int)(b.Location.X + (b.Size.Width / 2 - l.Size.Width / 2));
            int posy = b.Location.Y - Constants.Element_y_padding - l.Size.Height;
            l.Location = new Point(posx, posy);
        }

        public void selectButton(Button b, Boolean select, Color c)
        {
            if (b == null) return;
            if (select)
            {
                b.BackColor = c;
            }
            else
            {
                b.BackColor = Constants.Button_color;
            }

        }

        public void UpdateActiveDMXScene(int old_scene, bool force)
        {
            for (int i = 0; i < config.DMXScenes.Count; i++)
            {
                if (config.DMXScenes[i].ButtonElement != null)
                {
                    selectButton(config.DMXScenes[i].ButtonElement, config.DMXSceneSetting == i, Constants.selected_color);
                }
            }
            int channel = 0;
            foreach (ColorSlider.ColorSlider slider in FormColorSlides)
            {
                if (slider == null || (int)slider.Tag <= 0 || (int)slider.Tag >= 3 || slider.Name == null || slider.Name.Length <= 0)
                {
                    continue;
                }
                try
                {
                    channel = config.Dimmerchannel[Int32.Parse(slider.Name)];
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                    continue;
                }
                int old_value = 0;
                if ((old_scene < 0))
                {
                    old_value = (int)slider.Value;
                }
                else
                {
                    old_value = (int)((float)((float)(config.DMXScenes[old_scene].Channelvalues[channel]) / (float)255.0) * 100);
                }
                if (slider.Value == old_value || force)
                {
                    slider.ValueChanged -= form.Dimmer_Change;
                    slider.Value = (int)((float)((float)(config.DMXScenes[config.DMXSceneSetting].Channelvalues[channel]) / (float)255.0) * 100);
                    slider.ValueChanged += form.Dimmer_Change;
                }
            }
        }

        public void setActiveDMXScene(int index, bool force)
        {
            if (config == null)
            {
                return;
            }
            int old = config.DMXSceneSetting;
            if (index < config.DMXScenes.Count && index >= 0)
            {
                config.DMXSceneSetting = index;
                config.lastchangetime = DateTime.Now;
            }
            UpdateActiveDMXScene(old, force);
        }

        public void setConfig(Config c)
        {
            if (c == null)
            {
                return;
            }
            config = c;
            if (config.showtime)
            {
                createTimePageElements();
            }
            if (config.GastroUrl != null && config.GastroUrl.Length >= 0)
            {
                form.GastronomieWebview.Source = new Uri(config.GastroUrl, UriKind.Absolute);
            }
            form.UIControl.SelectTab(1);
            if (config.showcolor)
            {
                createColorPageElements();
                /*
                 int red = 0;
                int green = 0;
                int blue = 0;

                if (config.colorwheelvalues[0] != null && config.colorwheelvalues[0].Length > 0)
                {
                    red = config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.colorwheelvalues[0][0]];
                }
                if (config.colorwheelvalues[1] != null && config.colorwheelvalues[1].Length > 0)
                {
                    green = config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.colorwheelvalues[1][0]];
                }
                if (config.colorwheelvalues[2] != null && config.colorwheelvalues[2].Length > 0)
                {
                    blue = config.DMXScenes[config.DMXSceneSetting].Channelvalues[config.colorwheelvalues[2][0]];
                }
                form.colorWheelElement.Color = Color.FromArgb(1, red, green, blue);

                if (config.GastroUrl != null && config.GastroUrl.Length >= 0)
                {
                    form.GastronomieWebview.Source = new Uri(config.GastroUrl, UriKind.Absolute);
                }
                */
                form.UIControl.SelectTab(2);
            }

            int Pos_x, Pos_y;
            GendynamicAmbientButtons();
            GenNewPassword();
            if (config.AmbienteBackgroundImage != null && config.AmbienteBackgroundImage.Length > 2 && File.Exists(config.AmbienteBackgroundImage))
            {
                form.AmbientePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.AmbientePage.BackgroundImage = Image.FromFile(config.AmbienteBackgroundImage);
            }
            if (config.ColorBackgroundImage != null && config.ColorBackgroundImage.Length > 2 && File.Exists(config.ColorBackgroundImage))
            {
                form.ColorPage.BackgroundImageLayout = ImageLayout.Zoom;
                form.ColorPage.BackgroundImage = Image.FromFile(config.ColorBackgroundImage);
            }
            if (config.MediaBackgroundImage != null && config.MediaBackgroundImage.Length > 2 && File.Exists(config.MediaBackgroundImage))
            {
                form.MediaPage.BackgroundImageLayout = ImageLayout.Zoom;
                form.MediaPage.BackgroundImage = Image.FromFile(config.MediaBackgroundImage);
            }
            if (config.TimeBackgroundImage != null && config.TimeBackgroundImage.Length > 2 && File.Exists(config.TimeBackgroundImage))
            {
                form.TimePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.TimePage.BackgroundImage = Image.FromFile(config.TimeBackgroundImage);
            }
            if (config.ServiceBackgroundImage != null && config.ServiceBackgroundImage.Length > 2 && File.Exists(config.ServiceBackgroundImage))
            {
                form.ServicePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.ServicePage.BackgroundImage = Image.FromFile(config.ServiceBackgroundImage);
            }
            if (config.WartungBackgroundImage != null && config.WartungBackgroundImage.Length > 2 && File.Exists(config.WartungBackgroundImage))
            {
                form.WartungPage.BackgroundImageLayout = ImageLayout.Zoom;
                form.WartungPage.BackgroundImage = Image.FromFile(config.WartungBackgroundImage);
            }

            GendynamicServiceButtons();

            UpdateActiveDMXScene(-1, true);

            foreach (ColorSlider.ColorSlider s in FormColorSlides)
            {
                Label l = new Label();
                switch (FormColorSlides.IndexOf(s))
                {
                    case 0:
                        l = form.Dimmer1ColorSliderDescribtion; break;
                    case 1:
                        l = form.Dimmer2ColorSliderDescribtion; break;
                    case 2:
                        l = form.AmbientelautstärkeColorSliderDescribtion; break;
                    case 3:
                        l = form.TVSettingsVolumeColorSliderDescribtion; break;
                }
                SetupLabelofTrackbar(s, l, config.slidernames[((int)s.Tag) - 1]);
            }
            Showonallsites();
            Application.DoEvents();
        }

        public void GendynamicServiceButtons()
        {
            int Pos_x, Pos_y;
            while (ServicePageButtons.Count > 0)
            {
                Button but = ServicePageButtons[0];
                but.Parent.Controls.Remove(but);
                but.Hide();
                ServicePageButtons.Remove(but);
                but.Dispose();
            }
            form.HowCanIHelpYouDescribtion.Text = config.ServicesSettings[0].ShowText;
            form.HowCanIHelpYouDescribtion.Location = new Point((Constants.windowwidth / 2) - (form.HowCanIHelpYouDescribtion.Size.Width / 2), form.HowCanIHelpYouDescribtion.Location.Y);

            int Numhelps = Math.Min(config.ServicesSettings.Count, Constants.maxhelps);
            for (int i = 1; i < Numhelps; i++)
            {
                GetDynamicPosition(Numhelps - 1, i - 1, out Pos_x, out Pos_y, 0, 3, false);
                Button bu = null;
                Constants.createButton(Pos_x, Pos_y, ServicePageButtons, config.ServicesSettings[i].ShowText, config.ServicesSettings[i], form.ServicePage, form, form.Service_Request_Handle, out bu);
                config.ServicesSettings[i].ButtonElement = bu;
            }
        }

        public void GendynamicAmbientButtons()
        {
            int Pos_x, Pos_y;
            while (AmbientePagedynamicButtons.Count > 0)
            {
                Button but = AmbientePagedynamicButtons[0];
                but.Parent.Controls.Remove(but);
                but.Hide();
                AmbientePagedynamicButtons.Remove(but);
                AmbientePageButtons.Remove(but);
                but.Dispose();
            }
            int notshowenscenes = 2;
            int Numscenes = config.DMXScenes.Count - notshowenscenes;
            Numscenes = Math.Min(Numscenes, Constants.maxscenes);
            for (int i = notshowenscenes; i < config.DMXScenes.Count && i <= Constants.maxscenes; i++)
            {
                Constants.DMXScene scene = config.DMXScenes[i];
                if (scene != null)
                {
                    if (i == notshowenscenes)
                    {
                        int x = 0;
                        foreach (int j in scene.Channelvalues)
                        {
                            x += j;
                        }
                        if (x <= 0)
                        {
                            Numscenes++;
                            continue;
                        }
                    }
                    GetDynamicPosition(Numscenes, i - notshowenscenes, out Pos_x, out Pos_y, 0, 0, true);
                    Button bu = null;
                    Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, scene.ShowText, scene, form.AmbientePage, form, form.Ambiente_Change_Handler, out bu);
                    scene.ButtonElement = bu;
                    AmbientePagedynamicButtons.Add(bu);
                }
            }
        }

        private void Showonallsites()
        {
            while (globalLogos.Count > 0)
            {
                PictureBox Log = globalLogos[0];
                Log.Parent.Controls.Remove(Log);
                Log.Hide();
                globalLogos.Remove(Log);
                Log.Dispose();
            }
            globalLogos = new List<PictureBox>();
            for (int i = 0; i < config.showLogo.Length && i < tabs.Count; i++)
            {
                if (config.showLogo[i])
                {
                    PictureBox Logoview = new PictureBox();
                    Logoview.SizeMode = PictureBoxSizeMode.Zoom;
                    try
                    {
                        Logoview.Image = Image.FromFile(config.LogoFilePath);
                    }
                    catch (IOException e)
                    {
                        Log.Print(e.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                    }
                    Logoview.Size = new Size(Constants.Logoxsize, Constants.Logoysize);
                    SetEdgePosition(Logoview, config.Logoposition);
                    Logoview.TabStop = false;
                    globalLogos.Add(Logoview);
                    tabs[i].Controls.Add(Logoview);
                    Logoview.Show();
                    Logoview.BringToFront();
                }
            }
            while (globaltimelabels.Count > 0)
            {
                Label Lab = globaltimelabels[0];
                Lab.Parent.Controls.Remove(Lab);
                Lab.Hide();
                globaltimelabels.Remove(Lab);
                Lab.Dispose();
            }
            globaltimelabels = new List<Label>();
            if (config.showedgetime)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    if (i == 1 + ((config.showcolor) ? 1 : 0) || (config.showcolor && i == 4 + ((config.showcolor) ? 1 : 0)))
                    {
                        continue;
                    }
                    Label Labeltimeview = new Label();
                    SetEdgePosition(Labeltimeview, config.edgetimePosition);
                    Labeltimeview.AutoSize = true;
                    Labeltimeview.TabStop = false;
                    Labeltimeview.Font = Constants.Standart_font;
                    globaltimelabels.Add(Labeltimeview);
                    tabs[i].Controls.Add(Labeltimeview);
                    Labeltimeview.Show();
                    Labeltimeview.BringToFront();
                }
            }
            int posx, posy = 0;
            while (globalinformationlabels.Count > 0)
            {
                Label Lab = globalinformationlabels[0];
                Lab.Parent.Controls.Remove(Lab);
                Lab.Hide();
                globalinformationlabels.Remove(Lab);
                Lab.Dispose();
            }
            globalinformationlabels = new List<Label>();
            posx = Constants.windowwidth / 2;


            globalinformationlabels.Add(createLabelforpage(posx, 0));
            globalinformationlabels.Add(createLabelforpage(posx, 1));
            globalinformationlabels.Add(createLabelforpage(posx, 4));
            globalinformationlabels.Add(createLabelforpage(posx, tabs.Count - 1));
        }

        private Label createLabelforpage(int posx, int page)
        {
            Label Labelview = new Label();
            SetEdgePosition(Labelview, 3);
            Labelview.Location = new Point(posx, Labelview.Location.Y);
            Labelview.Font = Constants.Standart_font;
            Labelview.TabStop = false;
            Labelview.AutoSize = true;
            tabs[page].Controls.Add(Labelview);
            Labelview.Hide();
            Labelview.BringToFront();
            return Labelview;
        }

        public void SetEdgePosition(Control c, int pos)
        {
            int posx, posy = 0;
            if (pos % 2 == 0)
            {
                posx = (Constants.windowwidth - Constants.EdgeItemposxdist) - c.Size.Width;
            }
            else
            {
                posx = Constants.EdgeItemposxdist;
            }
            if (pos > 2)
            {
                posy = (Constants.tabheight - Constants.EdgeItemposydist * 2) - c.Size.Height;
            }
            else
            {
                posy = Constants.EdgeItemposydist;
            }
            c.Location = new Point(posx, posy);
        }

        private void GenNewPassword()
        {
            if (form.HandleCreate)
            {
                try
                {
                    form.BeginInvoke(new MyNoArgument(delegateGenNewPassword));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("GenNewPassword", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegateGenNewPassword();
                }
            }
            else
            {
                try
                {
                    delegateGenNewPassword();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("GenNewPassword", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    form.BeginInvoke(new MyNoArgument(delegateGenNewPassword));
                }
            }
        }

        private async void delegateGenNewPassword()
        {
            config.password = "";
            Task router = null;
            if (form != null && form.net != null && !Constants.noNet)
            {
                await Task.Delay(15);
                router = Task.Run(() => {
                    Task ta = Task.Run(() => form.net.setuprouterpassword(form));
                    ta.Wait();
                    ta = Task.Run(() => form.net.setuprouterssid(form));
                    ta.Wait();
                    ta = Task.Run(() => form.net.wakeup(form));
                    ta.Wait();
                });
            }
            if (form.loadscreen != null)
            {
                form.loadscreen.Debugtext($"Setting up Wifi Router", false);
            }

            int Pos_x, Pos_y;
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 1.8, false);
            form.WiFiSSIDLabel.Location = new Point(Pos_x, Pos_y);
            form.WiFiSSIDLabel.Hide();

            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 2.8, false);
            form.WiFiPasswortLabel.Location = new Point(Pos_x, Pos_y);
            form.WiFiPasswortLabel.Hide();

            setnewPassword();
            form.loadscreen.Debugtext($"", false);
        }

        public delegate void MyNoArgument();
        public void setnewPassword()
        {
            if (form.HandleCreate)
            {
                try
                {
                    form.Invoke(new MyNoArgument(delegatesetnewPassword));
                }
                catch (InvalidOperationException ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("setnewPassword", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegatesetnewPassword();
                }
            }
            else
            {
                try
                {
                    delegatesetnewPassword();
                }
                catch (Exception ex)
                {
                    Log.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Log.Print("setnewPassword", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    form.Invoke(new MyNoArgument(delegatesetnewPassword));
                }
            }
        }

        private void delegatesetnewPassword()
        {
            if (form != null)
            {
                if (config.password != null && config.password.Length > 0)
                {
                    CreateMediaControllingElemets(2.3);
                    form.WiFiSSIDLabel.Show();
                    form.WiFiPasswortLabel.Show();
                    form.WiFiPasswordTitle.Show();
                    form.WiFiSSIDTitle.Show();
                    form.generateQRCode(form.WiFiQRCodePicturebox, 20, false, (int)(Constants.Element_width * 1.5), true);
                    if (form.vlc != null)
                    {
                        form.vlc.newsession();
                    }

                }
                else
                {
                    CreateMediaControllingElemets(1);
                    form.WiFiPasswordTitle.Hide();
                    form.WiFiSSIDTitle.Hide();
                    form.WiFiSSIDLabel.Hide();
                    form.WiFiPasswortLabel.Hide();
                }
            }
            form.WiFiPasswortLabel.Text = config.password;
        }


        private void setConfigRestricted(Config config)
        {
            if (config == null)
            {
                return;
            }
            int Pos_x, Pos_y;

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Button b = null;
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, config.DMXScenes[1].ShowText, config.DMXScenes[1], form.WartungPage, form, form.Ambiente_Change_Handler, out b);
            config.DMXScenes[1].ButtonElement = b;
            selectButton(b, config.DMXSceneSetting == 1, Constants.selected_color);
        }

        

        public void createConsolePage()
        {
            while (ConsoleElements.Count > 0)
            {
                Control rem = WartungPageButtons[0];
                rem.Tag = -1;
                rem.Hide();
                form.UIControl.Controls.Remove(rem);
                ConsoleElements.Remove(rem);
            }
            int posx, posy = 0;
            form.UIControl.Controls.Add(form.ConsolePage);
            form.consoleshown = true;

            GetDynamicPosition(3, 1, out posx, out posy, 0, 1, false);
            form.consoletype = new ComboBox();
            Array types = Enum.GetValues(typeof(Logger.MessageType));
            foreach (Logger.MessageType mt in types)
            {
                if (Log.getList(Log.MTypetobyte<Logger.MessageType>(mt)).Count > 0)
                {
                    form.consoletype.Items.Add(new Constants.ComboItem { Text = mt.ToString(), ID = Log.MTypetobyte<Logger.MessageType>(mt) });
                }
            }
            form.consoletype.Location = new Point(posx, posy);
            form.consoletype.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.consoletype.SelectedIndexChanged += form.comboconsoleItemchanged;
            ConsoleElements.Add(form.consoletype);
            form.ConsolePage.Controls.Add(form.consoletype);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 0, false);
            Label lab = new Label();
            lab.AutoSize = true;
            lab.Text = "TCP Message";
            lab.ForeColor = Constants.Text_color;
            lab.Location = new Point(posx, posy);
            lab.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point, 0);
            form.ConsolePage.Controls.Add(lab);
            ConsoleElements.Add(lab);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 0.5, false);
            Label la = new Label();
            la.AutoSize = true;
            la.Text = $"Room: {config.Room}";
            la.ForeColor = Constants.Text_color;
            la.Location = new Point(posx, posy);
            la.Font = Constants.Standart_font;
            form.ConsolePage.Controls.Add(la);
            ConsoleElements.Add(la);

            GetDynamicPosition(3, 1, out posx, out posy, 0, 0, false);
            Label l = new Label();
            l.AutoSize = true;
            l.Text = "Console";
            l.ForeColor = Constants.Text_color;
            l.Location = new Point(posx, posy);
            l.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point, 0);
            form.ConsolePage.Controls.Add(l);
            ConsoleElements.Add(l);

            /*
                public ComboBox tcptype;
                public TextBox CommandboxLabel;
                public NumericUpDown Commandboxid;
                public TextBox Commandboxvalues;
            */

            GetDynamicPosition(3, 0, out posx, out posy, 0, 1, false);
            form.tcptype = new ComboBox();
            for (int i = 0; i<config.Typenames.Length;i++)
            {
                form.tcptype.Items.Add(new Constants.ComboItem { Text = config.Typenames[i], ID = i });
            }
            form.tcptype.Location = new Point(posx, posy);
            form.tcptype.Size = new Size(Constants.Element_width, Constants.Element_height);
            //form.tcptype.SelectedIndexChanged += form.comboTCPItemchanged;
            ConsoleElements.Add(form.tcptype);
            form.ConsolePage.Controls.Add(form.tcptype);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 1.5, false);
            form.CommandboxLabel = new TextBox();
            form.CommandboxLabel.PlaceholderText = "Label";
            form.CommandboxLabel.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.CommandboxLabel.Location = new Point(posx, posy);
            form.CommandboxLabel.TextChanged += form.TCPMessage_Change_handler;
            ConsoleElements.Add(form.CommandboxLabel);
            form.ConsolePage.Controls.Add(form.CommandboxLabel);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 2, false);
            form.Commandboxid = new TextBox();
            form.Commandboxid.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.Commandboxid.Location = new Point(posx, posy);
            form.Commandboxid.KeyPress += form.CommandId_KeyPress;
            form.Commandboxid.TextChanged += form.TCPMessage_Change_handler;
            ConsoleElements.Add(form.Commandboxid);
            form.ConsolePage.Controls.Add(form.Commandboxid);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 2.5, false);
            form.Commandboxvalues = new TextBox();
            form.Commandboxvalues.PlaceholderText = "values (komma seperated)";
            form.Commandboxvalues.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.Commandboxvalues.Location = new Point(posx, posy);
            ConsoleElements.Add(form.Commandboxvalues);
            form.ConsolePage.Controls.Add(form.Commandboxvalues);

            GetDynamicPosition(3, 0, out posx, out posy, 0, 3, false);
            Button bu = null; 
            Constants.createButton(posx, posy, (List<Button>)null, "Send Message", "sendtcp", form.ConsolePage, form, form.sendTCPfromconsole,out bu);
            ConsoleElements.Add(bu);

            GetDynamicPosition(3, 2, out posx, out posy, 0, 1, false);
            form.Textscroll = createColorSlide(0);
            form.Textscroll.Orientation = Orientation.Vertical;
            form.Textscroll.Location = new Point(posx, posy);
            form.Textscroll.Size = new Size(form.Textscroll.Size.Width, (Constants.Element_height+Constants.Element_y_padding)*3);
            form.Commandboxvalues.TextChanged += form.TCPMessage_Change_handler;
            ConsoleElements.Add(form.Textscroll);
            form.ConsolePage.Controls.Add(form.Textscroll);

            Point lp = new Point(0, 4);
            GetDynamicPosition(3, lp.X, out posx, out posy, 0, lp.Y, false);
            form.Messagepreview = new Label();
            form.Messagepreview.AutoSize = true;
            form.Messagepreview.Text = "";
            form.Messagepreview.ForeColor = Constants.Text_color;
            form.Messagepreview.Location = new Point(posx, posy);
            form.Messagepreview.Tag = new Point(0, 4);
            form.Messagepreview.Font = Constants.Standart_font;
            form.ConsolePage.Controls.Add(form.Messagepreview);
            ConsoleElements.Add(form.Messagepreview);

            GetDynamicPosition(3, 2, out posx, out posy, 0, 1, false);
            form.ConsoleTextscroll = createColorSlide(0);
            form.ConsoleTextscroll.ShowDivisionsText = false;
            form.ConsoleTextscroll.ShowSmallScale = false;
            form.ConsoleTextscroll.Orientation = Orientation.Vertical;
            form.ConsoleTextscroll.Location = new Point(posx, posy);
            form.ConsoleTextscroll.Size = new Size(form.ConsoleTextscroll.Size.Width, (Constants.Element_height + Constants.Element_y_padding) * 3);
            form.ConsoleTextscroll.Hide();
            form.ConsoleTextscroll.ValueChanged += form.consolescroll;
            ConsoleElements.Add(form.ConsoleTextscroll);
            form.ConsolePage.Controls.Add(form.ConsoleTextscroll);

            form.resizeUIControlItems();
        }

        public void removeConsolePage()
        {
            form.UIControl.Controls.Remove(form.ConsolePage);
            form.consoleshown = false;
            form.vlc.toggleConsoleBox(false);
            Debug.Print(form.vlc.GetConsoleText());
            form.resizeUIControlItems();
            while (ConsoleElements.Count > 0)
            {
                Control rem = WartungPageButtons[0];
                rem.Tag = -1;
                rem.Hide();
                form.UIControl.Controls.Remove(rem);
                ConsoleElements.Remove(rem);
            }
        }

        public void checkforKeyboard()
        {
            while (true)
            {
                bool keyboardPresent = false;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_Keyboard");

                foreach (ManagementObject keyboard in searcher.Get())
                {
                    foreach (PropertyData prop in keyboard.Properties)
                    {
                        if (Convert.ToString(prop.Value).Contains(USB))
                        {
                            keyboardPresent = true;
                            break;
                        }
                    }
                }

                if (keyboardPresent)
                {

                }
            }
        }
    }
}
