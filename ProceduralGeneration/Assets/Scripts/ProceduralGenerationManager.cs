using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// TODO: - Calculer les positions interdites quand on monte et descend sans impacter la currentTileLocation
//       - Backtracking si on se retouve bloquer 

enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT,
    FORWARD,
    BACKWARD
};

public class ProceduralGenerationManager : MonoBehaviour
{
    [Header("Generation parameters")]
    [SerializeField] int pathLength;
    [SerializeField] int minUpInARow;
    [SerializeField] int maxUpInARow;
    [SerializeField] int minPlatformSize;
    [SerializeField] int minBeforeFirstSlope;
    [SerializeField] float upProba;
    [SerializeField] float downProba;
    [SerializeField] float platformProba;

    [Header("Debug")]
    [SerializeField] List<GameObject> pathInstances = new();
    [SerializeField] Vector3 currentTileLocation;
    [SerializeField] List<Vector2> prohibitedPositions;
    [SerializeField] List<Direction> directionsChosed = new();
    [SerializeField] List<int> numbersOfProhibitedPositions = new();
    bool nextStep;

    [Header("Tiles Prefabs")]
    [SerializeField] GameObject pathTile;
    [SerializeField] GameObject grassTile;
    [SerializeField] GameObject slopePathTile;
    [SerializeField] GameObject slopeGrassTile;
    [SerializeField] GameObject cliffTile;
    [SerializeField] GameObject ladderTile;

    [Header("Directions")]
    [SerializeField] Vector3[] directionsTab;
    [SerializeField] Dictionary<Direction, Vector3> directions = new();


    bool isGoingUp;
    bool isGoingDown;
    bool isPlatform;
    int nbGenerated;
    int totalNbUp;
    int totalNbDown;
    int currentNbUp;
    int currentNbDown;
    int currentNbPlatform;
    List<Vector2> nextPossibleProhibitedPositions = new();

    Direction nextDirection;
    Direction lastDirection = Direction.FORWARD;
    Direction slopeDirection;



