
using QRCoder;
using System.Diagnostics;
using System.Windows.Forms;

namespace Spa_Interaction_Screen
{
    public class UIHelper
    {
        private MainForm form;
        private Config config;

        public List<TabPage> tabs = new List<TabPage>();

        public List<ColorSlider.ColorSlider> FormColorSlides = new List<ColorSlider.ColorSlider>();
        public List<PictureBox> Logos = new List<PictureBox>();
        public List<Control> AllElements = new List<Control>();

        public List<Button> AmbientePageButtons = new List<Button>();
        public List<Label> AmbientePageLabel = new List<Label>();
        public List<ColorSlider.ColorSlider> AmbientePageColorSlide = new List<ColorSlider.ColorSlider>();

        public List<Button> MediaPageButtons = new List<Button>();
        public List<Label> MediaPageLabel = new List<Label>();

        public List<Button> ServicePageButtons = new List<Button>();

        public List<Button> WartungPageButtons = new List<Button>();
        public List<Label> WartungPageLabel = new List<Label>();

        public List<Button> RestrictedPageButtons = new List<Button>();

        private const double Restrictedyoffset = 0.5;
        private int Restrictedycoord = (Constants.Element_height + Constants.Element_y_padding) + (int)((Constants.Element_height + Constants.Element_y_padding) * Restrictedyoffset);
        private int Restrictedstarty = (Constants.Element_height + Constants.Element_y_padding);

        public UIHelper(MainForm f, Config c)
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

            createRestrictedPageElements();
            removeRestrictedPageElements();

            createWartungPageElements();
            setConfig(config);
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
                tabs.Add(form.MediaPage);
                tabs.Add(form.ServicePage);
                tabs.Add(form.WartungPage);
                FormColorSlides = new List<ColorSlider.ColorSlider>();
                Logos = new List<PictureBox>();
                AllElements = new List<Control>();
            }

            //Ambiente Lists
            {
                AmbientePageButtons = new List<Button>();

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

        }

