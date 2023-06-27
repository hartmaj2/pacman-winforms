using System.ComponentModel;

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
    abstract class GridObject
    {
        private int gridX;
        private int gridY;
        public GridObject(int x, int y)
        {
            this.gridX = x;
            this.gridY = y;
        }
        public int GetGridX()
        { 
            return gridX; 
        }
        public int GetGridY() 
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

        protected int maxTweenFrame;
        protected int tweenFrame;
        protected int tweenSpeed;

        protected bool isTweening;

        public TweeningObjects(int gridX, int gridY, int speed)
        {
            pixelX = gridX * InputManager.GetCellSize();
            pixelY = gridY * InputManager.GetCellSize();
            SetTweenSpeed(speed); // automatically adjust movements speed to be a multiple of cell size
            SetNotTweening();
        }
        public override void Move(Map map)
        { 
            if (isTweening)
            {
                ContinueTween();
            }
            if (!isTweening)
            {
                TryStartTweenCycle(map);
                ContinueTween();
            }
            Wraparound(map);
        }
        protected virtual void TryStartTweenCycle(Map map)
        {
            if (CanStartNextTween(map, direction))
            {
                SetTweening();
            }
            else
            {
                ResetDirection();
            }
        }
        private void ContinueTween()
        {
            if (tweenFrame < maxTweenFrame)
            {
                tweenFrame++;
                pixelX += direction.X * tweenSpeed;
                pixelY += direction.Y * tweenSpeed;
            }
            else
            {
                SetNotTweening();
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
        protected bool CanStartNextTween(Map map, Direction proposedDirection)
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
        protected void SetNotTweening()
        {
            isTweening = false;
        }
        protected void SetTweening()
        {
            isTweening = true;
            tweenFrame = 0;
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
            tweenSpeed = speed;
            maxTweenFrame = InputManager.GetCellSize() / tweenSpeed;
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
        protected override void TryStartTweenCycle(Map map)
        {
            TryEatPellet(map);
            if (CanStartNextTween(map, nextDirection))
            {
                direction = nextDirection;
            }
            if (CanStartNextTween(map,direction))
            {
                SetTweening();
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

        public Ghost(int x, int y, int speed) : base(x, y, speed)
        {
            direction = Direction.Up;
            target = new Point(3, 3);
        }
        protected override void TryStartTweenCycle(Map map)
        {
            if (!CanStartNextTween(map,direction))
            {
                if (map.IsGhostHome(GetGridX(),GetGridY()))
                {
                    TurnAround();
                }
            }
            else
            {
                SetTweening();
            }
        }

        protected override bool IsReachableCell(int x, int y, Map map)
        {
            if (map.IsFreeGridCell(x,y) || map.IsGhostHome(x,y))
            {
                return true;
            }
            return false;
        }
    }
}