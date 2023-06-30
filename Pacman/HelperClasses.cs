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
        public readonly int gridWidth;
        public readonly int gridHeight;

        public readonly int pixelWidth;
        public readonly int pixelHeight;

        public readonly int cellSize;

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

            pixelWidth = gridWidth * cellSize;
            pixelHeight = gridHeight * cellSize;

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

        public Ghost GetRedGhost() 
        {
            return redGhost;
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
         * The two following check whether the interactive grid contains pellet/energizer
         * If yes, the methods decrease the current pellet/energizer counter, remove the pellet/energizer from the cell
         * Finally they return true if the pellet/energizer was removed and false otherwise
         */
        public bool TryRemovePellet(int gridX, int gridY)
        {
            if (interactiveGrid[gridX, gridY] is Pellet)
            {
                pelletsRemaining--;
                interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace(null, gridX, gridY);
                return true;
            }
            return false;
        }
        public bool TryRemoveEnergizer(int gridX, int gridY)
        {
            if (interactiveGrid[gridX, gridY] is Energizer)
            {
                energizersRemaining--;
                interactiveGrid[gridX, gridY] = new InteractiveLayerBlankSpace(null, gridX, gridY);
                return true;
            }
            return false;
   
        }

        public StaticGridObject GetStaticGridObject(int gridX, int gridY)
        {
            return staticGrid[gridX,gridY];
        }
        public InteractiveGridObject GetInteractiveGridObject(int gridX,int gridY)
        {
            return interactiveGrid[gridX,gridY];
        }

        /*
         * The two following are used by moving objects to determine which cells they can reach
         * The second one is used by ghosts specifically
         */
        public bool IsBlankCell(int gridX, int gridY)
        {
            return staticGrid[gridX,gridY] is StaticLayerBlankSpace;
        }
        public bool IsFence(int gridX, int gridY)
        {
            return staticGrid[gridX, gridY] is Fence;
        }

        /*
         * The following two methods help moving game objects to appear on the other side of the map if they leave the map
         */
        public Point GetWrappedGridLocation(int gridX, int gridY)
        {
            if (gridX >= gridWidth) gridX = gridX - gridWidth;
            if (gridX < 0) gridX = gridX + gridWidth;
            if (gridY >= gridHeight) gridY = gridY - gridHeight;
            if (gridY < 0) gridY = gridY + gridHeight;
            return new Point(gridX, gridY);
        }
        public Point GetWrappedPixelLocation(int pixelX, int pixelY)
        {
            if (pixelX < 0)
            {
                pixelX = pixelWidth - cellSize + pixelX;
            }
            if (pixelX > pixelWidth - cellSize)
            {
                pixelX = pixelX - (pixelWidth - cellSize);
            }

            if (pixelY < 0)
            {
                pixelY = pixelHeight - cellSize + pixelY;
            }
            if (pixelY > pixelHeight - cellSize)
            {
                pixelY = pixelY - (pixelHeight - cellSize);
            }

            return new Point(pixelX, pixelY);
        }

        /*
         * Returns true if either one of the pixel coordinates is out of bounds
         */
        public bool IsOutOfBoundsPixelLocation(int pixelX, int pixelY)
        {
            if (pixelX < 0 || pixelX > pixelWidth - cellSize)
            {
                return true;
            }
            if (pixelY < 0 || pixelY > pixelHeight - cellSize)
            {
                return true;
            }
            return false;
        }
        public List<TweeningObject> GetAllMovingObjects() 
        {
            return movingObjects;
        }

        /* 
         * Returns all adjacent cells that are blank. It is used by ghosts pathfinding algorithm
         */
        public List<StaticLayerBlankSpace> GetAdjacentBlankCells(int gridX, int gridY)
        {
            List<StaticLayerBlankSpace> adjacentExits = new List<StaticLayerBlankSpace>();

            // we start with pointing up from the current tile and then we rotate the direction four times to check all adjacent cells
            Direction direction = Direction.Up;
            for (int i = 0; i < 4; i++)
            {
                Point adjacentLocation = GetWrappedGridLocation(gridX + direction.X, gridY + direction.Y);
                StaticGridObject neighboringCell = GetStaticGridObject(adjacentLocation.X, adjacentLocation.Y);
                if (neighboringCell is StaticLayerBlankSpace)
                {
                    adjacentExits.Add((StaticLayerBlankSpace)neighboringCell);
                }
                direction.RotateRight();
            }
            return adjacentExits;
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
        private readonly Graphics formGraphics; // the actual graphics elements that gets painted on the form
        private readonly Graphics bufferGraphics; // the buffer that we paint on before propagating change to formGraphics
        private readonly Bitmap bufferBitmap;

        private readonly int spriteSize;
        private readonly int formWidth;
        private readonly int formHeight;

        public Painter(Form form, Map map)
        {
            spriteSize = map.cellSize;
            formWidth = map.pixelWidth;
            formHeight = map.pixelHeight;
            form.ClientSize = new Size(formWidth, formHeight);
            formGraphics = form.CreateGraphics();
            bufferBitmap = new Bitmap(formWidth, formHeight);
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
            for (int dy = 0; dy < map.gridHeight;dy++)
            {
                for (int dx = 0; dx < map.gridWidth; dx++)
                {
                    PaintStaticGridObjectAtCoordinate(map, dx, dy);
                    PaintInteractiveGridObjectAtCoordinate(map, dx, dy);
                    
                }
            }
        }
        private void PaintMovingObjects(Map map)
        {
            foreach (TweeningObject movingObjectToPaint in map.GetAllMovingObjects())
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
