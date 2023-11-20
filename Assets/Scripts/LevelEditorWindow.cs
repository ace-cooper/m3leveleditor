using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevel;
    private List<Tile> tileSlots = new List<Tile>(); // List to hold the tile slots
    private Tile selectedTile; // Currently selected tile for placement
    private Vector2 scrollPosition; // Used for scrolling the level grid
    private Vector2 tilesScrollPosition; // Used for scrolling the tile slots
    private int width = 10; // Default width
    private int height = 10; // Default height
   private Dictionary<Sprite, Texture2D> spriteTextureCache = new Dictionary<Sprite, Texture2D>(); // Cache to hold sprite textures

    [MenuItem("Match3/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);

        // Inputs for the width and height of the map grid
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        // Button to create a new level
        if (GUILayout.Button("Create New Level"))
        {
            currentLevel = new LevelData(width, height);
        }

        // Button to add a new tile slot
        if (GUILayout.Button("Add Tile Slot"))
        {
            tileSlots.Add(null);
        }

        // Draw the tile slots
        DrawTileSlots();

        // Draw the level grid
        if (currentLevel != null)
        {
            DrawLevelGrid();
        }
    }

    private void DrawTileSlots()
    {
        EditorGUILayout.LabelField("Tile Slots", EditorStyles.boldLabel);
        tilesScrollPosition = EditorGUILayout.BeginScrollView(tilesScrollPosition, GUILayout.Height(100));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < tileSlots.Count; i++)
        {
            Rect slotRect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
            if (tileSlots[i] != null && tileSlots[i].sprite != null)
            {
                Texture2D tex = AssetPreview.GetAssetPreview(tileSlots[i].sprite);
                if (tex == null)
                {
                    // If the asset preview is null, try converting the sprite to texture directly
                    tex = SpriteToTexture(tileSlots[i].sprite);
                }

                if (tex != null)
                {
                    GUI.DrawTexture(slotRect, tex);
                }
                else
                {
                    // If the texture is still null, draw a placeholder
                    EditorGUI.DrawRect(slotRect, Color.grey);
                }
            }
            else
            {
                // Draw an empty slot
                EditorGUI.DrawRect(slotRect, Color.grey);
            }

            HandleSlotDragAndDrop(slotRect, i);

            // Select the tile for placing when the slot is clicked
            if (GUI.Button(slotRect, GUIContent.none, GUIStyle.none))
            {
                selectedTile = tileSlots[i];
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void HandleSlotDragAndDrop(Rect slotRect, int index)
    {
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!slotRect.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Tile tile)
                        {
                            tileSlots[index] = tile;
                        }
                    }
                    evt.Use();
                }
                break;
        }
    }

 private void DrawLevelGrid()
{
    EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);
    for (int y = 0; y < currentLevel.height; y++)
    {
        EditorGUILayout.BeginHorizontal();
        for (int x = 0; x < currentLevel.width; x++)
        {
            Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
            Tile tile = currentLevel.tiles[x, y];
            if (tile != null && tile.sprite != null)
            {
                Texture2D tex = spriteTextureCache.TryGetValue(tile.sprite, out var cachedTexture) ? cachedTexture : SpriteToTexture(tile.sprite);
                if (tex != null)
                {
                    GUI.DrawTexture(cellRect, tex);
                }
            }
            else if (GUI.Button(cellRect, GUIContent.none, GUIStyle.none))
            {
                if (selectedTile != null)
                {
                    currentLevel.tiles[x, y] = selectedTile;
                    // You might need to call Repaint to refresh the editor window
                    Repaint();
                }
            }
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(cellRect, tile == null ? Color.gray : Color.white);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}


private Texture2D SpriteToTexture(Sprite sprite)
{
    if (spriteTextureCache.TryGetValue(sprite, out var cachedTexture))
    {
        return cachedTexture;
    }

    if (sprite.texture.isReadable)
    {
        // Create a new Texture2D with the correct dimensions
        Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        // Get the pixels from the sprite and apply them to the new Texture2D
        Color[] newColors = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height
        );
        newText.SetPixels(newColors);
        newText.Apply();
        
        spriteTextureCache[sprite] = newText;
        return newText;
    }
    else
    {
        Debug.LogError("Sprite texture is not readable: " + sprite.name);
        return null;
    }
}

}