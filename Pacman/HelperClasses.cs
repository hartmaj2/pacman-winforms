namespace Pacman
{
    /*
     * Represents a 2D integer vector
     */
    struct Direction
    {
        public static Direction Up { get; } = new Direction(0, -1);
        public static Direction Right { get; } = new Direction(1, 0);
        public static Direction Down { get; } = new Direction(0, 1);
        public static Direction Left { get; } = new Direction(-1, 0);
        public static Direction None { get; } = new Direction(0, 0);
        public int X { get; set; }
        public int Y { get; set; }

        public Direction(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Direction OppositeDirection()
        {
            return new Direction(-X, -Y);
        }

        public bool Equals(Direction other)
        {
            if (other.X == X && other.Y == Y)
            {
                return true;
            }
            return false;
        }
        public void RotateRight()
        {
            int temp = X; X = Y; Y = temp;
            if (Y == 0)
            {
                Y = -Y;
                X = -X;
            }
        }

        public void RotateLeft()
        {
            int temp = X; X = Y; Y = temp;
            if (X == 0)
            {
                Y = -Y;
                X = -X;
            }
        }

    }
    /*
     * Represents the topological space in which everything in the game lives. Has both a grid like map
     * implemented by two types of grids. Also enables the movable characters to move independently of the grid.
     */
    class Map
    {
        private StaticObject[,] staticGrid = InputManager.GetStaticGrid();
        private DynamicObject[,] dynamicGrid = InputManager.GetDynamicGrid();
        private List<TweeningMovableObject> tweeningMovableObjects = InputManager.GetTweeningMovableObjects();

        private int gridWidth;
        private int gridHeight;

        private int cellSize = InputManager.GetCellSize();

        public Map()
        {

            gridWidth = staticGrid.GetLength(1);
            gridHeight = staticGrid.GetLength(0);

        }

        public int GetCellSize()
        { 
            return cellSize; 
        }
        public void RemoveFromDynamicGrid(int gridX,  int gridY)
        {
            dynamicGrid[gridY, gridX] = new DynamicBlank();
        }
        public bool ContainsPellet(int gridX, int gridY)
        {
            if (dynamicGrid[gridY, gridX] is Pellet)
            {
                return true;
            }
            return false;
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
        public StaticObject GetStaticObjectAtCoordinates(int x, int y)
        {
            return staticGrid[y,x];
        }
        public DynamicObject GetDynamicObjectAtCoordinates(int x,int y)
        {
            return dynamicGrid[y,x];
        }
        public bool IsFreeCoordinate(int x, int y)
        {
            x = GetWrappedXCoordinate(x);
            y = GetWrappedYCoordinate(y);
            if (staticGrid[y,x] is StaticBlank)
            {
                return true;
            }
            return false;
        }
        private int GetWrappedXCoordinate(int x)
        {
            if (x > gridWidth - 1) return 0;
            if (x < 0) return gridWidth - 1;
            return x;
        }
        private int GetWrappedYCoordinate(int y)
        {
            if (y > gridHeight - 1) return 0;
            if (y < 0) return gridHeight - 1;
            return y;
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
        private Form gameForm;

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
            gameForm = form;
        }
        private void ClearBuffer()
        {
            bufferGraphics.Clear(Color.Black);
        }
        private void WriteBuffer()
        {
            formGraphics.DrawImageUnscaled(bufferBitmap, 0, 0);
        }
        private void PaintStaticObjectAtCoordinate(Map map, int dx, int dy)
        {
            StaticObject staticGameObject = map.GetStaticObjectAtCoordinates(dx, dy);
            if (staticGameObject is Wall)
            {
                bufferGraphics.DrawImage(wallSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        private void PaintDynamicObjectAtCoordinate(Map map, int dx, int dy)
        {
            DynamicObject dynamicGameObject = map.GetDynamicObjectAtCoordinates(dx, dy);
            if (dynamicGameObject is Pellet)
            {
                bufferGraphics.DrawImage(pelletSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        private void PaintGrids(Map map)
        {
            for (int dy = 0; dy < map.GetCoordinateHeight();dy++)
            {
                for (int dx = 0; dx < map.GetCoordinateWidth(); dx++)
                {
                    
                    PaintStaticObjectAtCoordinate(map, dx, dy);
                    
                    PaintDynamicObjectAtCoordinate(map, dx, dy);
                    
                }
            }
        }
        private void PaintTweeningMovableGameObjects(Map map)
        {
            foreach (TweeningMovableObject tweeningMovableGameObject in map.GetTweeningMovableObjects())
            {
                int xPos = tweeningMovableGameObject.GetPixelX();
                int yPos = tweeningMovableGameObject.GetPixelY();
                if (tweeningMovableGameObject is Hero)
                {
                    bufferGraphics.DrawImage(heroSprite, xPos, yPos);
                }
                if (tweeningMovableGameObject is Ghost)
                {
                    bufferGraphics.DrawImage(ghostSprite, xPos, yPos);
                }
            }
        }
        public void Paint(Map map, int score)
        {
            ClearBuffer();
            PaintGrids(map);
            PaintTweeningMovableGameObjects(map);
            DisplayScore(score);
            WriteBuffer();
        }

        private void DisplayScore(int score)
        {
            bufferGraphics.DrawString(FormConstantsManager.scoreText + " " + score, FormConstantsManager.textFont, FormConstantsManager.textBrush, 0, 0);
        }
    }
}
