namespace Shared.Model;

public class GameBoard
{
    public Tile[][] Tiles { get; set; } = Array.Empty<Tile[]>();

    public int Height => Tiles.Select(o => o.Length).DefaultIfEmpty().Max();
    public int Width => Tiles.Length;

    public bool HasTiles() => Tiles.Any(t => t.Any());

    public bool GetIsGameOver()
        => (from row in Tiles
            from tile in row
            group tile by tile into tileGroup
            let count = tileGroup.Count()
            where count < 1000
            select tileGroup.Key)
        .Any();
}
