using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevel;
    private List<Tile> tileSlots = new List<Tile>(); // List to hold the tile slots

    private Tile selectedTile; // Tile that is currently selected for placement
    private int selectedTileIndex = -1; // Index of the selected tile slot
    private Vector2 scrollPosition; // Used for scrolling the level grid
    private Vector2 tilesScrollPosition; // Used for scrolling the tile slots
    private int width = 10; // Default width
    private int height = 10; // Default height

    private bool isLeftMouseButtonDown = false;
    private bool isRightMouseButtonDown = false;

    private Dictionary<Sprite, Texture2D> spriteTextureCache = new Dictionary<Sprite, Texture2D>();

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

            if (selectedTileIndex == i)
            {
                Handles.DrawSolidRectangleWithOutline(slotRect, new Color(0, 0, 0, 0), Color.yellow);
            }

            HandleSlotDragAndDrop(slotRect, i);

            // Select the tile for placing when the slot is clicked
            if (GUI.Button(slotRect, GUIContent.none, GUIStyle.none))
            {
                selectedTile = tileSlots[i];
                selectedTileIndex = i;
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
    EditorGUI.BeginChangeCheck();
    for (int y = 0; y < currentLevel.height; y++)
    {
        EditorGUILayout.BeginHorizontal();
        for (int x = 0; x < currentLevel.width; x++)
        {
            Rect cellRect = EditorGUILayout.GetControlRect(GUILayout.Width(50), GUILayout.Height(50));
            Tile tile = currentLevel.tiles[x, y];

            // Only try to draw the texture if the sprite is cached
            if (tile != null && tile.sprite != null)
            {
                        Texture2D tex;
                    if (!spriteTextureCache.TryGetValue(tile.sprite, out tex))
                    {
                        tex = SpriteToTexture(tile.sprite);
                        if (tex != null)
                        {
                            spriteTextureCache[tile.sprite] = tex;
                        }
                    }
                    
                    if (tex != null)
                    {
                        GUI.DrawTexture(cellRect, tex);
                    }
            }
            else if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(cellRect, tile == null ? Color.gray : Color.white);
            }

            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) &&
                cellRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0) // Left click
                {
                    if (selectedTile != null && !isRightMouseButtonDown)
                    {
                        currentLevel.tiles[x, y] = selectedTile;
                        isLeftMouseButtonDown = true;
                    }
                    Event.current.Use();
                }
                else if (Event.current.button == 1) // Right click
                {
                    if (!isLeftMouseButtonDown)
                    {
                        currentLevel.tiles[x, y] = null;
                        isRightMouseButtonDown = true;
                    }
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.MouseUp)
            {
                isLeftMouseButtonDown = false;
                isRightMouseButtonDown = false;
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    if (EditorGUI.EndChangeCheck())
    {
        Repaint();
    }
}


 private Texture2D SpriteToTexture(Sprite sprite)
    {
        if (spriteTextureCache.TryGetValue(sprite, out var cachedTexture))
        {
            return cachedTexture;
        }

        try
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
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
        catch (UnityException e)
        {
            Debug.LogError($"Error converting sprite to texture: {e.Message}");
            return null;
        }
    }

}