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
        protected int gridX;
        protected int gridY;
        protected Direction direction;

        public MovableObject(int x, int y)
        {
            gridX = x;
            gridY = y;
        }

        public int GetX()
        {
            return gridX;
        }
        public int GetY()
        {
            return gridY;
        }

        public void Move(Map map)
        {
            int newX = gridX + direction.X;
            int newY = gridY + direction.Y;

            if (map.IsFreeCoordinate(newX, newY))
            {
                gridX += direction.X;
                gridY += direction.Y;
            }
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

        public TweeningMovableObject(int gridX, int gridY) : base(gridX, gridY)
        {
            isTweening = false;
            pixelX = gridX * InputManager.GetCellSize();
            pixelY = gridY * InputManager.GetCellSize();
            tweenFrame = 0;
            maxTweenFrame = 20;
            tweenSpeed = 5;
        }

        public void StartTweening(Direction direction)
        {
            if (!isTweening)
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
    class Ghost : MovableObject
    {
        public Ghost(int x, int y) : base(x, y)
        {
            direction.X = 0;
            direction.Y = 1;
        }

    }
}