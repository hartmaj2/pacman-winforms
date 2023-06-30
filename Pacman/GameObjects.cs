/*
 * Jan Hartman, 1st year, group 38
 * Summer Semester 2022/23
 * Programming NPRG031
*/

namespace Pacman
{
    /*
    * Everything that lives inside the game should inherit this. It has to be something that can 
    * be located somewhere in the game world. It doesn't have to be moving. It can be walls, pellets, anything.
    * It can be something that can be drawn but it necessarily doesn't have to (hence the IsDrawable method).
    */
    abstract class GameObject
    {
        protected Bitmap sprite;
        public abstract int GetGridX();
        public abstract int GetGridY();
        public double GetDistanceToCell(int x, int y)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x - GetGridX()), 2) + Math.Pow(Math.Abs(y - GetGridY()), 2));
        }
        public virtual Bitmap GetImageToDraw()
        {
            return sprite;
        }
        public virtual bool IsDrawable()
        {
            return sprite != null;
        }
    } 
    abstract class GridObject : GameObject
    {
        private int gridX;
        private int gridY;
        public GridObject(Bitmap image, int x, int y)
        {
            sprite = image;
            gridX = x;
            gridY = y;
        }
        public override int GetGridX()
        { 
            return gridX; 
        }
        public override int GetGridY() 
        {  
            return gridY; 
        }
    }
    /*
     * This is a game object that occupies a certain place on the grid but the player cannot interact with like walls
     * fences etc.
     */
    abstract class StaticGridObject : GridObject
    {
        public StaticGridObject(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    /*
     * These objects are not moving but can be eaten or otherwise interacted with by some kind of a player. 
     * They live on a different layer on the map
     */
    abstract class InteractiveGridObject : GridObject
    {
        public InteractiveGridObject(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    /* 
     * This is anything in the game that can move its position. It may be both something that can move 
     * on the grid or something that moves independently of the grid.
     */
    abstract class MovingObject : GameObject
    {       

        protected Direction direction;

        public void TurnAround()
        {
            direction = direction.OppositeDirection();
        }
        public abstract void Move(Map map);

        protected abstract bool IsReachableCell(int x, int y, Map map);

        /*
         * Sets the direction towards a neighboring cell. DOESN'T WORK ON DIAGONAL PIECES
         */
        protected void SetDirectionTowardsNeighbor(StaticGridObject neighbor)
        {
            Direction newDirection = Direction.None;
            newDirection.X = neighbor.GetGridX() - GetGridX();
            newDirection.Y = neighbor.GetGridY() - GetGridY();
            direction = newDirection;

        }

    }
  
    /* 
     * These objects can move independently of the game grid but are able to tell you what would be their 
     * corresponding location on the game grid
     * Their movement consists of movement cycles when they are just moving to the next grid cell.
     * In between those cycles these objects are deciding what to do next using the TryStartNextMovement
     */
    abstract class TweeningObject : MovingObject
    {
        protected int pixelX;
        protected int pixelY;

        protected int maxMovementFrame;
        protected int movementFrame;
        protected int movementSpeed;

        protected int mapCellSize;

        protected bool isMoving; // tracks if we are currenly inside a movement cycle or just finished moving

        public TweeningObject(Bitmap image, int gridX, int gridY, int speed, int cellSize)
        {
            sprite = image;
            mapCellSize = cellSize;
            pixelX = gridX * mapCellSize;
            pixelY = gridY * mapCellSize;
            SetTweenSpeed(speed); // automatically adjust movements speed to divide cell size without remiander
            SetNotMoving();
        }

        /*
         * Implements the basic outline of how a general tweening object moves (both player and ghosts)
         */
        public override void Move(Map map)
        { 
            if (isMoving)
            {
                ContinueMoving(); // move by certain number of pixels
            }
            if (!isMoving)
            {
                StartNextMovementCycle(map); // decide where to move next
                ContinueMoving(); // start moving (this can also mean no movement if direction is set to none)
            }
            WraparoundIfOutOfBounds(map); // if character could get out of bounds, we put him on the other end of map
        }   
        
        /*
         * Called at end of every movement cycle. Decalred abstract as every subclass 
         * has its own way of handling this state
         */
        protected abstract void StartNextMovementCycle(Map map);
 
        /*
         * Moves the character by the amount of pixels represented by movement speed 
         * and checks whether we have reached the end of movement cycle
         */
        private void ContinueMoving()
        {
            if (movementFrame < maxMovementFrame)
            {
                movementFrame++;
                pixelX += direction.X * movementSpeed;
                pixelY += direction.Y * movementSpeed;
            }
            else
            {
                SetNotMoving();
            }      
        }
        public int GetPixelX()
        {
            return pixelX;
        }
        public int GetPixelY()
        {
            return pixelY;
        }
        protected bool CanGoInDirection(Map map, Direction proposedDirection)
        {
            int nextGridX = map.GetWrappedXCoordinate(GetGridX() + proposedDirection.X);
            int nextGridY = map.GetWrappedYCoordinate(GetGridY() + proposedDirection.Y);
            if (IsReachableCell(nextGridX, nextGridY,map))
            {
                return true;
            }
            return false;
        }
        public override int GetGridX()
        {
            return (pixelX + (mapCellSize / 2)) / mapCellSize;
        }
        public override int GetGridY()
        {
            return (pixelY + (mapCellSize / 2)) / mapCellSize;
        }

        protected void SetNotMoving()
        {
            isMoving = false;
        }
        protected void SetMoving()
        {
            isMoving = true;
            movementFrame = 0;
        }
        /*
         * Because each movable object needs to end a tweening cycle at a precise grid location, we need the 
         * cell size to be a multiple of the speed. This funciton takes care of readjusting the speed corectly.
         */
        protected void SetTweenSpeed(int newSpeed)
        {
            newSpeed = ReduceToDivisibleWithoutRemainder(newSpeed);
            movementSpeed = newSpeed;
            maxMovementFrame = mapCellSize / movementSpeed;
        }
        private int ReduceToDivisibleWithoutRemainder(int newSpeed)
        {
            while (mapCellSize % newSpeed != 0)
            {
                newSpeed--;
            }
            return newSpeed;
        }

        /*
         * Checks the X and Y pixel coordinates of the other object and if they are less than a cell size 
         * away, we determine that as a collision
         */
        public bool IsTouchingTweeningObject(TweeningObject other)
        {
            return Math.Abs(other.pixelX - pixelX) < mapCellSize && Math.Abs(other.pixelY - pixelY) < mapCellSize;

        }
        private void WraparoundIfOutOfBounds(Map map)
        {
            pixelX = map.GetWrappedPixelXCoordinate(pixelX);
            pixelY = map.GetWrappedPixelYCoordinate(pixelY);
        }
    }
    /* 
     * Represents a blank space in the static grid. I wanted to be explicit and not relying on null. 
     * This can be useful for debugging purposes in the future.
     */
    class StaticLayerBlankSpace : StaticGridObject
    {
        public StaticLayerBlankSpace(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    class InteractiveLayerBlankSpace : InteractiveGridObject
    {
        public InteractiveLayerBlankSpace(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    class Fence : StaticGridObject
    {
        public Fence(Bitmap image, int x, int y) : base(image, x, y) { }

    }
    class Energizer : InteractiveGridObject
    {
        public Energizer(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    class GhostHome : StaticGridObject
    {
        public GhostHome(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : StaticGridObject
    {
        public Wall(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    /*
     * Main playable character of the game. So far I will make it non playable but will
     * add controls later.
     */
    class Hero : TweeningObject
    {
        private Direction nextDirection;

        public Hero(Bitmap image, int x, int y, int speed, int mapCellSize) : base(image, x, y, speed, mapCellSize)
        {
            direction = Direction.Right;
        }
        public bool TryEatPellet(Map map)
        {
            if (map.ContainsPellet(GetGridX(), GetGridY())) 
            {
                map.RemovePellet(GetGridX(), GetGridY());
                return true;
            }
            return false;
        }
        public bool TryEatEnergizer(Map map)
        {
            if (map.ContainsEnergizer(GetGridX(), GetGridY())) 
            {
                map.RemoveEnergizer(GetGridX(), GetGridY());
                return true;
            }
            return false;
        }
        public void SetNextDirection(Direction newDirection)
        {   
            nextDirection = newDirection;
        }
        protected override void StartNextMovementCycle(Map map)
        {
            if (CanGoInDirection(map, nextDirection))
            {
                direction = nextDirection;
            }
            if (CanGoInDirection(map,direction))
            {
                SetMoving();
            }
        }
        public bool IsTouchingAnyGhost(List<Ghost> ghosts)
        {
            foreach (Ghost ghost in ghosts)
            {
                if (IsTouchingTweeningObject(ghost))
                {
                    return true;
                }
            }
            return false;
        }

        public Ghost GetOneNearbyGhost(List<Ghost> ghosts)
        {
            foreach (Ghost ghost in ghosts)
            {
                if (IsTouchingTweeningObject(ghost))
                {
                    return ghost;
                }
            }
            return null;
        }
        protected override bool IsReachableCell(int x, int y, Map map)
        {
            if (map.IsBlankCell(x,y))
            {
                return true;
            }
            return false;
        }
        public Direction GetDirection()
        {
            return direction;
        }
    }
    /*
     * Things that player eats and gets points for that
     */
    class Pellet : InteractiveGridObject
    {
        public Pellet(Bitmap image, int x, int y) : base(image, x, y) { }
    }
    /* 
     * Enemies that will be chasing the player
     */
    enum GhostMode { Preparing ,Chase, Scatter, Frightened };
    abstract class Ghost : TweeningObject
    {
        protected GhostMode currentMode;
        protected Bitmap frighenedModeSprite;

        protected Point startingLocation;
        protected Point target;
        protected Point lastOccupiedCell;

        protected TimeSpan prepareDuration;
        protected DateTime ghostHouseEnterTime;
        public Ghost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize)
        {
            SetModeIfValid(GhostMode.Preparing);
            direction = Direction.Up;
            startingLocation = new Point(x, y);
            target = new Point(mapCellSize - 1, mapCellSize - 1);
            prepareDuration = TimeSpan.FromSeconds(prepareTimeInSeconds);
            ghostHouseEnterTime = DateTime.Now;
            lastOccupiedCell = new Point(GetGridX(),GetGridY());
            frighenedModeSprite = frightenedImage;
        }

        private void TryExitGhostHouse()
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime - ghostHouseEnterTime > prepareDuration)
            {
                currentMode = GhostMode.Chase; // here we cannot use SetModeIfValid because we have to override the preparing mode
            }
        }

        /*
         * Used when ghost is frightened. On each intersection it randomly places its target somewhere on the map
         */
        private void SetTargetToRandom(Map map)
        {
            Random rand = new Random();
            int randomGridX = rand.Next(map.GetGridWidth());
            int randomGridY = rand.Next(map.GetGridHeight());
            target = new Point(randomGridX, randomGridY);
        }

        // the two following methods are implemented in a unique way by each ghost
        protected abstract void SetTargetToScatterTarget(Map map);
        protected abstract void SetTargetToChaseTarget(Map map);
        
        public void BeEaten()
        {
            SetModeIfValid(GhostMode.Preparing);
            ghostHouseEnterTime = DateTime.Now;
            pixelX = startingLocation.X * mapCellSize;
            pixelY = startingLocation.Y * mapCellSize;
            direction = Direction.Up;
            SetNotMoving();
        }
        /*
         * Setting a new mode to preparing overrides all possible mode the ghost had previously
         * On the other hand, when the ghost is preparing its mode cannot be overriden
         */
        public void SetModeIfValid(GhostMode newMode)
        {
            if (newMode == GhostMode.Preparing || currentMode != GhostMode.Preparing)
            {
                currentMode = newMode;
                Console.WriteLine($"Mode of {this} set to {newMode}");
            }
            
        }

        private void SetTargetBasedOnMode(Map map)
        {
            switch(currentMode) 
            {
                case GhostMode.Scatter:
                    SetTargetToScatterTarget(map);
                    break;
                case GhostMode.Chase:
                    SetTargetToChaseTarget(map);
                    break;
                case GhostMode.Frightened:
                    SetTargetToRandom(map);
                    break;
            }
        }

        /*
         * The target is ony taken into account if the ghost is at an intersection
         */
        protected override void StartNextMovementCycle(Map map)
        {
            if (currentMode == GhostMode.Preparing)
            {
                TryExitGhostHouse();
            }
            if (IAmAtIntersection(map))
            {
                SetTargetBasedOnMode(map);
                StaticLayerBlankSpace targetNeighbour = FindNeighbourClosestToTarget(map);
                SetDirectionTowardsNeighbor(targetNeighbour);
            }
            else if (!CanGoInDirection(map,direction))
            {
                if (IAmHome(map))
                {
                    TurnAround();
                }
                else
                {
                    TryTurnOnCurve(map);
                } 
            }
            SetCurrentLocationLastOccupied();
            SetMoving();
            
        }

        //HACK: even though the ghosts could theoretically enter ghost home again when they
        // are in chase/scatter/frightened mode this doesn't happen because the ghost home
        // is surrounded by fence tiles which are not taken into account when the ghost is 
        // deciding where to go at an intersection
        protected override bool IsReachableCell(int x, int y, Map map)
        {
            switch (currentMode)
            {
                case GhostMode.Preparing:
                    return (map.IsBlankCell(x, y) || map.IsGhostHome(x, y));
                case GhostMode.Chase:
                case GhostMode.Frightened:
                case GhostMode.Scatter:
                    return (map.IsBlankCell(x, y) || map.IsGhostHome(x, y) || map.IsFence(x,y));
                   
            }
            return false;
                

        }
        private bool WasLastOccupied(StaticLayerBlankSpace neighbour)
        {
            if (neighbour.GetGridX() == lastOccupiedCell.X && neighbour.GetGridY() == lastOccupiedCell.Y)
            {
                return true;
            }
            return false;

        }
        private void TryTurnOnCurve(Map map)
        {
            if (map.GetNeighboringCellsCount(GetGridX(), GetGridY()) == 2)
            {
                foreach (StaticLayerBlankSpace neighbour in map.GetNeighboringBlankCells(GetGridX(), GetGridY()))
                {
                    if (!WasLastOccupied(neighbour))
                    {
                        SetDirectionTowardsNeighbor(neighbour);
                    }
                }
            }
        }
        private void SetCurrentLocationLastOccupied()
        {
            lastOccupiedCell.X = GetGridX();
            lastOccupiedCell.Y = GetGridY();
        }
        private StaticLayerBlankSpace FindNeighbourClosestToTarget(Map map)
        {
            double closestDistance = Double.MaxValue;
            StaticLayerBlankSpace closestNeighbour = null;
            foreach (StaticLayerBlankSpace neighbour in map.GetNeighboringBlankCells(GetGridX(),GetGridY()))
            {
                if (!WasLastOccupied(neighbour))
                {
                    double distanceToTarget = neighbour.GetDistanceToCell(target.X,target.Y);
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        closestNeighbour = neighbour;
                    }
                }
            }
            return closestNeighbour;
        }
        private bool IAmHome(Map map)
        {
            if(map.IsGhostHome(GetGridX(), GetGridY()))
            {
                return true;
            }
            return false;
        }
        private bool IAmAtIntersection(Map map)
        {
            if (map.IsAnIntersection(GetGridX(), GetGridY()))
            {
                return true;
            }
            return false;
        }
        protected void SetTargetAheadOfHero(Map map, int tilesAhead)
        {
            Direction heroDirection = map.GetHero().GetDirection();
            int xAhead = map.GetWrappedXCoordinate((tilesAhead * heroDirection.X) + map.GetHeroGridLocation().X);
            int yAhead = map.GetWrappedYCoordinate((tilesAhead * heroDirection.Y) + map.GetHeroGridLocation().Y);
            target = new Point(xAhead, yAhead);
        }

        public override Bitmap GetImageToDraw()
        {
            if (currentMode == GhostMode.Frightened) return frighenedModeSprite;
            return sprite;
        }

        public GhostMode GetCurrentMode()
        {
            return currentMode;
        }

    }

    class RedGhost : Ghost
    {
        public RedGhost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage,x, y,speed,mapCellSize,prepareTimeInSeconds)
        { 
        }
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetAheadOfHero(map,0);
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(map.GetGridWidth(), 0);
        }

    }

    class PinkGhost : Ghost
    {
        public PinkGhost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage, x, y, speed, mapCellSize, prepareTimeInSeconds)
        {
        }
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetAheadOfHero(map, 4);
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(0, 0);
        }

    }

    class BlueGhost : Ghost
    {
        public BlueGhost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage, x, y, speed, mapCellSize, prepareTimeInSeconds)
        {
        }
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetAheadOfHero(map, 2);
            MoveTargetAwayFromRedGhost(map);

        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(map.GetGridWidth(), map.GetGridHeight());
        }
        private void MoveTargetAwayFromRedGhost(Map map)
        {
            int vectorXToAdd = (target.X - map.GetRedGhostGridLocation().X);
            int vectorYToAdd = (target.Y - map.GetRedGhostGridLocation().Y);
            target = new Point(map.GetWrappedXCoordinate(vectorXToAdd+target.X),map.GetWrappedYCoordinate(vectorYToAdd+target.Y));
        }
    }

    class OrangeGhost : Ghost
    {
        public OrangeGhost(Bitmap image,Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage, x, y, speed, mapCellSize, prepareTimeInSeconds)
        {
        }
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetBasedOnHeroDistance(map, 8);
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(0, map.GetGridHeight());
        }

        private void SetTargetBasedOnHeroDistance(Map map, double distanceLimit)
        {
            double distanceToHero = GetDistanceToCell(map.GetHeroGridLocation().X, map.GetHeroGridLocation().Y);
            if (distanceToHero > distanceLimit) 
            {
                SetTargetAheadOfHero(map, 0);
            }
            else
            {
                SetTargetToScatterTarget(map);
            }
        }
    }
}