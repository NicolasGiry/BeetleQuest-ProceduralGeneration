using UnityEngine;

public class Utils
{
    // return the opposite direction of the given direction
    public Direction OppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                return Direction.DOWN;
            case Direction.DOWN:
                return Direction.UP;
            case Direction.LEFT:
                return Direction.RIGHT;
            case Direction.RIGHT:
                return Direction.LEFT;
            case Direction.FORWARD:
                return Direction.BACKWARD;
            case Direction.BACKWARD:
                return Direction.FORWARD;
            default:
                return direction;
        }
    }

    public Quaternion TileOrientation(Direction nextDirection, Direction slopeDirection)
    {
        if (nextDirection == Direction.UP)
        {
            switch (slopeDirection)
            {
                case Direction.FORWARD:
                    return Quaternion.identity;
                case Direction.BACKWARD:
                    return Quaternion.Euler(new Vector3(0, 180, 0));
                case Direction.RIGHT:
                    return Quaternion.Euler(new Vector3(0, 90, 0));
                case Direction.LEFT:
                    return Quaternion.Euler(new Vector3(0, -90, 0));
                default:
                    return Quaternion.identity;
            }
        }
        else if (nextDirection == Direction.DOWN)
        {
            switch (slopeDirection)
            {
                case Direction.FORWARD:
                    return Quaternion.Euler(new Vector3(0, 180, 0));

                case Direction.BACKWARD:
                    return Quaternion.identity;
                case Direction.RIGHT:
                    return Quaternion.Euler(new Vector3(0, -90, 0));
                case Direction.LEFT:
                    return Quaternion.Euler(new Vector3(0, 90, 0));
                default:
                    return Quaternion.identity;
            }
        }
        switch (nextDirection)
        {
            case Direction.FORWARD:
                return Quaternion.identity;
            case Direction.BACKWARD:
                return Quaternion.Euler(new Vector3(0, 180, 0));
            case Direction.RIGHT:
                return Quaternion.Euler(new Vector3(0, 90, 0));
            case Direction.LEFT:
                return Quaternion.Euler(new Vector3(0, -90, 0));
            default:
                return Quaternion.identity;
        }
    }

    public float Distance (Vector3 A, Vector3 B) {
        return Mathf.Sqrt((B.x-A.x)*(B.x-A.x) + (B.z-A.z)*(B.z-A.z));
    }
}
