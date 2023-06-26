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

        private int pelletsEaten;

        public GameManager(Form form)
        {
            map = new Map();
            painter = new Painter(form, map);
            hero = InputManager.GetHero();
        }
        public void DiscreteTick()
        {
            foreach (DiscreteMovableObject discreteMovingObject in map.GetDiscreteMovingObjects())
            {
                discreteMovingObject.Move(map);
            }
        }
        public void TweenTick()
        {
            foreach (TweeningMovableObject tweeningMovableObject in map.GetTweeningMovableObjects())
            {
                tweeningMovableObject.Move(map);
            }
        }
        public void Draw()
        {
            painter.Paint(map);
        }
        public void SetHeroDirection(Direction direction)
        {
            hero.SetNextDirection(direction);
        }
    }
}
