using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int width;
    public int height;
    public Tile[,] tiles;

    public LevelData(int width, int height)
    {
        this.width = width;
        this.height = height;
        tiles = new Tile[width, height];
    }
}