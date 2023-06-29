using System.IO;

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

        private const int cellSize = 48;
        private const int heroSpeed = 12; // if this is not a multiple of cellSize, it gets automatically readjusted to first smallest mutliple
        private const int ghostSpeed = 10;
        private const int ghostPrepareTime = 5;

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

        private const string mapFolder = "MapData";
        private const string mapFile = "map.txt";
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

        private static Bitmap blankSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, blankImage)), cellSize, cellSize);
        private static Bitmap fenceSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, fenceImage)), cellSize, cellSize);
        private static Bitmap ghostRedSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, ghostRedImage)), cellSize, cellSize);
        private static Bitmap heroSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, heroImage)), cellSize, cellSize);
        private static Bitmap pelletSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, pelletImage)), cellSize, cellSize);
        private static Bitmap wallSprite = new Bitmap(new Bitmap(Path.Combine(Application.StartupPath, imageFolder, wallImage)), cellSize, cellSize);

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
            List<Fence> fences = new List<Fence>();
            Hero hero = null;

            int pelletsCount = 0;

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
                        case ghostChar:
                            Ghost ghost = new Ghost(ghostRedSprite,x, y, ghostSpeed, cellSize, ghostPrepareTime);
                            tweeningObjects.Add(ghost);
                            ghosts.Add(ghost);
                            staticGrid[x, y] = new GhostHome(null, x,y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x,y);
                            break;
                        case ghostHomeChar:
                            staticGrid[x, y] = new GhostHome(null, x,y);
                            interactiveGrid[x, y] = new InteractiveLayerBlankSpace(null, x,y);
                            break;
                        case fenceChar:
                            Fence fence = new Fence(fenceSprite,x,y);
                            fences.Add(fence);
                            staticGrid[x, y] = fence;
                            interactiveGrid[x,y ] = new InteractiveLayerBlankSpace(null, x,y);
                            break;
                    }
                }
            }

            return new Map(cellSize,hero,ghosts,fences,interactiveGrid,staticGrid,tweeningObjects,pelletsCount);


        }


    }
}
