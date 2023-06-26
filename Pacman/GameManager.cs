namespace Pacman
{
     /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be own by some specific object but that should be visible in the entire game as a whole
     */
    class GameManager
    {
        private Map map;
        private Painter painter;
        private Hero hero;

        private int score;

        public GameManager(Form form)
        {
            map = new Map();
            painter = new Painter(form, map);
            hero = InputManager.GetHero();
        }
        
        public void Tick()
        {
            foreach (TweeningMovableObject tweeningMovableObject in map.GetTweeningMovableObjects())
            {
                tweeningMovableObject.Move(map);
            }
            score = hero.GetPelletsEaten();
        }
        public void Draw()
        {
            painter.Paint(map,score);
        }
        public void SetHeroNextDirection(Direction direction)
        {
            hero.SetNextDirection(direction);
        }
    }
}
