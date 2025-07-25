using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int position;
    public CubeController cube;
    public RocketController rocket;

    public bool isEmpty => cube == null && rocket == null;
    public bool hasCube => cube != null;
    public bool hasRocket => rocket != null;

    public GridCell(int x, int y)
    {
        position = new Vector2Int(x, y);
        cube = null;
        rocket = null;
    }

    public void SetCube(CubeController newCube)
    {
        cube = newCube;
        rocket = null; // Clear rocket if placing cube
        if (cube != null)
        {
            cube.SetGridPosition(position);
        }
    }

    public void SetRocket(RocketController newRocket)
    {
        rocket = newRocket;
        cube = null; // Clear cube if placing rocket
        if (rocket != null)
        {
            rocket.SetGridPosition(position);
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

    public void Clear()
    {
        ClearCube();
        ClearRocket();
    }
}
