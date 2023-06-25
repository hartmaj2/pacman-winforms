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
    static class InputManager
    {

        private static readonly Bitmap blankSprite = Properties.Resources.blank;
        private static readonly Bitmap wallSprite = Properties.Resources.wall;
        private static readonly Bitmap heroSprite = Properties.Resources.hero;
        private static readonly Bitmap pelletSprite = Properties.Resources.pellet;
        private static readonly Bitmap ghostSprite = Properties.Resources.ghost;
         
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
    abstract class GameObject
    {
       
    }
    class Blank : GameObject
    {

    }
    class Wall : GameObject
    {

    }
    class Hero : GameObject
    {
        
    }
    class Pellet : GameObject
    {

    }
    class Ghost : GameObject
    {

    }
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
                            grid[y, x] = new Hero();
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
