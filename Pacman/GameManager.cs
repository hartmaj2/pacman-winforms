﻿namespace Pacman
{
     /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be own by some specific object but that should be visible in the entire game as a whole
     */
    class GameManager
    {
        private Map map;
        private Painter painter;
        private List<Ghost> ghosts;
        private Hero hero;

        private int score;

        public GameManager(Form form)
        {
            map = new Map();
            painter = new Painter(form, map);
            hero = InputManager.GetHero();
            ghosts = InputManager.GetGhosts();
        }
        public void Update(Keys keyPressed)
        {
            CheckKeyPressed(keyPressed);
            MoveAllTweeningMovableObjects();
            CheckGhostCollisions();
            UpdateScore();
        }
        public void Render()
        {
            painter.Paint(map,score);
        }
        private void CheckKeyPressed(Keys keyPressed)
        {

            switch (keyPressed)
            {
                case Keys.Up:
                    hero.SetNextDirection(Direction.Up);
                    break;
                case Keys.Right:
                    hero.SetNextDirection(Direction.Right);
                    break;
                case Keys.Down:
                    hero.SetNextDirection(Direction.Down);
                    break;
                case Keys.Left:
                    hero.SetNextDirection(Direction.Left);
                    break;
            }
        }
        private void UpdateScore()
        {
            score = hero.GetPelletsEaten();
        }
        private void CheckGhostCollisions()
        {
            if (hero.IsTouchingAnyGhost(ghosts))
            {
                Console.WriteLine("I've touched a ghost and I liked iiitt");
            }
        }
        private void MoveAllTweeningMovableObjects()
        {
            foreach (TweeningMovableObject tweeningMovableObject in map.GetTweeningMovableObjects())
            {
                tweeningMovableObject.Move(map);
            }
        }
    }
}
