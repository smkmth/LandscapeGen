using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour {


    public enum DrawMode
    {
        noiseMap,
        colorMap,
        Mesh
    }
    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int previewLOD;

    MapDisplay display;

    public int numOctaives;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public float noiseScale;
    public bool autoUpdate;
    public float meshHeightMultipler;
    public AnimationCurve meshHeightCurve;

    public TerrainType[] regions;

    public int seed;
    public Vector2 offset;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapdata = GenerateMapData(Vector2.zero);
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
            display.DrawMesh(MeshGenerator.CreateTerrainMesh(mapdata.heightMap, meshHeightMultipler, meshHeightCurve, previewLOD), TextureCreator.TextureFromColorMap(mapdata.colorMap, mapChunkSize, mapChunkSize));
        }

    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();


    }

    public void RequestMeshData(MapData mapdata, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapdata, lod, callback);
        };

        new Thread(threadStart).Start();


    }
    public void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapdata = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapdata));

        }

    }
    public void MeshDataThread(MapData mapdata,int lod, Action<MeshData> callback)
    {
        MeshData meshdata = MeshGenerator.CreateTerrainMesh(mapdata.heightMap, meshHeightMultipler, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshdata));

        }

    }

    public void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadinfo = mapDataThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadinfo = meshDataThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, numOctaives, persistance, lacunarity, center+ offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
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

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
