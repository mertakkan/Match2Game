using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int position;
    public CubeController cube;
    public RocketController rocket;

    public DuckController duck;

    public BalloonController balloon;

    public bool isEmpty => cube == null && rocket == null && duck == null && balloon == null;
    public bool hasCube => cube != null;
    public bool hasRocket => rocket != null;
    public bool hasDuck => duck != null;
    public bool hasBalloon => balloon != null;

    public GridCell(int x, int y)
    {
        position = new Vector2Int(x, y);
        cube = null;
        rocket = null;

        duck = null;

        balloon = null;
    }

    public void SetCube(CubeController newCube)
    {
        cube = newCube;
        rocket = null;
        duck = null;
        if (cube != null)
        {
            cube.SetGridPosition(position);
        }
    }

    public void SetRocket(RocketController newRocket)
    {
        rocket = newRocket;
        cube = null;
        duck = null;
        if (rocket != null)
        {
            rocket.SetGridPosition(position);
        }
    }

    public void SetBalloon(BalloonController newBalloon)
    {
        Clear();
        balloon = newBalloon;
        if (balloon != null)
        {
            balloon.SetGridPosition(position);
        }
    }

    public void SetDuck(DuckController newDuck)
    {
        duck = newDuck;
        cube = null;
        rocket = null;
        if (duck != null)
        {
            duck.SetGridPosition(position);
        }
    }

    public void ClearCube()
    {
        if (cube != null)
        {
            cube.SetGridPosition(new Vector2Int(-1, -1));
        }
        cube = null;
    }

    public void ClearRocket()
    {
        rocket = null;
    }

    public void ClearBalloon()
    {
        balloon = null;
    }

    public void ClearDuck()
    {
        duck = null;
    }

    public void Clear()
    {
        ClearCube();
        ClearRocket();

        ClearDuck();
        ClearBalloon();
    }
}
