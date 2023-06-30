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

        /*
         * Used by all ghosts pathfinding AI and also by orange ghost's targetting scheme
         */
        public double GetDistanceToCell(int x, int y)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x - GetGridX()), 2) + Math.Pow(Math.Abs(y - GetGridY()), 2));
        }

        /*
         * Used by painter to draw this GameObject's corresponding Bitmap/sprite
         */
        public virtual Bitmap GetImageToDraw()
        {
            return sprite;
        }

        /*
         * This gets called by the Painter class to determine whether the given object should be rendered at all
         */
        public virtual bool IsDrawable()
        {
            return sprite != null;
        }
    } 

    /*
     * An object that occupies an exact place on the grid and cannot move in between cells (walls, pellets, energizers, fences etc)
     */
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
     * This is a game object that occupies a certain place on the grid but the player cannot interact with (except by collding with it)
     * these are (walls, fences or blank spaces)
     */
    abstract class StaticGridObject : GridObject
    {
        public StaticGridObject(Bitmap image, int x, int y) : base(image, x, y) { }
    }

    /*
     * These objects are not moving but can be eaten or otherwise interacted with by the moving characters (player or ghosts) 
     * They live on a different layer on the map and get painted after all static grid objects are painted
     */
    abstract class InteractiveGridObject : GridObject
    {
        public InteractiveGridObject(Bitmap image, int x, int y) : base(image, x, y) { }
    }

    /* 
     * This is anything in the game that can move its position. It could in theory be something that moves
     * a whole cell at a time but I din't implement that behavior as both ghosts and pacman move in smaller steps.
     */
    abstract class MovingObject : GameObject
    {       

        protected Direction direction;

        public abstract void Move(Map map);
        protected abstract bool IsReachableCell(int x, int y, Map map);

        public void TurnAround()
        {
            direction.RotateRight();
            direction.RotateRight();
        }

        /*
         * Sets the direction towards a neighboring cell. 
         * DOESN'T WORK IF USED ON A NON NEIGHBORING TILE! (except when the tile gets wrapped because it is out of bounds)
         */
        protected void SetDirectionTowardsExit(StaticGridObject neighbor)
        {
            Direction newDirection = Direction.None;
            newDirection.X = GetFixedOutOfBoundsCoordinate(neighbor.GetGridX() - GetGridX());
            newDirection.Y = GetFixedOutOfBoundsCoordinate(neighbor.GetGridY() - GetGridY());

            direction = newDirection;

        }

        /*
         * Fixes a bug when the adjacent tile is on the other side of the map, that would result in 
         * a ghost direction coordinate value greater than 1 or smaller than -1 when calling SetDirectionTowardsExit
         */
        private int GetFixedOutOfBoundsCoordinate(int coordinate)
        {
            if (coordinate < -1)
            {
                coordinate = 1;
            }
            else if (coordinate > 1)
            {
                coordinate = -1;
            }
            return coordinate;
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

        // keeps track of what was the last gridX and gridY location (is updated only at start of next movement cycle or when the ghost went out of bounds and reappeared on the other side)
        protected Point lastOccupiedCell; 

        protected int mapCellSize;

        protected bool isMoving; // tracks if we are currenly inside a movement cycle or just finished moving

        public TweeningObject(Bitmap image, int gridX, int gridY, int speed, int cellSize)
        {
            sprite = image;
            mapCellSize = cellSize;
            pixelX = gridX * mapCellSize;
            pixelY = gridY * mapCellSize;
            lastOccupiedCell = new Point(gridX, gridY);
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
         * Checks if the location of a cell is reachable for this object in the proposed direction (after wrapping the coordinates)
         */
        protected bool CanGoInDirection(Map map, Direction proposedDirection)
        {
            int nextGridX = map.GetWrappedXCoordinate(GetGridX() + proposedDirection.X);
            int nextGridY = map.GetWrappedYCoordinate(GetGridY() + proposedDirection.Y);
            return IsReachableCell(nextGridX, nextGridY, map);
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

        /*
         * Used to correct this characters speed so it always ends its movement cycle exactly in the middle of a cell
         */
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

        /*
         * Changes this objects location if it would be out of bounds
         */
        private void WraparoundIfOutOfBounds(Map map)
        {
            if (map.IsOutOfBoundsPixelX(pixelX))
            {
                pixelX = map.GetWrappedPixelXCoordinate(pixelX);
                UpdateLastOccupied();
            }
            if (map.IsOutOfBoundsPixelY(pixelY)) 
            {
                pixelY = map.GetWrappedPixelYCoordinate(pixelY);
                UpdateLastOccupied();
            }
        }

        protected void UpdateLastOccupied()
        {
            lastOccupiedCell.X = GetGridX();
            lastOccupiedCell.Y = GetGridY();
        }

        public override int GetGridX()
        {
            return (pixelX + (mapCellSize / 2)) / mapCellSize;
        }
        public override int GetGridY()
        {
            return (pixelY + (mapCellSize / 2)) / mapCellSize;
        }
        public int GetPixelX()
        {
            return pixelX;
        }
        public int GetPixelY()
        {
            return pixelY;
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

    /*
     * A fence to prevent ghosts from leaving the ghost house too soon (and prevent player and ghosts to enter the ghost house later)
     */
    class Fence : StaticGridObject
    {
        public Fence(Bitmap image, int x, int y) : base(image, x, y) { }

    }

    /*
     * Energizer that allows player to eat ghosts for a limited time interval
     */
    class Energizer : InteractiveGridObject
    {
        public Energizer(Bitmap image, int x, int y) : base(image, x, y) { }
    }

    /* 
     * A wall that all moving objects (player and ghosts) collide with.
     */
    class Wall : StaticGridObject
    {
        public Wall(Bitmap image, int x, int y) : base(image, x, y) { }
    }

    /*
     * The main character that is controlled by the player.
     */
    class Hero : TweeningObject
    {
        private Direction nextDirection; // allows player to preset direction of pacman before it is possible to go that way

        public Hero(Bitmap image, int x, int y, int speed, int mapCellSize) : base(image, x, y, speed, mapCellSize) {}
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

        /*
         * If pacman can go in the next direction proposed by the player, the current direction is 
         * updated to this new direction. Then pacman starts moving in the direction it 
         * actually has if he can
         */
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

        /*
         * Returns the first nearby Ghost object encountered when searching for nearby ghosts
         * If there are no ghosts nearby returns null
         */
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

        /*
         * Sets which cells should appear to pacman as reachable
         */
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
     * Enemies that kill the player when they collide with him all have modes
     * These modes get changed mostly with respect to time except for frightened mode which is set when hero eats and energizer
     */
    enum GhostMode { Preparing ,Chase, Scatter, Frightened };

    /*
     * All ghosts in the game inherit from this general Ghost class. They try to eat the player except when frightened
     * Each ghost must have a target which it is trying to reach at all times but how this target is set depends on each ghost
     * Every ghost also spends unique amount of time in the ghost house before leaving it 
     */
    abstract class Ghost : TweeningObject
    {

        protected readonly Bitmap frighenedModeSprite;
        protected readonly Point startingLocation; // used to return ghost back home after being eaten by pacman
        protected readonly TimeSpan prepareDuration;

        protected Point target; // the location the ghost is trying to reach currently
        
        protected DateTime ghostHouseEnterTime;
        protected GhostMode currentMode;
        protected bool leftHouse; // track if the ghost has left the house already (we check if he crossed a fence)

        public Ghost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize)
        {
            frighenedModeSprite = frightenedImage;
            startingLocation = new Point(x, y);
            prepareDuration = TimeSpan.FromSeconds(prepareTimeInSeconds);

            target = new Point(mapCellSize - 1, mapCellSize - 1);

            ghostHouseEnterTime = DateTime.Now;
            SetModeIfValid(GhostMode.Preparing);
            leftHouse = false;
            
            direction = Direction.Up;
        }

        /*
         * Releases the ghost from preparing mode thus allowing him to leave the ghost house
         * (that's because changing the mode changes what the IsReachable method returns)
         */
        private void TryStopPreparing()
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
            leftHouse = false;
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
            }
            
        }

        /*
         * Calls mostly ghost specific target setting methods except when this ghost is frightened (frightened behavior is same for all ghosts)
         */
        private void UpdateTargetBasedOnMode(Map map)
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
         * The core of a general ghost decision making, the ghost specific behavior lies in UpdateTargetBasedOnMode method
         */
        protected override void StartNextMovementCycle(Map map)
        {
            if (currentMode == GhostMode.Preparing)
            {
                TryStopPreparing(); // checks time to see if we should change this ghost's mode
            }
            if (!leftHouse)
            {
                CheckIfLeftHouse(map); // sets leftHouse to true if the ghost is standing at a fence
                if (!CanGoInDirection(map,direction))
                {
                    TurnAround();
                }
            }
            else // normal movement behaviour when ghost is out of the house and not preparing
            {
                List<StaticLayerBlankSpace> adjacentExits = map.GetAdjacentBlankCells(GetGridX(),GetGridY());
                int numberOfExits = adjacentExits.Count;
                if (numberOfExits > 2) // the ghost is at an intersection
                {
                    UpdateTargetBasedOnMode(map); // if on intersection, we want to let the ghost pick its new target
                    StaticLayerBlankSpace chosenIntersectionExit = FindExitClosestToTarget(map, adjacentExits);
                    SetDirectionTowardsExit(chosenIntersectionExit);
                }
                else if (numberOfExits == 2) // ghost continues straight or needs to turn on a curve
                {
                    ChooseNonReturningExit(map, adjacentExits);
                }
                else // this happends only when the ghost reached a dead end (doesn't happen on the classic pacman map)
                {
                    TurnAround();
                }
            }
            UpdateLastOccupied();
            SetMoving();
            
        }

        /*
         * The ghosts decides which cells are reachable depending on his current mode and the fact that it has left the house
         */
        protected override bool IsReachableCell(int x, int y, Map map)
        {
            // if the ghost is preparing in the house or it is not preparing anymore but it has left the house already then it can't pass through fences
            if (leftHouse || currentMode == GhostMode.Preparing) return map.IsBlankCell(x, y);
            else return (map.IsBlankCell(x,y) || map.IsFence(x,y));
        }

        /*
         * We need to check this in case the ghost is out of the house already because we don't want it
         * to cross the fence and go back home again after leaving once
         */
        private void CheckIfLeftHouse(Map map)
        {
            if (map.IsFence(GetGridX(),GetGridY()))
            {
                leftHouse = true;
            }
        }

        /*
         * Checks whether the ghost has just left this blank tile in the last movement cycle
         */
        private bool WasLastOccupied(StaticLayerBlankSpace adjacentCell)
        {
            if (adjacentCell.GetGridX() == lastOccupiedCell.X && adjacentCell.GetGridY() == lastOccupiedCell.Y)
            {
                return true;
            }
            return false;

        }

        /*
         * If there are exactly two exits the ghost can choose, he picks the one that he didn't came from
         */
        private void ChooseNonReturningExit(Map map, List<StaticLayerBlankSpace> adjacentExits)
        {
            foreach (StaticLayerBlankSpace neighbor in adjacentExits)
            {
                if (!WasLastOccupied(neighbor))
                {
                    SetDirectionTowardsExit(neighbor);
                }
            } 
        }

        /*
         * Picks a neighbouring tile that is closest to this ghost's target
         */
        private StaticLayerBlankSpace FindExitClosestToTarget(Map map, List<StaticLayerBlankSpace> adjacentExits)
        {
            double closestDistance = Double.MaxValue;
            StaticLayerBlankSpace closestToTarget = null;
            foreach (StaticLayerBlankSpace neighbour in adjacentExits)
            {
                if (!WasLastOccupied(neighbour))
                {
                    double distanceToTarget = neighbour.GetDistanceToCell(target.X,target.Y);
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        closestToTarget = neighbour;
                    }
                }
            }
            return closestToTarget;
        }


        /*
         * This can be used to set the target directly on hero if tilesAhead = 0
         */
        protected void SetTargetAheadOfHero(Map map, int tilesAhead)
        {
            Direction heroDirection = map.GetHero().GetDirection();
            int xAhead = map.GetWrappedXCoordinate((tilesAhead * heroDirection.X) + map.GetHeroGridLocation().X);
            int yAhead = map.GetWrappedYCoordinate((tilesAhead * heroDirection.Y) + map.GetHeroGridLocation().Y);
            target = new Point(xAhead, yAhead);
        }

        /*
         * We nedd to draw different image if ghost is frightened
         */
        public override Bitmap GetImageToDraw()
        {
            if (currentMode == GhostMode.Frightened) return frighenedModeSprite;
            else return sprite;
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
            SetTargetAheadOfHero(map,0); // sets target directly on hero in chase mode
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(map.GetGridWidth(), 0); // sets target to upper right corner in scatter mode
        }

    }

    class PinkGhost : Ghost
    {
        public PinkGhost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage, x, y, speed, mapCellSize, prepareTimeInSeconds)
        {
        }
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetAheadOfHero(map, 4); // sets target 4 tiles ahead of hero in chase mode
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(0, 0); // sets target to upper left corner in scatter mode
        }

    }

    class BlueGhost : Ghost
    {
        public BlueGhost(Bitmap image, Bitmap frightenedImage, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, frightenedImage, x, y, speed, mapCellSize, prepareTimeInSeconds)
        {
        }

        /*
         * Blue ghost uses pacmans location as well as the red ghosts location to determine the target position
         */
        protected override void SetTargetToChaseTarget(Map map)
        {
            SetTargetAheadOfHero(map, 2); // first it sets the target 2 tiles ahead of pacman
            MoveTargetAwayFromRedGhost(map); // after that it moves the target by the distance between pacman and red ghost in the direction away from red ghost

        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(map.GetGridWidth(), map.GetGridHeight()); // scatter target location bottom right corner
        }
        private void MoveTargetAwayFromRedGhost(Map map)
        {
            // calculate components of the vector pointing from red ghost to pacman
            int vectorXToAdd = (target.X - map.GetRedGhostGridLocation().X);
            int vectorYToAdd = (target.Y - map.GetRedGhostGridLocation().Y);

            // add this vector to the current target location and wrap the coordinates on over/underflow
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
            // if pacman is less than 8 tiles away, he sets the target to his scatter location, otherwise directly on hero
            SetTargetBasedOnHeroDistance(map, 8); 
        }
        protected override void SetTargetToScatterTarget(Map map)
        {
            target = new Point(0, map.GetGridHeight()); // in scatter mode sets target to lower left corner
        }

        private void SetTargetBasedOnHeroDistance(Map map, double distanceLimit)
        {
            double distanceToHero = GetDistanceToCell(map.GetHeroGridLocation().X, map.GetHeroGridLocation().Y);
            if (distanceToHero > distanceLimit) 
            {
                SetTargetAheadOfHero(map, 0); // set target directly on hero 
            }
            else
            {
                SetTargetToScatterTarget(map);
            }
        }
    }
}