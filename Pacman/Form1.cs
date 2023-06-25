namespace Pacman
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initializeForm();
        }

        private GameManager gameManager;
        private void initializeForm()
        {
            this.Text = "Pacman Game " + count;
            initializeStartButton();
            initializeEndButton();
        }

        private void initializeStartButton()
        {
            startButton.Visible = true;
            gameManager = new GameManager(this);
            startButton.Left = (ClientSize.Width - startButton.Width) / 2;
            startButton.Top = (ClientSize.Height - startButton.Height) / 2;
        }

        private void initializeEndButton()
        {
            endButton.Visible = false;
            endButton.Left = (ClientSize.Width - endButton.Width) / 2;
            endButton.Top = (ClientSize.Height - endButton.Height) / 2;
        }

        public static int count = 0;

        private void gameLoopTimer_Tick(object sender, EventArgs e)
        {
            this.Text = "Pacman Game " + count;
            count++;
            gameManager.Tick();
            gameManager.Draw();

        }

        private void startButton_click(object sender, EventArgs e)
        {
            startButton.Visible = false;
            endButton.Visible = false;
            gameManager.Draw();
            gameLoopTimer.Enabled = true;

        }

        private void endButton_click(object sender, EventArgs e)
        {
            gameLoopTimer.Stop();
            Application.Exit();
        }
    }
}