namespace Pacman
{
    public partial class GameForm : Form
    {

        private GameManager gameManager;
        private System.Windows.Forms.Timer tweeningTimer = new System.Windows.Forms.Timer();
        public GameForm()
        {
            InitializeComponent();
            gameManager = new GameManager(this);
            this.KeyPreview = true; // makes sure that the form receives key events before their are passed to other components with focus
            initializeForm();

        }
        private void initializeTimers()
        {
            tweeningTimer.Interval = 1000;
            tweeningTimer.Tick += tweenUpdate;
        }
        private void initializeForm()
        {
            initializeTimers();
            initializeStartButton();
            initializeEndButton();
        }
        private void initializeStartButton()
        {
            startButton.Visible = true;
            startButton.Left = (ClientSize.Width - startButton.Width) / 2;
            startButton.Top = (ClientSize.Height - startButton.Height) / 2;
        }
        private void initializeEndButton()
        {
            endButton.Visible = false;
            endButton.Left = (ClientSize.Width - endButton.Width) / 2;
            endButton.Top = (ClientSize.Height - endButton.Height) / 2;
        }
        private void tweenUpdate(object sender, EventArgs e)
        {
            gameManager.Tick();
            gameManager.Draw();
        }
        private void gameLoopTimer_Tick(object sender, EventArgs e)
        {
            //gameManager.Tick();
            //gameManager.Draw();
        }
        private void startButton_click(object sender, EventArgs e)
        {
            startButton.Visible = false;
            endButton.Visible = false;
            gameManager.Draw();
            tweeningTimer.Enabled = true;

        }
        private void endButton_click(object sender, EventArgs e)
        {
            tweeningTimer.Enabled = false;
            Application.Exit();
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