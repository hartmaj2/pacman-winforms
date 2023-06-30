/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
{
    /*
     * GameState is used by the GameManager to decide, how to handle Update and Render methods called from
     * the main game loop depending on what state the game is currently in
     */
    enum GameState { StartScreen, Running, GameOverScreen};

     /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be owned by some specific object but that should be visible in the entire game as a whole.
     * This includes the score, durations of different ghost modes or the amount of ghosts eaten during
     * the current frightened ghost mode
     */
    class GameManager
    {
        // holds reference to the Windows Forms form, this form would usually be named Form1
        private GameForm gameForm;

        private Map map;
        private Painter painter;

        private GameState gameState;


        // BEGINNING OF GHOST RELATED CODE
        // takes care of ghost modes that are switched on simultaneously for all ghosts
        // ghosts also have their own private modes in case the are preparing or frightened because those durations vary from ghost to ghost
        // the mode GhostMode.Preparing is never set as a global ghost mode (it is always unique to each ghost)
        private GhostMode currentGhostMode;


        // variables that have to do with timing changes of global ghost modes
        // these could be constants at this phase of the program because they do not change (except for lastModeChange)
        // in the future those could be made to be decreasing over time so that's why I didn't want them to be constants
        private DateTime lastModeChange;
        private TimeSpan scatterModeDuration;
        private TimeSpan chaseModeDuration;
        private TimeSpan frightenedModeDuration;

        // in order to calculate how much score player should receive for eating a ghost during frightened phase
        // we need to know how many ghosts he has eaten so far during this mode (it gets reset at the beginning of every frightened mode start)
        private int frightenedGhostsEaten;

        // END OF GHOST RELATED CODE

        private int score;

        // is used to decide, which game over screen to print (either a YOU LOST or YOU WON message)
        // other option would be to have two separate GameStates, one for GameLost and one for GameWon but I didn't want to go that way
        private bool gameLost; 

        public GameManager(GameForm form)
        {
            gameForm = form;

            map = InputManager.PrepareAndReturnMap();
            painter = new Painter(form, map);

            gameState = GameState.StartScreen;

            currentGhostMode = GhostMode.Scatter;

            lastModeChange = DateTime.Now;
            scatterModeDuration = TimeSpan.FromSeconds(InputManager.scatterModeDuration);
            chaseModeDuration = TimeSpan.FromSeconds(InputManager.chaseModeDuration);
            frightenedModeDuration = TimeSpan.FromSeconds(InputManager.frightenedModeDuration);

            frightenedGhostsEaten = 0;
            score = 0;
            gameLost = false;
        }
        public void Update(Keys keyPressed)
        {
            switch(gameState)
            {
                case GameState.StartScreen:
                    CheckStartScreenKeyPresses(keyPressed);
                    break;
                case GameState.Running:
                    CheckRunningGameKeyPresses(keyPressed);
                    MoveAllMovingObjects();
                    CheckGhostCollisions();
                    ChangeModeIfTime();
                    TryEat();
                    CheckGameWon();
                    break;
                case GameState.GameOverScreen:
                    CheckGameOverKeyPresses(keyPressed);
                    break;
                    
            }
            
        }
        public void Render()
        {
            switch (gameState)
            {
                case GameState.StartScreen:
                    painter.PaintStartScreen();
                    break;
                case GameState.Running:
                    painter.PaintRunningGame(map, score);
                    break;
                case GameState.GameOverScreen:
                    if (gameLost)
                    {
                        painter.PaintGameOverScreen(FormText.GetGameLostText(score));
                    }
                    else
                    {
                        painter.PaintGameOverScreen(FormText.GetGameWonText(score));
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
        private void TryEat()
        {
            if (map.GetHero().TryEatPellet(map))
            {
                score += 10;
            }
            if (map.GetHero().TryEatEnergizer(map))
            {
                score += 50;
                //Console.WriteLine("Ghosts set to frightened mode");
                foreach (Ghost ghost in map.GetGhosts())
                {
                    ghost.SetFrightenedModeIfValid();
                }
                currentGhostMode = GhostMode.Frightened;
                lastModeChange = DateTime.Now;
                frightenedGhostsEaten = 0;
            }
        }
        private void CheckGameWon()
        {
            if (map.GetRemainingPelletsCount() == 0 && map.GetRemainingEnergizersCount() == 0)
            {
                gameState = GameState.GameOverScreen;
            }
        }
        private void ChangeModeIfTime()
        {
            DateTime currentTime = DateTime.Now;
            switch (currentGhostMode)
            {
                case GhostMode.Scatter:
                    if (currentTime - lastModeChange > scatterModeDuration)
                    {
                        //Console.WriteLine("Mode changed to chase");
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetChaseModeIfValid();
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Chase;
                    }
                    break;
                case GhostMode.Chase:
                    if (currentTime - lastModeChange > chaseModeDuration)
                    {
                        //Console.WriteLine("Mode changed to scatter");
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetScatterModeIfValid();
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Scatter;
                    }
                    break;
                case GhostMode.Frightened:
                    if (currentTime - lastModeChange > frightenedModeDuration) 
                    {
                        // set all ghosts to chase mode if I can (if they are preparing, we don't want to change the mode)
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetChaseModeIfValid();
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Chase;
                    }
                    break;
            }
            
            
        }
        private void CheckGhostCollisions()
        {
            if (map.GetHero().IsTouchingAnyGhost(map.GetGhosts()))
            {
                Ghost ghostTouched = map.GetHero().GetOneNearbyGhost(map.GetGhosts());
                if (ghostTouched.GetCurrentMode() == GhostMode.Frightened)
                {
                    ghostTouched.BeEaten();
                    frightenedGhostsEaten++;
                    score += 200 * frightenedGhostsEaten;
                }
                else
                {
                    gameLost = true;
                    gameState = GameState.GameOverScreen;
                }
                    
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
            score = 0;
            currentGhostMode = GhostMode.Scatter;
        }
    }
}
