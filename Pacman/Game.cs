using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
         * The sprites are just PNG images converted to a Bitmap to be rendered by the Painter class later.
         */
        private static readonly Bitmap blankSprite = Properties.Resources.blank;
        private static readonly Bitmap wallSprite = Properties.Resources.wall;
        private static readonly Bitmap heroSprite = Properties.Resources.hero;
        private static readonly Bitmap pelletSprite = Properties.Resources.pellet;
        private static readonly Bitmap ghostSprite = Properties.Resources.ghost;

        private static readonly char blankChar = 'B';
        private static readonly char wallChar = 'W';
        private static readonly char heroChar = 'H';
        private static readonly char pelletChar = 'P';
        private static readonly char ghostChar = 'G';

        private static readonly int size = 100;

        private static readonly string map = Properties.Resources.map;

        private static GameObject[,] nonMovableGrid;
        private static List<MovableGameObject> movableGameObjects;
        private static Hero hero;

        public static int GetSize()
        {
            return size;
        }
        public static string GetMap()
        {
            return map;
        }
        public static Bitmap GetBlankSprite()
        {
            return blankSprite;
        }
        public static Bitmap GetWallSprite()
        {
            return wallSprite;
        }
        public static Bitmap GetHeroSprite()
        {
            return heroSprite;
        }
        public static Bitmap GetPelletSprite()
        {
            return pelletSprite;
        }
        public static Bitmap GetGhostSprite()
        {
            return ghostSprite;
        }
        public static Hero GetHero()
        {
            return hero;
        }
        public static GameObject[,] GetGrid()
        {
            if (nonMovableGrid == null)
            {
                PrepareMapData();
            }
            return nonMovableGrid;
        }
        public static List<MovableGameObject> GetMovableGameObjects()
        {
            if (movableGameObjects == null)
            {
                PrepareMapData();
            }
            return movableGameObjects;
        }
        private static void PrepareMapData()
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
                        case 'B':
                            nonMovableGrid[y, x] = new Blank();
                            break;
                        case 'W':
                            nonMovableGrid[y, x] = new Wall();
                            break;
                        case 'H':
                            hero = new Hero(x, y);
                            movableGameObjects.Add(hero);
                            nonMovableGrid[y, x] = new Blank();
                            break;
                        case 'P':
                            nonMovableGrid[y, x] = new Pellet();
                            break;
                        case 'G':
                            movableGameObjects.Add(new Ghost(x,y));
                            nonMovableGrid[y, x] = new Blank();
                            break;

                    }
                }
            }

            
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

            if (map.IsFree(newX, newY))
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
    //TODO: implement movement of a player, communication with Map
    /*
     * Represents the underlying grid on which everything moves
     */
    class Map
    {

        private GameObject[,] grid;

        private List<MovableGameObject> movableObjects;

        private int width;
        private int height;

        public Map()
        {
            grid = InputManager.GetGrid();
            movableObjects = InputManager.GetMovableGameObjects();
            height = grid.GetLength(0);
            width = grid.GetLength(1);
        }

        public int GetWidth()
        {
            return width;
        }

        public int GetHeight()
        {
            return height;
        }

        public GameObject GetObjectAt(int x, int y)
        {
            return grid[y,x];
        }

        public bool IsFree(int x, int y)
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
        private Form form;

        private Bitmap blankSprite;
        private Bitmap wallSprite;
        private Bitmap heroSprite;
        private Bitmap pelletSprite;
        private Bitmap ghostSprite;

        private int spriteSize = InputManager.GetSize();
        public Painter(Form form, Map map)
        {
            this.form = form;
            adjustFormSize(map);
            formGraphics = form.CreateGraphics(); 
            loadSprites();
            //MessageBox.Show($"The sprite size is {spriteSize}");

        }

        private void adjustFormSize(Map map)
        {
           form.ClientSize = new Size(spriteSize * map.GetWidth(), spriteSize * map.GetHeight());
        }

        public void PaintGrid(Map map)
        {
            for (int dy = 0; dy < map.GetHeight();dy++)
            {
                for (int dx = 0; dx < map.GetWidth(); dx++)
                {
                    GameObject gameObject = map.GetObjectAt(dx,dy);
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

        /*
         * Initializes all the bitmaps for the corresponding GameObjects from the InputManager
         */
        private void loadSprites()
        {
            blankSprite = new Bitmap(InputManager.GetBlankSprite(), new Size(spriteSize,spriteSize));
            wallSprite = new Bitmap(InputManager.GetWallSprite(), new Size(spriteSize, spriteSize));
            heroSprite = new Bitmap(InputManager.GetHeroSprite(), new Size(spriteSize, spriteSize));
            pelletSprite = new Bitmap(InputManager.GetPelletSprite(), new Size(spriteSize, spriteSize));
            ghostSprite = new Bitmap(InputManager.GetGhostSprite(), new Size(spriteSize, spriteSize));
        }
    }
}
