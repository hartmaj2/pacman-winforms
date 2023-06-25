namespace Pacman
{
    /*
     * Takes care of all the input data. It is an abstraction so that if I change the way the data is input
     * I won't have to change the code anywhere else than here. 
     * 
     * Also all the program settings can be found here and are not spread across the program.
     */
    static class InputManager
    {

        /*
         * Sets how different game objects are represented in the map.txt file
         */
        private const char blankChar = 'B';
        private const char wallChar = 'W';
        private const char heroChar = 'H';
        private const char pelletChar = 'P';
        private const char ghostChar = 'G';

        private const int cellSize = 100;

        /*
         * The sprites are just PNG images converted to a Bitmap to be rendered by the Painter class later.
         */
        private static Bitmap blankSprite = Properties.Resources.blank;
        private static Bitmap wallSprite = Properties.Resources.wall;
        private static Bitmap heroSprite = Properties.Resources.hero;
        private static Bitmap pelletSprite = Properties.Resources.pellet;
        private static Bitmap ghostSprite = Properties.Resources.ghost;

        private static readonly string map = Properties.Resources.map;

        private static GameObject[,] nonMovableGrid;
        private static List<MovableGameObject> movableGameObjects;
        private static Hero hero;

        private static bool mapDataLoaded = false;
        private static bool spriteSizeAdjusted = false;

        public static int GetCellSize()
        {
            return cellSize;
        }
        public static string GetMap()
        {
            return map;
        }
        public static Bitmap GetBlankSprite()
        {
            if (!spriteSizeAdjusted)
            {
                adjustSpriteSize();
            }
            return blankSprite;
        }
        public static Bitmap GetWallSprite()
        {
            if (!spriteSizeAdjusted)
            {
                adjustSpriteSize();
            }
            return wallSprite;
        }
        public static Bitmap GetHeroSprite()
        {
            if (!spriteSizeAdjusted)
            {
                adjustSpriteSize();
            }
            return heroSprite;
        }
        public static Bitmap GetPelletSprite()
        {
            if (!spriteSizeAdjusted)
            {
                adjustSpriteSize();
            }
            return pelletSprite;
        }
        public static Bitmap GetGhostSprite()
        {
            if (!spriteSizeAdjusted)
            {
                adjustSpriteSize();
            }
            return ghostSprite;
        }
        public static Hero GetHero()
        {
            return hero;
        }
        public static GameObject[,] GetGrid()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return nonMovableGrid;
        }
        public static List<MovableGameObject> GetMovableGameObjects()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return movableGameObjects;
        }
        private static void prepareMapData()
        {
            string mapString = GetMap();
            string[] separated = mapString.Split(new[] { "\r\n" }, StringSplitOptions.None);

            int height = separated.Length;
            int width = separated[0].Length;

            nonMovableGrid = new GameObject[height, width];
            movableGameObjects = new List<MovableGameObject>();

            for (int y = 0; y < height; y++)
            {
                char[] lineChars = separated[y].ToCharArray();
                for (int x = 0; x < width; x++)
                {
                    char gameObjectChar = lineChars[x];
                    //Console.WriteLine($"({y},{x}) {gameObjectChar}");
                    switch (gameObjectChar)
                    {
                        case blankChar:
                            nonMovableGrid[y, x] = new Blank();
                            break;
                        case wallChar:
                            nonMovableGrid[y, x] = new Wall();
                            break;
                        case heroChar:
                            hero = new Hero(x, y);
                            movableGameObjects.Add(hero);
                            nonMovableGrid[y, x] = new Blank();
                            break;
                        case pelletChar:
                            nonMovableGrid[y, x] = new Pellet();
                            break;
                        case ghostChar:
                            movableGameObjects.Add(new Ghost(x,y));
                            nonMovableGrid[y, x] = new Blank();
                            break;

                    }
                }
            }

            
        }

        private static void adjustSpriteSize()
        {
            blankSprite = new Bitmap(blankSprite, new Size(cellSize, cellSize));
            wallSprite = new Bitmap(wallSprite, new Size(cellSize, cellSize));
            heroSprite = new Bitmap(heroSprite, new Size(cellSize, cellSize));
            pelletSprite = new Bitmap(pelletSprite, new Size(cellSize, cellSize));
            ghostSprite = new Bitmap(ghostSprite, new Size(cellSize, cellSize));
        }

    }
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
            painter.PaintGrid(map);
            painter.PaintMovableGameObjects(map);
        }

        public void SetHeroDirection(Direction direction)
        {
            hero.SetDirection(direction);
        }

    }
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
    abstract class MovableGameObject : GameObject
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
     * Represents a blank space where nothing lives but in case I wanted to add some behavior I din't want 
     * it to be null.
     */
    class Blank : GameObject
    {
    
    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : GameObject
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
    class Pellet : GameObject
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
     * Represents the underlying grid on which everything moves
     */
    class Map
    {
        private GameObject[,] grid;
        private List<MovableGameObject> movableObjects;

        private int gridWidth;
        private int gridHeight;

        private int cellSize = InputManager.GetCellSize();

        public Map()
        {
            grid = InputManager.GetGrid();
            movableObjects = InputManager.GetMovableGameObjects();

            gridWidth = grid.GetLength(1);
            gridHeight = grid.GetLength(0);

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
        public GameObject GetObjectAtCoordinates(int x, int y)
        {
            return grid[y,x];
        }
        public bool IsFreeCoordinate(int x, int y)
        {
            if (grid[y,x] is Blank)
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
    class Painter
    {
        private Graphics formGraphics;
        private Graphics bufferGraphics;
        private Form form;

        private Bitmap blankSprite = InputManager.GetBlankSprite();
        private Bitmap wallSprite = InputManager.GetWallSprite();
        private Bitmap heroSprite = InputManager.GetHeroSprite();
        private Bitmap pelletSprite = InputManager.GetPelletSprite();
        private Bitmap ghostSprite = InputManager.GetGhostSprite();

        private int spriteSize = InputManager.GetCellSize();
        public Painter(Form form, Map map)
        {
            this.form = form;
            adjustFormSize(map);
            formGraphics = form.CreateGraphics();
            bufferGraphics = Graphics.FromImage(new Bitmap(spriteSize * map.GetCoordinateWidth(), spriteSize * map.GetCoordinateHeight()));
        }

        private void adjustFormSize(Map map)
        {
           
           form.ClientSize = new Size(spriteSize * map.GetCoordinateWidth(), spriteSize * map.GetCoordinateHeight());
        }

        public void PaintGrid(Map map)
        {
            formGraphics.Clear(Color.Black);
            for (int dy = 0; dy < map.GetCoordinateHeight();dy++)
            {
                for (int dx = 0; dx < map.GetCoordinateWidth(); dx++)
                {
                    GameObject gameObject = map.GetObjectAtCoordinates(dx,dy);
                    if (gameObject != null)
                    {
                        if (gameObject is Blank)
                        {
                            formGraphics.DrawImage(blankSprite, spriteSize * dx, spriteSize * dy);
                        }
                        if (gameObject is Wall)
                        {
                            formGraphics.DrawImage(wallSprite, spriteSize * dx, spriteSize * dy);
                        }        
                        if (gameObject is Pellet)
                        {
                            formGraphics.DrawImage(pelletSprite, spriteSize * dx, spriteSize * dy);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Encountered a null GameObject reference");
                    }
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
                        formGraphics.DrawImage(heroSprite, spriteSize *  xPos, spriteSize * yPos);
                    }
                    if (movableGameObject is Ghost)
                    {
                        formGraphics.DrawImage(ghostSprite, spriteSize * xPos, spriteSize * yPos);
                    }
                }
            }
        }

    }
}
