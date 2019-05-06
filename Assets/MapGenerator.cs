using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {


    public enum DrawMode
    {
        noiseMap,
        colorMap,
        Mesh
    }
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;

    MapDisplay display;

    public int numOctaives;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public float noiseScale;
    public bool autoUpdate;
    public float meshHeightMultipler;
    public AnimationCurve meshHeightCurve;

    public TerrainType[] regions;

    public int seed;
    public Vector2 offset;

    public void DrawMapInEditor()
    {
        MapData mapdata = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.noiseMap)
        {
            display.DrawTexture(TextureCreator.TextureFromHeightMap(mapdata.heightMap));
        }
        else if (drawMode == DrawMode.colorMap)
        {
            display.DrawTexture(TextureCreator.TextureFromColorMap(mapdata.colorMap, mapChunkSize, mapChunkSize));

        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.CreateTerrainMesh(mapdata.heightMap, meshHeightMultipler, meshHeightCurve, levelOfDetail), TextureCreator.TextureFromColorMap(mapdata.colorMap, mapChunkSize, mapChunkSize));
        }

    }

    MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, numOctaives, persistance, lacunarity, offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x= 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i =0; i< regions.Length; i++)
                {
                    if (currentHeight <=regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break; 

                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
      
    }

    public void OnValidate()
    {
      
        if (lacunarity < 1)
        {
            lacunarity = 1;

        }
        if (numOctaives < 0)
        {
            numOctaives = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
