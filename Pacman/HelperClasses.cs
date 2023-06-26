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
     * Represents the topological space in which everything in the game lives. Has both a grid like map
     * implemented by two types of grids. Also enables the movable characters to move independently of the grid.
     */
    class Map
    {
        private StaticGameObject[,] staticGrid;
        private DynamicGameObject[,] dynamicGrid;
        private List<MovableGameObject> movableObjects;
        private List<TweeningMovableObject> tweeningMovableObjects;

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
        public List<TweeningMovableObject> GetTweeningMovableObjects() 
        {
            return tweeningMovableObjects;
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
            formGraphics.DrawImageUnscaled(bufferBitmap, 0, 0);
        }

    }
}
