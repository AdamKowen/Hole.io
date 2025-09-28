// Assets/Editor/AssembleTiledMap.cs
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AssembleTiledMap : EditorWindow
{
    string resourcesFolder = "MapTiles"; // under Assets/Resources/MapTiles
    int tilePixelSize = 2048;            // your crop size from ImageMagick
    float pixelsPerUnit = 512f;          // must match your import PPU

    [MenuItem("Tools/Assemble Map Tiles (Tilemap)")]
    public static void ShowWindow()
    {
        GetWindow<AssembleTiledMap>("Assemble Map Tiles");
    }

    void OnGUI()
    {
        resourcesFolder = EditorGUILayout.TextField("Resources Folder", resourcesFolder);
        tilePixelSize = EditorGUILayout.IntField("Tile Pixel Size", tilePixelSize);
        pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit);

        if (GUILayout.Button("Build Tilemap From r/c Filenames"))
        {
            Build();
        }
    }

    void Build()
    {
        var sprites = Resources.LoadAll<Sprite>(resourcesFolder);
        if (sprites == null || sprites.Length == 0)
        {
            EditorUtility.DisplayDialog("Assemble Map", "No sprites found in Resources/" + resourcesFolder, "OK");
            return;
        }

        // Create Grid + Tilemap
        GameObject gridGo = new GameObject("MapGrid");
        var grid = gridGo.AddComponent<Grid>();
        float cellSize = tilePixelSize / pixelsPerUnit;
        grid.cellSize = new Vector3(cellSize, cellSize, 0f);

        GameObject tmGo = new GameObject("MapTilemap");
        tmGo.transform.SetParent(gridGo.transform, false);
        var tilemap = tmGo.AddComponent<Tilemap>();
        var renderer = tmGo.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;

        var rx = new Regex(@"tile_r(?<r>\d+)_c(?<c>\d+)", RegexOptions.IgnoreCase);

        int placed = 0, maxR = 0, maxC = 0;

        foreach (var s in sprites)
        {
            var m = rx.Match(s.name);
            if (!m.Success)
                continue;

            int r = int.Parse(m.Groups["r"].Value);
            int c = int.Parse(m.Groups["c"].Value);

            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = s;

            // Row increases downward → y negative
            var pos = new Vector3Int(c, -r, 0);
            tilemap.SetTile(pos, tile);

            maxR = Mathf.Max(maxR, r);
            maxC = Mathf.Max(maxC, c);
            placed++;
        }

        tilemap.RefreshAllTiles();
        Selection.activeObject = gridGo;

        Debug.Log($"Assembled {placed} tiles. World size ≈ {(maxC+1)*cellSize} x {(maxR+1)*cellSize} units.");
        EditorUtility.DisplayDialog("Assemble Map", $"Done. Placed {placed} tiles.", "OK");
    }
}
