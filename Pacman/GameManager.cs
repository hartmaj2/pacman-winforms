using System.Net.Http.Headers;

namespace Pacman
{
    enum GameState { MainScreen, Running, RestartScreen };
     /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be own by some specific object but that should be visible in the entire game as a whole
     */
    class GameManager
    {
        GameState gameState;

        private Map map;
        private Painter painter;

        private int score;

        public GameManager(Form form)
        {
            map = InputManager.PrepareAndReturnMap();
            painter = new Painter(form, map);
            gameState = GameState.MainScreen;
        }
        public void Update(Keys keyPressed)
        {
            switch(gameState)
            {
                case GameState.MainScreen:
                    CheckMainScreenKeyPresses(keyPressed);
                    break;
                case GameState.Running:
                    CheckRunningGameKeyPresses(keyPressed);
                    MoveAllMovingObjects();
                    CheckGhostCollisions();
                    UpdateScore();
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
        private void CheckMainScreenKeyPresses(Keys keyPressed)
        {
            switch (keyPressed)
            {
                case Keys.Enter:
                    gameState = GameState.Running;
                    break;
            }
             
        }
        private void UpdateScore()
        {
            score = map.GetHero().GetPelletsEaten();
        }
        private void CheckGhostCollisions()
        {
            if (map.GetHero().IsTouchingAnyGhost(map.GetGhosts()))
            {
                Console.WriteLine("I've touched a ghost and I liked iiitt");
            }
        }
        private void MoveAllMovingObjects()
        {
            foreach (TweeningObject objectToMove in map.GetMovingObjects())
            {
                objectToMove.Move(map);
            }
        }
    }
}
