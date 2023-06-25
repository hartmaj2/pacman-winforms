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
    abstract class StaticGameObject
    {

    }
    abstract class DynamicGameObject
    {

    }
    abstract class MovableGameObject : DynamicGameObject
    {
        protected int xPos;
        protected int yPos;
        protected Direction direction;

        public MovableGameObject(int x, int y)
        {
            xPos = x;
            yPos = y;
        }

        public int GetX()
        {
            return xPos;
        }
        public int GetY()
        {
            return yPos;
        }

        public void Move(Map map)
        {
            int newX = xPos + direction.X;
            int newY = yPos + direction.Y;

            if (map.IsFreeCoordinate(newX, newY))
            {
                xPos += direction.X;
                yPos += direction.Y;
            }
        }

    }
    /* 
     * Represents a blank space in the static grid. I wanted to be explicit and not relying on null. 
     * This can be useful for debugging purposes in the future.
     */
    class StaticBlank : StaticGameObject
    {

    }
    class DynamicBlank : DynamicGameObject
    {

    }
    /* 
     * A wall that the player will collide with.
     */
    class Wall : StaticGameObject
    {

    }
    /*
     * Main playable character of the game. So far I will make it non playable but will
     * add controls later.
     */
    class Hero : MovableGameObject
    {
        public Hero(int x, int y) : base(x, y)
        {
            direction.X = 1;
            direction.Y = 0;
        }

        public void SetDirection(Direction direction)
        {
            this.direction = direction;
        }

    }
    /*
     * Things that player eats and gets points for that
     */
    class Pellet : DynamicGameObject
    {

    }
    /* 
     * Enemies that will be chasing the player
     */
    class Ghost : MovableGameObject
    {
        public Ghost(int x, int y) : base(x, y)
        {
            direction.X = 0;
            direction.Y = 1;
        }

    }
}