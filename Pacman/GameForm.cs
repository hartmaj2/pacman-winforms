using System.Diagnostics;
using System.Runtime.InteropServices;

/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
{
    public static class FormConstants
    {
        public const string gameFormText = "Pacman Game";

        public const string startScreenText = "Press \"enter\" to start the game";
        public static Font startScreenFont = new Font("Arial", 25);

        public static SolidBrush textBrush = new SolidBrush(Color.Yellow);
        public static Font textFont = new Font("Arial", 20);
        public const string scoreText = "Score: ";

        public const string playAgainOrQuitText = "\nPress \"enter\" to play again or \"q\" to quit";
        public static string GetGameLostText(int score)
        {
            return "You lost !\nYour score was: " + score + playAgainOrQuitText;
        }

        public static string GetGameWonText(int score)
        {
            return "You Won !\nYour score is: " + score + playAgainOrQuitText;
        }
    }
    public partial class GameForm : Form
    {

        readonly TimeSpan gameTickInterval = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

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

        /* This simulates a game loop which makes the game update at an interval calculated by 
         * the stopWatch
         */
        private void Loop()
        {
            TimeSpan currentTime = stopWatch.Elapsed; // how much time elapsed since the stopwatch started 
            TimeSpan elapsedSinceLastAccumulation = currentTime - lastTime; // calculate how much time elapsed since last game update
            
            accumulatedTime += elapsedSinceLastAccumulation; 
            lastTime = currentTime; // we accumulated time passed since last time in the previous instruction so we need to reset lastTime

            bool updated = false;

            // If there was lots of accumulated time, the game simulation updates many times without rendering
            while (accumulatedTime >= gameTickInterval)
            {
                Update();
                accumulatedTime -= gameTickInterval;
                updated = true;
            }

            // we only draw to the screen once even if there were many ticks of the game logic
            if (updated)
            {
                Render();
            }
        }
        private void Update()
        {
            gameManager.Update(keyPressed);
            keyPressed = Keys.None;
        }
        private void Render()
        {
            gameManager.Render();
        }

        /* 
         *  We are overriding a message processing pipeline Windows Forms method that lets us handle
         *  key presses before regular key processing would take place
         */
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter:
                case Keys.Q:
                case Keys.Up:
                case Keys.Right:
                case Keys.Down:
                case Keys.Left:
                    keyPressed = keyData;
                    return true; // we should return true if the key was processed
            }
            return base.ProcessCmdKey(ref msg, keyData); // otherwise we propagate the key message up the control hierarchy
        }
        
        /*
         * Calls the PeekMessage function. We are only interested in getting true or false and not in the
         * message itself which is saved to the `result` variable
         */
        private bool IsApplicatoinIdle()
        {
            NativeMessage result; // this variable is passed to PeekMessage as an `out` parameter which means PeekMessage can modify it
            return PeekMessage(out result, IntPtr.Zero, 0, 0, 0) == 0;
        }

        /*
         * This takes care that we try to tick the game loop when there are no messages on Windows Message Queue
         * this allowed me to tick whenever it is possible and thus I don't have to rely on the 
         * Windows Forms Timer which is not as reliable as the Stopwatch builtin class
         */
        private void HandleApplicationIdle(object sender, EventArgs e)
        {
            while(IsApplicatoinIdle())
            {
                Loop();
            }
        }

        /*
         * .NET doesn't allow for direct access of Windows APIs so we need to define the structure ourselves. 
         * It represents a Windows OS message (the native windows equivalent is called MSG)
         */
        [StructLayout(LayoutKind.Sequential)] // this tells .NET to lay out the memory fields in the same order as declared (so it behaves like C++)
        public struct NativeMessage 
        {
            public IntPtr Handle;
            public uint Message;
            public IntPtr WParameter;
            public IntPtr LParameter;
            public uint Time;
            public Point Location;
        }

        [DllImport("user32.dll")] // this tells .NET that the PeekMessage method is implemented in the user32.dll library
        //PeekMessage is a method implemented externally (it is unmanaged/not in C#) thus the word extern
        public static extern int PeekMessage(out NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);
    }
        
}