using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pacman
{
    public static class FormConstantsManager
    {
        public const string gameFormText = "Pacman Game";

        public const int startButtonHeightValue = 10;
        public const int startButtonWidthValue = 5;
        public const string startButtonText = "Start Game";

        public static readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);

        public static SolidBrush textBrush = new SolidBrush(Color.White);
        public static Font textFont = new Font("Arial", 16);
        public const string scoreText = "Score:";
    }
    public partial class GameForm : Form
    {

        // HACK INIT
        Stopwatch stopWatch = Stopwatch.StartNew();
        TimeSpan accumulatedTime;
        TimeSpan lastTime;
        //HACK INIT END

        private GameManager gameManager;
        public GameForm()
        {
            
            InitializeComponent();
            gameManager = new GameManager(this);
            this.KeyPreview = true; // makes sure that the form receives key events before their are passed to other components with focus
            Application.Idle += HandleApplicationIdle;

        }
        
        //HACK CODE
        bool IsApplicatoinIdle()
        {
            NativeMessage result;
            return PeekMessage(out result, IntPtr.Zero, (uint)0, (uint)0, (uint)0) == 0;
        }
        void HandleApplicationIdle(object sender, EventArgs e)
        {
            while(IsApplicatoinIdle())
            {

                TimeSpan currentTime = stopWatch.Elapsed;
                TimeSpan elapsedTime = currentTime - lastTime;
                lastTime = currentTime;

                accumulatedTime += elapsedTime;

                bool updated = false;

                while (accumulatedTime >= FormConstantsManager.TargetElapsedTime)
                {
                    gameManager.Update();
                    accumulatedTime -=  FormConstantsManager.TargetElapsedTime;
                    updated = true;
                }

                if (updated)
                {
                    gameManager.Render();
                }

                
            }
        }

        
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

        //HACK CODE END
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