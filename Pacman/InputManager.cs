﻿namespace Pacman
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

        private static StaticObject[,] staticGrid = GetStaticGrid();
        private static DynamicObject[,] dynamicGrid = GetDynamicGrid();
        private static List<DiscreteMovableObject> discreteMovableObjects = GetDiscreteMovableObjects();
        private static List<TweeningMovableObject> tweeningMovableObjeccts = GetTweeningMovableObjects();
        private static Hero hero = GetHero();

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
            if (hero == null)
            {
                prepareMapData();
            }
            return hero;
        }
        public static DynamicObject[,] GetDynamicGrid()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return dynamicGrid;
        }
        public static StaticObject[,] GetStaticGrid()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return staticGrid;
        }
        public static List<DiscreteMovableObject> GetDiscreteMovableObjects()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return discreteMovableObjects;
        }
        public static List<TweeningMovableObject> GetTweeningMovableObjects()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
                mapDataLoaded = true;
            }
            return tweeningMovableObjeccts;
        }
        private static void prepareMapData()
        {
            string mapString = GetMap();
            string[] separated = mapString.Split(new[] { "\r\n" }, StringSplitOptions.None);

            int height = separated.Length;
            int width = separated[0].Length;

            staticGrid = new StaticObject[height, width];
            dynamicGrid = new DynamicObject[height, width];
            discreteMovableObjects = new List<DiscreteMovableObject>();
            tweeningMovableObjeccts = new List<TweeningMovableObject>();

            for (int y = 0; y < height; y++)
            {
                char[] lineChars = separated[y].ToCharArray();
                for (int x = 0; x < width; x++)
                {
                    char gameObjectChar = lineChars[x];

                    switch (gameObjectChar)
                    {
                        case blankChar:
                            staticGrid[y, x] = new StaticBlank();
                            dynamicGrid[y, x] = new DynamicBlank();
                            break;
                        case wallChar:
                            staticGrid[y, x] = new Wall();
                            dynamicGrid[y, x] = new DynamicBlank();
                            break;
                        case heroChar:
                            hero = new Hero(x, y);
                            tweeningMovableObjeccts.Add(hero);
                            staticGrid[y, x] = new StaticBlank();
                            dynamicGrid[y, x] = new DynamicBlank();
                            break;
                        case pelletChar:
                            staticGrid[y, x] = new StaticBlank();
                            dynamicGrid[y, x] = new Pellet();
                            break;
                        case ghostChar:
                            discreteMovableObjects.Add(new Ghost(x, y));
                            staticGrid[y, x] = new StaticBlank();
                            dynamicGrid[y, x] = new DynamicBlank();
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
}
