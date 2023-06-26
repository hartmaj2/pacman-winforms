namespace Pacman
{
    public static class FormConstantsManager
    {
        public const string gameFormText = "Pacman Game";

        public const int startButtonHeightValue = 10;
        public const int startButtonWidthValue = 5;
        public const string startButtonText = "Start Game";

        public const int gameLoopTimerInterval = 10;
        //public const int tweenTicksPerDiscreteTick = 10;
    }
    public partial class GameForm : Form
    {

        private int tweenTickCounter = 0;

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
            //tweenTickCounter++;
            //if (tweenTickCounter > FormConstantsManager.tweenTicksPerDiscreteTick)
            //{
            //    gameManager.DiscreteTick();
            //    tweenTickCounter = 0;
            //}
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

            Direction newDirection = new Direction(0, 0);

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newDirection = new Direction(0, -1);
                    break;
                case Keys.Right:
                    newDirection = new Direction(1, 0);
                    break;
                case Keys.Down:
                    newDirection = new Direction(0, 1);
                    break;
                case Keys.Left:
                    newDirection = new Direction(-1, 0);
                    break;
            }

            gameManager.SetHeroDirection(newDirection);

        }
    }
}