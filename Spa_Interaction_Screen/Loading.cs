using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.LinkLabel;

namespace Spa_Interaction_Screen
{
    public partial class Loading : CForm
    {
        private MainForm form;
        private Label progress;
        private ProgressBar progressBar;
        private Label DebugText;
        private Button exit_b;

        private delegate void MyDebugText(String Text, bool show);

        public Loading(MainForm f, Screen screen)
        {
            InitializeComponent();
            this.HandleCreated += new EventHandler((sender, args) =>
            {
                HandleCreate = true;
            });
            form = f;
            form.EnterFullscreen(this, screen, false);
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
            object[] delegateArray = new object[2];
            delegateArray[0] = Text;
            delegateArray[1] = show;
            if (HandleCreate)
            {
                try
                {
                    this.Invoke(new MyDebugText(delegateDebugtext), delegateArray);
                }
                catch (InvalidOperationException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Logger.Print("Debugtext", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    delegateDebugtext(Text, show);
                }
            }
            else
            {
                try
                {
                    delegateDebugtext(Text, show);
                }
                catch (InvalidOperationException ex)
                {
                    MainForm.currentState = 7;
                    Logger.Print(ex.Message, Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Error);
                    Logger.Print("Debugtext", Logger.MessageType.Hauptprogramm, Logger.MessageSubType.Notice);
                    this.Invoke(new MyDebugText(delegateDebugtext), delegateArray);
                }
            }
        }

        private void delegateDebugtext(String Text, bool show)
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
            progressBar.Value = percentage;
            updateloadGUI();
        }

    }
}
