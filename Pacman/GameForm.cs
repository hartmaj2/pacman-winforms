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
            KeyPreview = true; // makes sure that the form receives key events before their are passed to other components with focus
            Application.Idle += HandleApplicationIdle;

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

        private void Loop()
        {
            TimeSpan currentTime = stopWatch.Elapsed;
            TimeSpan elapsedTime = currentTime - lastTime;
            lastTime = currentTime;

            accumulatedTime += elapsedTime;

            bool updated = false;

            while (accumulatedTime >= FormConstantsManager.TargetElapsedTime)
            {
                Tick();
                accumulatedTime -= FormConstantsManager.TargetElapsedTime;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            keyPressed = keyData;
            return true;
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    base.OnKeyDown(e);

        //    Direction newDirection = Direction.None;



        //    gameManager.CheckKeyPressed(newDirection);

        //}
    }
}