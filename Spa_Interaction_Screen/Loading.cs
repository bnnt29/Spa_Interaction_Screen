namespace Spa_Interaction_Screen
{
    public partial class Loading : Form
    {

        private Label progress;
        private ProgressBar progressBar;

        public Loading(MainForm f, Screen screen)
        {
            InitializeComponent();
            f.EnterFullscreen(this, screen);
            this.BackColor = Color.Black;
            addcomponents();
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

        public void updateProgress(int percentage)
        {
            progressBar.Value = percentage;
        }

    }
}
