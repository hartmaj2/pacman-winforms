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
        private const char blankChar = '-';
        private const char wallChar = '#';
        private const char heroChar = 'P';
        private const char pelletChar = '.';
        private const char ghostChar = 'G';
        private const char ghostHomeChar = 'H';
        private const char fenceChar = 'F';

        private const int cellSize = 48;
        private const int heroSpeed = 12; // if this is not a multiple of cellSize, it gets automatically readjusted to first smallest mutliple
        private const int ghostSpeed = 10;

        /*
         * The sprites are just PNG images converted to a Bitmap to be rendered by the Painter class later.
         */
        private const string imageFolder = "Images";
        private const string blankImage = "blank.png";
        private const string fenceImage = "fence.png";
        private const string ghostRedImage = "ghost_red.png";
        private const string heroImage = "hero.png";
        private const string pelletImage = "pellet.png";
        private const string wallImage = "wall.png";

        private static string imagePath = Path.Combine(Application.StartupPath, imageFolder);

        private static Bitmap blankSprite;
        private static Bitmap fenceSprite;
        private static Bitmap ghostSprite;
        private static Bitmap heroSprite;
        private static Bitmap pelletSprite;
        private static Bitmap wallSprite;

        private static readonly string map = Properties.Resources.map;

        private static StaticGridObject[,] staticGrid;
        private static InteractiveGridObject[,] dynamicGrid;
        private static List<TweeningObjects> tweeningObjects;
        private static Hero hero;
        private static List<Ghost> ghosts;
        private static List<Fence> fences;

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
        public static Bitmap GetFenceSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return fenceSprite;
        }
        public static Bitmap GetBlankSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return blankSprite;
        }
        public static Bitmap GetWallSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return wallSprite;
        }
        public static Bitmap GetHeroSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return heroSprite;
        }
        public static Bitmap GetPelletSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return pelletSprite;
        }
        public static Bitmap GetGhostSprite()
        {
            if (!spriteSizeAdjusted)
            {
                LoadSprites();
            }
            return ghostSprite;
        }
        public static Hero GetHero()
        {
            if (!mapDataLoaded)
            {
                PrepareMapData();
            }
            return hero;
        }
        public static List<Fence> GetFences()
        {
            if (!mapDataLoaded) 
            { 
                PrepareMapData();
            }
            return fences;
        }
        public static List<Ghost> GetGhosts()
        {
            if (!mapDataLoaded)
            {
                PrepareMapData();
            }
            return ghosts;
        }
        public static InteractiveGridObject[,] GetDynamicGrid()
        {
            if (!mapDataLoaded)
            {
                PrepareMapData();
            }
            return dynamicGrid;
        }
        public static StaticGridObject[,] GetStaticGrid()
        {
            if (!mapDataLoaded)
            {
                PrepareMapData();
            }
            return staticGrid;
        }
        public static List<TweeningObjects> GetTweeningObjects()
        {
            if (!mapDataLoaded)
            {
                PrepareMapData();
            }
            return tweeningObjects;
        }
        private static void PrepareMapData()
        {
            mapDataLoaded = true;

            string mapString = GetMap();
            string[] separated = mapString.Split(new[] { "\r\n" }, StringSplitOptions.None);

            int height = separated.Length;
            int width = separated[0].Length;

            staticGrid = new StaticGridObject[width, height];
            dynamicGrid = new InteractiveGridObject[width, height];
            tweeningObjects = new List<TweeningObjects>();
            ghosts = new List<Ghost>();
            fences = new List<Fence>();

            for (int y = 0; y < height; y++)
            {
                char[] lineChars = separated[y].ToCharArray();
                for (int x = 0; x < width; x++)
                {
                    char gameObjectChar = lineChars[x];

                    switch (gameObjectChar)
                    {
                        case blankChar:
                            staticGrid[x, y] = new StaticLayerBlankSpace(x,y);
                            dynamicGrid[x, y] = new InteractiveLayerBlankSpace(x,y);
                            break;
                        case wallChar:
                            staticGrid[x, y] = new Wall(x,y);
                            dynamicGrid[x, y] = new InteractiveLayerBlankSpace(x,y);
                            break;
                        case heroChar:
                            hero = new Hero(x, y, heroSpeed);
                            tweeningObjects.Add(hero);
                            staticGrid[x, y] = new StaticLayerBlankSpace(x,y);
                            dynamicGrid[x, y] = new InteractiveLayerBlankSpace(x,y);
                            break;
                        case pelletChar:
                            staticGrid[x, y] = new StaticLayerBlankSpace(x,y);
                            dynamicGrid[x, y] = new Pellet(x,y);
                            break;
                        case ghostChar:
                            Ghost ghost = new Ghost(x, y, ghostSpeed);
                            tweeningObjects.Add(ghost);
                            ghosts.Add(ghost);
                            staticGrid[x, y] = new GhostHome(x,y);
                            dynamicGrid[x, y] = new InteractiveLayerBlankSpace(x,y);
                            break;
                        case ghostHomeChar:
                            staticGrid[x, y] = new GhostHome(x,y);
                            dynamicGrid[x, y] = new InteractiveLayerBlankSpace(x,y);
                            break;
                        case fenceChar:
                            Fence fence = new Fence(x,y);
                            fences.Add(fence);
                            staticGrid[x, y] = fence;
                            dynamicGrid[x,y ] = new InteractiveLayerBlankSpace(x,y);
                            break;
                    }
                }
            }


        }
        private static void LoadSprites()
        {
            blankSprite = new Bitmap(new Bitmap(Path.Combine(imagePath,blankImage)),cellSize,cellSize);
            fenceSprite = new Bitmap(new Bitmap(Path.Combine(imagePath, fenceImage)), cellSize, cellSize);
            wallSprite = new Bitmap(new Bitmap(Path.Combine(imagePath, wallImage)), cellSize, cellSize);
            heroSprite = new Bitmap(new Bitmap(Path.Combine(imagePath, heroImage)), cellSize, cellSize);
            pelletSprite = new Bitmap(new Bitmap(Path.Combine(imagePath, pelletImage)), cellSize, cellSize);
            ghostSprite = new Bitmap(new Bitmap(Path.Combine(imagePath, ghostRedImage)), cellSize, cellSize);
        }

    }
}
