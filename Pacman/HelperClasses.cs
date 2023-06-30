/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
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
        private StaticGridObject[,] staticGrid;
        private InteractiveGridObject[,] interactiveGrid;
        private List<TweeningObject> movingObjects;
        private List<Fence> fences;
        private List<Ghost> ghosts;
        private Hero hero;
        private Ghost redGhost;

        private int pelletsRemaining;
        private int energizersRemaining;

        private int gridWidth;
        private int gridHeight;

        private int cellSize;

        public Map(int cellSize, Hero hero, Ghost redGhost, List<Ghost> ghosts, List<Fence> fences, InteractiveGridObject[,] interactiveGrid, StaticGridObject[,] staticGrid, List<TweeningObject> movingObjects, int pelletsCount, int energizersCount)
        {
            this.cellSize = cellSize;
            this.hero = hero;
            this.redGhost = redGhost;
            this.ghosts = ghosts;
            this.fences = fences;
            this.interactiveGrid = interactiveGrid;
            this.staticGrid = staticGrid;
            this.movingObjects = movingObjects;
            pelletsRemaining = pelletsCount;
            energizersRemaining = energizersCount;
            gridWidth = staticGrid.GetLength(0);
            gridHeight = staticGrid.GetLength(1);

        }
        public List<Ghost> GetGhosts()
        {
            return ghosts;
        }
        public Hero GetHero() 
        { 
            return hero; 
        }
        public int GetCellSize()
        { 
            return cellSize; 
        }
        public int GetRemainingPelletsCount()
        {
            return pelletsRemaining;
        }
        public int GetRemainingEnergizersCount()
        {
            return energizersRemaining;
        }
        /*
         * Writes the corresponding BlankSpace object to the interactive grid thus effectively removing the object
         */
        public void RemovePellet(int gridX,  int gridY)
        {
            pelletsRemaining--;
            interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace(null,gridX,gridY);
        }
        public void RemoveEnergizer(int gridX, int gridY)
        {
            energizersRemaining--;
            interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace(null, gridX, gridY);
        }
        public bool ContainsPellet(int gridX, int gridY)
        {
            if (interactiveGrid[gridX, gridY] is Pellet)
            {
                return true;
            }
            return false;
        }
        public bool ContainsEnergizer(int gridX, int gridY)
        {
            if (interactiveGrid[gridX, gridY] is Energizer)
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
        public bool IsBlankCell(int x, int y)
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
        public bool IsFence(int x, int y)
        {
            return staticGrid[x, y] is Fence;
        }
        public int GetWrappedXCoordinate(int x)
        {
            if (x >= gridWidth) return x - gridWidth;
            if (x < 0) return x + gridWidth;
            return x;
        }
        public int GetWrappedYCoordinate(int y)
        {
            if (y >= gridHeight) return y - gridHeight;
            if (y < 0) return y + gridHeight;
            return y;
        }
        public int GetWrappedPixelXCoordinate(int pixelX)
        {
            if (pixelX < 0)
            {
                pixelX = GetPixelWidth() - GetCellSize() + pixelX;
            }
            if (pixelX > GetPixelWidth() - GetCellSize())
            {
                pixelX = pixelX - (GetPixelWidth() - GetCellSize());
            }
            return pixelX;
        }
        public int GetWrappedPixelYCoordinate(int pixelY)
        {
            if (pixelY < 0)
            {
                pixelY = GetPixelHeight() - GetCellSize() + pixelY;
            }
            if (pixelY > GetPixelHeight() - GetCellSize())
            {
                pixelY = pixelY - (GetPixelHeight() - GetCellSize());
            }
            return pixelY;
        }
        public List<TweeningObject> GetMovingObjects() 
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
        public Point GetHeroGridLocation()
        {
            return new Point(hero.GetGridX(), hero.GetGridY());
        }
        public Point GetRedGhostGridLocation()
        {
            return new Point(redGhost.GetGridX(), redGhost.GetGridY());
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

        private int spriteSize;
        private int formWidth;
        private int formHeight;

        public Painter(Form form, Map map)
        {
            spriteSize = map.GetCellSize();
            formWidth = map.GetPixelWidth();
            formHeight = map.GetPixelHeight();
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
            foreach (TweeningObject movingObjectToPaint in map.GetMovingObjects())
            {
                
                if (movingObjectToPaint.IsDrawable())
                {
                    int xPos = movingObjectToPaint.GetPixelX();
                    int yPos = movingObjectToPaint.GetPixelY();
                    bufferGraphics.DrawImage(movingObjectToPaint.GetImageToDraw(), xPos, yPos);
                }
            }
        }
        public void PaintStartScreen()
        {
            ClearBuffer();
            DisplayStartScreen();
            WriteBuffer();
        }
        public void PaintRunningGame(Map map, int score)
        {
            ClearBuffer();
            PaintGrids(map);
            PaintMovingObjects(map);
            DisplayScore(score);
            WriteBuffer();
        }
        public void PaintGameOverScreen(string gameOverText)
        {
            ClearBuffer();
            DisplayGameOverText(gameOverText);
            WriteBuffer();
        }
        private void DisplayScore(int score)
        {
            bufferGraphics.DrawString(FormText.scoreText + " " + score, FormText.scoreTextFont, FormText.textBrush, 0, 0);
        }
        private void DisplayStartScreen()
        {
            float textWidth = bufferGraphics.MeasureString(FormText.startScreenText, FormText.scoreTextFont).Width;
            bufferGraphics.DrawString(FormText.startScreenText, FormText.startScreenFont, FormText.textBrush, (formWidth-textWidth)/2, formHeight/2);
        }
        private void DisplayGameOverText(string gameOverText)
        {
            float textWidth = bufferGraphics.MeasureString(gameOverText, FormText.startScreenFont).Width;
            bufferGraphics.DrawString(gameOverText,FormText.startScreenFont,FormText.textBrush, (formWidth-textWidth)/2,formHeight/2);
        }

    }
}
