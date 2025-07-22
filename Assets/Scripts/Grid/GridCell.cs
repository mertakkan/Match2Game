using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int position;
    public CubeController cube;
    public bool isEmpty => cube == null;

    public GridCell(int x, int y)
    {
        position = new Vector2Int(x, y);
        cube = null;
    }

    public void SetCube(CubeController newCube)
    {
        cube = newCube;
        if (cube != null)
        {
            cube.SetGridPosition(position);
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
}
