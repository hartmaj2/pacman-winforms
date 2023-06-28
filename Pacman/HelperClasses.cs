﻿namespace Pacman
{
    /*
     * Represents a 2D integer vector with unit size or a zero vector
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
        private List<TweeningObjects> movingObjects = InputManager.GetTweeningObjects();
        private List<Fence> fences = InputManager.GetFences();
        private Hero hero = InputManager.GetHero();

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
        /*
         * Writes the corresponding BlankSpace object to the interactive grid thus effectively removing the object
         */
        public void RemoveFromInteractiveGrid(int gridX,  int gridY)
        {
            interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace(null,gridX,gridY);
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
        public bool IsGhostHome(int x, int y)
        {
            if (staticGrid[x, y] is GhostHome)
            {
                return true;
            }
            return false;
        }
        public bool IsOpenFence(int x, int y)
        {
            if (staticGrid[x, y] is Fence)
            {
                if (!((Fence)staticGrid[x, y]).IsClosed())
                {
                    return true;
                }
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
        public List<TweeningObjects> GetMovingObjects() 
        {
            return movingObjects;
        }
        public bool IsAnIntersection(int x, int y)
        {
            if (GetNeighboringCellsCount(x,y) > 2)
            {
                return true;
            }
            return false;
        }
        public int GetNeighboringCellsCount(int currentX, int currentY)
        {
            Direction direction = Direction.Up;
            int blankSpacesCount = 0;
            for (int i = 0; i < 4; i++)
            {
                int neighbourX = GetWrappedXCoordinate(currentX + direction.X);
                int neighbourY = GetWrappedYCoordinate(currentY + direction.Y);
                StaticGridObject neighboringCell = GetStaticGridObject(neighbourX, neighbourY);
                if (neighboringCell is StaticLayerBlankSpace)
                {
                    blankSpacesCount++;
                }
                direction.RotateRight();
            }
            return blankSpacesCount;
        }
        public List<StaticLayerBlankSpace> GetNeighboringBlankCells(int x, int y)
        {
            List<StaticLayerBlankSpace> neighbours = new List<StaticLayerBlankSpace>();
            Direction direction = Direction.Up;
            for (int i = 0; i < 4; i++)
            {
                StaticGridObject neighboringCell = GetStaticGridObject(x + direction.X, y + direction.Y);
                if (neighboringCell is StaticLayerBlankSpace)
                {
                    neighbours.Add((StaticLayerBlankSpace)neighboringCell);
                }
                direction.RotateRight();
            }
            return neighbours;
        }
        public void OpenAllFences()
        {
            foreach (Fence fence in fences)
            {
                fence.Open();
            }
        }
        public Point GetHeroLocaion()
        {
            return new Point(hero.GetGridX(), hero.GetGridY());
        }
    }
    /*
     * Takes care of drawing to the form. Implements double buffering to get rid of the flickering.
     * The objects are painted in three different layers. 
     * First, the statc objects on the grid are painted.
     * Second, the interactive objects on the grid.
     * Lastly, the moving objects anywhere on the map.
     */
    class Painter
    {
        private Graphics formGraphics; // the actual graphics elements that gets painted on the form
        private Graphics bufferGraphics; // the buffer that we paint on before propagating change to formGraphics
        private Bitmap bufferBitmap;

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
            StaticGridObject staticObjectToPaint = map.GetStaticGridObject(dx, dy);
            if (staticObjectToPaint.IsDrawable())
            {
                bufferGraphics.DrawImage(staticObjectToPaint.GetImageToDraw(), spriteSize * dx, spriteSize * dy);
            }

        }
        private void PaintInteractiveGridObjectAtCoordinate(Map map, int dx, int dy)
        {
            InteractiveGridObject interactiveObjectToPaint = map.GetInteractiveGridObject(dx, dy);
            if (interactiveObjectToPaint.IsDrawable())
            {
                bufferGraphics.DrawImage(interactiveObjectToPaint.GetImageToDraw(), spriteSize * dx, spriteSize * dy);
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
        private void PaintMovingObjects(Map map)
        {
            foreach (TweeningObjects movingObjectToPaint in map.GetMovingObjects())
            {
                
                if (movingObjectToPaint.IsDrawable())
                {
                    int xPos = movingObjectToPaint.GetPixelX();
                    int yPos = movingObjectToPaint.GetPixelY();
                    bufferGraphics.DrawImage(movingObjectToPaint.GetImageToDraw(), xPos, yPos);
                }
            }
        }
        public void Paint(Map map, int score)
        {
            ClearBuffer();
            PaintGrids(map);
            PaintMovingObjects(map);
            DisplayScore(score);
            WriteBuffer();
        }
        private void DisplayScore(int score)
        {
            bufferGraphics.DrawString(FormConstantsManager.scoreText + " " + score, FormConstantsManager.textFont, FormConstantsManager.textBrush, 0, 0);
        }
    }
}
