using System;

/// <summary>
/// This is a position on the Grid System using cube coordinates.
/// </summary>
public struct CubeGridPosition : IEquatable<CubeGridPosition>
{
    public int q;
    public int r;
    public int s;
    public int floor;

    public CubeGridPosition(int q, int r, int floor)
    {
        this.q = q;
        this.r = r;
        this.s = -q-r;
        this.floor = floor;
    }

    public override bool Equals(object obj)
    {
        return obj is CubeGridPosition position &&
            q == position.q && 
            r == position.r &&
            s == position.s &&
            floor == position.floor;
    }

    public bool Equals(CubeGridPosition other)
    {   
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(q, r, s, floor);
    }

    public override string ToString()
    {
        return "q: " + q + "; r: " + r + "; s: " + s;

    }

    public static bool operator ==(CubeGridPosition a, CubeGridPosition b)
    {
        return a.q == b.q &&
            a.r == b.r && 
            a.s == b.s &&
            a.floor == b.floor;
    }

    public static bool operator !=(CubeGridPosition a, CubeGridPosition b)
    {
        return a.q != b.q ||
            a.r != b.r ||
            a.s != b.s || 
            a.floor != b.floor;
    }
 
    public static CubeGridPosition operator +(CubeGridPosition a, CubeGridPosition b)
    {
        return new CubeGridPosition(
            a.q + b.q, 
            a.r + b.r,
            a.floor + b.floor);
    }

    public static CubeGridPosition operator -(CubeGridPosition a, CubeGridPosition b)
    {
        return new CubeGridPosition(
            a.q - b.q, 
            a.r - b.r,
            a.floor - b.floor);
    }
}