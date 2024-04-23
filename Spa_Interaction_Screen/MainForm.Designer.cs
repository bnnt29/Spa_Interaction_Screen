using Microsoft.VisualBasic.FileIO;
using System.CodeDom;
using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System;
using System.ComponentModel.DataAnnotations;

using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Microsoft.Web.WebView2.Core;
using ColorSlider;
using Cyotek.Windows.Forms;


namespace Spa_Interaction_Screen
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        /// 

        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            UIControl = new TabControl();
            AmbientePage = new TabPage();
            TimePage = new TabPage();
            GastronomiePage = new TabPage();
            GastronomieWebview = new Microsoft.Web.WebView2.WinForms.WebView2();
            MediaPage = new TabPage();
            WiFiSSIDTitle = new Label();
            WiFiSSIDLabel = new Label();
            WiFiPasswordTitle = new Label();
            WiFiPasswortLabel = new Label();
            WiFiQRCodePicturebox = new PictureBox();
            TVSettingsTitle = new Label();
            ServicePage = new TabPage();
            HowCanIHelpYouDescribtion = new Label();
            WartungPage = new TabPage();
            WartungCodeField = new Label();
            RestrictedAreaDescribtion = new Label();
            RestrictedAreaTitle = new Label();
            ColorPage = new TabPage();
            UIControl.SuspendLayout();
            AmbientePage.SuspendLayout();
            GastronomiePage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GastronomieWebview).BeginInit();
            MediaPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)WiFiQRCodePicturebox).BeginInit();
            TimePage.SuspendLayout();
            ColorPage.SuspendLayout();
            ServicePage.SuspendLayout();
            WartungPage.SuspendLayout();
            SuspendLayout();
            // 
            // UIControl
            // 
            UIControl.Alignment = TabAlignment.Top;
            UIControl.Controls.Add(AmbientePage);
            UIControl.Controls.Add(ColorPage);
            UIControl.Controls.Add(GastronomiePage);
            UIControl.Controls.Add(MediaPage);
            UIControl.Controls.Add(TimePage);
            UIControl.Controls.Add(ServicePage);
            UIControl.Controls.Add(WartungPage);
            UIControl.Dock = DockStyle.Fill;
            UIControl.Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);

            UIControl.ItemSize = new Size(Constants.windowwidth / 6, Constants.windowheight-Constants.tabheight);
            UIControl.Location = new Point(0, 0);
            UIControl.Name = "UIControl";
            UIControl.SelectedIndex = 0;
            UIControl.Size = new Size(Constants.windowwidth, Constants.windowheight);
            UIControl.SizeMode = TabSizeMode.Fixed;
            UIControl.TabIndex = 4;
            UIControl.SelectedIndexChanged += logoutTab_Handler;
            // 
            // AmbientePage
            // 
            AmbientePage.BackColor = Constants.Background;
            AmbientePage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AmbientePage.Location = new Point(4, 4);
            AmbientePage.Name = "AmbientePage";
            ContextMenuStrip contextMenuStrip = new ContextMenuStrip();//TODO
            AmbientePage.Padding = new Padding(3);
            AmbientePage.RightToLeft = RightToLeft.No;
            AmbientePage.Size = new Size(1396, 678);
            AmbientePage.TabIndex = 1;
            AmbientePage.Text = "Ambiente";
            // 
            // ColorPage
            // 
            ColorPage.BackColor = Constants.Background;
            ColorPage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ColorPage.ForeColor = Constants.Text;
            ColorPage.Location = new Point(4, 4);
            ColorPage.Name = "ColorWheel";
            ColorPage.Padding = new Padding(3);
            ColorPage.Size = new Size(1396, 678);
            ColorPage.TabIndex = 6;
            ColorPage.Text = "Farbe";
            // 
            // GastronomiePage
            // 
            GastronomiePage.Controls.Add(GastronomieWebview);
            GastronomiePage.BackColor = Constants.Background;
            GastronomiePage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            GastronomiePage.ForeColor = Constants.Text;
            GastronomiePage.Location = new Point(4, 4);
            GastronomiePage.Name = "GastronomiePage";
            GastronomiePage.Padding = new Padding(3);
            GastronomiePage.Size = new Size(1396, 678);
            GastronomiePage.TabIndex = 2;
            GastronomiePage.Text = "Gastronomie";
            // 
            // GastronomieWebview
            // 
            GastronomieWebview.AllowExternalDrop = true;
            GastronomieWebview.CreationProperties = null;
            GastronomieWebview.DefaultBackgroundColor = Constants.Background;
            GastronomieWebview.Dock = DockStyle.Fill;
            GastronomieWebview.Location = new Point(3, 3);
            GastronomieWebview.Name = "GastronomieWebview";
            GastronomieWebview.Size = new Size(1390, 672);
            GastronomieWebview.Source = new Uri(Constants.PreConfigGastroURL, UriKind.Absolute);
            GastronomieWebview.TabIndex = 1;
            GastronomieWebview.ZoomFactor = 1D;
            // 
            // MediaPage
            // 
            MediaPage.Controls.Add(WiFiSSIDTitle);
            MediaPage.Controls.Add(WiFiSSIDLabel);
            MediaPage.Controls.Add(WiFiPasswordTitle);
            MediaPage.Controls.Add(WiFiPasswortLabel);
            MediaPage.Controls.Add(WiFiQRCodePicturebox);
            MediaPage.Controls.Add(TVSettingsTitle);
            MediaPage.BackColor = Constants.Background;
            MediaPage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MediaPage.ForeColor = Constants.Text;
            MediaPage.Location = new Point(4, 4);
            MediaPage.Name = "MediaPage";
            MediaPage.Padding = new Padding(3);
            MediaPage.Size = new Size(1396, 678);
            MediaPage.TabIndex = 3;
            MediaPage.Text = "Media";
            // 
            // WiFiSSIDTitle
            // 
            WiFiSSIDTitle.AutoSize = true;
            WiFiSSIDTitle.BackColor = Constants.Background;
            WiFiSSIDTitle.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            WiFiSSIDTitle.ForeColor = Constants.Text;
            WiFiSSIDTitle.Location = new Point(598, 244);
            WiFiSSIDTitle.Name = "WiFiSSIDTitle";
            WiFiSSIDTitle.Size = new Size(117, 25);
            WiFiSSIDTitle.TabIndex = 1;
            WiFiSSIDTitle.Text = "WLAN SSID:";
            // 
            // WiFiSSIDLabel
            // 
            WiFiSSIDLabel.AutoSize = true;
            WiFiSSIDLabel.ForeColor = Constants.Text;
            WiFiSSIDLabel.Location = new Point(598, 293);
            WiFiSSIDLabel.Name = "WiFiSSIDLabel";
            WiFiSSIDLabel.Size = new Size(191, 25);
            WiFiSSIDLabel.TabIndex = 2;
            WiFiSSIDLabel.Text = "Spa Placeholder SSDI";
            // 
            // WiFiPasswordTitle
            // 
            WiFiPasswordTitle.AutoSize = true;
            WiFiPasswordTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            WiFiPasswordTitle.ForeColor = Constants.Text;
            WiFiPasswordTitle.Location = new Point(598, 347);
            WiFiPasswordTitle.Name = "WiFiPasswordTitle";
            WiFiPasswordTitle.Size = new Size(148, 25);
            WiFiPasswordTitle.TabIndex = 3;
            WiFiPasswordTitle.Text = "WLAN Passwort";
            // 
            // WiFiPasswortLabel
            // 
            WiFiPasswortLabel.AutoSize = true;
            WiFiPasswortLabel.ForeColor = Constants.Text;
            WiFiPasswortLabel.Location = new Point(598, 397);
            WiFiPasswortLabel.Name = "WiFiPasswortLabel";
            WiFiPasswortLabel.Size = new Size(227, 25);
            WiFiPasswortLabel.TabIndex = 4;
            WiFiPasswortLabel.Text = "Spa Placeholder Passwort";
            // 
            // WiFiQRCodePicturebox
            // 
            WiFiQRCodePicturebox.Image = null;
            try
            {
                WiFiQRCodePicturebox.Image = Image.FromFile(@"C:\Users\Berni\Documents\GitHub\Spa_Interaction_Screen\QRplaceholderstillcreating.png");
            }catch(IOException e) 
            {
                Debug.Print(e.Message);
            }
            WiFiQRCodePicturebox.BackColor = Constants.Background;
            WiFiQRCodePicturebox.Location = new Point(124, 164);
            WiFiQRCodePicturebox.Name = "WiFiQRCodePicturebox";
            WiFiQRCodePicturebox.Size = new Size(424, 332);
            WiFiQRCodePicturebox.SizeMode = PictureBoxSizeMode.Zoom;
            WiFiQRCodePicturebox.TabIndex = 5;
            WiFiQRCodePicturebox.TabStop = false;
            // 
            // TVSettingsTitle
            // 
            TVSettingsTitle.AutoSize = true;
            TVSettingsTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            TVSettingsTitle.ForeColor = Constants.Text;
            TVSettingsTitle.Location = new Point(906, 175);
            TVSettingsTitle.Name = "TVSettingsTitle";
            TVSettingsTitle.Size = new Size(192, 25);
            TVSettingsTitle.TabIndex = 6;
            TVSettingsTitle.Text = "TV Video Einstellung:";
            // 
            // TimePage
            // 
            TimePage.BackColor = Constants.Background;
            TimePage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TimePage.ForeColor = Constants.Text;
            TimePage.Location = new Point(4, 4);
            TimePage.Name = "TimePage";
            TimePage.Padding = new Padding(3);
            TimePage.Size = new Size(1396, 678);
            TimePage.TabIndex = 6;
            TimePage.Text = "Uhr";
            // 
            // ServicePage
            // 
            ServicePage.Controls.Add(HowCanIHelpYouDescribtion);
            ServicePage.BackColor = Constants.Background;
            ServicePage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ServicePage.ForeColor = Constants.Text;
            ServicePage.Location = new Point(4, 4);
            ServicePage.Name = "ServicePage";
            ServicePage.Padding = new Padding(3);
            ServicePage.Size = new Size(1396, 678);
            ServicePage.TabIndex = 4;
            ServicePage.Text = "Service";
            // 
            // HowCanIHelpYouDescribtion
            // 
            HowCanIHelpYouDescribtion.AutoSize = true;
            HowCanIHelpYouDescribtion.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            HowCanIHelpYouDescribtion.ForeColor = Constants.Text;
            HowCanIHelpYouDescribtion.Location = new Point(526, 389);
            HowCanIHelpYouDescribtion.Name = "HowCanIHelpYouDescribtion";
            HowCanIHelpYouDescribtion.Size = new Size(226, 25);
            HowCanIHelpYouDescribtion.TabIndex = 1;
            HowCanIHelpYouDescribtion.Text = "Was kann ich für Sie tun?";
            // 
            // WartungPage
            // 
            WartungPage.Controls.Add(WartungCodeField);
            WartungPage.Controls.Add(RestrictedAreaDescribtion);
            WartungPage.Controls.Add(RestrictedAreaTitle);
            WartungPage.BackColor = Constants.Background;
            WartungPage.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
            WartungPage.ForeColor = Constants.Text;
            WartungPage.Location = new Point(4, 4);
            WartungPage.Name = "WartungPage";
            WartungPage.Padding = new Padding(3);
            WartungPage.Size = new Size(1396, 678);
            WartungPage.TabIndex = 5;
            WartungPage.Text = "Wartung";
            // 
            // WartungCodeField
            // 
            WartungCodeField.AutoSize = true;
            WartungCodeField.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            WartungCodeField.ForeColor = Constants.Text;
            WartungCodeField.Location = new Point(577, 58);
            WartungCodeField.Name = "WartungCodeField";
            WartungCodeField.Size = new Size(89, 25);
            WartungCodeField.TabIndex = 1;
            WartungCodeField.Text = "Enter Pin";
            // 
            // RestrictedAreaDescribtion
            // 
            RestrictedAreaDescribtion.AutoSize = true;
            RestrictedAreaDescribtion.Font = new Font("Segoe UI", 14F, FontStyle.Italic, GraphicsUnit.Point, 0);
            RestrictedAreaDescribtion.ForeColor = Constants.Text;
            RestrictedAreaDescribtion.Location = new Point(511, 173);
            RestrictedAreaDescribtion.Name = "RestrictedAreaDescribtion";
            RestrictedAreaDescribtion.Size = new Size(249, 25);
            RestrictedAreaDescribtion.TabIndex = 2;
            RestrictedAreaDescribtion.Text = "Zugriff nur für Mitarbeitende";
            // 
            // RestrictedAreaTitle
            // 
            RestrictedAreaTitle.AutoSize = true;
            RestrictedAreaTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point, 0);
            RestrictedAreaTitle.ForeColor = Constants.Text;
            RestrictedAreaTitle.Location = new Point(438, 263);
            RestrictedAreaTitle.Name = "RestrictedAreaTitle";
            RestrictedAreaTitle.Size = new Size(435, 41);
            RestrictedAreaTitle.TabIndex = 2;
            RestrictedAreaTitle.Text = "Zugriff nur für Mitarbeitende";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1404, 761);
            ControlBox = false;
            Controls.Add(UIControl);
            Name = "Form1";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Ambiente";
            TopMost = true;
            WindowState = FormWindowState.Maximized;
            Load += Main_Load;
            UIControl.ResumeLayout(false);
            AmbientePage.ResumeLayout(false);
            AmbientePage.PerformLayout();
            ColorPage.ResumeLayout(false);
            ColorPage.PerformLayout();
            GastronomiePage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GastronomieWebview).EndInit();
            MediaPage.ResumeLayout(false);
            MediaPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)WiFiQRCodePicturebox).EndInit();
            TimePage.ResumeLayout(false);
            TimePage.PerformLayout();
            ServicePage.ResumeLayout(false);
            ServicePage.PerformLayout();
            WartungPage.ResumeLayout(false);
            WartungPage.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public System.ComponentModel.BackgroundWorker backgroundWorker1;
        public System.ComponentModel.BackgroundWorker backgroundWorker2;

        //TabControl
        public TabControl UIControl;

        //UIControl Pages
        public TabPage AmbientePage;
        public TabPage ColorPage;
        public TabPage GastronomiePage;
        public TabPage MediaPage;
        public TabPage TimePage;
        public TabPage ServicePage;
        public TabPage WartungPage;

        //AmbientePage
        public Label Dimmer1ColorSliderDescribtion;
        public Label Dimmer2ColorSliderDescribtion;
        public Label AmbientelautstärkeColorSliderDescribtion;

        //ColorPage
        public ColorWheel colorWheelElement;

        //GastronomiePage
        public Microsoft.Web.WebView2.WinForms.WebView2 GastronomieWebview;

        //MediaPage
        public Label WiFiSSIDTitle;
        public Label WiFiSSIDLabel;
        public Label WiFiPasswordTitle;
        public Label WiFiPasswortLabel;
        public PictureBox WiFiQRCodePicturebox;
        public Label TVSettingsTitle;
        public Button TVSettingsAmbienteButton;
        public Button TVSettingsStreamingButton;
        public Label TVSettingsVolumeColorSliderDescribtion;

        //TimePage
        public Label clock;
        public Label timer;
        public Label sessionEnd;

        //ServicePage
        public Label HowCanIHelpYouDescribtion;

        //WartungPage
        public Label WartungCodeField;
        public Label RestrictedAreaDescribtion;

        //RestrictedPage
        public Label RestrictedAreaTitle;

    }
}
