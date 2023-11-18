namespace Client;

public static class TileCssUtil
{
    public static string Height = $"(100vh - 10rem) / {GlobalConstants.ySize}";
    public static string Width = $"((100vw - 2rem - 40px) / {GlobalConstants.xSize})";
}
