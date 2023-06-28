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
        private static Bitmap blankSprite = Properties.Resources.blank;
        private static Bitmap wallSprite = Properties.Resources.wall;
        private static Bitmap heroSprite = Properties.Resources.hero;
        private static Bitmap pelletSprite = Properties.Resources.pellet;
        private static Bitmap ghostSprite = Properties.Resources.ghost;
        private static Bitmap fenceSprite = Properties.Resources.fence;

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
                adjustSpriteSize();
            }
            return fenceSprite;
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
            if (!mapDataLoaded)
            {
                prepareMapData();
            }
            return hero;
        }
        public static List<Fence> GetFences()
        {
            if (!mapDataLoaded) 
            { 
                prepareMapData();
            }
            return fences;
        }
        public static List<Ghost> GetGhosts()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
            }
            return ghosts;
        }
        public static InteractiveGridObject[,] GetDynamicGrid()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
            }
            return dynamicGrid;
        }
        public static StaticGridObject[,] GetStaticGrid()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
            }
            return staticGrid;
        }
        public static List<TweeningObjects> GetTweeningObjects()
        {
            if (!mapDataLoaded)
            {
                prepareMapData();
            }
            return tweeningObjects;
        }
        private static void prepareMapData()
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
        private static void adjustSpriteSize()
        {
            blankSprite = new Bitmap(blankSprite, new Size(cellSize, cellSize));
            wallSprite = new Bitmap(wallSprite, new Size(cellSize, cellSize));
            heroSprite = new Bitmap(heroSprite, new Size(cellSize, cellSize));
            pelletSprite = new Bitmap(pelletSprite, new Size(cellSize, cellSize));
            ghostSprite = new Bitmap(ghostSprite, new Size(cellSize, cellSize));
            fenceSprite = new Bitmap(fenceSprite, new Size(cellSize, cellSize));
        }

    }
}
