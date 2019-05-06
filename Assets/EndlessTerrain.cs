using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {


    public const float maxViewDist = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunckSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Use this for initialization
    private void Start()
    {
        chunckSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunckSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void UpdateVisibleChunks()
    {
        foreach(TerrainChunk chunk in terrainChunksVisibleLastUpdate)
        {
            chunk.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();


        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunckSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunckSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }

                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunckSize, transform));
                }
            }
        }
    }




    public class TerrainChunk
    {
        Vector2 pos;
        GameObject meshObject;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {

            pos = coord * size;
            bounds = new Bounds(pos, Vector3.one * size);
            Vector3 posv3 = new Vector3(pos.x, 0, pos.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = posv3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            SetVisible(false);
            meshObject.transform.parent = parent;
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);

        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
