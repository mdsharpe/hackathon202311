namespace Shared.Model
{
    public class Tile
    {
        public TileColour TileColour { get; set; } = TileColour.EmptyCell;

        public bool IsDestroyed { get; set; } = false;
    }
}