    void Start()
    {
        // directions dictionnary initialization
        for (int i = 0; i < directionsTab.Length; i++)
        {
            directions.Add((Direction)i, directionsTab[i]);
        }

        float debut = Time.realtimeSinceStartup;
        //StartGeneration();
        StartCoroutine(StartGenerationCoroutine());
        print("Durée : " + (Time.realtimeSinceStartup - debut));
    }


    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            nextStep = true;
        }
    }

    IEnumerator StartGenerationCoroutine()
    {
        directionsChosed.Add(Direction.FORWARD);
        for (int i = 0; i < pathLength; i++)
        {
            nextDirection = ChooseNextDirection();
            print(nextDirection.ToString());
            //lastDirection = PlaceNextTile();
            PlaceNextTile();
            while (!nextStep)
            {
                yield return null;
            }
            nextStep = false;
        }
    }

    void StartGeneration()
    {
        directionsChosed.Add(Direction.FORWARD);
        for (int i = 0; i < pathLength; i++)
        {
            nextDirection = ChooseNextDirection();
            print(nextDirection.ToString());
            //lastDirection = PlaceNextTile();
            PlaceNextTile();
        }
    }

    // Choose NextDirection according to defined rules
    Direction ChooseNextDirection()
    {
        List<Direction> possibleDirections = new List<Direction>();

        if (isGoingUp)
        {
            if (IsGoingUp())
            {
                return GoingUp();
            }
            else
            {
                isGoingUp = false;
                currentNbUp = 0;
                isPlatform = true;
                currentNbPlatform = 0;
            }
        }
        else if (isGoingDown)
        {
            if (IsGoingDown())
            {
                return GoingDown();
            }
            else
            {
                isGoingDown = false;
                currentNbDown = 0;
                isPlatform = true;
                currentNbPlatform = 0;
            }
        }
        else if (isPlatform)
        {
            currentNbPlatform++;
            if (currentNbPlatform > minPlatformSize)
            {
                isPlatform = ContinuePlatform();
            }
        }
        else
        {
            if (IsGoingUp())
            {
                isGoingUp = true;
                return GoingUp();
            }

            if (IsGoingDown())
            {
                isGoingDown = true;
                return GoingDown();
            }
        }
        if (directionsChosed[directionsChosed.Count - 1] == Direction.UP || directionsChosed[directionsChosed.Count - 1] == Direction.DOWN)
        {
            return slopeDirection;
        }

        possibleDirections.Add(Direction.LEFT);
        possibleDirections.Add(Direction.RIGHT);
        possibleDirections.Add(Direction.FORWARD);
        possibleDirections.Add(Direction.BACKWARD);
        possibleDirections.Remove(OppositeDirection(directionsChosed[directionsChosed.Count - 1]));

        possibleDirections = CanChoseDirections(possibleDirections);

        if (possibleDirections.Count == 0)
        {
            BackTrack();
            return ChooseNextDirection();
        }

        Direction directionChosed = possibleDirections[Random.Range(0, possibleDirections.Count)];
        directionsChosed.Add(directionChosed);

        return directionChosed;
    }

    // Instantiate next tile, add prohibited position of the last tile placed and return the future last direction
    Direction PlaceNextTile()
    {
        Vector3 nextTileLocation = ComputeNextTileLocation();
        GameObject pathTileinstance;
        int numberOfProhibitedPosition = 0;

        prohibitedPositions.Add(new Vector2(nextTileLocation.x, nextTileLocation.z));

        for (int i = 0; i < nextPossibleProhibitedPositions.Count; i++)
        {
            if (!prohibitedPositions.Contains(nextPossibleProhibitedPositions[i]))
            {
                prohibitedPositions.Add(nextPossibleProhibitedPositions[i]);
                numberOfProhibitedPosition++;
            }
        }
        numbersOfProhibitedPositions.Add(numberOfProhibitedPosition);

        nextPossibleProhibitedPositions.Add(new Vector2(nextTileLocation.x + 2, nextTileLocation.z));
        nextPossibleProhibitedPositions.Add(new Vector2(nextTileLocation.x - 2, nextTileLocation.z));
        nextPossibleProhibitedPositions.Add(new Vector2(nextTileLocation.x, nextTileLocation.z + 2));
        nextPossibleProhibitedPositions.Add(new Vector2(nextTileLocation.x, nextTileLocation.z - 2));

        if (nextDirection == Direction.UP)
        {
            pathTileinstance = Instantiate(slopePathTile, nextTileLocation, TileOrientation()) as GameObject;
            nextTileLocation += directions[nextDirection];
        }
        else if (nextDirection == Direction.DOWN)
        {
            pathTileinstance = Instantiate(slopePathTile, nextTileLocation, TileOrientation()) as GameObject;
        }
        else
        {
            pathTileinstance = Instantiate(pathTile, nextTileLocation, TileOrientation()) as GameObject;
        }

        nbGenerated++;
        pathInstances.Add(pathTileinstance);
        currentTileLocation = nextTileLocation;
        return nextDirection;
    }

    // calculate next tile location 
    Vector3 ComputeNextTileLocation()
    {
        if (nextDirection == Direction.UP && currentNbUp == 1 || nextDirection == Direction.DOWN && currentNbDown == 1)
        {
            slopeDirection = directionsChosed[directionsChosed.Count - 1];
            currentTileLocation += directions[slopeDirection];
        }
        else if (nextDirection == Direction.UP || nextDirection == Direction.DOWN)
        {
            currentTileLocation += directions[slopeDirection];
        }

        if (nextDirection == Direction.UP)
        {
            return currentTileLocation;
        }


        return currentTileLocation + directions[nextDirection];
    }

    Vector3 ComputeNextTileLocation(Direction nextDirection)
    {
        if (nextDirection == Direction.UP && currentNbUp == 1 || nextDirection == Direction.DOWN && currentNbDown == 1)
        {
            slopeDirection = directionsChosed[directionsChosed.Count - 1];
            currentTileLocation += directions[slopeDirection];
        }
        else if (nextDirection == Direction.UP || nextDirection == Direction.DOWN)
        {
            currentTileLocation += directions[slopeDirection];
        }

        if (nextDirection == Direction.UP)
        {
            return currentTileLocation;
        }


        return currentTileLocation + directions[nextDirection];
    }

    // return the opposite direction of the given direction
    Direction OppositeDirection(Direction direction)
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

    List<Direction> CanChoseDirections(List<Direction> possibleDirections)
    {
        Vector3 nextPosition;
        Vector2 position2d;
        List<Direction> toRemove = new();
        foreach (Direction direction in possibleDirections)
        {
            nextPosition = ComputeNextTileLocation(direction);
            position2d = new Vector2(nextPosition.x, nextPosition.z);
            if (prohibitedPositions.Contains(position2d))
            {
                toRemove.Add(direction);
            }
        }

        foreach (Direction direction in toRemove)
        {
            possibleDirections.Remove(direction);
        }
        return possibleDirections;
    }

    // chose if it goes up
    bool IsGoingUp()
    {
        //Vector3 nextPosition = ComputeNextTileLocation(Direction.UP);
        //Vector2 position2d = new Vector2(nextPosition.x, nextPosition.z);

        if (directionsChosed[directionsChosed.Count - 1] == Direction.FORWARD || directionsChosed[directionsChosed.Count - 1] == Direction.LEFT)
        {
            if (isGoingUp)
            {
                if (currentNbUp < minUpInARow)
                {
                    return true;
                }
                return (currentNbUp <= maxUpInARow) && Random.Range(0f, 1f) < upProba;// && !nextPossibleProhibitedPositions.Contains(position2d);
            }
            else
            {
                return nbGenerated > minBeforeFirstSlope && Random.Range(0f, 1f) < upProba;// && !nextPossibleProhibitedPositions.Contains(position2d);
            }
        }
        else
        {
            return false;
        }
    }

    // chose if it goes down
    bool IsGoingDown()
    {
        //Vector3 nextPosition = ComputeNextTileLocation(Direction.DOWN);
        //Vector2 position2d = new Vector2(nextPosition.x, nextPosition.z);

        if (directionsChosed[directionsChosed.Count - 1] == Direction.BACKWARD || directionsChosed[directionsChosed.Count - 1] == Direction.RIGHT)
        {
            if (isGoingDown)
            {
                if (currentNbDown < minUpInARow)
                {
                    return true;
                }
                return (currentNbDown <= maxUpInARow && totalNbDown < totalNbUp) && Random.Range(0f, 1f) < downProba;// && !nextPossibleProhibitedPositions.Contains(position2d);
            }
            else
            {
                return totalNbDown < totalNbUp && Random.Range(0f, 1f) < downProba;// && !nextPossibleProhibitedPositions.Contains(position2d);
            }
        }
        else
        {
            return false;
        }
    }

    // chose if it continue the platform after reaching the min platform size
    bool ContinuePlatform()
    {
        return Random.Range(0f, 1f) < platformProba;
    }

    Direction GoingUp()
    {
        totalNbUp++;
        currentNbUp++;
        return Direction.UP;
    }

    Direction GoingDown()
    {
        totalNbDown++;
        currentNbDown++;
        return Direction.DOWN;
    }

    Quaternion TileOrientation()
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

    Vector3 ComputeBacktrackLocation(Direction lastDirection)
    {
        return currentTileLocation - directionsTab[(int)lastDirection];
    }

    void BackTrack()
    {

        print("BACKTRACK");

        // Destroy last path instance
        Destroy(pathInstances[pathInstances.Count - 1]);
        pathInstances.RemoveAt(pathInstances.Count - 1);

        // Remove prohibited positions from last path
        for (int i = 0; i < numbersOfProhibitedPositions[numbersOfProhibitedPositions.Count - 1]; i++)
        {
            prohibitedPositions.RemoveAt(prohibitedPositions.Count - 1);
        }
        numbersOfProhibitedPositions.RemoveAt(numbersOfProhibitedPositions.Count - 1);

        // Return to last direction choosed
        lastDirection = directionsChosed[directionsChosed.Count - 1];
        directionsChosed.RemoveAt(directionsChosed.Count - 1);
        currentTileLocation = ComputeBacktrackLocation(lastDirection);

        //     directionsTab[(int) lastDirection];
        pathLength++;

    }
}
