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
            return interactiveGrid[gridX, gridY] is Pellet;
        }
        public bool ContainsEnergizer(int gridX, int gridY)
        {
            return interactiveGrid[gridX, gridY] is Energizer;
   
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
            return staticGrid[x,y] is StaticLayerBlankSpace;
        }
        public bool IsFence(int x, int y)
        {
            return staticGrid[x, y] is Fence;
        }

        public Point GetWrappedGridLocation(int x, int y)
        {
            if (x >= gridWidth) x = x - gridWidth;
            if (x < 0) x = x + gridWidth;
            if (y >= gridHeight) y = y - gridHeight;
            if (y < 0) y = y + gridHeight;
            return new Point(x, y);
        }

        public bool IsOutOfBoundsPixelX(int pixelX)
        {
            if (pixelX < 0)
            {
                return true;
            }
            if (pixelX > pixelWidth - cellSize)
            {
                return true;
            }
            return false;
        }
        public bool IsOutOfBoundsPixelY(int pixelY)
        {
            if (pixelY < 0)
            {
                return true;
            }
            if (pixelY > pixelHeight - cellSize)
            {
                return true;
            }
            return false;
        }
        public int GetWrappedPixelXCoordinate(int pixelX)
        {
            if (pixelX < 0)
            {
                pixelX = pixelWidth - cellSize + pixelX;
            }
            if (pixelX > pixelWidth - cellSize)
            {
                pixelX = pixelX - (pixelWidth - cellSize);
            }
            return pixelX;
        }
        public int GetWrappedPixelYCoordinate(int pixelY)
        {
            if (pixelY < 0)
            {
                pixelY = pixelHeight - cellSize + pixelY;
            }
            if (pixelY > pixelHeight - cellSize)
            {
                pixelY = pixelY - (pixelHeight - cellSize);
            }
            return pixelY;
        }
        public List<TweeningObject> GetAllMovingObjects() 
        {
            return movingObjects;
        }
        public List<StaticLayerBlankSpace> GetAdjacentBlankCells(int x, int y)
        {
            List<StaticLayerBlankSpace> adjacentExits = new List<StaticLayerBlankSpace>();
            Direction direction = Direction.Up;
            for (int i = 0; i < 4; i++)
            {
                Point adjacentLocation = GetWrappedGridLocation(x + direction.X, y + direction.Y);
                StaticGridObject neighboringCell = GetStaticGridObject(adjacentLocation.X, adjacentLocation.Y);
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
