namespace Pacman
{
    /*
     * Represents a 2D integer vector
     */
    struct Direction
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Direction(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    /*
     * Everything that lives inside the game should inherit this. It has to be something that can 
     * occupy a grid and that can be drawn
     */
    abstract class GameObject
    {
       
    }
    /* 
     * Special type of GameObject that can also move around
     */
    abstract class StaticGameObject
    {

    }
    abstract class DynamicGameObject
    {

    }
    abstract class MovableGameObject : DynamicGameObject
    {
        protected int xPos;
        protected int yPos;
        protected Direction direction;

        public MovableGameObject(int x, int y)
        {
            xPos = x;
            yPos = y;
        }

        public int GetX()
        {
            return xPos;
        }
        public int GetY()
        {
            return yPos;
        }

        public void Move(Map map)
        {
            int newX = xPos + direction.X;
            int newY = yPos + direction.Y;

            if (map.IsFreeCoordinate(newX, newY))
            {
                xPos += direction.X;
                yPos += direction.Y;
            }
        }

    }
    /* 
     * Represents a blank space in the static grid. I wanted to be explicit and not relying on null. 
     * This can be useful for debugging purposes in the future.
     */
    class StaticBlank : StaticGameObject
    {
    
    }
    class DynamicBlank : DynamicGameObject
    {

    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : StaticGameObject
    {

    }
    /*
     * Main playable character of the game. So far I will make it non playable but will
     * add controls later.
     */
    class Hero : MovableGameObject
    {
        public Hero(int x, int y) : base(x,y)
        {
            direction.X = 1;
            direction.Y = 0;
        }

        public void SetDirection(Direction direction)
        {
            this.direction = direction;
        }

    }
    /*
     * Things that player eats and gets points for that
     */
    class Pellet : DynamicGameObject
    {

    }
    /* 
     * Enemies that will be chasing the player
     */
    class Ghost : MovableGameObject
    {
        public Ghost(int x, int y) : base (x,y)
        {
            direction.X = 0;
            direction.Y = 1;
        }

    }
    /*
     * Represents the topological space in which everything in the game lives. Has both a grid like map
     * implemented by two types of grids. Also enables the movable characters to move independently of the grid.
     */
    class Map
    {
        private StaticGameObject[,] staticGrid;
        private DynamicGameObject[,] dynamicGrid;
        private List<MovableGameObject> movableObjects;

        private int gridWidth;
        private int gridHeight;

        private int cellSize = InputManager.GetCellSize();

        public Map()
        {
            staticGrid = InputManager.GetStaticGrid();
            dynamicGrid = InputManager.GetDynamicGrid();
            movableObjects = InputManager.GetMovableGameObjects();

            gridWidth = staticGrid.GetLength(1);
            gridHeight = staticGrid.GetLength(0);

        }

        public int GetPixelWidth()
        {
            return gridWidth * cellSize;
        }
        public int GetPixelHeight()
        {
            return gridHeight * cellSize;
        }
        public int GetCoordinateWidth()
        {
            return gridWidth;
        }
        public int GetCoordinateHeight()
        {
            return gridHeight;
        }
        public StaticGameObject GetStaticObjectAtCoordinates(int x, int y)
        {
            return staticGrid[y,x];
        }
        public DynamicGameObject GetDynamicObjectAtCoordinates(int x,int y)
        {
            return dynamicGrid[y,x];
        }
        public bool IsFreeCoordinate(int x, int y)
        {
            if (staticGrid[y,x] is StaticBlank)
            {
                return true;
            }
            return false;
        }
        public List<MovableGameObject> GetMovableGameObjects()
        {
            return movableObjects;
        }
    }
    /*
     * Takes care of drawing to the form. Implements buffering to get rid of the flickering.
     */
    class Painter
    {
        private Graphics formGraphics;
        private Graphics bufferGraphics;
        private Bitmap bufferBitmap;

        private Bitmap wallSprite = InputManager.GetWallSprite();
        private Bitmap heroSprite = InputManager.GetHeroSprite();
        private Bitmap pelletSprite = InputManager.GetPelletSprite();
        private Bitmap ghostSprite = InputManager.GetGhostSprite();

        private int spriteSize = InputManager.GetCellSize();
        public Painter(Form form, Map map)
        {
            form.ClientSize = new Size(map.GetPixelWidth(), map.GetPixelHeight());
            formGraphics = form.CreateGraphics();
            bufferBitmap = new Bitmap(map.GetPixelWidth(), map.GetPixelHeight());
            bufferGraphics = Graphics.FromImage(bufferBitmap);
        }
        private void PaintStaticObjectAtCoordinate(Map map, int dx, int dy)
        {
            StaticGameObject staticGameObject = map.GetStaticObjectAtCoordinates(dx, dy);
            if (staticGameObject is Wall)
            {
                bufferGraphics.DrawImage(wallSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        private void PaintDynamicObjectAtCoordinate(Map map, int dx, int dy)
        {
            DynamicGameObject dynamicGameObject = map.GetDynamicObjectAtCoordinates(dx, dy);
            if (dynamicGameObject is Pellet)
            {
                bufferGraphics.DrawImage(pelletSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        public void PaintGrids(Map map)
        {
            bufferGraphics.Clear(Color.Black);
            for (int dy = 0; dy < map.GetCoordinateHeight();dy++)
            {
                for (int dx = 0; dx < map.GetCoordinateWidth(); dx++)
                {
                    
                    PaintStaticObjectAtCoordinate(map, dx, dy);
                    
                    PaintDynamicObjectAtCoordinate(map, dx, dy);
                    
                }
            }
        }
        public void PaintMovableGameObjects(Map map)
        {
            foreach  (MovableGameObject movableGameObject in map.GetMovableGameObjects())
            {
                if (movableGameObject != null)
                {
                    int xPos = movableGameObject.GetX();
                    int yPos = movableGameObject.GetY();
                    if (movableGameObject is Hero)
                    {
                        bufferGraphics.DrawImage(heroSprite, spriteSize *  xPos, spriteSize * yPos);
                    }
                    if (movableGameObject is Ghost)
                    {
                        bufferGraphics.DrawImage(ghostSprite, spriteSize * xPos, spriteSize * yPos);
                    }
                }
            }
            formGraphics.DrawImageUnscaled(bufferBitmap, 0, 0);
        }

    }
}
