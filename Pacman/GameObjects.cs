using System.ComponentModel;
using System.Security.Policy;

namespace Pacman
{
    /*
    * Everything that lives inside the game should inherit this. It has to be something that can 
    * be located somewhere in the game world. It doesn't have to be moving. It can be walls, pellets, anything.
    */
    abstract class GameObject
    {
        public abstract int GetGridX();
        public abstract int GetGridY();
        public double GetDistanceToCell(int x, int y)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x - GetGridX()), 2) + Math.Pow(Math.Abs(y - GetGridY()), 2));
        }
    } 
    abstract class GridObject : GameObject
    {
        private int gridX;
        private int gridY;
        public GridObject(int x, int y)
        {
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
        public StaticGridObject(int x, int y) : base(x, y) { }
    }
    /*
     * These objects are not moving but can be eaten or otherwise interacted with by some kind of a player. 
     * They live on a different layer on the map
     */
    abstract class InteractiveGridObject : GridObject
    {
        public InteractiveGridObject(int x, int y) : base(x, y) { }
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

        public DiscreteMovingObject(int x, int y)
        {
            gridX = x;
            gridY = y;
        }
        public override void Move(Map map)
        {
            int newX = gridX + direction.X;
            int newY = gridY + direction.Y;

            if (map.IsFreeGridCell(newX, newY))
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
    abstract class TweeningObjects : MovingObject
    {
        protected int pixelX;
        protected int pixelY;

        protected int maxMovementFrame;
        protected int movementFrame;
        protected int movementSpeed;

        protected bool isMoving;

        public TweeningObjects(int gridX, int gridY, int speed)
        {
            pixelX = gridX * InputManager.GetCellSize();
            pixelY = gridY * InputManager.GetCellSize();
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
            Wraparound(map);
        }
        protected virtual void TryStartNextMovement(Map map)
        {
            if (CanGoInDirection(map, direction))
            {
                SetMoving();
            }
            else
            {
                ResetDirection();
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
            return (pixelX + (InputManager.GetCellSize() / 2)) / InputManager.GetCellSize();
        }
        public override int GetGridY()
        {
            return (pixelY + (InputManager.GetCellSize() / 2)) / InputManager.GetCellSize();
        }
        protected void ResetDirection()
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
        protected void SetTweenSpeed(int speed)
        {
            while (InputManager.GetCellSize() % speed != 0)
            {
                speed--;
            }
            movementSpeed = speed;
            maxMovementFrame = InputManager.GetCellSize() / movementSpeed;
        }
        public bool IsTouchingTweeningObject(TweeningObjects other)
        {
            if (Math.Abs(other.pixelX - pixelX) < InputManager.GetCellSize() && Math.Abs(other.pixelY - pixelY) < InputManager.GetCellSize())
            {
                return true;
            }
            return false;
        }
        private void Wraparound(Map map)
        {
            if (pixelX < 0)
            {
                pixelX = map.GetPixelWidth() - map.GetCellSize() + pixelX;
            }
            if (pixelX > map.GetPixelWidth() - map.GetCellSize())
            {
                pixelX = pixelX - (map.GetPixelWidth() - map.GetCellSize());
            }
            if (pixelY < 0)
            { 
                pixelY = map.GetPixelHeight() - map.GetCellSize() + pixelY;
            }
            if (pixelY > map.GetPixelHeight() - map.GetCellSize())
            {
                pixelY = pixelY - (map.GetPixelHeight() - map.GetCellSize());
            }
        }
    }
    /* 
     * Represents a blank space in the static grid. I wanted to be explicit and not relying on null. 
     * This can be useful for debugging purposes in the future.
     */
    class StaticLayerBlankSpace : StaticGridObject
    {
        public StaticLayerBlankSpace(int x, int y) : base(x, y) { }
    }
    class InteractiveLayerBlankSpace : InteractiveGridObject
    {
        public InteractiveLayerBlankSpace(int x, int y) : base(x, y) { }
    }
    class Fence : StaticGridObject
    {
        public Fence(int x, int y) : base(x, y) { }

        private bool open = false;
        public void Open()
        {
            open = true;
        }
        public void Close()
        {
            open = false;
        }
        public bool IsClosed()
        {
            return !open;
        }
    }
    class GhostHome : StaticGridObject
    {
        public GhostHome(int x, int y) : base(x, y) { }
    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : StaticGridObject
    {
        public Wall(int x, int y) : base(x, y) { }
    }
    /*
     * Main playable character of the game. So far I will make it non playable but will
     * add controls later.
     */
    class Hero : TweeningObjects
    {
        private int pelletsEaten = 0;
        private Direction nextDirection;

        public Hero(int x, int y, int speed) : base(x, y, speed)
        {
            direction = Direction.Right;
        }
        private void TryEatPellet(Map map)
        {
            if (map.ContainsPellet(GetGridX(), GetGridY())) 
            {
                map.RemoveFromInteractiveGrid(GetGridX(), GetGridY());
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
            else
            {
                ResetDirection();
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
            if (map.IsFreeGridCell(x,y))
            {
                return true;
            }
            return false;
        }
    }
    /*
     * Things that player eats and gets points for that
     */
    class Pellet : InteractiveGridObject
    {
        public Pellet(int x, int y) : base(x, y) { }
    }
    /* 
     * Enemies that will be chasing the player
     */
    class Ghost : TweeningObjects
    {
        private Point target;
        private Point lastOccupiedCell;
        public Ghost(int x, int y, int speed) : base(x, y, speed)
        {
            direction = Direction.Up;
            target = new Point(3, 3);
            lastOccupiedCell = new Point(GetGridX(),GetGridY());
        }
        protected override void TryStartNextMovement(Map map)
        {
            if (IAmAtIntersection(map))
            {
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
            if (map.IsFreeGridCell(x,y) || map.IsGhostHome(x,y) || map.IsOpenFence(x,y))
            {
                return true;
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
    }
}