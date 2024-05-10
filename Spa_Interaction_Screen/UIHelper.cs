
using BrbVideoManager.Controls;

namespace Spa_Interaction_Screen
{
    public class UIHelper
    {
        private MainForm form = null;

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

        private const double Restrictedyoffset = 0.5;
        private int Restrictedycoord =(int) (((Constants.Element_height + Constants.Element_y_padding) + (int)((Constants.Element_height + Constants.Element_y_padding) * Restrictedyoffset))/1.5);
        private int Restrictedstarty = (Constants.Element_height + Constants.Element_y_padding);

        private delegate object MyNoArgument();

        public UIHelper(MainForm f)
        {
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
        }

        public void init()
        {
            form.SuspendLayout();
            createAmbientePageElements();

            createMediaPageElements();

            createServicePageElements();

            createGastroPageElements();

            createRestrictedPageElements();
            removeRestrictedPageElements();

            createWartungPageElements();

            createButtonTester();
            form.ResumeLayout(true);
        }

        private ColorSlider.ColorSlider createColorSlide(int max)
        {
            ColorSlider.ColorSlider slide = new ColorSlider.ColorSlider();
            slide.BackColor = Color.Transparent;
            slide.ElapsedInnerColor = Color.White;
            slide.ElapsedPenColorBottom = Color.White;
            slide.ElapsedPenColorTop = Color.White;

            slide.BarPenColorBottom = Color.Black;
            slide.BarPenColorTop = Color.Black;
            slide.BarInnerColor = Color.Black;

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
            slide.ThumbRoundRectSize = new Size(27, 27);
            slide.ThumbSize = new Size(35, 35);
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
            form.AmbientePage.SuspendLayout();
            int Pos_x, Pos_y;

            GetDynamicPosition(3, 1, out Pos_x, out Pos_y, 0, 2, false);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, Config.Objectname, (int)0, form.AmbientePage, form, form.Ambiente_Design_Handler);

            ColorSlider.ColorSlider newslider = null;
            GetDynamicPosition(3, 0, out Pos_x, out Pos_y, 0, 2, false);
            form.Dimmer1ColorSlider = createColorSlide(100);
            form.AmbientePage.Controls.Add(form.Dimmer1ColorSlider);
            form.Dimmer1ColorSlider.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.Dimmer1ColorSlider.Location = new Point(Pos_x, Pos_y);
            form.Dimmer1ColorSlider.Tag = 1;
            form.Dimmer1ColorSlider.Name = "0";
            form.Dimmer1ColorSlider.TabIndex = AmbientePageButtons.Count + 1;
            //newslider.Value = Config.DMXScenes[Config.DMXSceneSetting].Channelvalues[Config.Dimmerchannel[0]];
            form.Dimmer1ColorSlider.ValueChanged += form.Dimmer_Change;
            FormColorSlides.Add(form.Dimmer1ColorSlider);

            form.Dimmer1ColorSliderDescribtion = new Label();
            form.Dimmer1ColorSliderDescribtion.BackColor = Color.Transparent;
            form.Dimmer1ColorSliderDescribtion.AutoSize = true;
            form.Dimmer1ColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.AmbientePage.Controls.Add(form.Dimmer1ColorSliderDescribtion);

            GetDynamicPosition(3, 2, out Pos_x, out Pos_y, 0, 2, false);
            form.Dimmer2ColorSlider = createColorSlide(100);
            form.AmbientePage.Controls.Add(form.Dimmer2ColorSlider);
            form.Dimmer2ColorSlider.Size = new Size(Constants.Element_width, Constants.Element_height);
            form.Dimmer2ColorSlider.Location = new Point(Pos_x, Pos_y);
            form.Dimmer2ColorSlider.Tag = 2;
            form.Dimmer2ColorSlider.Name = "1";
            form.Dimmer2ColorSlider.TabIndex = AmbientePageButtons.Count + 1;
            //newslider.Value = Config.DMXScenes[Config.DMXSceneSetting].Channelvalues[Config.Dimmerchannel[1]];
            FormColorSlides.Add(form.Dimmer2ColorSlider);
            form.Dimmer2ColorSlider.ValueChanged += form.Dimmer_Change;

            form.Dimmer2ColorSliderDescribtion = new Label();
            form.Dimmer2ColorSliderDescribtion.BackColor = Color.Transparent;
            form.Dimmer2ColorSliderDescribtion.AutoSize = true;
            form.Dimmer2ColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.AmbientePage.Controls.Add(form.Dimmer2ColorSliderDescribtion);
            
            GetDynamicPosition(3, 0, out Pos_x, out Pos_y, 0, 3, true);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, "Sauna Lampe 1", (int)0, form.AmbientePage, form, form.Ambiente_Sauna_Handler);

