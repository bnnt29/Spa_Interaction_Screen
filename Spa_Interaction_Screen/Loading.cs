using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.LinkLabel;
using static QRCoder.PayloadGenerator;

namespace Spa_Interaction_Screen
{
    public partial class Loading : CForm
    {
        private MainForm form;
        private Label progress;
        private ProgressBar progressBar;
        private Label DebugText;
        private Button exit_b;

        private delegate object MyDebugText(String Text, bool show);
        private delegate object MyupdateProgress(int percentage);

        public Loading(MainForm f, Screen screen) : base()
        {
            InitializeComponent();
            form = f;
            form.EnterFullscreen(this, screen);

            this.BackColor = Constants.Background_color;
            addcomponents();
            exit_b = Constants.createButton(Constants.Element_width, Constants.Element_height, 0, 0, (List<Button>)null, "ExitProgramm", null, this, null, form.ExitProgramm);
            exit_b.Location = new Point((this.Size.Width/2)-(exit_b.Size.Width/2),(progressBar.Location.Y-exit_b.Height)-10);
            exit_b.Hide();
            updateloadGUI();
        }

        private void addcomponents()
        {
            progressBar = new ProgressBar();
            progressBar.Size = new Size((int)(this.Size.Width / 3) * 2, 50);
            progressBar.Location = new Point((this.Size.Width / 2) - (progressBar.Size.Width / 2), (this.Size.Height / 2) - (progressBar.Size.Height / 2));
            this.Controls.Add(progressBar);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
        }

        public void Debugtext(String Text, bool show)
        {
            Constants.InvokeDelegate<object>([Text, show], new MyDebugText(delegateDebugtext), this);
        }

        private object delegateDebugtext(String Text, bool show)
        {
            if (DebugText == null)
            {
                DebugText = new Label();
                DebugText.AutoSize = true;
                DebugText.ForeColor = Constants.Text_color;
                this.Controls.Add(DebugText);
                DebugText.Show();
            }
            DebugText.Text = Text;
            DebugText.Location = new Point((this.Size.Width / 2) - (DebugText.Size.Width / 2), progressBar.Location.Y + progressBar.Size.Height + 10);
            if (show)
            {
                DebugText.Show();
                MainForm.currentState = 0;
                Logger.Print($"DebugText: {Text}", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
            }
            else
            {
                DebugText.Hide();
            }
            updateloadGUI();
            return null;
        }

        public void updateloadGUI()
        {
            /*
            this.Invalidate();
            this.Update();
            this.Refresh();
            */
            Application.DoEvents();
        }

        public void exitp(bool show)
        {
            if(show)
            {
                exit_b.Show();  
            }
            else
            {
                exit_b.Hide();
            }
            updateloadGUI();
        }

        public void exit_Handler(object sender, EventArgs e)
        {
            form.ExitProgramm(null, null);
            Application.Exit();
        }

        public void updateProgress(int percentage)
        {
            Constants.InvokeDelegate<object>([percentage], new MyupdateProgress(delegateupdateProgress), this);
        }

        public object delegateupdateProgress(int percentage)
        {
            progressBar.Value = percentage;
            updateloadGUI();
            return null;
        }


        public override void OnFormClosed(object sender, EventArgs e)
        {
            if (form != null)
            {
                if (form.loadscreen != null)
                {
                    form.loadscreen.Dispose();
                    form.loadscreen = null;
                }
            }
            this.Hide();
            Logger.Print("Shutdown Loading", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
        }
    }
}
