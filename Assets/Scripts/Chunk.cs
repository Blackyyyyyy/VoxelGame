using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    GameObject chunkObject;
    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }
    Vector3 chunkLocalPosition;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelMap = new byte[VoxelData.ChunkSize, VoxelData.ChunkSize, VoxelData.ChunkSize];

    World world;
    bool isUndergroundChunk;

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }


    public Chunk(ChunkCoord _coord, World _world, bool _isUndergroundChunk)
    {
        coord = _coord;
        world = _world;
        isUndergroundChunk = _isUndergroundChunk;

        chunkObject = new GameObject();

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;

        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.localPosition = new Vector3(coord.x * VoxelData.ChunkSize, coord.y * VoxelData.ChunkSize, coord.z * VoxelData.ChunkSize);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + ", " + coord.z;
        chunkLocalPosition = chunkObject.transform.localPosition;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkSize; y++)
        {
            for (int x = 0; x < VoxelData.ChunkSize; x++)
            {
                for (int z = 0; z < VoxelData.ChunkSize; z++)
                {
                    int xWorld = x + (int)(chunkLocalPosition.x + 0.5f);
                    int yWorld = y + (int)(chunkLocalPosition.y + 0.5f);
                    int zWorld = z + (int)(chunkLocalPosition.z + 0.5f);

                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(xWorld, yWorld, zWorld), isUndergroundChunk);
                }
            }
        }
    }

    int GetSextant(int x, int y, int z) //15 possible (0 - 14) values -> can be in up to 3 sextants at the same time
    {
        int absX = Math.Abs(x);
        int absY = Math.Abs(y);
        int absZ = Math.Abs(z);

        if (absZ > absX && absZ > absY)
        {
            if (z > 0) return 0;
            else return 1;
        }
        else if (absY > absX && absY > absZ)
        {
            if (y > 0) return 2;
            else return 3;
        }
        else if (absX > absY && absX > absZ)
        {
            if (x < 0) return 4;
            else return 5;
        }
        else if (absX == absY && absX == absZ) //all diagonals
        {/*
            if (x < 0 && y > 0 && z > 0) return 6;
            else if (x > 0 && y > 0 && z > 0) return 7;
            else if (x < 0 && y < 0 && z > 0) return 8;
            else if (x > 0 && y < 0 && z > 0) return 9;
            else if (x < 0 && y > 0 && z < 0) return 10;
            else if (x > 0 && y > 0 && z < 0) return 11;
            else if (x < 0 && y < 0 && z < 0) return 12;
            else if (x > 0 && y < 0 && z < 0) return 13;
            else return 14;*/
            return 6;
        }
        else if (absX == absY) //left and right edges; 4 (top and bottom)
        {
            if (x == y)
            {
                if (x > 0) return 7;
                else return 8;
            }
            else if (x < y) return 9;
            else return 10;
        }
        else if(absX == absZ) //front and back edges; 4 (left and right)
        {
            if (x == z)
            {
                if (x > 0) return 11;
                else return 12;
            }
            else if (x < z) return 13;
            else return 14;
        }
        else if(absY == absZ) //top and bottom edges; 4 (front and back)
        {
            if (y == z)
            {
                if (y > 0) return 15;
                else return 16;
            }
            else if (y > z) return 17;
            else return 18;
        }

        return -1;
    }

    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkSize; y++)
        {
            for (int x = 0; x < VoxelData.ChunkSize; x++)
            {
                for (int z = 0; z < VoxelData.ChunkSize; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
            {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                if (!world.blockTypes[blockID].sameSided)
                {
                    Tuple<int, int> faceData = GetFaceData(p, pos);
                    AddTexture(world.blockTypes[blockID].GetTextureID(faceData.Item1), faceData.Item2);
                }
                else AddTexture(world.blockTypes[blockID].GetTextureID(p), 0);

                
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    bool isVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkSize - 1 || y < 0 || y > VoxelData.ChunkSize - 1 || z < 0 || z > VoxelData.ChunkSize - 1) return false;
        else return true;
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!isVoxelInChunk(x, y, z)) return world.blockTypes[world.GetVoxel(pos + chunkLocalPosition, isUndergroundChunk)].isSolid;

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    // 0      1     2     3      4     5
    //back, front, top, bottom, left, right
    Tuple<int, int> GetFaceData(int p, Vector3 pos)
    {
        int x = (int)(pos.x + chunkLocalPosition.x + 0.5f);
        int y = (int)(pos.y + chunkLocalPosition.y + 0.5f);
        int z = (int)(pos.z + chunkLocalPosition.z + 0.5f);

        int absX = Math.Abs(x);
        int absY = Math.Abs(y);
        int absZ = Math.Abs(z);
        
        switch(GetSextant(x, y, z))
        {
            case 0: //(Abs) z > x/y; +z > 0; back face;
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(3, 0);
                    case 1:
                        return new Tuple<int, int>(2, 0);
                    case 2:
                        return new Tuple<int, int>(0, 0);
                    case 3:
                        return new Tuple<int, int>(1, 0);
                    case 4:
                        return new Tuple<int, int>(p, 1);
                    case 5:
                        return new Tuple<int, int>(p, 3);
                    default:
                        return new Tuple<int, int>(p, 0);
                }
            case 1: //(Abs) z > x/y; -z < 0; front face;
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(2, 0);
                    case 1:
                        return new Tuple<int, int>(3, 0);
                    case 2:
                        return new Tuple<int, int>(1, 2);
                    case 3:
                        return new Tuple<int, int>(0, 2);
                    case 4:
                        return new Tuple<int, int>(p, 3);
                    case 5:
                        return new Tuple<int, int>(p, 1);
                    default:
                        return new Tuple<int, int>(p, 0);
                }
            case 2: //(Abs) y > x/z; +y > 0; top face;
                return new Tuple<int, int>(p, 0);
            case 3: //(Abs) y > x/z; -y < 0; bottom face;
                switch (p)
                {
                    case 2:
                        return new Tuple<int, int>(3, 0);
                    case 3:
                        return new Tuple<int, int>(2, 0);
                    default:
                        return new Tuple<int, int>(p, 2);
                }
            case 4: //(Abs) x > y/z; -x < 0; left face;
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 1);
                    case 1:
                        return new Tuple<int, int>(p, 3);
                    case 2:
                        return new Tuple<int, int>(5, 1);
                    case 3:
                        return new Tuple<int, int>(4, 3);
                    case 4:
                        return new Tuple<int, int>(2, 0);
                    case 5:
                        return new Tuple<int, int>(3, 0);
                    default:
                        return new Tuple<int, int>(p, 0);
                }
            case 5: //(Abs) x > y/z; +x > 0; right face;
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 3);
                    case 1:
                        return new Tuple<int, int>(p, 1);
                    case 2:
                        return new Tuple<int, int>(4, 3);
                    case 3:
                        return new Tuple<int, int>(5, 1);
                    case 4:
                        return new Tuple<int, int>(3, 0);
                    case 5:
                        return new Tuple<int, int>(2, 0);
                    default:
                        return new Tuple<int, int>(p, 0);
                }
            case 6: //(Abs) x = y = z; all diagonals;
                return new Tuple<int, int>(2, 0);
            case 7: //top right edge
                switch(p)
                {
                    case 3:
                        return new Tuple<int, int>(5, 3);
                    case 4:
                        return new Tuple<int, int>(p, 0);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 8: //bottom left edge
                switch(p)
                {
                    case 2:
                        return new Tuple<int, int>(5, 3);
                    case 5:
                        return new Tuple<int, int>(p, 2);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 9: //top left edge
                switch (p)
                {
                    case 3:
                        return new Tuple<int, int>(4, 1);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 10: //bottom right edge
                switch(p)
                {
                    case 2:
                        return new Tuple<int, int>(4, 1);
                    case 4:
                        return new Tuple<int, int>(p, 2);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 11: //back right edge
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 1);
                    case 4:
                        return new Tuple<int, int>(p, 3);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 12: //front left edge
                switch (p)
                {
                    case 1:
                        return new Tuple<int, int>(p, 1);
                    case 5:
                        return new Tuple<int, int>(p, 3);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 13: //back left edge
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 3);
                    case 5:
                        return new Tuple<int, int>(p, 1);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 14: //front right edge
                switch (p)
                {
                    case 1:
                        return new Tuple<int, int>(p, 3);
                    case 4:
                        return new Tuple<int, int>(p, 1);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 15: //back top edge
                switch(p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 0);
                    case 3:
                        return new Tuple<int, int>(4, 0);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 16: //front bottom edge
                switch (p)
                {
                    case 1:
                        return new Tuple<int, int>(p, 2);
                    case 2:
                        return new Tuple<int, int>(4, 2);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 17: //front top edge
                switch (p)
                {
                    case 1:
                        return new Tuple<int, int>(p, 0);
                    case 3:
                        return new Tuple<int, int>(4, 2);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            case 18: //back bottom edge
                switch (p)
                {
                    case 0:
                        return new Tuple<int, int>(p, 2);
                    case 2:
                        return new Tuple<int, int>(4, 0);
                    default:
                        return new Tuple<int, int>(2, 0);
                }
            default:
                return new Tuple<int, int>(p, 0);
        }
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID, int orientation)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;


        switch(orientation)
        {
            case 1: //right
                uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x, y));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
                break;
            case 2: //down
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
                uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x, y));
                break;
            case 3: //left
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
                uvs.Add(new Vector2(x, y));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
                break;
            default: //0; up
                uvs.Add(new Vector2(x, y));
                uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
                uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
                break;

        }
    }
}

public class ChunkCoord
{
    public int x;
    public int y;
    public int z;

    public ChunkCoord(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}