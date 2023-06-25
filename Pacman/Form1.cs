namespace Pacman
{
    public partial class Form1 : Form
    {

        public static int count = 0;

        private GameManager gameManager;
        public Form1()
        {
            InitializeComponent();
            gameManager = new GameManager(this);
            this.KeyPreview = true; // makes sure that the form receives key events before their are passed to other components with focus
            initializeForm();
        }
        private void initializeForm()
        {
            
            this.Text = "Pacman Game " + count;
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
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            Direction newDirection = new Direction(0,0);

            switch(e.KeyCode)
            {
                case Keys.Up:
                    newDirection = new Direction(0,-1);
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