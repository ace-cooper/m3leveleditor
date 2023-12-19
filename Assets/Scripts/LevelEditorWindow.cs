using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelData currentLevel;
    private List<Tile> tileSlots = new List<Tile>();

    private Tile selectedTile;
    private int selectedTileIndex = -1;
    private Vector2 scrollPosition;
    private Vector2 tilesScrollPosition;
    private int width = 10;
    private int height = 10;

    private bool isLeftMouseButtonDown = false;
    private bool isRightMouseButtonDown = false;

    private Dictionary<Sprite, Texture2D> spriteTextureCache = new Dictionary<Sprite, Texture2D>();

    [MenuItem("M3G/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Level Editor", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (GUILayout.Button("Create New Level"))
        {
            currentLevel = new LevelData(width, height);
        }

        if (GUILayout.Button("Save Level"))
        {
            SaveLevelAsScriptableObject();
        }
        
        if (GUILayout.Button("Add Tile Slot"))
        {
            tileSlots.Add(null);
        }
        
        DrawTileSlots();

       
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
                  
                    tex = SpriteToTexture(tileSlots[i].sprite);
                }

                if (tex != null)
                {
                    GUI.DrawTexture(slotRect, tex);
                }
                else
                {
                    
                    EditorGUI.DrawRect(slotRect, Color.grey);
                }
            }
            else
            {
                
                EditorGUI.DrawRect(slotRect, Color.grey);
            }

            if (selectedTileIndex == i)
            {
                Handles.DrawSolidRectangleWithOutline(slotRect, new Color(0, 0, 0, 0), Color.yellow);
            }

            HandleSlotDragAndDrop(slotRect, i);

         
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

    private void SaveLevelAsScriptableObject()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Level as ScriptableObject",
            "NewLevel.asset",
            "asset",
            "Please enter a file name to save the level data to"
        );

        if (path.Length != 0) 
        {
 
            LevelDataAsset levelDataAsset = CreateInstance<LevelDataAsset>();

            levelDataAsset.tiles = currentLevel.tiles;

            AssetDatabase.CreateAsset(levelDataAsset, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = levelDataAsset;
        }
    }

}