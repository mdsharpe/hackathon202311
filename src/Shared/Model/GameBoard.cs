namespace Shared.Model;

public class GameBoard
{
    public Tile[][] Tiles { get; set; } = Array.Empty<Tile[]>();

    public int Height => Tiles.Select(o => o.Length).DefaultIfEmpty().Max();
    public int Width => Tiles.Length;

    public IEnumerable<Tile> EnumerateAll() => Tiles.SelectMany(o => o);

    public bool GetIsGameOver()
        => (from tile in EnumerateAll()
            group tile by tile.TileColour into tileGroup
            let count = tileGroup.Count()
            where count < 3
            select tileGroup.Key)
        .Any();
}
