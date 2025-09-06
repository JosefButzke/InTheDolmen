using UnityEngine;

public enum TerrainLayer
{
    Surface,
    Underground
}

public class TerrainData
{
    public static float terrainSurface = 0f;
    public static int width = 64;
    public static int height = 128;
    public static float surfaceLevel = 0f;
}