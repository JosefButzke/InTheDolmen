using UnityEngine;

public enum TerrainLayer
{
    Surface,
    Underground
}

public class TerrainData
{
    public static float terrainSurface = 0f;
    public static int width = 32;
    public static int height = 32;
    public static float surfaceLevel = 0f;
}