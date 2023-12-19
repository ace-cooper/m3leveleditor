using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "M3G/Level Data Asset")]
public class LevelDataAsset : ScriptableObject
{
    public Tile[,] tiles;
}
