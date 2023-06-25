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
            this.map = new Map();
            this.painter = new Painter(form, map);
            this.hero = InputManager.GetHero();
        }
        public void Tick()
        {
            foreach (MovableGameObject movableGameObject in map.GetMovableGameObjects())
            {
                movableGameObject.Move(map);
            }
        }
        public void Draw()
        {
            painter.PaintGrids(map);
            painter.PaintMovableGameObjects(map);
        }
        public void SetHeroDirection(Direction direction)
        {
            hero.SetDirection(direction);
        }
    }
}
