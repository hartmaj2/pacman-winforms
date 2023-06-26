namespace Pacman
{
    /*
    * Everything that lives inside the game should inherit this. It has to be something that can 
    * occupy a grid and that can be drawn
    */
    abstract class GameObject
    {

    }
    /* 
     * Special type of GameObject that can also move around
     */
    abstract class StaticObject
    {

    }
    abstract class DynamicObject
    {

    }
    abstract class MovableObject : DynamicObject
    {       

        protected Direction direction;

        public abstract int GetGridX();

        public abstract int GetGridY();

    }
    abstract class DiscreteMovableObject : MovableObject
    {
        protected int gridX;
        protected int gridY;

        public DiscreteMovableObject(int x, int y)
        {
            gridX = x;
            gridY = y;
        }
        public void DiscreteMove(Map map)
        {
            int newX = gridX + direction.X;
            int newY = gridY + direction.Y;

            if (map.IsFreeCoordinate(newX, newY))
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
    abstract class TweeningMovableObject : MovableObject
    {
        protected int pixelX;
        protected int pixelY;

        protected int maxTweenFrame;
        protected int tweenFrame;
        protected int tweenSpeed;

        protected bool isTweening;

        public TweeningMovableObject(int gridX, int gridY)
        {
            isTweening = false;
            pixelX = gridX * InputManager.GetCellSize();
            pixelY = gridY * InputManager.GetCellSize();
            tweenFrame = 0;
            maxTweenFrame = 20;
            tweenSpeed = 5;
        }
        public void StartTweening(Map map)
        {
            if (!isTweening && CanContinueTweening(map))
            {
                isTweening = true;
                tweenFrame = 0;
            }

        }
        public void Tween()
        {
            if (isTweening)
            {
                if (tweenFrame < maxTweenFrame)
                {
                    tweenFrame++;
                    pixelX += direction.X * tweenSpeed;
                    pixelY += direction.Y * tweenSpeed;
                }
                else
                {
                    tweenFrame = 0;
                    isTweening = false;
                }
                
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

        private bool CanContinueTweening(Map map)
        {
            int nextGridX = GetGridX() + direction.X;
            int nextGridY = GetGridY() + direction.Y;
            if (map.IsFreeCoordinate(nextGridX, nextGridY))
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
    }
    /* 
     * Represents a blank space in the static grid. I wanted to be explicit and not relying on null. 
     * This can be useful for debugging purposes in the future.
     */
    class StaticBlank : StaticObject
    {

    }
    class DynamicBlank : DynamicObject
    {

    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : StaticObject
    {

    }
    /*
     * Main playable character of the game. So far I will make it non playable but will
     * add controls later.
     */
    class Hero : TweeningMovableObject
    {
        public Hero(int x, int y) : base(x, y)
        {
            direction.X = 1;
            direction.Y = 0;
        }

        public void SetDirection(Direction direction)
        {
            if (!isTweening)
            {
                this.direction = direction;
            }
        }


    }
    /*
     * Things that player eats and gets points for that
     */
    class Pellet : DynamicObject
    {

    }
    /* 
     * Enemies that will be chasing the player
     */
    class Ghost : DiscreteMovableObject
    {
        public Ghost(int x, int y) : base(x, y)
        {
            direction.X = 0;
            direction.Y = 1;
        }

    }
}