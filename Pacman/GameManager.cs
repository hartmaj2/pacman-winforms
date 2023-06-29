using System.Net.Http.Headers;

namespace Pacman
{
    enum GameState { MainScreen, Running, GameOver};
     /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be own by some specific object but that should be visible in the entire game as a whole
     */
    class GameManager
    {
        private GameState gameState;

        private GameForm gameForm;

        private Map map;
        private Painter painter;

        private int score;
        private bool gameLost = false;

        public GameManager(GameForm form)
        {
            map = InputManager.PrepareAndReturnMap();
            painter = new Painter(form, map);
            gameState = GameState.MainScreen;
            gameForm = form;
        }
        public void Update(Keys keyPressed)
        {
            switch(gameState)
            {
                case GameState.MainScreen:
                    CheckStartScreenKeyPresses(keyPressed);
                    break;
                case GameState.Running:
                    CheckRunningGameKeyPresses(keyPressed);
                    MoveAllMovingObjects();
                    CheckGhostCollisions();
                    UpdateScore();
                    CheckGameWon();
                    break;
                case GameState.GameOver:
                    CheckGameOverKeyPresses(keyPressed);
                    break;
                    
            }
            
        }
        public void Render()
        {
            switch (gameState)
            {
                case GameState.MainScreen:
                    painter.PaintStartScreen();
                    break;
                case GameState.Running:
                    painter.PaintRunningGame(map, score);
                    break;
                case GameState.GameOver:
                    if (gameLost)
                    {
                        painter.PaintGameOverScreen(FormConstants.GetGameLostText(score));
                    }
                    else
                    {
                        painter.PaintGameOverScreen(FormConstants.GetGameWonText(score));
                    }
                    break;
            }
            
            
        }
        private void CheckStartScreenKeyPresses(Keys keyPressed)
        {
            switch (keyPressed)
            {
                case Keys.Enter:
                    InitializeGame();
                    gameState = GameState.Running;
                    break;
            }
             
        }
        private void CheckRunningGameKeyPresses(Keys keyPressed)
        {

            switch (keyPressed)
            {
                case Keys.Up:
                    map.GetHero().SetNextDirection(Direction.Up);
                    break;
                case Keys.Right:
                    map.GetHero().SetNextDirection(Direction.Right);
                    break;
                case Keys.Down:
                    map.GetHero().SetNextDirection(Direction.Down);
                    break;
                case Keys.Left:
                    map.GetHero().SetNextDirection(Direction.Left);
                    break;
                case Keys.Enter:
                    map.OpenAllFences();
                    break;
            }
        }
        private void CheckGameOverKeyPresses(Keys keyPressed)
        {
            switch(keyPressed)
            {
                case Keys.Enter:
                    InitializeGame();
                    break;
                case Keys.Q:
                    Application.Exit();
                    break;
            }
        }
        private void UpdateScore()
        {
            score = map.GetHero().GetPelletsEaten();
        }
        private void CheckGameWon()
        {
            if (map.GetRemainingPelletsCount() == 0)
            {
                gameState = GameState.GameOver;
            }
        }
        private void CheckGhostCollisions()
        {
            if (map.GetHero().IsTouchingAnyGhost(map.GetGhosts()))
            {
                gameLost = true;
                gameState = GameState.GameOver;
            }
        }
        private void MoveAllMovingObjects()
        {
            foreach (TweeningObject objectToMove in map.GetMovingObjects())
            {
                objectToMove.Move(map);
            }
        }
        private void InitializeGame()
        {
            map = InputManager.PrepareAndReturnMap();
            painter = new Painter(gameForm, map);
            gameLost = false;
            gameState = GameState.Running;
        }
    }
}
