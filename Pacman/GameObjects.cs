using System.ComponentModel;
using System.Diagnostics;
using System.Security.Policy;

namespace Pacman
{
    /*
    * Everything that lives inside the game should inherit this. It has to be something that can 
    * be located somewhere in the game world. It doesn't have to be moving. It can be walls, pellets, anything.
    * It also has to be something that can be drawn by the painter to the game bitmap.
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
            this.sprite = image;
            this.gridX = x;
            this.gridY = y;
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
     * This is an object that can move but only in discrete steps ending somewhere on the game grid
     */
    abstract class DiscreteMovingObject : MovingObject
    {
        protected int gridX;
        protected int gridY;

        public DiscreteMovingObject(Bitmap image, int x, int y)
        {
            this.sprite = image;
            gridX = x;
            gridY = y;
        }
        public override void Move(Map map)
        {
            int newX = gridX + direction.X;
            int newY = gridY + direction.Y;

            if (map.IsBlankCell(newX, newY))
            {
                gridX += direction.X;
                gridY += direction.Y;
            }
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
     * These objects can move independently of the game grid but are able to tell you what would be their 
     * corresponding location on the game grid
     */
    abstract class TweeningObject : MovingObject
    {
        protected int pixelX;
        protected int pixelY;

        protected int maxMovementFrame;
        protected int movementFrame;
        protected int movementSpeed;

        protected int mapCellSize;

        protected bool isMoving;

        public TweeningObject(Bitmap image, int gridX, int gridY, int speed, int cellSize)
        {
            sprite = image;
            mapCellSize = cellSize;
            pixelX = gridX * mapCellSize;
            pixelY = gridY * mapCellSize;
            SetTweenSpeed(speed); // automatically adjust movements speed to be a multiple of cell size
            SetNotMoving();
        }
        public override void Move(Map map)
        { 
            if (isMoving)
            {
                ContinueMovement();
            }
            if (!isMoving)
            {
                TryStartNextMovement(map);
                ContinueMovement();
            }
            WraparoundIfOutOfBounds(map);
        }
        protected virtual void TryStartNextMovement(Map map)
        {
            if (CanGoInDirection(map, direction))
            {
                SetMoving();
            }
            else
            {
                ClearDirection();
            }
        }
        private void ContinueMovement()
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
        protected void ClearDirection()
        {
            direction = Direction.None;
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
        public bool IsTouchingTweeningObject(TweeningObject other)
        {
            if (Math.Abs(other.pixelX - pixelX) < mapCellSize && Math.Abs(other.pixelY - pixelY) < mapCellSize)
            {
                return true;
            }
            return false;
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
        private int pelletsEaten = 0;
        private Direction nextDirection;

        public Hero(Bitmap image, int x, int y, int speed, int mapCellSize) : base(image, x, y, speed, mapCellSize)
        {
            direction = Direction.Right;
        }
        private void TryEatPellet(Map map)
        {
            if (map.ContainsPellet(GetGridX(), GetGridY())) 
            {
                map.RemovePellet(GetGridX(), GetGridY());
                pelletsEaten++;
            }
        }
        public int GetPelletsEaten()
        {
            return pelletsEaten;
        }
        public void SetNextDirection(Direction newDirection)
        {   
            nextDirection = newDirection;
        }
        protected override void TryStartNextMovement(Map map)
        {
            TryEatPellet(map);
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

        protected Point target;
        protected Point lastOccupiedCell;

        protected TimeSpan prepareDuration;
        protected DateTime ghostHouseEnterTime;
        public Ghost(Bitmap image, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize)
        {
            currentMode = GhostMode.Preparing;
            direction = Direction.Up;
            target = new Point(3, 3);
            prepareDuration = TimeSpan.FromSeconds(prepareTimeInSeconds);
            ghostHouseEnterTime = DateTime.Now;
            lastOccupiedCell = new Point(GetGridX(),GetGridY());
        }

        private void TryExitGhostHouse()
        {
            DateTime currentTime = DateTime.Now;
            if (currentTime - ghostHouseEnterTime > prepareDuration)
            {
                currentMode = GhostMode.Chase;
            }
        }
        private void SetTargetToRandom(Map map)
        {
            Random rand = new Random();
            int randomGridX = rand.Next(map.GetGridWidth());
            int randomGridY = rand.Next(map.GetGridHeight());
            target = new Point(randomGridX, randomGridY);
        }
        protected abstract void SetTargetToScatterTarget(Map map);
        protected abstract void SetTargetToChaseTarget(Map map);
        
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
        protected override void TryStartNextMovement(Map map)
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

    }

    class RedGhost : Ghost
    {
        public RedGhost(Bitmap image, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image,x,y,speed,mapCellSize,prepareTimeInSeconds)
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
        public PinkGhost(Bitmap image, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize, prepareTimeInSeconds)
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
        public BlueGhost(Bitmap image, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize, prepareTimeInSeconds)
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
        public OrangeGhost(Bitmap image, int x, int y, int speed, int mapCellSize, int prepareTimeInSeconds) : base(image, x, y, speed, mapCellSize, prepareTimeInSeconds)
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