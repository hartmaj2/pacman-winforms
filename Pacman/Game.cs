using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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


    }
    /*
     * Takes care of all the game logic. Holds the Map, has access to the Painter and all the variables that 
     * shouldn't be own by some specific object but that should be visible in the entire game as a whole
     */
    class GameManager
    {
        private Map map;
        private Painter painter;

        private int pelletsEaten;

        public GameManager(Form form)
        {
            this.map = new Map();
            this.painter = new Painter(form, map);
        }

        public void Draw()
        {
            painter.Paint(map);
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
        protected abstract int xPos { get; set; }
        protected abstract int yPos { get; set; }
        protected abstract Direction direction { get; set; }
        public abstract void Move(Map map);

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

        private int _xPos;
        private int _yPos;
        private Direction _direction;

        public Hero(int x, int y)
        {
            _xPos = x;
            _yPos = y;
            _direction.X = 1;
            _direction.Y = 0;
        }

        public override void Move(Map map)
        {
            int newX = _xPos + _direction.X;
            int newY = _yPos + _direction.Y;

            
        }

        protected override int yPos
        {
            get { return _yPos; }
            set { _yPos = value; }
        }
        protected override int xPos
        {
            get { return _xPos; }
            set { _xPos = value; }
        }

        protected override Direction direction
        {
            get { return _direction; }
            set { _direction = value; }
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
    class Ghost : GameObject
    {

    }
    
    //TODO: implement movement of a player, communication with Map
    /*
     * Represents the underlying grid on which everything moves
     */
    class Map
    {

        private GameObject[,] grid;

        private int width;
        private int height;

        public Map()
        {
            readMap();
        }

        
        private void readMap()
        {
            string mapString = InputManager.GetMap();
            string[] separated = mapString.Split(new[] { "\r\n" },StringSplitOptions.None);

            height = separated.Length;
            width = separated[0].Length;

            grid = new GameObject[height, width];

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
                            grid[y, x] = new Blank();
                            break;
                        case 'W':
                            grid[y, x] = new Wall();
                            break;
                        case 'H':
                            grid[y, x] = new Hero(x,y);
                            break;
                        case 'P':
                            grid[y, x] = new Pellet();
                            break;
                        case 'G':
                            grid[y, x] = new Ghost();
                            break;

                    }
                }
            }

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

        public void Paint(Map map)
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
                        if (gameObject is Hero)
                        {
                            formGraphics.DrawImage(heroSprite, spriteSize * dx, spriteSize * dy);
                        }
                        if (gameObject is Pellet)
                        {
                            formGraphics.DrawImage(pelletSprite, spriteSize * dx, spriteSize * dy);
                        }
                        if (gameObject is Ghost)
                        {
                            formGraphics.DrawImage(ghostSprite, spriteSize * dx, spriteSize * dy);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Encountered a null GameObject reference");
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
