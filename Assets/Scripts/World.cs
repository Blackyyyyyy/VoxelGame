using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public BlockType[] blockTypes;

    Chunk[,,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private void Start()
    {
        GenerateWorld();
        GetComponent<BoxCollider>().size = new Vector3(VoxelData.ChunkSize * VoxelData.WorldSizeInChunks, 
                                                       VoxelData.ChunkSize * VoxelData.WorldSizeInChunks, 
                                                       VoxelData.ChunkSize * VoxelData.WorldSizeInChunks);
        GetComponent<Rigidbody>().mass = (float) Math.Pow((VoxelData.ChunkSize * VoxelData.WorldSizeInChunks), 3);
    }

    void GenerateWorld()
    {
        for(int x = 0; x < VoxelData.WorldSizeInChunks; x++)
        {
            for (int y = 0; y < VoxelData.WorldSizeInChunks; y++)
            {
                for (int z = 0; z < VoxelData.WorldSizeInChunks; z++)
                {
                    CreateNewChunk(x, y, z);
                }
            }
        }
    }

    public byte GetVoxel(Vector3 pos, bool isUndergroundChunk)
    {
        if (!isVoxelInWorld(pos)) return 0;

        if (isUndergroundChunk)
        {
            return 1;
        }

        if (Mathf.Abs(pos.x) == (VoxelData.WorldSizeInChunks * VoxelData.ChunkSize / 2 - 1) ||
           Mathf.Abs(pos.y) == (VoxelData.WorldSizeInChunks * VoxelData.ChunkSize / 2 - 1) ||
           Mathf.Abs(pos.z) == (VoxelData.WorldSizeInChunks * VoxelData.ChunkSize / 2 - 1)) return 2;
        else return 1;
    }

    void CreateNewChunk(int x, int y, int z)
    {
        int xOffset = x - VoxelData.WorldSizeInChunks / 2;
        int yOffset = y - VoxelData.WorldSizeInChunks / 2;
        int zOffset = z - VoxelData.WorldSizeInChunks / 2;

        chunks[x, y, z] = new Chunk(new ChunkCoord(xOffset, yOffset, zOffset), this, isUndergroundChunk(x, y, z));
    }

    bool isUndergroundChunk(int x, int y, int z)
    {
        if (x == 0 || x == VoxelData.WorldSizeInChunks - 1 ||
            y == 0 || y == VoxelData.WorldSizeInChunks - 1 ||
            z == 0 || z == VoxelData.WorldSizeInChunks - 1 ) return false;
        return true;
    }

    bool isChunkInWorld(ChunkCoord coord) //probably needs change
    {
        if (coord.x >= 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1) return true;
        else return false;
    }

    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.WorldSizeInVoxels &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels) return true;
        else return false;
    }
}


[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public bool sameSided;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    //back, front, top, bottom, left, right

    public int GetTextureID(int faceIndex)
    {
        switch(faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}
