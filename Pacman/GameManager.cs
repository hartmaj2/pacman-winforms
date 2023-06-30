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

            map = GamePresets.PrepareAndReturnMap();
            painter = new Painter(form, map);

            gameState = GameState.StartScreen;

            currentGhostMode = GhostMode.Scatter;

            lastModeChange = DateTime.Now;
            scatterModeDuration = TimeSpan.FromSeconds(GamePresets.scatterModeDuration);
            chaseModeDuration = TimeSpan.FromSeconds(GamePresets.chaseModeDuration);
            frightenedModeDuration = TimeSpan.FromSeconds(GamePresets.frightenedModeDuration);

            frightenedGhostsEaten = 0;
            score = 0;
            gameLost = false;
        }

        /*
         * Updates the game according to the current game state and also the latest key press registered 
         */
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
                    ChangeGhostModeIfTime();
                    TryEat();
                    CheckGameWon();
                    break;
                case GameState.GameOverScreen:
                    CheckGameOverScreenKeyPresses(keyPressed);
                    break;
                    
            }
            
        }

        /*
         * Paints what needs to be painted according to the current state of the game
         */
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
                case GameState.GameOverScreen: // if game is over, we display text based on if the game was won or lost
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

        private void CheckGameOverScreenKeyPresses(Keys keyPressed)
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

        /*
         * Calls corresponding hero methods that make the hero try eat pellets and energizers
         */
        private void TryEat()
        {
            if (map.GetHero().TryEatPellet(map))
            {
                score += GamePresets.scorePerPellet;
            }
            if (map.GetHero().TryEatEnergizer(map))
            {
                score += GamePresets.scorePerEnergizer;
                SetAllGhostsToFrightenedIfPossible();
            }
        }

        /*
         * Gets called when we eat an energizer power-up
         */
        private void SetAllGhostsToFrightenedIfPossible()
        {
            foreach (Ghost ghost in map.GetGhosts())
            {
                ghost.SetModeIfValid(GhostMode.Frightened);
            }
            currentGhostMode = GhostMode.Frightened;
            lastModeChange = DateTime.Now;
            frightenedGhostsEaten = 0;
        }

        /*
         * The game is won if there are no more pellets and energizers to be eaten
         */
        private void CheckGameWon()
        {
            if (map.GetRemainingPelletsCount() == 0 && map.GetRemainingEnergizersCount() == 0)
            {
                gameState = GameState.GameOverScreen;
            }
        }

        /*
         * Depending on what mode the ghosts should be currently in globally, we examine the time passed
         * since last change to see if we should change their mode again
         */
        private void ChangeGhostModeIfTime()
        {
            DateTime currentTime = DateTime.Now;
            switch (currentGhostMode)
            {
                case GhostMode.Scatter:
                    if (currentTime - lastModeChange > scatterModeDuration)
                    {
                        // iterate through all ghosts in the game and try to change their mode to chase if we can
                        // (we can't always change the mode for example if they are preparing in the house we don't want them to become frightened)
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetModeIfValid(GhostMode.Chase);
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Chase;
                    }
                    break;
                case GhostMode.Chase:
                    if (currentTime - lastModeChange > chaseModeDuration)
                    {
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetModeIfValid(GhostMode.Scatter);
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Scatter;
                    }
                    break;
                case GhostMode.Frightened:
                    if (currentTime - lastModeChange > frightenedModeDuration) 
                    {
                        foreach (Ghost ghost in map.GetGhosts())
                        {
                            ghost.SetModeIfValid(GhostMode.Chase);
                        }
                        lastModeChange = currentTime;
                        currentGhostMode = GhostMode.Chase;
                    }
                    break;
            }
            
            
        }

        /*
         * Checks if the hero is colliding with some of the ghosts which can result in different
         * outcomes based on which mode the ghosts are in
         */
        private void CheckGhostCollisions()
        {
            if (map.GetHero().IsTouchingAnyGhost(map.GetGhosts()))
            {
                Ghost ghostTouched = map.GetHero().GetOneNearbyGhost(map.GetGhosts());
                if (ghostTouched.GetCurrentMode() == GhostMode.Frightened)
                {
                    ghostTouched.BeEaten();
                    frightenedGhostsEaten++;
                    score += GamePresets.scoreIncreasePerGhostEaten * frightenedGhostsEaten;
                }
                else
                {
                    gameLost = true;
                    gameState = GameState.GameOverScreen;
                }
                    
            }
        }

        /*
         * Move player and all the ghosts
         */
        private void MoveAllMovingObjects()
        {
            foreach (MovingObject objectToMove in map.GetAllMovingObjects())
            {
                objectToMove.Move(map);
            }
        }

        /*
         * Rereads the game field so it can reset the game state to the starting state
         * Resets all the variables that keep track of the current game state
         */
        private void InitializeGame()
        {
            map = GamePresets.PrepareAndReturnMap();
            painter = new Painter(gameForm, map);
            gameState = GameState.Running;

            lastModeChange = DateTime.Now;
            currentGhostMode = GhostMode.Scatter;

            frightenedGhostsEaten = 0;
            score = 0;
            gameLost = false;
        }
    }
}
