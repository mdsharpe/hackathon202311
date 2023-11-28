namespace Shared.Model;

public class GameBoard
{
    public Tile[][] Tiles { get; set; } = [];

    public int Height => Tiles.Select(o => o.Length).DefaultIfEmpty().Max();
    public int Width => Tiles.Length;

    public IEnumerable<Tile> EnumerateAll() => Tiles.SelectMany(o => o);

    public bool GetIsGameOver()
        => (from tile in EnumerateAll()
            where tile.TileColour != TileColour.EmptyCell
            group tile by tile.TileColour into tileGroup
            let count = tileGroup.Count()
            select new { Colour = tileGroup.Key, Tiles = count })
        .All(o => o.Tiles < 3);
}
