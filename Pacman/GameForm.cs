namespace Pacman
{
    public static class FormConstantsManager
    {
        public const string gameFormText = "Pacman Game";

        public const int startButtonHeightValue = 10;
        public const int startButtonWidthValue = 5;
        public const string startButtonText = "Start Game";

        public const int gameLoopTimerInterval = 10;
    }
    public partial class GameForm : Form
    {

        private GameManager gameManager;
        private System.Windows.Forms.Timer gameLoopTimer = new System.Windows.Forms.Timer();
        private Button startButton = new Button();
        public GameForm()
        {
            InitializeComponent();
            gameManager = new GameManager(this);
            this.KeyPreview = true; // makes sure that the form receives key events before their are passed to other components with focus
            initializeForm();

        }
        private void initializeForm()
        {
            this.Text = FormConstantsManager.gameFormText;
            initializeTimer();
            initializeStartButton();
        }
        private void initializeTimer()
        {
            gameLoopTimer.Interval = FormConstantsManager.gameLoopTimerInterval;
            gameLoopTimer.Tick += UpdateGameLoop;
        }
        private void initializeStartButton()
        {
            startButton.Text = FormConstantsManager.startButtonText;
            startButton.Height = (ClientSize.Height / FormConstantsManager.startButtonHeightValue);
            startButton.Width = (ClientSize.Width / FormConstantsManager.startButtonWidthValue);
            startButton.Left = (ClientSize.Width - startButton.Width) / 2;
            startButton.Top = (ClientSize.Height - startButton.Height) / 2;
            startButton.Click += StartButton_Click;
            startButton.Visible = true;
            this.Controls.Add(startButton);
        }
        private void UpdateGameLoop(object sender, EventArgs e)
        {
            gameManager.TweenTick();
            gameManager.Draw();
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            startButton.Visible = false;
            gameManager.Draw();
            gameLoopTimer.Enabled = true;

        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Direction newDirection = Direction.None;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newDirection = Direction.Up;
                    break;
                case Keys.Right:
                    newDirection = Direction.Right;
                    break;
                case Keys.Down:
                    newDirection = Direction.Down;
                    break;
                case Keys.Left:
                    newDirection = Direction.Left;
                    break;
            }

            gameManager.SetHeroNextDirection(newDirection);

        }
    }
}