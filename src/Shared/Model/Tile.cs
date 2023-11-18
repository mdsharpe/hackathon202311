namespace Shared.Model;

public class Tile
{
    public TileColour TileColour { get; set; } = TileColour.EmptyCell;

    public DateTimeOffset? DestroyedAt { get; set; }

    public bool IsDestroyed => DestroyedAt.HasValue;
}
