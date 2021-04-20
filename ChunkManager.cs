using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [Header("ChunkGeneration")]
    [SerializeField] private Vector2 seed;
    [SerializeField] private Material[] terrainMaterialsFull;
    [SerializeField] private int chunkSize;
    
    [Header("World Object")] 
    [SerializeField] private List<GameObject> treePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> grassPrefabs = new List<GameObject>();
    
    [Header("Buildings")]
    [SerializeField] private GameObject villagePrefab;
    [SerializeField] private GameObject campPrefab;
    [SerializeField] private GameObject castlePrefab;

    [Header("Player")] 
    [SerializeField]private Transform player;
    
    //Chunk render distance
    int chunkDist = 5;
    
    public Dictionary<Vector2, GameObject> RenderedChunks = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, ChunkData> ChunkDict = new Dictionary<Vector2, ChunkData>();

    FastNoise _noise = new FastNoise();

    public int ChunkSize => chunkSize;

    private void Start()
    {
        for (int z = -3; z < 3; z++)
        {
            for (int x = -3; x < 3; x++)
            {
                CreateChunk(new Vector2(x * chunkSize, z * chunkSize), true);
            }
        }
    }

    private void Update()
    {
        LoadChunk();
    }

    /// <summary>
    /// Create an new chunk 
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="createFullyNewChunk"></param>
    /// <return>"chunkGameObject" So you can parent castles, trees etc when you load the chunk</returns>
    private GameObject CreateChunk(Vector2 pos, bool createFullyNewChunk)
    {
        GameObject chunkGameObject = new GameObject();
        chunkGameObject.transform.position = new Vector3(pos.x, 0, pos.y);
        RenderedChunks.Add(pos, chunkGameObject);
        chunkGameObject.name = "Chunk: " + pos;
        chunkGameObject.layer = 6;
        var chunk = chunkGameObject.AddComponent<Chunk>();
        chunk.Seed = seed;
        chunkGameObject.GetComponent<MeshRenderer>().materials = terrainMaterialsFull;
        chunk.Init(chunkSize);
        
        if (createFullyNewChunk)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.Parent = chunkGameObject;
            ChunkDict.Add(pos, chunkData);
            CreateTrees(pos, chunkData.Parent.transform, chunkData);
            chunkData.ChunkActivated = true;
        }
        
        return chunkGameObject;
    }

    /// <summary>
    /// Fill the chunk with trees
    /// </summary>
    /// <param name="chunkData"></param>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    private void CreateTrees(Vector2 pos, Transform parent, ChunkData chunkData)
    {
        System.Random randSeed = new System.Random((int)pos.x * 11200 + (int)pos.y);

        float simplex = _noise.GetSimplex((int)pos.x * .8f, (int)pos.y * .8f);

        if (simplex > 0)
        {
            simplex *= 2f;

            int treeCount = Mathf.FloorToInt((float) randSeed.NextDouble() * (chunkSize - 1) * simplex);

            for (int i = 0; i < treeCount; i++)
            {
                int xPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.x;
                int zPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.y;

                int y = 200;

                var randTree = Random.Range(0, treePrefabs.Count);

                var tree = Instantiate(treePrefabs[randTree], new Vector3(xPos, y, zPos),
                    Quaternion.Euler(PickRandomRotation()));

                RaycastHit hit;
                
                if (Physics.Raycast(tree.transform.position, tree.transform.TransformDirection(Vector3.down), out hit,
                    10000))
                {
                    var trans = tree.transform;
                    trans.position = hit.point;
                    var randomRotation = Random.Range(0, 360);
                    trans.rotation = Quaternion.Euler(trans.rotation.x, randomRotation,
                        trans.rotation.z);
                    tree.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    
                    // if(tree.transform.rotation)
                    
                    SaveObjectOnChunk(tree, treePrefabs[randTree] ,chunkData);
                    
                    tree.transform.SetParent(parent);
                }
            }
        }

        CreateGrass(pos, parent, chunkData);
    }

    /// <summary>
    /// Fill the chunk with grass
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    private void CreateGrass(Vector2 pos, Transform parent, ChunkData chunkData )
    {    
        
        System.Random randSeed = new System.Random((int)pos.x * 10000 + (int)pos.y);

        float simplex = _noise.GetSimplex((int)pos.x * .8f, (int)pos.y * .8f);

        if (simplex > -0.8F)
        {
            simplex *= 2f;

            int grassCount = Mathf.FloorToInt((float) randSeed.NextDouble() * (chunkSize - 1) * simplex);

            for (int i = 0; i < grassCount; i++)
            {
                int xPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.x;
                int zPos = (int)(randSeed.NextDouble() * (chunkSize - 2)) + 1 + (int)pos.y;

                int y = 200;

                var randGrass = Random.Range(0, grassPrefabs.Count);

                var grass = Instantiate(grassPrefabs[randGrass], new Vector3(xPos, y, zPos),
                    Quaternion.Euler(PickRandomRotation()));

                RaycastHit hit;
                
                if (Physics.Raycast(grass.transform.position, grass.transform.TransformDirection(Vector3.down), out hit,
                    10000))
                {
                    grass.transform.position = hit.point;
                    var randomRotation = Random.Range(0, 360);
                    grass.transform.rotation = Quaternion.Euler(grass.transform.rotation.x, randomRotation,
                        grass.transform.rotation.z);
                    grass.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                    
                    SaveObjectOnChunk(grass, grassPrefabs[randGrass] ,chunkData);
                    
                    grass.transform.SetParent(parent);
                }
            }
        }

        CreateBuildings(pos, parent, chunkData);
    }

    /// <summary>
    /// Pick an building/village to create
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="chunkData"></param>
    private void CreateBuildings(Vector2 pos,Transform parent,ChunkData chunkData )
    {
        if (chunkData.ObjectsOnChunk.Count > 0)
            return;

        var rand = Random.Range(0, 50);

        switch (rand)
        {
            case 1 when chunkData.TerrainType == TerrainTypes.Hilly:
                return;
            case 1:
                CreateBuilding(castlePrefab ,pos, parent, chunkData);
                break;
            case 2:
            case 3:
            {
                if (chunkData.TerrainType == TerrainTypes.Hilly)
                    return;
                CreateBuilding(villagePrefab ,pos, parent, chunkData);
                break;
            }
            case 4:
            case 5:
                CreateBuilding(campPrefab ,pos, parent, chunkData);
                break;
        }
    }
    
    /// <summary>
    /// Spawn an village
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="parent"></param>
    /// <param name="chunkData"></param>
    private void CreateBuilding(GameObject prefab,Vector2 pos,Transform parent, ChunkData chunkData )
    {
        int y = 200;

        var instance = Instantiate(prefab, new Vector3(pos.x, y, pos.y),  Quaternion.identity);
                
        instance.transform.SetParent(parent);

        SaveObjectOnChunk(instance, prefab, chunkData);
    }
   
     /// <summary>
     /// Pick a random rotation
     /// </summary>
     /// <returns></returns>
     private Vector3 PickRandomRotation()
     {
         var rand = Random.Range(0, 360);
         
         return new Vector3(0, rand, 0);
     }

     /// <summary>
     /// Save the object data to the dictionary
     /// </summary>
     /// <param name="objInWorld"></param>
     /// <param name="prefabToSave"></param>
     /// <param name="chunkData"></param>
     private void SaveObjectOnChunk(GameObject objInWorld, GameObject prefabToSave ,ChunkData chunkData)
     {
         ObjectData obj = new ObjectData();

         obj.Object = prefabToSave;
         obj.Position = objInWorld.transform.position;
         obj.Rotation = objInWorld.transform.rotation;
                    
         chunkData.ObjectsOnChunk.Add(obj);
     }
     

     private Vector2 curChunk = new Vector2(-1, -1);
     
     /// <summary>
     /// Load an chunk
     /// </summary>
     public void LoadChunk()
     {
         var curChunkPosX = RoundDown((int)player.position.x);
         var curChunkPosZ = RoundDown((int)player.position.z);
         
         //entered a new chunk
         if (curChunk.x != curChunkPosX || curChunk.y != curChunkPosZ)
         {
             curChunk.x = curChunkPosX;
             curChunk.y = curChunkPosZ;

             //Create or load the chunks
             for (int i = curChunkPosX - chunkSize * chunkDist; i <= curChunkPosX + chunkSize * chunkDist; i += chunkSize)
             {
                 for (int j = curChunkPosZ - chunkSize * chunkDist; j <= curChunkPosZ + chunkSize * chunkDist; j += chunkSize)
                 {
                     Vector2 cp = new Vector2(i, j);

                     
                     //If chunk not already in dictionary create an fully new chunk
                     if(!ChunkDict.ContainsKey(cp))
                     {
                         CreateChunk(cp, true);
                     }
                     else
                     {
                         ChunkData chunkData = ChunkDict[cp];
                         
                         //If already activated and vissible in world
                         if (chunkData.ChunkActivated)
                         {
                         }
                         else
                         {
                             //Create an new chunk (mesh only)
                             var chunkGameObject = CreateChunk(cp, false);
                             
                             chunkData.ChunkActivated = true;
                             
                             //Load the objects that where on the chunk (Trees, Houses, Castles etc...)
                             for (int k = 0; k < chunkData.ObjectsOnChunk.Count; k++)
                             {
                                 var obj = Instantiate(chunkData.ObjectsOnChunk[k].Object,
                                     chunkData.ObjectsOnChunk[k].Position,
                                     chunkData.ObjectsOnChunk[k].Rotation);
                                 obj.transform.SetParent(chunkGameObject.transform);
                             }
                         }
                     }
                 }
             }
             
             List<Vector2> toUnload = new List<Vector2>();

             //remove chunks that are too far away
             //unload chunks
             foreach(KeyValuePair<Vector2, GameObject> c in RenderedChunks)
             {
                 Vector2 cp = c.Key;
                 if(Mathf.Abs(curChunkPosX - cp.x) > chunkSize * (chunkDist + 3) || 
                    Mathf.Abs(curChunkPosZ - cp.y) > chunkSize * (chunkDist + 3))
                 {
                     RenderedChunks[cp].SetActive(false);
                     toUnload.Add(c.Key);
                 }
             }
             
             //Remove and delete the chunks from the list
             foreach(Vector2 cp in toUnload)
             {
                 GameObject objToDestroy = RenderedChunks[cp].gameObject;
                 
                 RenderedChunks.Remove(cp);
                 
                 Destroy(objToDestroy);

                 ChunkDict[cp].ChunkActivated = false;

             }
         }
     }
     
     
     int RoundDown(int toRound)
     {
         return toRound - toRound % ChunkSize;
     }
     
}

