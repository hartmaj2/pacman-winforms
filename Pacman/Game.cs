using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Pacman
{

    static class InputManager
    {
        private static readonly Bitmap[] sprites = {Properties.Resources.blank,
                                  Properties.Resources.wall,
                                  Properties.Resources.hero,
                                  Properties.Resources.pellet,
                                  Properties.Resources.ghost
                                 };

        private static readonly double scale = 4;
        
        private static readonly string map = Properties.Resources.map;

        public static Bitmap[] GetSprites()
        {
            return sprites;
        }
                
        public static double GetScale()
        {
            return scale;
        }


    }
    class GameManager
    {
        private Map map;
        private Painter painter;

        private int pelletsEaten;

        public GameManager(Graphics formGraphics)
        {
            this.painter = new Painter(InputManager.GetScale(), InputManager.GetSprites(), formGraphics);
        }

        public void Draw()
        {
            painter.Paint(map);
        }
    }
    class Map
    {
        private char[,] grid;
        private int width;
        private int height;

        private void readMap()
        {

        }
    }

    class Painter
    {
        private Graphics formGraphics;
        private Bitmap[] sprites;
        public Painter(double imageScale, Bitmap[] inputSprites, Graphics formGraphics)
        {
            this.formGraphics = formGraphics;
            loadSprites(imageScale, inputSprites);
            
        }

        public void Paint(Map map)
        {
            formGraphics.DrawImage(sprites[0], 0, 0);
        }

        private void loadSprites(double scale, Bitmap[] inputSprites)
        {
            this.sprites = new Bitmap[inputSprites.Length];
            for (int i = 0; i < inputSprites.Length; i++)
            {
                this.sprites[i] = new Bitmap(inputSprites[i], new Size((int)(inputSprites[i].Width * scale), (int)(inputSprites[i].Height * scale)));

            }
        }
    }
}
