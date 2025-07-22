[System.Serializable]
public struct GridPosition
{
    public int x;
    public int y;

    public GridPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static GridPosition operator +(GridPosition a, GridPosition b)
    {
        return new GridPosition(a.x + b.x, a.y + b.y);
    }

    public static bool operator ==(GridPosition a, GridPosition b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(GridPosition a, GridPosition b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj is GridPosition)
            return this == (GridPosition)obj;
        return false;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    // Get adjacent positions (up, down, left, right)
    public GridPosition[] GetAdjacent()
    {
        return new GridPosition[]
        {
            new GridPosition(x, y + 1), // Up
            new GridPosition(x, y - 1), // Down
            new GridPosition(x - 1, y), // Left
            new GridPosition(x + 1, y) // Right
        };
    }
}
