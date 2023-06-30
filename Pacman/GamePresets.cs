/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
{
    /*
     * Takes care of all the input data. It is an abstraction so that if I change the way the data is input
     * I won't have to change the code anywhere else than here. 
     * 
     * Also all the program settings can be found here and are not spread across the program.
     */
    static class GamePresets
    {

        private const int cellSize = 48;

        private const int heroSpeed = 10; // if this is not a multiple of cellSize, it gets automatically readjusted to first smallest mutliple
        private const int ghostSpeed = 6;

        private const int redGhostPrepareTime = 1;
        private const int pinkGhostPrepareTime = 4;
        private const int blueGhostPrepareTime = 7;
        private const int orangeGhostPrepareTime = 12;

        public const double scatterModeDuration = 7;
        public const double chaseModeDuration = 15;
        public const double frightenedModeDuration = 8;

        public const int scorePerPellet = 10;
        public const int scorePerEnergizer = 50;
        public const int scoreIncreasePerGhostEaten = 200;

        /*
         * Sets how different game objects are represented in the map.txt file
         */
        private const char blankChar = '-';
        private const char wallChar = '#';
        private const char heroChar = 'H';
        private const char pelletChar = '.';
        private const char ghostRedChar = 'R';
        private const char ghostPinkChar = 'P';
        private const char ghostBlueChar = 'B';
        private const char ghostOrangeChar = 'O';
        private const char fenceChar = 'F';
        private const char energizerChar = 'E';

        private const string mapFolder = "MapData";
        private const string mapFile = "map.txt";

        /*
         * The sprites are just PNG images converted to a Bitmap to be rendered by the Painter class later.
         */
        private const string imageFolder = "Images";
        private const string fenceImage = "fence.png";
        private const string ghostRedImage = "ghost_red.png";
        private const string ghostPinkImage = "ghost_pink.png";
        private const string ghostBlueImage = "ghost_blue.png";
        private const string ghostOrangeImage = "ghost_orange.png";
        private const string ghostFrightenedImage = "ghost_scared.png";
        private const string heroImage = "hero.png";
        private const string pelletImage = "pellet.png";
        private const string wallImage = "wall.png";
        private const string energizerImage = "energizer.png";

        //private static Bitmap blankSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, blankImage)), cellSize, cellSize);
        private static Bitmap fenceSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, fenceImage)), cellSize, cellSize);
        private static Bitmap ghostRedSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostRedImage)), cellSize, cellSize);
        private static Bitmap ghostPinkSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostPinkImage)), cellSize, cellSize);
        private static Bitmap ghostBlueSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostBlueImage)), cellSize, cellSize);
        private static Bitmap ghostOrangeSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostOrangeImage)), cellSize, cellSize);
        private static Bitmap ghostFrightenedSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostFrightenedImage)), cellSize, cellSize);
        private static Bitmap heroSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, heroImage)), cellSize, cellSize);
        private static Bitmap pelletSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, pelletImage)), cellSize, cellSize);
        private static Bitmap wallSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, wallImage)), cellSize, cellSize);
        private static Bitmap energizerSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, energizerImage)), cellSize, cellSize);

        /*
         * Takes care of reading all the data from the map.txt text file that holds the layout of the map
         * after reading this data it creates and returns a corresponding Map object
         */
        public static Map PrepareAndReturnMap()
        {

            string mapString = File.ReadAllText(Path.Combine(Application.StartupPath,mapFolder,mapFile));
            string[] separated = mapString.Split(new[] { "\r\n" }, StringSplitOptions.None);

            int height = separated.Length;
            int width = separated[0].Length;

            StaticGridObject[,] staticGrid = new StaticGridObject[width, height];
            InteractiveGridObject[,] interactiveGrid = new InteractiveGridObject[width, height];
            List<TweeningObject> tweeningObjects = new List<TweeningObject>();
            List<Ghost> ghosts = new List<Ghost>();
            Hero hero = null;
            Ghost redGhost = null;

            int pelletsCount = 0;
            int energizersCount = 0;

            for (int y = 0; y < height; y++)
            {
                char[] lineChars = separated[y].ToCharArray();
                for (int x = 0; x < width; x++)
                {
                    char gameObjectChar = lineChars[x];

                    switch (gameObjectChar)
                    {
                        case blankChar:
                            staticGrid[x, y] = new StaticLayerBlankSpace(null,x,y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null,x,y);
                            break;
                        case wallChar:
                            staticGrid[x, y] = new Wall(wallSprite,x,y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null,x,y);
                            break;
                        case heroChar:
                            hero = new Hero(heroSprite,x, y, heroSpeed, cellSize);
                            tweeningObjects.Add(hero);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null,x,y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null,x,y);
                            break;
                        case pelletChar:
                            pelletsCount++;
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x,y);
                            interactiveGrid[x, y] = new Pellet(pelletSprite,x,y);
                            break;
                        case ghostRedChar:
                            redGhost = new RedGhost(ghostRedSprite, ghostFrightenedSprite,x, y, ghostSpeed, cellSize, redGhostPrepareTime);
                            tweeningObjects.Add(redGhost);
                            ghosts.Add(redGhost);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x, y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x,y);
                            break;
                        case ghostPinkChar:
                            PinkGhost pinkGhost = new PinkGhost(ghostPinkSprite, ghostFrightenedSprite, x, y, ghostSpeed, cellSize, pinkGhostPrepareTime);
                            tweeningObjects.Add(pinkGhost);
                            ghosts.Add(pinkGhost);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x, y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x, y);
                            break;
                        case ghostBlueChar:
                            BlueGhost blueGhost = new BlueGhost(ghostBlueSprite, ghostFrightenedSprite, x, y, ghostSpeed, cellSize, blueGhostPrepareTime);
                            tweeningObjects.Add(blueGhost);
                            ghosts.Add(blueGhost);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x, y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x, y);
                            break;
                        case ghostOrangeChar:
                            OrangeGhost orangeGhost = new OrangeGhost(ghostOrangeSprite, ghostFrightenedSprite, x, y, ghostSpeed, cellSize, orangeGhostPrepareTime);
                            tweeningObjects.Add(orangeGhost);
                            ghosts.Add(orangeGhost);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x, y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x, y);
                            break;
                        case fenceChar:
                            Fence fence = new Fence(fenceSprite,x,y);
                            staticGrid[x, y] = fence;
                            interactiveGrid[x,y ] = new InteractiveLayerBlankSpace(null, x,y);
                            break;
                        case energizerChar:
                            Energizer energizer = new Energizer(energizerSprite,x,y);
                            staticGrid[x, y] = new StaticLayerBlankSpace(null, x, y);
                            interactiveGrid[x, y] = energizer;
                            energizersCount++;
                            break;
                    }
                }
            }

            return new Map(cellSize,hero,redGhost,ghosts,interactiveGrid,staticGrid,tweeningObjects,pelletsCount,energizersCount);


        }


    }
}
