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
        private StaticGridObject[,] staticGrid = InputManager.GetStaticGrid();
        private InteractiveGridObject[,] interactiveGrid = InputManager.GetDynamicGrid();
        private List<TweeningObjects> tweeningObjects = InputManager.GetTweeningObjects();

        private int gridWidth;
        private int gridHeight;

        private int cellSize = InputManager.GetCellSize();

        public Map()
        {

            gridWidth = staticGrid.GetLength(0);
            gridHeight = staticGrid.GetLength(1);

        }

        public int GetCellSize()
        { 
            return cellSize; 
        }
        public void RemoveFromInteractiveGrid(int gridX,  int gridY)
        {
            interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace();
        }
        public bool ContainsPellet(int gridX, int gridY)
        {
            if (interactiveGrid[gridX, gridY] is Pellet)
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
        public int GetGridWidth()
        {
            return gridWidth;
        }
        public int GetGridHeight()
        {
            return gridHeight;
        }
        public StaticGridObject GetStaticGridObject(int x, int y)
        {
            return staticGrid[x,y];
        }
        public InteractiveGridObject GetInteractiveGridObject(int x,int y)
        {
            return interactiveGrid[x,y];
        }
        public bool IsFreeGridCell(int x, int y)
        {
            if (staticGrid[x,y] is StaticLayerBlankSpace)
            {
                return true;
            }
            return false;
        }
        public int GetWrappedXCoordinate(int x)
        {
            if (x > gridWidth - 1) return 0;
            if (x < 0) return gridWidth - 1;
            return x;
        }
        public int GetWrappedYCoordinate(int y)
        {
            if (y > gridHeight - 1) return 0;
            if (y < 0) return gridHeight - 1;
            return y;
        }
        public List<TweeningObjects> GetTweeningObjects() 
        {
            return tweeningObjects;
        }
        private void LocateIntersections()
        {
            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0 ; j < gridWidth; j++)
                {

                }
            }
        }

        private bool IsAnIntersection(int x, int y)
        {
            return false;
        }
    }
    /*
     * Takes care of drawing to the form. Implements double buffering to get rid of the flickering.
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
        private void ClearBuffer()
        {
            bufferGraphics.Clear(Color.Black);
        }
        private void WriteBuffer()
        {
            formGraphics.DrawImageUnscaled(bufferBitmap, 0, 0);
        }
        private void PaintStaticGridObjectAtCoordinate(Map map, int dx, int dy)
        {
            StaticGridObject staticGameObject = map.GetStaticGridObject(dx, dy);
            if (staticGameObject is Wall)
            {
                bufferGraphics.DrawImage(wallSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        private void PaintInteractiveGridObjectAtCoordinate(Map map, int dx, int dy)
        {
            InteractiveGridObject dynamicGameObject = map.GetInteractiveGridObject(dx, dy);
            if (dynamicGameObject is Pellet)
            {
                bufferGraphics.DrawImage(pelletSprite, spriteSize * dx, spriteSize * dy);
            }
        }
        private void PaintGrids(Map map)
        {
            for (int dy = 0; dy < map.GetGridHeight();dy++)
            {
                for (int dx = 0; dx < map.GetGridWidth(); dx++)
                {
                    
                    PaintStaticGridObjectAtCoordinate(map, dx, dy);
                    
                    PaintInteractiveGridObjectAtCoordinate(map, dx, dy);
                    
                }
            }
        }
        private void PaintTweeningObjects(Map map)
        {
            foreach (TweeningObjects tweeningMovableGameObject in map.GetTweeningObjects())
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
            PaintTweeningObjects(map);
            DisplayScore(score);
            WriteBuffer();
        }
        private void DisplayScore(int score)
        {
            bufferGraphics.DrawString(FormConstantsManager.scoreText + " " + score, FormConstantsManager.textFont, FormConstantsManager.textBrush, 0, 0);
        }
    }
}
