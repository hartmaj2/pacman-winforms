namespace Pacman
{
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
        private void initializeTimers()
        {
            gameLoopTimer.Interval = 500;
            gameLoopTimer.Tick += UpdateGameLoop;
        }
        private void initializeForm()
        {
            initializeTimers();
            initializeStartButton();
        }
        private void initializeStartButton()
        {
            startButton.Text = "Start Game";
            startButton.Height = (ClientSize.Height / 10);
            startButton.Width = (ClientSize.Width / 5);
            startButton.Left = (ClientSize.Width - startButton.Width) / 2;
            startButton.Top = (ClientSize.Height - startButton.Height) / 2;
            startButton.Click += StartButton_Click;
            startButton.Visible = true;
            this.Controls.Add(startButton);
        }
        private void UpdateGameLoop(object sender, EventArgs e)
        {
            gameManager.Tick();
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