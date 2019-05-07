using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    const float viewerMoveThreshold = 25f;
    const float squareViewerMoveThreshold = viewerMoveThreshold * viewerMoveThreshold;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    int chunckSize;
    int chunksVisibleInViewDst;

    public LODInfo[] detailLevels;
    public static float maxViewDist = 450;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Use this for initialization
    private void Start()
    {
        chunckSize = MapGenerator.mapChunkSize - 1;
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunckSize);
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;


        UpdateVisibleChunks();

    }



    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((viewerPositionOld- viewerPosition).sqrMagnitude > squareViewerMoveThreshold)
        {
            UpdateVisibleChunks();
            viewerPositionOld = viewerPosition;
        }
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
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunckSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }




    public class TerrainChunk
    {
        Vector2 pos;
        GameObject meshObject;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataRecived;

        int previousLODIndex = -1;


        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            pos = coord * size;
            bounds = new Bounds(pos, Vector3.one * size);
            Vector3 posv3 = new Vector3(pos.x, 0, pos.y);
            meshObject = new GameObject("terrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = posv3;
            SetVisible(false);
            meshObject.transform.parent = parent;
            lodMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i< detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }
            mapGenerator.RequestMapData(pos, OnMapDataRecived);
            

        }


        void OnMapDataRecived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecived = true;
            Texture2D texture = TextureCreator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }


     

        public void UpdateTerrainChunk()
        {
            if (mapDataRecived)
            {
                float viewerDistanceFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromEdge <= maxViewDist;
                SetVisible(visible);
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodmesh = lodMeshes[lodIndex];
                        if (lodmesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodmesh.mesh;
                        }
                        else if (!lodmesh.hasRequestedMesh)
                        {
                            lodmesh.RequestMesh(mapData);

                        }
                    }
                }
            }
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

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        System.Action updateCallback;

        int lod;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;

        }
        public void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();

        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,lod, OnMeshDataReceived);
        }
    }


    [System.Serializable]
    public struct LODInfo{
        public int lod;
        public float visibleDistanceThreshold;

    }
}
