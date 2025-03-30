using System;

/// <summary>
/// This is a position on the Grid System using axial coordinates.
/// </summary>
public struct AxialGridPosition : IEquatable<AxialGridPosition>
{
    public int q;
    public int r;
    public int s;
    public int floor;

    public AxialGridPosition(int x, int z, int s, int floor)
    {
        this.q = x;
        this.r = z;
        this.s = s;
        this.floor = floor;
    }

    public override bool Equals(object obj)
    {
        return obj is AxialGridPosition position &&
            q == position.q && 
            r == position.r &&
            s == position.s &&
            floor == position.floor;
    }

    public bool Equals(AxialGridPosition other)
    {   
        return this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(q, r, s, floor);
    }

    public override string ToString()
    {
        return "q: " + q + "; r: " + r + "; s: " + s + "; floor: " + floor;

    }

    public static bool operator ==(AxialGridPosition a, AxialGridPosition b)
    {
        return a.q == b.q &&
            a.r == b.r && 
            a.s == b.s &&
            a.floor == b.floor;
    }

    public static bool operator !=(AxialGridPosition a, AxialGridPosition b)
    {
        return a.q != b.q ||
            a.r != b.r ||
            a.s != b.s || 
            a.floor != b.floor;
    }
 
    public static AxialGridPosition operator +(AxialGridPosition a, AxialGridPosition b)
    {
        return new AxialGridPosition(
            a.q + b.q, 
            a.r + b.r, 
            a.s + b.s, 
            a.floor + b.floor);
    }

    public static AxialGridPosition operator -(AxialGridPosition a, AxialGridPosition b)
    {
        return new AxialGridPosition(
            a.q - b.q, 
            a.r - b.r, 
            a.s - b.s,
            a.floor - b.floor);
    }
}