            GetDynamicPosition(3, 1, out Pos_x, out Pos_y, 0, 3, true);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, "Sauna Lampe 2", (int)1, form.AmbientePage, form, form.Ambiente_Sauna_Handler);

            GetDynamicPosition(3, 2, out Pos_x, out Pos_y, 0, 3, true);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, "Sauna Lampe 3", (int)2, form.AmbientePage, form, form.Ambiente_Sauna_Handler);


            GetDynamicPosition(2, 0, out Pos_x, out Pos_y, 0, 3, false);
            newslider = createColorSlide(100);
            newslider.Size = new Size(Constants.Element_width * 2, Constants.Element_height);
            newslider.Location = new Point(Pos_x, Pos_y);
            newslider.TabIndex = AmbientePageButtons.Count + 1;
            newslider.Tag = 3;
            newslider.ValueChanged += form.AmbientVolume_Handler;
            //form.AmbientePage.Controls.Add(newslider);

            /*if (Config.Volume >= newslider.Minimum && Config.Volume <= newslider.Maximum)
            {
                newslider.Value = Config.Volume;
            }*/

            // 
            // AmbientelautstärkeColorSliderDescribtion
            // 

            form.AmbientelautstärkeColorSliderDescribtion = new Label();
            form.AmbientelautstärkeColorSliderDescribtion.BackColor = Color.Transparent;
            form.AmbientelautstärkeColorSliderDescribtion.AutoSize = true;
            form.AmbientelautstärkeColorSliderDescribtion.ForeColor = Constants.Text_color;
            //form.AmbientePage.Controls.Add(form.AmbientelautstärkeColorSliderDescribtion);
            FormColorSlides.Add(newslider);
            form.AmbientePage.ResumeLayout(true);
        }

        public void createColorPageElements()
        {
            form.ColorPage.SuspendLayout();
            int Posx, Posy;
            GetDynamicPosition(1, 0, out Posx, out Posy, 0, 0, false);
            Label label = new Label();
            label.BackColor = Color.Transparent;
            label.AutoSize = true;
            label.ForeColor = Constants.Text_color;
            label.Text = "Ambientebeleuchtungsfarbe";
            form.ColorPage.Controls.Add(label);
            label.Location = new Point((Constants.windowwidth / 2) - (label.Size.Width / 2), Posy - 15);
            ColorSlider.ColorSlider newslider = null;
            if (form.colorWheelElement != null && form.colorWheelElement.Tag == null)
            {
                newslider = createColorSlide(100);
                newslider.Orientation = Orientation.Vertical;
                newslider.Value = 100;
                form.ColorPage.Controls.Add(newslider);
                newslider.ValueChanged += form.ColorChanged_Handler;
            }
            else if(form.colorWheelElement != null && form.colorWheelElement.Tag != null)
            {
                newslider=((ColorSlider.ColorSlider)(form.colorWheelElement.Tag));
            }

            if (form.colorWheelElement == null)
            {
                newslider = createColorSlide(100);
                newslider.Orientation = Orientation.Vertical;
                newslider.Value = 100;
                form.ColorPage.Controls.Add(newslider);
                newslider.ValueChanged += form.ColorChanged_Handler;

                form.colorWheelElement = new Cyotek.Windows.Forms.ColorWheel();
                form.colorWheelElement.Size = new Size((int)(Constants.windowwidth / 3), (int)(Constants.windowwidth / 3));
                form.ColorPage.Controls.Add(form.colorWheelElement);
                form.colorWheelElement.Location = new Point((Constants.windowwidth / 2) - (form.colorWheelElement.Size.Width / 2), ((Constants.tabheight - (label.Location.Y + label.Size.Height)) / 2) - (form.colorWheelElement.Size.Height / 2) + (label.Location.Y + label.Size.Height) - 15);
                form.colorWheelElement.ColorChanged += form.ColorChanged_Handler;
                form.colorWheelElement.Tag = (ColorSlider.ColorSlider)newslider;
                form.ColorPage.BackColor = Constants.Background_color;
            }

            newslider.Size = new Size(150, (int)(form.colorWheelElement.Size.Height * 0.5)); 
            newslider.Location = new Point((int)(form.colorWheelElement.Size.Width*1 + form.colorWheelElement.Location.X), (Constants.tabheight / 2) - (newslider.Size.Height / 2));

            GetDynamicPosition(5, 0, out Posx, out Posy, 0, 0, false);
            form.resetcolorbutton = Constants.createButton<Button>(Constants.Element_width, Constants.Element_height, Posx, (Constants.tabheight / 2) - (Constants.Element_height / 2), null, "Warmweiß", null, form.ColorPage, form, form.resetcolorwheel);
            form.ResumeLayout(true);
        }

        public void createGastroPageElements()
        {
            Constants.createButton<Button>((int)(Constants.windowwidth / 2) - (Constants.Element_width / 2), (Constants.tabheight / 2) - (Constants.Element_height / 2), null, Config.ServicesSettings[1].ShowText, (Constants.ServicesSetting)Config.ServicesSettings[1], form.GastronomiePage, form, form.Service_Request_Handle, out form.GastroEx);
            form.GastroEx.Hide();

            form.GastroExDescription = new Label();
            form.GastroExDescription.BackColor = Color.Transparent;
            form.GastroExDescription.AutoSize = true;
            form.GastroExDescription.ForeColor = Constants.Text_color;
            form.GastroExDescription.Text = "Leider ist die Bestellseite zur Zeit nicht verfügbar.\nBitte melden Sie sich bei dem Service Personal,\nfalls Sie etwas bestellen möchten.";
            form.GastronomiePage.Controls.Add(form.GastroExDescription);
            form.GastroExDescription.Location = new Point((Constants.windowwidth / 2) - (form.GastroExDescription.Size.Width / 2), (int)(Constants.tabheight / 4) - (form.GastroExDescription.Height / 2));
            form.GastroExDescription.Hide();
        }

        public void createMediaPageElements()
        {
            form.MediaPage.SuspendLayout();
            int Pos_x, Pos_y = 0;

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, 1, false);
            form.WiFiQRCodePicturebox = new PictureBox();
            form.WiFiQRCodePicturebox.BackColor = Constants.Background_color;
            form.WiFiQRCodePicturebox.Name = "WiFiQRCodePicturebox";
            form.WiFiQRCodePicturebox.Size = new Size(424, 332);
            form.WiFiQRCodePicturebox.SizeMode = PictureBoxSizeMode.Zoom;
            form.WiFiQRCodePicturebox.TabIndex = 5;
            form.WiFiQRCodePicturebox.TabStop = false;
            form.WiFiQRCodePicturebox.Location = new Point(Pos_x, Pos_y);
            form.WiFiQRCodePicturebox.Hide();
            form.MediaPage.Controls.Add(form.WiFiQRCodePicturebox);

            form.WiFiSSIDTitle.Text = "Wlan Name:";
            GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0.55, 1.5, false);
            form.WiFiSSIDTitle.Location = new Point(Pos_x, Pos_y);

            form.WiFiPasswordTitle.Text = "Wlan Passwort:";
            GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0.55, 2.5, false);
            form.WiFiPasswordTitle.Location = new Point(Pos_x, Pos_y);

            CreateMediaControllingElemets(5);
            form.MediaPage.ResumeLayout(true);
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
            GetDynamicPosition(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 0.8, false, sizex, sizey);
            form.TVSettingsAmbienteButton = Constants.createButton(sizex, sizey, Pos_x, Pos_y, MediaPageButtons, "Ambiente Video", true, form.MediaPage, form, form.Content_Change_Handler);
            selectButton(form.TVSettingsAmbienteButton, true, Constants.selected_color);
            form.TVSettingsAmbienteButton.Name = "AmbientVideo";

            GetDynamicPosition(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 1.8, false, sizex, sizey);
            form.TVSettingsStreamingButton = Constants.createButton(sizex, sizey, Pos_x, Pos_y, MediaPageButtons, "Streaming Video", false, form.MediaPage, form, form.Content_Change_Handler);
            form.TVSettingsStreamingButton.Name = "StreamingVideo";

            SetupLabelofButton(form.TVSettingsAmbienteButton, form.TVSettingsTitle, "Video Einstellungen:");

            form.MediaPageAmbientVolumeSlider = null;
            GetDynamicPosition(Elementsinwidth, Elementsinwidth - 1, out Pos_x, out Pos_y, widthelements, 2.8, false, sizex, sizey);
            form.MediaPageAmbientVolumeSlider = createColorSlide(100);
            form.MediaPageAmbientVolumeSlider.Location = new Point(Pos_x, Pos_y);
            form.MediaPageAmbientVolumeSlider.Size = new Size(sizex, 200);
            form.MediaPageAmbientVolumeSlider.Tag = 3;
            form.MediaPage.Controls.Add(form.MediaPageAmbientVolumeSlider);
            FormColorSlides.Add(form.MediaPageAmbientVolumeSlider);
            form.MediaPageAmbientVolumeSlider.ValueChanged += form.AmbientVolume_Handler;
            /*if (Config.Volume >= newslider.Minimum && Config.Volume <= newslider.Maximum)
            {
                newslider.Value = Config.Volume;
            }*/

            form.TVSettingsVolumeColorSliderDescribtion = new Label();
            form.TVSettingsVolumeColorSliderDescribtion.BackColor = Color.Transparent;
            form.TVSettingsVolumeColorSliderDescribtion.AutoSize = true;
            form.TVSettingsVolumeColorSliderDescribtion.ForeColor = Constants.Text_color;
            form.MediaPage.Controls.Add(form.TVSettingsVolumeColorSliderDescribtion);
        }

        public void createTimePageElements()
        {
            form.TimePage.SuspendLayout();
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

            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0.5, 2, true);
            form.timer = new Label();
            form.timer.AutoSize = true;
            form.timer.Font = Constants.Standart_font;
            form.timer.ForeColor = Constants.Text_color;
            form.timer.Name = "Timer";
            form.timer.TabIndex = 3;
            form.timer.BackColor = Color.Transparent;
            form.TimePage.Controls.Add(form.timer);
            form.timer.Location = new Point((Constants.windowwidth / 2) - (form.timer.Size.Width / 2), Pos_y);
            form.TimePage.ResumeLayout(true);
        }

        public void createServicePageElements()
        {
            form.ServicePage.SuspendLayout();
            int Pos_x, Pos_y = 0;
            //form.HowCanIHelpYouDescribtion.Text = Config.ServiceStrings[0][0];
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 3, false);
            form.HowCanIHelpYouDescribtion.Location = new Point(Pos_x, Pos_y - Constants.Element_y_padding - form.HowCanIHelpYouDescribtion.Size.Height);
            form.HowCanIHelpYouDescribtion.BackColor = Color.Transparent;
            int Numhelps = Config.DMXScenes.Count;
            foreach (Constants.DMXScene sc in Config.DMXScenes)
            {
                if (sc.ShowText == null || sc.ShowText.Length <= 0)
                {
                    Numhelps--;
                }
            }
            if (Numhelps > Constants.maxscenes)
            {
                Logger.Print("Es wurden zu viele Service Einstellungen eingelesen.", Logger.MessageType.Ohne_Kategorie, Logger.MessageSubType.Notice);
            }
            Numhelps = Math.Min(Config.ServicesSettings.Count, Constants.maxhelps);
            double pos = 4.5 + (Numhelps>Constants.InlineUntilXButtons?1:0);
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0.5, pos, false);
            form.ZentraleNotReachable = new Label();
            form.ZentraleNotReachable.BackColor = Color.Transparent;
            form.ZentraleNotReachable.AutoSize = true;
            form.ZentraleNotReachable.TabStop = false;
            form.ZentraleNotReachable.Text = Constants.ServiceNotReachableText;
            form.ZentraleNotReachable.ForeColor = Constants.Warning_color;
            form.ZentraleNotReachable.Font = Constants.Standart_font;
            form.ServicePage.Controls.Add(form.ZentraleNotReachable);
            form.ZentraleNotReachable.Show();
            form.ZentraleNotReachable.Location = new Point(Pos_x - (form.ZentraleNotReachable.Size.Width/2), Pos_y);

            form.Servicelockedlabel = new Label();
            form.Servicelockedlabel.BackColor = Color.Transparent;
            form.Servicelockedlabel.AutoSize = true;
            form.Servicelockedlabel.TabStop = false;
            form.Servicelockedlabel.Text = "Die Service Auswahl wurde deaktiviert. Bitten sie einen Mitarbeiter diese wieder freizushalten.";
            form.Servicelockedlabel.Font = Constants.Standart_font;
            form.ServicePage.Controls.Add(form.Servicelockedlabel);
            form.Servicelockedlabel.Hide();
            form.ServicePage.ResumeLayout(true);
        }

        public void createWartungPageElements()
        {
            form.WartungPage.SuspendLayout();
            int Pos_x, Pos_y = 0;
            /*
            form.WartungCodeField.Text = "Pin eingeben:";
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, 0, false);
            form.WartungCodeField.Location = new Point(Constants.windowwidth / 2 - form.WartungCodeField.Size.Width / 2, Pos_y-15);
            form.WartungCodeField.Show();
            */
            form.RestrictedAreaDescribtion.Text = Config.RestrictedDescription;
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
            form.WartungPage.ResumeLayout(true);
        }

        public void removeWartungPageElements()
        {
            form.WartungPage.SuspendLayout();
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
            form.WartungPage.ResumeLayout(true);
        }

        public void createRestrictedPageElements()
        {
            form.WartungPage.SuspendLayout();
            int width = Constants.Element_width;
            int height = (int)(Constants.Element_height);

            int Pos_x, Pos_y = 0;
            form.RestrictedAreaTitle.Text = "Zugriff nur für Mitarbeiter";
            GetDynamicPosition(1, 0, out Pos_x, out Pos_y, 0, -0.3, false, width, height);
            form.RestrictedAreaTitle.Location = new Point(Constants.windowwidth / 2 - form.RestrictedAreaTitle.Size.Width / 2, Pos_y);
            form.RestrictedAreaTitle.Show();

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            form.resetscenelockbutton = Constants.createButton(width, height, Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Szenen Auswahl freischalten", null, form.WartungPage, form, form.resetscenelock_Handler);
            if (form.Scenelocked)
            {
                form.resetscenelockbutton.Show();
            }
            else
            {
                form.resetscenelockbutton.Hide();
            }

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            form.resetServicelockbutton = Constants.createButton(width, height, Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Service Auswahl freischalten", null, form.WartungPage, form, form.resetServicelock_Handler);
            if (form.Servicelocked)
            {
                form.resetServicelockbutton.Show();
            }
            else
            {
                form.resetServicelockbutton.Hide();
            }

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            form.resetSessionlockbutton = Constants.createButton(width, height, Pos_x, Restrictedstarty + 3 * Restrictedycoord, RestrictedPageButtons, "Session Timer freischalten", null, form.WartungPage, form, form.resettimelock_Handler);
            if (form.Sessionlocked)
            {
                form.resetSessionlockbutton.Show();
            }
            else
            {
                form.resetSessionlockbutton.Hide();
            }
            
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Constants.createButton(width, height, Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "neue Session Starten", "SessionStart", form.WartungPage, form, form.NewSession_Handler);

            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Constants.createButton(width, height, Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Session Beenden", "SessionEnd", form.WartungPage, form, form.EndSession_Handler);

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Constants.createButton(width, height, Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, Constants.ExitFullscreenText, "ToggleFullscreen", form.WartungPage, form, form.Programm_Exit_Handler);

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Constants.createButton(width, height, Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Programm zurücksetzen", "Reset", form.WartungPage, form, form.reset_Handler);
            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            if (form.vlcclosed)
            {
                Constants.createButton(width, height, Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Öffne den Player", "VLCClose", form.WartungPage, form, form.OpenPlayer_Handler);
            }
            else
            {
                Constants.createButton(width, height, Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Schließe den Player", "VLCClose", form.WartungPage, form, form.closePlayer_Handler);
            }
            GetDynamicPosition(5, 4, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Constants.createButton(width, height, Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "Ausloggen", "Logout", form.WartungPage, form, form.logoutbutton_Handler);
            GetDynamicPosition(5, 4, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            if (form.showconsoleonallsites)
            {
                form.showconsoleonallsites_button = Constants.createButton(width, height, Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Zeige Konsole nicht immer", "ConsoleEverywhere", form.WartungPage, form, form.ShowConsoleNotOnallSites);
            }
            else
            {
                form.showconsoleonallsites_button = Constants.createButton(width, height, Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Zeige Konsole immer", "ConsoleEverywhere", form.WartungPage, form, form.ShowConsoleOnallSites);
            }
            if (Logger.consoleshown)
            {
                Constants.createButton(width, height, Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Schließe Konsole", null, form.WartungPage, form, form.CloseConsole);
            }
            else
            {
                Constants.createButton(width, height, Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Öffne Konsole", null, form.WartungPage, form, form.ShowConsole);
                form.showconsoleonallsites_button.Hide();
            }

            GetDynamicPosition(5, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
            Button b = null;
            b = Constants.createButton(width, height, Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, Config.DMXScenes[1].JsonText, Config.DMXScenes[1], form.WartungPage, form, form.Ambiente_Change_Handler);
            Config.DMXScenes[1].ButtonElement = b;
            selectButton(b, Config.DMXSceneSetting == 1, Constants.selected_color);
            int wartungs = Config.TCPSettings.Count;
            foreach (Constants.DMXScene sc in Config.DMXScenes)
            {
                if (sc.ShowText == null || sc.ShowText.Length <= 0)
                {
                    wartungs--;
                }
            }
            if (wartungs > Constants.maxscenes)
            {
                Logger.Print("Es wurden zu viele TCPSettings eingelesen.", Logger.MessageType.TCPSend, Logger.MessageSubType.Notice);
            }
            wartungs = Math.Min(wartungs, Constants.maxtcpws);
            int x = 0;
            Button bu = null;
            for (int i = 0; i < Config.TCPSettings.Count && x < Constants.maxtcpws; i++)
            {
                if (Config.TCPSettings[i].ShowText == null || Config.TCPSettings[i].ShowText.Length <= 0)
                {
                    Constants.createButton<Button>(null, null, null, "", Config.TCPSettings[i], null, form, null, out bu);
                    bu.Hide();
                    Config.TCPSettings[i].ButtonElement = bu;
                    continue;
                }
                GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0, Restrictedyoffset, false, width, height);
                Button but = null;
                but = Constants.createButton(width, height, Pos_x, Restrictedstarty + x * Restrictedycoord, RestrictedPageButtons, Config.TCPSettings[i].ShowText, Config.TCPSettings[i], form.WartungPage, form, form.Wartung_Request_Handle);
                Config.TCPSettings[i].ButtonElement = but;
                x++;
            }
            form.WartungPage.ResumeLayout(true);
        }

        public void removeRestrictedPageElements()
        {
            form.WartungPage.SuspendLayout();
            form.RestrictedAreaTitle.Hide();

            while (RestrictedPageButtons.Count > 0)
            {
                Button rem = RestrictedPageButtons[0];
                rem.Tag = -1;
                rem.Hide();
                form.WartungPage.Controls.Remove(rem);
                RestrictedPageButtons.Remove(rem);
            }
            form.WartungPage.ResumeLayout(true);
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
            GetDynamicPosition(TotalButtonCount, CurrentButtonIndex, out pos_X,out pos_Y,widthOffsetinButtons, heightOffsetinButtons, useDoubleRow, Constants.Element_width, Constants.Element_height);
        }

        public void GetDynamicPosition(int TotalButtonCount, int CurrentButtonIndex, out int pos_X, out int pos_Y, double widthOffsetinButtons, double heightOffsetinButtons, bool useDoubleRow, int width, int height)
        {
            int xpadding = (int)(((double)Constants.windowwidth / (Constants.XButtonCount + 1)) - width)/2;
            int ypadding = (int)(((double)Constants.tabheight / (Constants.YButtonCount + 1)) - height)/2;
            GetDynamicPosition(TotalButtonCount, CurrentButtonIndex, out pos_X, out pos_Y, widthOffsetinButtons, heightOffsetinButtons, useDoubleRow, width, height, xpadding, ypadding);
        }

        public void GetDynamicPosition(int TotalButtonCount, int CurrentButtonIndex, out int pos_X, out int pos_Y, double widthOffsetinButtons, double heightOffsetinButtons, bool useDoubleRow, int width, int height, int xpadding, int ypadding)
        {
            int mod1, mod2;

            if (TotalButtonCount > Constants.InlineUntilXButtons)
            {

                mod1 = (TotalButtonCount % 2 == 0) ? TotalButtonCount / 2 : (CurrentButtonIndex >= TotalButtonCount / 2 + 1) ? TotalButtonCount / 2 + 1 : TotalButtonCount / 2 + 1; // 4
                mod2 = (TotalButtonCount % 2 == 0) ? TotalButtonCount / 2 : (CurrentButtonIndex >= TotalButtonCount / 2 + 1) ? TotalButtonCount / 2 : TotalButtonCount / 2 + 1; // 3
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

            mod1 = 0;

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
            if(l ==null ||  b == null) 
            {
                return;
            }
            l.Name = Text;
            l.Text = Text;
            int posx = (int)(b.Location.X + (b.Size.Width / 2 - l.Size.Width / 2));
            int posy = b.Location.Y + b.Size.Height;
            l.Location = new Point(posx, posy);
            l.BringToFront();
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
            for (int i = 0; i < Config.DMXScenes.Count; i++)
            {
                if (Config.DMXScenes[i].ButtonElement != null)
                {
                    selectButton(Config.DMXScenes[i].ButtonElement, Config.DMXSceneSetting == i, Constants.selected_color);
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
                    channel = Config.Dimmerchannel[Int32.Parse(slider.Name)];
                }
                catch (FormatException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                    continue;
                }catch (ArgumentOutOfRangeException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                    continue;
                }
                int old_value = 0;
                if ((old_scene < 0))
                {
                    old_value = (int)slider.Value;
                }
                else
                {
                    if(old_scene > 0 && old_scene < Config.DMXScenes.Count && channel>0 && channel< Config.DMXScenes[old_scene].Channelvalues.Length)
                    {
                        old_value = (int)((float)((float)(Config.DMXScenes[old_scene].Channelvalues[channel]) / (float)255.0) * 100);
                    }
                }
                if (slider.Value == old_value || force)
                {
                    if (Config.DMXSceneSetting > 0 && Config.DMXSceneSetting < Config.DMXScenes.Count && channel > 0 && channel < Config.DMXScenes[Config.DMXSceneSetting].Channelvalues.Length)
                    {
                        int value = (int)((float)((float)(Config.DMXScenes[Config.DMXSceneSetting].Channelvalues[channel]) / (float)255.0) * 100);
                        value = Math.Min(value, (int)slider.Maximum);
                        value = Math.Max (value, (int)slider.Minimum);
                        slider.ValueChanged -= form.Dimmer_Change;
                        slider.Value = value;
                        slider.ValueChanged += form.Dimmer_Change;
                    }
                }
            }
        }

        public void setActiveDMXScene(int index, bool force)
        {
            int old = Config.DMXSceneSetting;
            if (index < Config.DMXScenes.Count && index >= 0)
            {
                Config.DMXSceneSetting = index;
                Config.lastchangetime = DateTime.Now;
            }
            UpdateActiveDMXScene(old, force);
        }

        public void setConfig()
        {
            if (Config.showtime)
            {
                createTimePageElements();
            }
            if (Config.GastroUrl != null && Config.GastroUrl.Length >= 0)
            {
                form.GastronomieWebview.Source = new Uri(Config.GastroUrl, UriKind.Absolute);
            }
            form.UIControl.SelectTab(1);
            if (Config.showcolor)
            {
                createColorPageElements();
                form.UIControl.SelectTab(2);
            }

            int Pos_x, Pos_y;
            GendynamicAmbientButtons();
            GenNewPassword();
            if (Config.AmbienteBackgroundImage != null && Config.AmbienteBackgroundImage.Length > 2 && File.Exists(Config.AmbienteBackgroundImage))
            {
                form.AmbientePage.BackgroundImageLayout = ImageLayout.Zoom;
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.AmbienteBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.AmbientePage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            if (Config.ColorBackgroundImage != null && Config.ColorBackgroundImage.Length > 2 && File.Exists(Config.ColorBackgroundImage))
            {
                form.ColorPage.BackgroundImageLayout = ImageLayout.Zoom; 
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.ColorBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.ColorPage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            if (Config.MediaBackgroundImage != null && Config.MediaBackgroundImage.Length > 2 && File.Exists(Config.MediaBackgroundImage))
            {
                form.MediaPage.BackgroundImageLayout = ImageLayout.Zoom;
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.MediaBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.MediaPage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            if (Config.TimeBackgroundImage != null && Config.TimeBackgroundImage.Length > 2 && File.Exists(Config.TimeBackgroundImage))
            {
                form.TimePage.BackgroundImageLayout = ImageLayout.Zoom;
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.TimeBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.TimePage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            if (Config.ServiceBackgroundImage != null && Config.ServiceBackgroundImage.Length > 2 && File.Exists(Config.ServiceBackgroundImage))
            {
                form.ServicePage.BackgroundImageLayout = ImageLayout.Zoom;
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.ServiceBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.ServicePage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            if (Config.WartungBackgroundImage != null && Config.WartungBackgroundImage.Length > 2 && File.Exists(Config.WartungBackgroundImage))
            {
                form.WartungPage.BackgroundImageLayout = ImageLayout.Zoom;
                Task<Image>.Factory.StartNew(() =>
                {
                    return Bitmap.FromFile(Config.WartungBackgroundImage);
                }).ContinueWith(t =>
                {
                    form.WartungPage.BackgroundImage = t.Result;
                }, TaskScheduler.FromCurrentSynchronizationContext());
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
                SetupLabelofTrackbar(s, l, Config.slidernames[((int)s.Tag) - 1]);
            }
            Showonallsites();
            Application.DoEvents();
        }

        public void GendynamicServiceButtons()
        {
            form.ServicePage.SuspendLayout();
            int Pos_x, Pos_y;
            while (ServicePageButtons.Count > 0)
            {
                Button but = ServicePageButtons[0];
                but.Parent.Controls.Remove(but);
                but.Hide();
                ServicePageButtons.Remove(but);
                but.Dispose();
            }
            form.HowCanIHelpYouDescribtion.Text = Config.ServicesSettings[0].ShowText;
            form.HowCanIHelpYouDescribtion.Location = new Point((Constants.windowwidth / 2) - (form.HowCanIHelpYouDescribtion.Size.Width / 2), form.HowCanIHelpYouDescribtion.Location.Y);
            int Numhelps = Config.DMXScenes.Count;
            foreach (Constants.DMXScene sc in Config.DMXScenes)
            {
                if (sc.ShowText == null || sc.ShowText.Length <= 0)
                {
                    Numhelps--;
                }
            }
            if (Numhelps > Constants.maxscenes)
            {
                Logger.Print("Es wurden zu viele Service Einstellungen eingelesen.", Logger.MessageType.Ohne_Kategorie, Logger.MessageSubType.Notice);
            }
            Numhelps = Math.Min(Config.ServicesSettings.Count, Constants.maxhelps);
            int x = 0;
            for (int i = 1; i < Config.ServicesSettings.Count && x < Constants.maxhelps; i++)
            {
                Button bu = null;
                if (Config.ServicesSettings[i].ShowText == null || Config.ServicesSettings[i].ShowText.Length <= 0)
                {
                    Constants.createButton<Button>(null, null, null, "", Config.ServicesSettings[i], null, form, null, out bu);
                    bu.Hide();
                    Config.ServicesSettings[i].ButtonElement = bu;
                    continue;
                }
                GetDynamicPosition(Numhelps-1, x, out Pos_x, out Pos_y, 0, 3.5, false);
                Constants.createButton(Pos_x, Pos_y, ServicePageButtons, Config.ServicesSettings[i].ShowText, Config.ServicesSettings[i], form.ServicePage, form, form.Service_Request_Handle, out bu);
                Config.ServicesSettings[i].ButtonElement = bu;
                x++;
            }
            form.ServicePage.ResumeLayout(true);
        }

        public void GendynamicAmbientButtons()
        {
            form.AmbientePage.SuspendLayout();
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
            int Numscenes = Config.DMXScenes.Count;
            foreach(Constants.DMXScene sc in Config.DMXScenes)
            {
                if (sc.ShowText == null||sc.ShowText.Length<=0)
                {
                    Numscenes--;
                }
            }
            if(Numscenes > Constants.maxscenes)
            {
                Logger.Print("Es wurden zu viele DMX Szenen eingelesen. Die überschüssigen können nur über TCP angefragt werden.", Logger.MessageType.Licht, Logger.MessageSubType.Notice);
            }
            Numscenes = Math.Min(Numscenes, Constants.maxscenes);
            int x = 0;
            for (int i = 0; i < Config.DMXScenes.Count && x <= Constants.maxscenes; i++)
            {
                Constants.DMXScene scene = Config.DMXScenes[i];
                Button bu = null;
                if (Config.DMXScenes[i].ShowText == null || Config.DMXScenes[i].ShowText.Length <= 0)
                {
                    Constants.createButton<Button>(null, null, null, "", scene, null, form, null, out bu);
                    bu.Hide();
                    scene.ButtonElement = bu;
                    continue;
                }
                GetDynamicPosition(Numscenes, x, out Pos_x, out Pos_y, 0, 0, true);
                Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, scene.ShowText, scene, form.AmbientePage, form, form.Ambiente_Change_Handler, out bu);
                scene.ButtonElement = bu;
                bu.BringToFront();
                AmbientePagedynamicButtons.Add(bu);
                x++;
            }
            form.AmbientePage.ResumeLayout(true);
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
            for (int i = 0; i < Config.showLogo.Length && i < tabs.Count; i++)
            {
                if (Config.showLogo[i])
                {
                    PictureBox Logoview = new PictureBox();
                    Logoview.SizeMode = PictureBoxSizeMode.Zoom;
                    try
                    {
                        Logoview.Image = Image.FromFile(Config.LogoFilePath);
                    }
                    catch (IOException e)
                    {
                        MainForm.currentState = 7;
                        Logger.Print(e.Message, Logger.MessageType.Benutzeroberfläche, Logger.MessageSubType.Error);
                    }
                    Logoview.Size = new Size(Constants.Logoxsize, Constants.Logoysize);
                    SetEdgePosition(Logoview, Config.Logoposition);
                    Logoview.TabStop = false;
                    globalLogos.Add(Logoview);
                    tabs[i].Controls.Add(Logoview);
                    Logoview.Show();
                    Logoview.SendToBack();
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
            if (Config.showedgetime)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    if (i == 1 + ((Config.showcolor) ? 1 : 0) || (Config.showcolor && i == 4 + ((Config.showcolor) ? 1 : 0)))
                    {
                        continue;
                    }
                    Label Labeltimeview = new Label();
                    SetEdgePosition(Labeltimeview, Config.edgetimePosition);
                    Labeltimeview.AutoSize = true;
                    Labeltimeview.TabStop = false;
                    Labeltimeview.BackColor = Color.Transparent;
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
        }

        private Label createLabelforpage(int posx, int page)
        {
            Label Labelview = new Label();
            SetEdgePosition(Labelview, 3);
            Labelview.Location = new Point(posx, Labelview.Location.Y);
            Labelview.Font = Constants.Standart_font;
            Labelview.TabStop = false;
            Labelview.AutoSize = true;
            Labelview.BackColor = Color.Transparent;
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
                posy = (Constants.tabheight - Constants.EdgeItemposydist) - c.Size.Height;
            }
            else
            {
                posy = Constants.EdgeItemposydist;
            }
            c.Location = new Point(posx, posy);
        }

        private void GenNewPassword()
        {
            Constants.InvokeDelegate<object>([], new MyNoArgument(delegateGenNewPassword), form, Logger.MessageType.Hauptprogramm);
        }

        private object delegateGenNewPassword()
        {
            Config.Wifipassword = "";
            Task router = null;
            if (form != null && form.net != null && !Constants.noNet)
            {
                Task.Delay(15).Wait();
                router = Task.Run(() => {
                    Task ta = Task.Run(() => form.net.setuprouterpassword(form));
                    ta.Wait();
                    ta = Task.Run(() => form.net.setuprouterssid(form));
                    ta.Wait();
                    ta = Task.Run(() => form.net.wakeup(form));
                    ta.Wait();
                });
            }
            if (Config.WiFiSSID == null || Config.WiFiSSID.Length <= 0 || Config.Wifipassword == null || Config.Wifipassword.Length <= 0)
            {
                if (Constants.showdirectlypotentiallyfalsewificreds)
                {
                    Config.UseoldWiFicreds();
                }
            }
            if (form.loadscreen != null)
            {
                form.loadscreen.Debugtext($"Setting up Wifi Router", false);
            }

            setnewPassword();
            return null;
        }

        public void setnewPassword()
        {

            Constants.InvokeDelegate<object>([],new MyNoArgument(delegatesetnewPassword), form, Logger.MessageType.Hauptprogramm);
        }

        private object delegatesetnewPassword()
        {
            int Pos_x, Pos_y;
            GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0.55, 1.8, false);
            form.WiFiSSIDLabel.Location = new Point(Pos_x, Pos_y);
            form.WiFiSSIDLabel.Hide();

            GetDynamicPosition(5, 1, out Pos_x, out Pos_y, 0.55, 2.8, false);
            form.WiFiPasswortLabel.Location = new Point(Pos_x, Pos_y);
            form.WiFiPasswortLabel.Hide();
            if (form != null)
            {
                form.WiFiSSIDLabel.Text = Config.WiFiSSID;
                form.WiFiPasswortLabel.Text = Config.Wifipassword;
                if (Config.Wifipassword != null && Config.Wifipassword.Length > 0 && Config.WiFiSSID != null && Config.WiFiSSID.Length > 0)
                {
                    CreateMediaControllingElemets(2.3);
                    form.WiFiPasswortLabel.Show();
                    form.WiFiSSIDLabel.Show();
                    form.WiFiPasswordTitle.Show();
                    form.WiFiSSIDTitle.Show();
                    GC.KeepAlive(form.WiFiQRCodePicturebox);
                    form.generateQRCode(form.WiFiQRCodePicturebox, 20, false, (int)(Constants.Element_width * 1.4), true);

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
            Config.updateWificreds();
            return null;
        }

        public void createConsolePage()
        {
            form.ConsolePage.SuspendLayout();
            while (ConsoleElements.Count > 0)
            {
                Control rem = ConsoleElements[0];
                rem.Tag = -1;
                rem.Hide();
                form.UIControl.Controls.Remove(rem);
                ConsoleElements.Remove(rem);
            }
            int posx, posy = 0;
            form.UIControl.Controls.Add(form.ConsolePage);
            Logger.consoleshown = true;
            int elementsinwidth = 4;
            int curindex = 0;
            if(!Constants.noNet && form.net != null)
            {
                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 0, false);
                Label lab = new Label();
                lab.AutoSize = true;
                lab.Text = "TCP Message";
                lab.ForeColor = Constants.Text_color;
                lab.Location = new Point(posx, posy);
                lab.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point, 0);
                form.ConsolePage.Controls.Add(lab);
                ConsoleElements.Add(lab); 
                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 0.5, false);
                Label la = new Label();
                la.AutoSize = true;
                la.Text = $"Room: {Config.Room}";
                la.ForeColor = Constants.Text_color;
                la.Location = new Point(posx, posy);
                la.Font = Constants.Standart_font;
                form.ConsolePage.Controls.Add(la);
                ConsoleElements.Add(la);
                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1, false);
                form.tcptype = new ComboBox();
                for (int i = 0; i < Config.Typenames.Length; i++)
                {
                    form.tcptype.Items.Add(new Constants.ComboItem { Text = Config.Typenames[i], ID = i });
                }
                form.tcptype.Location = new Point(posx, posy);
                form.tcptype.Size = new Size(Constants.Element_width, Constants.Element_height);
                form.tcptype.SelectedIndexChanged += form.TCPMessage_Change_handler;
                ConsoleElements.Add(form.tcptype);
                form.ConsolePage.Controls.Add(form.tcptype);

                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1.5, false);
                form.CommandboxLabel = new TextBox();
                form.CommandboxLabel.PlaceholderText = "Label";
                form.CommandboxLabel.Size = new Size(Constants.Element_width, Constants.Element_height);
                form.CommandboxLabel.Location = new Point(posx, posy);
                form.CommandboxLabel.TextChanged += form.TCPMessage_Change_handler;
                ConsoleElements.Add(form.CommandboxLabel);
                form.ConsolePage.Controls.Add(form.CommandboxLabel);

                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 2, false);
                form.Commandboxid = new TextBox();
                form.Commandboxid.Size = new Size(Constants.Element_width, Constants.Element_height);
                form.Commandboxid.Location = new Point(posx, posy);
                form.Commandboxid.KeyPress += form.CommandId_KeyPress;
                form.Commandboxid.TextChanged += form.TCPMessage_Change_handler;
                ConsoleElements.Add(form.Commandboxid);
                form.ConsolePage.Controls.Add(form.Commandboxid);

                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 2.5, false);
                form.Commandboxvalues = new TextBox();
                form.Commandboxvalues.PlaceholderText = "values (komma seperated)";
                form.Commandboxvalues.Size = new Size(Constants.Element_width, Constants.Element_height);
                form.Commandboxvalues.Location = new Point(posx, posy);
                form.Commandboxvalues.TextChanged += form.TCPMessage_Change_handler;
                ConsoleElements.Add(form.Commandboxvalues);
                form.ConsolePage.Controls.Add(form.Commandboxvalues);

                GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 3, false);
                Button bu = null;
                Constants.createButton(posx, posy, (List<Button>)null, "Send Message", "sendtcp", form.ConsolePage, form, form.sendTCPfromconsole, out bu);
                ConsoleElements.Add(bu);

                Point lp = new Point(0, 4);
                GetDynamicPosition(elementsinwidth, lp.X, out posx, out posy, 0, lp.Y, false);
                form.Messagepreview = new Label();
                form.Messagepreview.AutoSize = true;
                form.Messagepreview.Text = "";
                form.Messagepreview.ForeColor = Constants.Text_color;
                form.Messagepreview.Location = new Point(posx, posy);
                form.Messagepreview.Tag = new Point(0, 4);
                form.Messagepreview.Font = Constants.Standart_font;
                form.ConsolePage.Controls.Add(form.Messagepreview);
                ConsoleElements.Add(form.Messagepreview);

                curindex++;
            }
            else
            {
                elementsinwidth--;
            }

            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 0, false);
            Label l = new Label();
            l.AutoSize = true;
            l.Text = "Console";
            l.ForeColor = Constants.Text_color;
            l.Location = new Point(posx, posy);
            l.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point, 0);
            form.ConsolePage.Controls.Add(l);
            ConsoleElements.Add(l);

            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1, false);
            Logger.consoletype = new ComboBox();
            Array types = Enum.GetValues(typeof(Logger.MessageType));
            foreach (Logger.MessageType mt in types)
            {
                List<Logger.Log_Element> lle = Logger.getList(Logger.MTypetobyte<Logger.MessageType>(mt));
                if (lle != null && lle.Count > 0)
                {
                    Logger.consoletype.Items.Add(new Constants.ComboItem { Text = mt.ToString(), ID = Logger.MTypetobyte<Logger.MessageType>(mt) });
                }
            }
            Logger.consoletype.Location = new Point(posx, posy);
            Logger.consoletype.Size = new Size(Constants.Element_width, Constants.Element_height);
            Logger.consoletype.SelectedIndexChanged += form.comboconsoleItemchanged;
            Logger.consoletype.SelectedIndex = 0;
            ConsoleElements.Add(Logger.consoletype);
            form.ConsolePage.Controls.Add(Logger.consoletype);


            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1.5, false);
            Logger.consolesubtype = new ComboBox();
            Logger.consolesubtype.Items.Add(new Constants.ComboItem { Text = "Alle", ID = null });
            types = Enum.GetValues(typeof(Logger.MessageSubType));
            foreach (Logger.MessageSubType mt in types)
            {
                Logger.consolesubtype.Items.Add(new Constants.ComboItem { Text = mt.ToString(), ID = Logger.MTypetobyte<Logger.MessageSubType>(mt) });
            }
            Logger.consolesubtype.Location = new Point(posx, posy);
            Logger.consolesubtype.Size = new Size(Constants.Element_width, Constants.Element_height);
            Logger.consolesubtype.SelectedIndexChanged += form.comboconsoleItemchanged;
            Logger.consolesubtype.SelectedIndex = 0;
            ConsoleElements.Add(Logger.consolesubtype);
            form.ConsolePage.Controls.Add(Logger.consolesubtype);

            curindex++;
            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1, false);
            Logger.ConsoleTextscroll = createColorSlide(0);
            Logger.ConsoleTextscroll.ShowDivisionsText = false;
            Logger.ConsoleTextscroll.ShowSmallScale = false;
            Logger.ConsoleTextscroll.Orientation = Orientation.Vertical;
            Logger.ConsoleTextscroll.Location = new Point(posx, posy);
            Logger.ConsoleTextscroll.Size = new Size(Logger.ConsoleTextscroll.Size.Width, (Constants.Element_height + Constants.Element_y_padding) * 3);
            Logger.ConsoleTextscroll.Hide();
            Logger.ConsoleTextscroll.ValueChanged += form.consolescroll;
            ConsoleElements.Add(Logger.ConsoleTextscroll);
            form.ConsolePage.Controls.Add(Logger.ConsoleTextscroll);

            curindex++;
            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 0, false);
            form.Programmstate = new Label();
            form.Programmstate.AutoSize = true;
            form.Programmstate.Text = $"Programmstatus: {MainForm.currentState}";
            form.Programmstate.ForeColor = Constants.Text_color;
            form.Programmstate.Location = new Point(posx, posy);
            form.Programmstate.Font = new Font("Segoe UI", 23F, FontStyle.Regular, GraphicsUnit.Point, 0);
            form.ConsolePage.Controls.Add(form.Programmstate);
            ConsoleElements.Add(form.Programmstate);

            GetDynamicPosition(elementsinwidth, curindex, out posx, out posy, 0, 1, false);
            Label state = new Label();
            state.AutoSize = true;
            state.Text = "0: Something wrong\r\n1: All right\r\n2: Error with TCP\r\n3: Error with Monitors\r\n4: Error when communicating with router\r\n5: Error when communicating with gastro\r\n6: Error when communicating with ambient Light\r\n7: Config Error\r\n8: Other Error (Investigate LOG or BLOG)\r\n";
            state.ForeColor = Constants.Text_color;
            state.Location = new Point(posx, posy);
            state.Font = Constants.Standart_font;
            form.ConsolePage.Controls.Add(state);
            ConsoleElements.Add(state);

            form.resizeUIControlItems();
            form.ConsolePage.ResumeLayout(true);
        }

        public void removeConsolePage()
        {
            form.ConsolePage.SuspendLayout();
            form.UIControl.Controls.Remove(form.ConsolePage);
            Logger.consoleshown = false;
            form.vlc.toggleConsoleBox(false);
            form.resizeUIControlItems();
            while (ConsoleElements.Count > 0)
            {
                Control rem = ConsoleElements[0];
                rem.Tag = -1;
                rem.Hide();
                form.UIControl.Controls.Remove(rem);
                ConsoleElements.Remove(rem);
            }
            form.ConsolePage.ResumeLayout(true);
        }

        public void createButtonTester()
        {
            if (!Constants.showbuttontester)
            {
                return;
            }
            form.ButtonPage.SuspendLayout();

            form.Testbuttontext = new TextBox();
            form.Testbuttontext.Size = new Size(Constants.Element_width * 2, form.Testbuttontext.Size.Height);
            form.Testbuttontext.Location = new Point((int)((Constants.windowwidth / 4)*3 - (form.Testbuttontext.Size.Width / 2)), (int)((Constants.tabheight / 6) * 1 - (form.Testbuttontext.Size.Height / 2)));
            form.Testbuttontext.TextChanged += updatetestButton;
            form.Testbuttontext.PlaceholderText = "Button Text eingeben";
            form.ButtonPage.Controls.Add(form.Testbuttontext);

            Label l = new Label();
            l.AutoSize = true;
            l.ForeColor = Constants.Text_color;
            form.ButtonPage.Controls.Add(l);
            form.Testbuttonwidth = createColorSlide(500);
            form.Testbuttonwidth.Tag = (Label)l;
            form.Testbuttonwidth.Size = new Size(Constants.Element_width * 2, form.Testbuttonwidth.Size.Height);
            form.Testbuttonwidth.Location = new Point((int)((Constants.windowwidth / 4) * 3 - (form.Testbuttonwidth.Size.Width / 2)), (int)((Constants.tabheight / 6)*2 - (form.Testbuttonwidth.Size.Height / 2)));
            form.Testbuttonwidth.Value = Constants.Element_width;
            form.Testbuttonwidth.ValueChanged += updatetestButton;
            form.ButtonPage.Controls.Add(form.Testbuttonwidth);
            l = null;
            l = new Label();
            l.AutoSize = true;
            l.ForeColor = Constants.Text_color;
            form.ButtonPage.Controls.Add(l);
            form.Testbuttonheight = createColorSlide(500);
            form.Testbuttonheight.Tag = (Label)l;
            form.Testbuttonheight.Size = new Size(Constants.Element_width * 2, form.Testbuttonheight.Size.Height);
            form.Testbuttonheight.Location = new Point((int)((Constants.windowwidth / 4) * 3 - (form.Testbuttonheight.Size.Width / 2)), (int)((Constants.tabheight / 6) * 3 - (form.Testbuttonheight.Size.Height / 2)));
            form.Testbuttonheight.Value = Constants.Element_height;
            form.Testbuttonheight.ValueChanged += updatetestButton;
            form.ButtonPage.Controls.Add(form.Testbuttonheight);
            l = null;
            l = new Label();
            l.AutoSize = true;
            l.ForeColor = Constants.Text_color;
            form.ButtonPage.Controls.Add(l);
            form.TestbuttonborderRadius = createColorSlide(((int)form.Testbuttonwidth.Value > (int)form.Testbuttonheight.Value) ? (int)form.Testbuttonwidth.Value : (int)form.Testbuttonheight.Value);
            form.TestbuttonborderRadius.Tag = (Label)l;
            form.TestbuttonborderRadius.Size = new Size(Constants.Element_width * 2, form.TestbuttonborderRadius.Size.Height);
            form.TestbuttonborderRadius.Location = new Point((int)((Constants.windowwidth / 4) * 3 - (form.TestbuttonborderRadius.Size.Width / 2)), (int)((Constants.tabheight / 6) * 4 - (form.TestbuttonborderRadius.Size.Height / 2)));
            form.TestbuttonborderRadius.Value = (Constants.Element_width > Constants.Element_height) ? Constants.Element_width : Constants.Element_height;
            form.TestbuttonborderRadius.ValueChanged += updatetestButton;
            form.ButtonPage.Controls.Add(form.TestbuttonborderRadius);
            l = null;
            l = new Label();
            l.AutoSize = true;
            l.ForeColor = Constants.Text_color;
            form.ButtonPage.Controls.Add(l);
            form.TestbuttonborderWidth = createColorSlide(((int)form.Testbuttonwidth.Value > (int)form.Testbuttonheight.Value) ? (int)form.Testbuttonwidth.Value / 4 : (int)form.Testbuttonheight.Value / 4);
            form.TestbuttonborderWidth.Tag = (Label)l;
            form.TestbuttonborderWidth.Size = new Size(Constants.Element_width * 2, form.TestbuttonborderWidth.Size.Height);
            form.TestbuttonborderWidth.Location = new Point((int)((Constants.windowwidth / 4) * 3 - (form.TestbuttonborderWidth.Size.Width / 2)), (int)((Constants.tabheight / 6) * 5 - (form.TestbuttonborderWidth.Size.Height / 2)));
            form.TestbuttonborderWidth.Value = 0;
            form.TestbuttonborderWidth.ValueChanged += updatetestButton;
            form.ButtonPage.Controls.Add(form.TestbuttonborderWidth);

            form.ButtonPage.ResumeLayout(true);
            updatetestButton(null, null);
        }

        public void updatetestButton(object sender, EventArgs e)
        {
            RoundedButton b = new RoundedButton();
            if (form.Testbutton != null)
            {
                b = (RoundedButton)form.Testbutton;
            }
            else
            {
                b.BackColor = Constants.Button_color;
                b.AutoEllipsis = true;
                b.UseVisualStyleBackColor = true;
                b.Location = new Point((int)((Constants.windowwidth / 4) - (b.Size.Width / 2)), (int)((Constants.tabheight / 2) - (b.Size.Height / 2)));

                form.ButtonPage.Controls.Add(b);
                form.Testbutton = b;
            }
            if (form.Testbutton != null && form.Testbuttonwidth != null && form.Testbuttonheight != null && form.TestbuttonborderRadius != null && form.TestbuttonborderWidth != null)
            {
                b.Size = new Size((int)form.Testbuttonwidth.Value, (int)form.Testbuttonheight.Value);
                b.Text = form.Testbuttontext.Text;
                form.TestbuttonborderRadius.Maximum = ((int)form.Testbuttonwidth.Value > (int)form.Testbuttonheight.Value) ? (int)form.Testbuttonwidth.Value : (int)form.Testbuttonheight.Value;
                b.BorderRadius = (int)form.TestbuttonborderRadius.Value;
                form.TestbuttonborderWidth.Maximum = ((int)form.Testbuttonwidth.Value > (int)form.Testbuttonheight.Value) ? (int)form.Testbuttonwidth.Value / 4 : (int)form.Testbuttonheight.Value / 4;
                b.BorderWidth = (int)form.TestbuttonborderWidth.Value;

                SetupLabelofTrackbar(form.Testbuttonwidth, ((Label)(form.Testbuttonwidth.Tag)), form.Testbuttonwidth.Value + "");
                SetupLabelofTrackbar(form.Testbuttonheight, ((Label)(form.Testbuttonheight.Tag)), form.Testbuttonheight.Value + "");
                SetupLabelofTrackbar(form.TestbuttonborderRadius, ((Label)(form.TestbuttonborderRadius.Tag)), form.TestbuttonborderRadius.Value + "");
                SetupLabelofTrackbar(form.TestbuttonborderWidth, ((Label)(form.TestbuttonborderWidth.Tag)), form.TestbuttonborderWidth.Value + "");
            }
        }
    }
}