        public void createAmbientePageElements()
        {
            int Pos_x, Pos_y;

            GetDynamicPosition(3, 1, out Pos_x, out Pos_y, 0, 2, false);
            Constants.createButton(Pos_x, Pos_y, AmbientePageButtons, "Design Beleuchtung", 0, form.AmbientePage, form, form.Ambiente_Design_Handler);

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
            label.Location = new Point((Constants.windowwidth / 2) - (label.Size.Width / 2), Posy-15);

            form.colorWheelElement = new Cyotek.Windows.Forms.ColorWheel();
            form.colorWheelElement.Size = new Size((int)(Constants.windowwidth / 3), (int)(Constants.windowwidth / 3)); 
            form.ColorPage.Controls.Add(form.colorWheelElement);
            form.colorWheelElement.Location = new Point((Constants.windowwidth / 2) - (form.colorWheelElement.Size.Width / 2), ((Constants.tabheight - (label.Location.Y + label.Size.Height)) / 2) - (form.colorWheelElement.Size.Height / 2) + (label.Location.Y + label.Size.Height) - 15);
            form.colorWheelElement.ColorChanged += form.ColorChanged_Handler;
            form.ColorPage.BackColor = Constants.Background_color;
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

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0.2, 1.5, false);
            form.TVSettingsAmbienteButton = Constants.createButton(Constants.Element_width * 2, (int)(Constants.Element_height * 0.75), Pos_x, Pos_y, MediaPageButtons, "Ambiente Video", true, form.MediaPage, form, form.Content_Change_Handler);
            selectButton(form.TVSettingsAmbienteButton, true, Constants.selected_color);
            form.TVSettingsAmbienteButton.Name = "AmbientVideo";

            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0.2, 2.3, false);
            form.TVSettingsStreamingButton = Constants.createButton(Constants.Element_width * 2, (int)(Constants.Element_height * 0.75), Pos_x, Pos_y, MediaPageButtons, "Streaming Video", false, form.MediaPage, form, form.Content_Change_Handler);
            form.TVSettingsStreamingButton.Name = "StreamingVideo";

            SetupLabelofButton(form.TVSettingsAmbienteButton, form.TVSettingsTitle, "Video Einstellungen:");

            ColorSlider.ColorSlider newslider = null;
            GetDynamicPosition(5, 3, out Pos_x, out Pos_y, 0.2, 3.3, false);
            newslider = createColorSlide(100);
            newslider.Location = new Point(Pos_x, Pos_y);
            newslider.Size = new Size(Constants.Element_width * 2, (int)(Constants.Element_height * 0.75));
            newslider.Tag = 3;
            form.MediaPage.Controls.Add(newslider);
            FormColorSlides.Add(newslider);
            newslider.ValueChanged += form.AmbientVolume_Handler;
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
            form.RestrictedAreaDescribtion.Text = "Zugriff nur für Mitarbeiter";
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

            GetDynamicPosition(4, 1, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "Starte eine neue Session", "SessionStart", form.WartungPage, form, form.NewSession_Handler);
            /*
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Beende die aktuelle Session", "SessionEnd", form.WartungPage, form, null);
            */
            GetDynamicPosition(4, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, Constants.ExitFullscreenText, "ToggleFullscreen", form.WartungPage, form, form.Programm_Exit_Handler);

            GetDynamicPosition(4, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 1 * Restrictedycoord, RestrictedPageButtons, "Programm zurücksetzen", "Reset", form.WartungPage, form, form.reset_Handler);
            GetDynamicPosition(4, 2, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 2 * Restrictedycoord, RestrictedPageButtons, "Schließe den Player", "VLCClose", form.WartungPage, form, form.closePlayer_Handler);

            GetDynamicPosition(4, 3, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, "Ausloggen", "Logout", form.WartungPage, form, form.logoutbutton_Handler);
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
                Constants.createButton(0, 0, null, Constants.ExitFullscreenText, "DebugExitButton", control, form, form.Programm_Exit_Handler);
            }
        }


        private string Passwordparts(string Password, Random rnd, int it)
        {
            for (int i = Password.Length; i < it; i++)
            {
                int index = rnd.Next(Constants.Sonderzeichen.Length + 10);
                if (index < Constants.Sonderzeichen.Length)
                {
                    Password += Constants.Sonderzeichen[index];
                }
                else
                {
                    Password += index - Constants.Sonderzeichen.Length;
                }
            }

            return Password;
        }

        private List<string> GetPasswortWords()
        {
            StreamReader stream = null;
            try
            {
                if (File.Exists(config.PasswordFilePath))
                {
                    stream = File.OpenText(config.PasswordFilePath);
                }
                else
                {
                    Debug.Print("No Password File found");
                }
            }
            catch (IOException ex)
            {
                Debug.Print(ex.ToString());
                Debug.Print("Could not open Passwort Words File");
            }
            if (stream == null)
            {
                return null;
            }
            List<string> words = new List<string>();
            string line = stream.ReadLine();
            while (line != null && line.Length > 0)
            {
                words.Add(line);
                if (stream.EndOfStream)
                {
                    break;
                }
                line = stream.ReadLine();
            }
            return words;
        }

        public void GetDynamicPosition(int TotalButtonCount, int CurrentButtonIndex, out int pos_X, out int pos_Y, double widthOffsetinButtons, double heightOffsetinButtons, bool useDoubleRow)
        {
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

            int offsetX = (CurrentButtonIndex % mod1) * (Constants.Element_width + Constants.Element_x_padding);

            pos_X = (Constants.windowwidth / 2) - (((Constants.Element_width + Constants.Element_x_padding) / 2) * mod2);
            pos_X += offsetX;

            offsetX = (int)(widthOffsetinButtons * (Constants.Element_width + Constants.Element_x_padding));
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

            int offsetY = mod1 * (Constants.Element_height + Constants.Element_y_padding);
            pos_Y = (Constants.tabheight / 2) - (((Constants.Element_height + Constants.Element_y_padding) / 2) * (mod2));
            pos_Y += offsetY;

            offsetY = (int)(heightOffsetinButtons * (Constants.Element_height + Constants.Element_y_padding));
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

        public void UpdateActiveDMXScene(int old_scene)
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
                    Debug.Print(ex.Message);
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
                if (slider.Value == old_value)
                {
                    slider.ValueChanged -= form.Dimmer_Change;
                    slider.Value = (int)((float)((float)(config.DMXScenes[config.DMXSceneSetting].Channelvalues[channel]) / (float)255.0) * 100);
                    slider.ValueChanged += form.Dimmer_Change;
                }
            }
        }

        public void setActiveDMXScene(int index)
        {
            if(config == null)
            {
                return;
            }
            int old = config.DMXSceneSetting;
            if (index < config.DMXScenes.Count && index >= 0)
            {
                config.DMXSceneSetting = index;
                config.lastchangetime = DateTime.Now;
            }
            UpdateActiveDMXScene(old);
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
                }
            }
            if (config.AmbienteBackgroundFilePath != null && config.AmbienteBackgroundFilePath.Length > 2 && File.Exists(config.AmbienteBackgroundFilePath))
            {
                form.AmbientePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.AmbientePage.BackgroundImage = Image.FromFile(config.AmbienteBackgroundFilePath);
            }
            GenNewPassword();

            if (config.MediaBackgroundFilePath != null && config.MediaBackgroundFilePath.Length > 2 && File.Exists(config.MediaBackgroundFilePath))
            {
                form.MediaPage.BackgroundImageLayout = ImageLayout.Zoom;
                form.MediaPage.BackgroundImage = Image.FromFile(config.MediaBackgroundFilePath);
            }
            if (config.TimeBackgroundFilePath != null && config.TimeBackgroundFilePath.Length > 2 && File.Exists(config.TimeBackgroundFilePath))
            {
                form.TimePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.TimePage.BackgroundImage = Image.FromFile(config.TimeBackgroundFilePath);
            }

            form.HowCanIHelpYouDescribtion.Text = config.ServicesSettings[0].ShowText;
            form.HowCanIHelpYouDescribtion.Location = new Point((Constants.windowwidth/2)-(form.HowCanIHelpYouDescribtion.Size.Width/2), form.HowCanIHelpYouDescribtion.Location.Y);

            int Numhelps = Math.Min(config.ServicesSettings.Count - 1, Constants.maxhelps);
            for (int i = 1; i < config.ServicesSettings.Count && i <= Constants.maxhelps; i++)
            {
                String[] help = { config.ServicesSettings[i].ShowText, config.ServicesSettings[i].JsonText };
                if (help != null)
                {
                    GetDynamicPosition(Numhelps, i - 1, out Pos_x, out Pos_y, 0, 3, false);
                    Constants.createButton(Pos_x, Pos_y, ServicePageButtons, help[0], help[1], form.ServicePage, form, form.Service_Request_Handle);
                }
            }
            if (config.ServiceBackgroundFilePath != null && config.ServiceBackgroundFilePath.Length > 2 && File.Exists(config.ServiceBackgroundFilePath))
            {
                form.ServicePage.BackgroundImageLayout = ImageLayout.Zoom;
                form.ServicePage.BackgroundImage = Image.FromFile(config.ServiceBackgroundFilePath);
            }
            UpdateActiveDMXScene(-1);

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
            while (Logos.Count > 0)
            {
                PictureBox Log = Logos[0];
                Log.Parent.Controls.Remove(Log);
                Log.Hide();
                Logos.Remove(Log);
                Log.Dispose();
            }
            int posx, posy = 0;
            if (config.Logoposition % 2 == 0)
            {
                posx = (Constants.windowwidth - Constants.Logoposxdist) - Constants.Logoxsize;
            }
            else
            {
                posx = Constants.Logoposxdist;
            }
            if (config.Logoposition > 2)
            {
                posy = (Constants.tabheight - Constants.Logoposydist) - Constants.Logoysize;
            }
            else
            {
                posy = Constants.Logoposydist;
            }
            for (int i = 0;i < config.showLogo.Length;i++)
            {
                if (config.showLogo[i])
                {
                    PictureBox Logoview = new PictureBox();
                    Logoview.SizeMode = PictureBoxSizeMode.Zoom;
                    try
                    {
                        Logoview.Image = Image.FromFile(config.LogoFilePath);
                    }catch(IOException e) 
                    { 
                        Debug.Print(e.Message);
                    }
                    Logoview.Size = new Size(Constants.Logoxsize, Constants.Logoysize);
                    Logoview.Location = new Point(posx, posy);
                    Logoview.TabStop = false;
                    Logos.Add(Logoview);
                    tabs[i].Controls.Add(Logoview);
                    Logoview.BringToFront();
                }
            }
        }

        private async void GenNewPassword()
        {
            config.password = "";
            /*
            List<string> strings = GetPasswortWords();

            if (strings == null && strings.Count <= 90)
            {
                Random r = new Random();
                Debug.Print("Fallback random PW generator");
                for (int i = 0; i < 10; i++)
                {
                    if (r.Next(0, 2) > 0)
                    {
                        config.password += Convert.ToChar(r.Next(65, 91));
                    }
                    else
                    {
                        config.password += Convert.ToChar(r.Next(48, 58));
                    }
                }
            }
            else
            {
                Random rnd = new();
                int it = rnd.Next(3);
                config.password = Passwordparts(config.password, rnd, it);
                config.password += strings[rnd.Next(strings.Count)];
                if (config.password.Length <= 7)
                {
                    config.password = Passwordparts(config.password, rnd, config.password.Length + rnd.Next(2));
                    config.password += strings[rnd.Next(strings.Count)];
                }
                config.password = Passwordparts(config.password, rnd, Constants.PasswordLength + rnd.Next(3));
            }

            form.loadscreen.Debugtext($"Sending Password \"{config.password}\" to the router", true);
            */
            
            if (form != null && form.net != null)
            {
                form.net.setuprouter(form);
            }

            form.loadscreen.Debugtext($"Setting up Wifi Router", false);

            int Pos_x, Pos_y; 
            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 1.8, false);
            form.WiFiSSIDLabel.Location = new Point(Pos_x, Pos_y);

            GetDynamicPosition(5, 2, out Pos_x, out Pos_y, 0.05, 2.8, false);
            form.WiFiPasswortLabel.Location = new Point(Pos_x, Pos_y);

            form.WiFiSSIDLabel.Text = config.WiFiSSID;

            form.WiFiPasswortLabel.Text = config.password;

            form.generateQRCode(form.WiFiQRCodePicturebox, 20, false, (int)(Constants.Element_width * 1.5), true);

            form.loadscreen.Debugtext($"", false);
        }

        public void setnewPassword()
        {
            if (form != null)
            {
                form.WiFiPasswortLabel.Text = config.password;
                form.generateQRCode(form.WiFiQRCodePicturebox, 20, false, (int)(Constants.Element_width * 1.5), true);
                if (form.vlc != null)
                {
                    form.vlc.newsession();
                }
            }
        }


        private void setConfigRestricted(Config config)
        {
            if (config == null)
            {
                return;
            }
            int Pos_x, Pos_y;

            GetDynamicPosition(4, 0, out Pos_x, out Pos_y, 0, Restrictedyoffset, false);
            Button b = null;
            Constants.createButton(Pos_x, Restrictedstarty + 0 * Restrictedycoord, RestrictedPageButtons, config.DMXScenes[1].ShowText, config.DMXScenes[1], form.WartungPage, form, form.Ambiente_Change_Handler, out b);
            config.DMXScenes[1].ButtonElement = b;
            selectButton(b, config.DMXSceneSetting == 1, Constants.selected_color);
        }

    }
}
