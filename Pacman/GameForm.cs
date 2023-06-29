using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pacman
{
    public static class FormConstants
    {
        public const string gameFormText = "Pacman Game";

        public const string startScreenText = "Press enter to start the game";
        public static Font startScreenFont = new Font("Arial", 20);

        public static SolidBrush textBrush = new SolidBrush(Color.White);
        public static Font textFont = new Font("Arial", 16);
        public const string scoreText = "Score:";

        public const string playAgainOrQuitText = "\nPress enter to play again or q to quit";
        public static string GetGameLostText(int score)
        {
            return "You lost\nYour score was " + score + playAgainOrQuitText;
        }

        public static string GetGameWonText(int score)
        {
            return "You Won\nYour score is " + score + playAgainOrQuitText;
        }
    }
    public partial class GameForm : Form
    {

        readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        Keys keyPressed = Keys.None;
        Stopwatch stopWatch = Stopwatch.StartNew();
        TimeSpan accumulatedTime;
        TimeSpan lastTime;

        private GameManager gameManager;
        public GameForm()
        {
            
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
            gameManager = new GameManager(this);
            Application.Idle += HandleApplicationIdle;

        }
        private void Loop()
        {
            TimeSpan currentTime = stopWatch.Elapsed;
            TimeSpan elapsedTime = currentTime - lastTime;
            lastTime = currentTime;

            accumulatedTime += elapsedTime;

            bool updated = false;

            // If there was lots of accumulated time, the game ticks many times without rendering
            while (accumulatedTime >= TargetElapsedTime)
            {
                Tick();
                accumulatedTime -= TargetElapsedTime;
                updated = true;
            }

            if (updated)
            {
                Render();
            }
        }
        private void Tick()
        {
            gameManager.Update(keyPressed);
            keyPressed = Keys.None;
        }
        private void Render()
        {
            gameManager.Render();
        }
        /* 
         *  Because I am using the built in enum Keys, I don't have to create my own enum
         */
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            keyPressed = keyData;
            return true;
        }
        private bool IsApplicatoinIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }
        private void HandleApplicationIdle(object sender, EventArgs e)
        {
            while(IsApplicatoinIdle())
            {
                Loop();
            }
        }
        /*
         * This is some kind of a hack that allows me to use the PeekMessage() function that returns 
         * true if the windows message pump is empty. If it is I can just go on continuing my game loop
         */
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }
        [DllImport("user32.dll")]
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);
    }
        
}