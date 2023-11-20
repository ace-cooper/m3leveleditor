using System;
using UnityEngine;

// Define a Tile as a ScriptableObject for easy editing and management.
[CreateAssetMenu(fileName = "NewTile", menuName = "M3G/Tile", order = 1)]
public class Tile : ScriptableObject
{
    public enum TileType
    {
        Empty,
        Normal,
        Obstacle,
        // Add more tile types as needed
    }

    public TileType type;
    public Sprite sprite; // The visual representation of the tile
    // Add more properties as needed, like special effects, etc.
}