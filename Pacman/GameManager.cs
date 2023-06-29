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

        private int score;

        public GameManager(Form form)
        {
            map = InputManager.PrepareAndReturnMap();
            painter = new Painter(form, map);
        }
        public void Update(Keys keyPressed)
        {
            CheckKeyPressed(keyPressed);
            MoveAllMovingObjects();
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
