using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{

    public static class InputManager
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

        public GameManager(Graphics formGraphics)
        {
            this.map = new Map();
            this.painter = new Painter(formGraphics);
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

        private GameObject[] grid;

        //private int width;
        //private int height;

        public Map()
        {
            readMap();
        }

        private void readMap()
        {
            string map = InputManager.GetMap();
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < map.Length; i++)
            {
                char gameObjectChar = map[i];
                switch(gameObjectChar) 
                {
                    case 'B':
                        list.Add(new Blank());
                        Console.WriteLine($"Adding a blank");
                        break;
                    case 'W':
                        list.Add(new Wall());
                        break;
                    case 'H':
                        list.Add(new Hero());
                        break;
                    case 'P':
                        list.Add(new Pellet());
                        break;
                    case 'G':
                        list.Add(new Ghost());
                        break;

                }
            }
            this.grid = list.ToArray();
        }

        public int GetSize()
        {
            return this.grid.Length;
        }

        public GameObject GetObjectAt(int index)
        {
            return grid[index];
        }
    }

    class Painter
    {
        private Graphics formGraphics;

        private Bitmap blankSprite;
        private Bitmap wallSprite;
        private Bitmap heroSprite;
        private Bitmap pelletSprite;
        private Bitmap ghostSprite;

        private int spriteSize = InputManager.GetSize();
        public Painter(Graphics formGraphics)
        {
            this.formGraphics = formGraphics;
            loadSprites();    
            //MessageBox.Show($"The sprite size is {spriteSize}");
            
        }

        public void Paint(Map map)
        {
            //formGraphics.DrawImage(sprites[0], 0, 0);
            for (int i = 0; i < map.GetSize();i++)
            {
                GameObject gameObject = map.GetObjectAt(i);
                if (gameObject != null)
                {
                    if (gameObject is Blank)
                    {
                        formGraphics.DrawImage(blankSprite, spriteSize * i, 0);
                    }
                    if (gameObject is Wall)
                    {
                        formGraphics.DrawImage(wallSprite, spriteSize * i, 0);
                    }
                    if (gameObject is Hero)
                    {
                        formGraphics.DrawImage(heroSprite, spriteSize * i, 0);
                    }
                    if (gameObject is Pellet)
                    {
                        formGraphics.DrawImage(pelletSprite, spriteSize * i, 0);
                    }
                    if (gameObject is Ghost)
                    {
                        formGraphics.DrawImage(ghostSprite, spriteSize * i, 0);
                    }
                }
                else
                {
                    MessageBox.Show("Encountered a null GameObject reference");
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
