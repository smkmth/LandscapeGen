using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {


    public static float[,] GenNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {

        System.Random pmrg = new System.Random(seed);
        Vector2[] octaveOffset = new Vector2[octaves];
        for(int i= 0; i< octaves; i++)
        {
            float offsetx = pmrg.Next(-100000, 100000) + offset.x;
            float offsety = pmrg.Next(-100000, 100000) + offset.y;
            octaveOffset[i] = new Vector2(offsetx, offsety);
        }

        if (scale <= 0)
        {
            scale = 0.001f;
        }

        float halfWidth = mapWidth / 2.0f;
        float halfHeight = mapHeight / 2.0f;
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseheight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x -halfWidth)/ scale * frequency + octaveOffset[i].x;
                    float sampleY = (y -halfHeight)/ scale * frequency + octaveOffset[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1 ;
                   // noiseMap[x, y] = perlinValue;
                    noiseheight += perlinValue * amplitude;
                    amplitude*= persistance;
                    frequency *= lacunarity;
                }
                if (noiseheight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseheight;
                }
                else if (noiseheight < minNoiseHeight)
                {
                    minNoiseHeight = noiseheight;
                }
                noiseMap[x, y] = noiseheight; 

            }
        }


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }

        return noiseMap;
        

    }

}
