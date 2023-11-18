
namespace Shared.Model;
public class Coordinates
{
    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }

    public int Y { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is Coordinates coordinates &&
               X == coordinates.X &&
               Y == coordinates.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}
