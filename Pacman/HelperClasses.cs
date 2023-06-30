/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
{
    /*
     * Represents either a cardinal direction vector with unit size or a zero vector
     */
    struct Direction
    {
        // aliases for all cardinal directions and the zero vector
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

        /*
         * Rotating a cardinal direciotn always switches the x and y component.
         * Sometimes we also have to flip the sign of the components (flipped zero stays zero)
         */
        public void RotateRight()
        {
            int temp = X; X = Y; Y = temp;
            if (Y == 0)
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
        private readonly int gridWidth;
        private readonly int gridHeight;
        private readonly int cellSize;

        private StaticGridObject[,] staticGrid;
        private InteractiveGridObject[,] interactiveGrid;

        private List<TweeningObject> movingObjects;

        private List<Ghost> ghosts;
        private Hero hero;
        private Ghost redGhost; // we need reference to the red ghost because orange ghost uses its position

        private int pelletsRemaining;
        private int energizersRemaining;


        public Map(int cellSize, Hero hero, Ghost redGhost, List<Ghost> ghosts, InteractiveGridObject[,] interactiveGrid, StaticGridObject[,] staticGrid, List<TweeningObject> movingObjects, int pelletsCount, int energizersCount)
        {
            gridWidth = staticGrid.GetLength(0);
            gridHeight = staticGrid.GetLength(1);
            this.cellSize = cellSize;

            this.staticGrid = staticGrid;
            this.interactiveGrid = interactiveGrid;

            this.movingObjects = movingObjects;

            this.ghosts = ghosts;
            this.hero = hero;
            this.redGhost = redGhost;

            pelletsRemaining = pelletsCount;
            energizersRemaining = energizersCount;

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

        public bool PixelXOutOfBound(int pixelX)
        {
            if (pixelX < 0)
            {
                return true;
            }
            if (pixelX > GetPixelWidth() - GetCellSize())
            {
                return true;
            }
            return false;
        }
        public bool PixelYOutOfBounds(int pixelY)
        {
            if (pixelY < 0)
            {
                return true;
            }
            if (pixelY > GetPixelHeight() - GetCellSize())
            {
                return true;
            }
            return false;
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
        public List<StaticLayerBlankSpace> GetAdjacentBlankCells(int x, int y)
        {
            List<StaticLayerBlankSpace> adjacentExits = new List<StaticLayerBlankSpace>();
            Direction direction = Direction.Up;
            for (int i = 0; i < 4; i++)
            {
                int adjacentX = GetWrappedXCoordinate(x + direction.X);
                int adjacentY = GetWrappedYCoordinate(y + direction.Y);
                StaticGridObject neighboringCell = GetStaticGridObject(adjacentX, adjacentY);
                if (neighboringCell is StaticLayerBlankSpace)
                {
                    adjacentExits.Add((StaticLayerBlankSpace)neighboringCell);
                }
                direction.RotateRight();
            }
            return adjacentExits;
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
