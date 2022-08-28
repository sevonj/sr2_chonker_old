using Godot;
using System;

using System.IO;

public class CityObject
{
    public Vector3 pos;
    public uint model;
}

// --- Model Buffer
public class ModelBuffer
{
    // Each of these have 1 ibuffer and 1 or more vbuffers
    public uint type;   // 0 is on chunk, 7 is in g_chunk
    public uint vertBufferCount; // 1 on physmodels, >1 on rendermodels
    public uint indexCount;
    public VertBuffer[] vertBuffers;
}

public class VertBuffer
{
    public uint unk2BCount;
    public uint uvSetCount;
    public uint vertSize;
    public uint vertCount;
}

/* --- Example ModelBuffer Content
// Physmodel
ModelBuffer
{
	uint type            = 0    // 0 means physmodel.
	uint indBufferSize   = 112
	uint vertBufferCount = 1    // Always just 1 on physmodels.
	VertBuffer[] vertBuffers
	[
		VertBuffer 0
		{
			uint unk2BCount = 0     // Unused on physmodel.
			uint uvSetCount = 0     // Unused on physmodel.
			uint vertSize   = 12    // Vert position takes 12B.
			uint vertCount  = 36
		}
	]
}

// Viewmodel
ModelBuffer
{
	uint type            = 7    // 7 means viewmodel
	uint indBufferSize   = 2785
	uint vertBufferCount = 8
	VertBuffer[] vertBuffers
	[
		// VertBuffer for each combination of unk2BCount and uvSetCount.

		VertBuffer 0
		{
			uint unk2BCount = 1     // 2B
			uint uvSetCount = 1     // 4B
			uint vertSize   = 18    // 12B + above
			uint vertCount  = 123
		}
		VertBuffer 1
		{
			uint unk2BCount = 1
			uint uvSetCount = 2
			uint vertSize   = 22
			uint vertCount  = 52
		}
		VertBuffer 2
		{
			uint unk2BCount = 2
			uint uvSetCount = 2
			uint vertSize   = 24
			uint vertCount  = 420
		}
		...
	]
}
*/

public class RenderModel
{
    public Vector3 bboxMin;
    public Vector3 bboxMax;
    public uint submeshCount = uint.MaxValue;
    public uint unk2_count = uint.MaxValue;
    public RenderModelSubmesh[] submeshes;
    //public RenderModelUnk2[] unk2s
}

public class RenderModelSubmesh
{
    public uint vertBuffer;
    public uint indexOffset;
    public uint vertOffset;
    public uint indexCount;
    public uint vertCount;
    public uint material;
}

public class Material
{
    public uint textureCount;
    public uint[] textures;
    public uint shaderFlagCount;
}

// --- The chunk

public class CPUChunk
{
    public uint MAGIC;
    public uint VERSION;

    public string[] texList;
    public uint texCount;

    public uint renderModelCount;
    public uint cityobjectCount;
    public uint modelBufferCount;
    public uint unknownCount3;
    public uint unknownCount4;

    public CityObject[] cityObjects;

    public uint unknownCount5;
    public uint unknownCount6;
    public uint unknownCount7;
    public uint unknownCount8;

    public uint moppSize;

    public ModelBuffer[] modelBuffers;
    public uint g_chunkVBufCount;

    public Material[] materials;
    public uint materialCount;
    public uint shaderParamCount;
    public uint matUnk2Count;
    public uint matUnk3Count;

    public RenderModel[] renderModels;
}

public class ChunkLoader : Node
{
    static uint MAGIC = 0xBBCACA12;
    static uint VERSION = 121;

    public CPUChunk LoadChunk(string filepath)
    {
        GD.Print(filepath);

        if (!System.IO.File.Exists(filepath))
        {
            GD.PushWarning("ChunkLoader.LoadChunk(): File doesn't exist! " + filepath);
            return null;
        }

        using (FileStream fs = System.IO.File.OpenRead(filepath))
        {
            BinaryReader br = new BinaryReader(fs);
            CPUChunk chunk = new CPUChunk();

            chunk.MAGIC = br.ReadUInt32();
            chunk.VERSION = br.ReadUInt32();

            if (chunk.MAGIC != MAGIC)
            {
                GD.PushWarning("Unknown Magic! " + filepath);
                return null;
            }
            if (chunk.VERSION != VERSION)
            {
                GD.PushWarning("Unknown Version: " + chunk.VERSION.ToString() + "! " + filepath);
                return null;
            }

            fs.Seek(256, 0);

            // --- Texture List --- //
            chunk.texCount = br.ReadUInt32();
            chunk.texList = new string[chunk.texCount];
            fs.Seek(chunk.texCount * 4, SeekOrigin.Current);

            for (int i = 0; i < chunk.texCount; i++)
            {
                string text = "";
                while (true)
                {
                    char temp = br.ReadChar();
                    if (temp == 0) { break; }
                    text += temp;
                }
                chunk.texList[i] = text;
            }

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // --- Model Header --- //
            chunk.renderModelCount = br.ReadUInt32();
            chunk.cityobjectCount = br.ReadUInt32();
            chunk.modelBufferCount = br.ReadUInt32();
            chunk.unknownCount3 = br.ReadUInt32();
            chunk.unknownCount4 = br.ReadUInt32();
            fs.Seek(12, SeekOrigin.Current);

            // --- Unknown Viewmodel Related --- //
            // 24B
            fs.Seek(24 * chunk.renderModelCount, SeekOrigin.Current);

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // --- City Objects --- //
            chunk.cityObjects = new CityObject[chunk.cityobjectCount];
            // 96B
            for (int i = 0; i < chunk.cityobjectCount; i++)
            {
                CityObject temp = new CityObject();
                temp.pos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                fs.Seek(76, SeekOrigin.Current);
                temp.model = br.ReadUInt32();
                fs.Seek(4, SeekOrigin.Current);
                chunk.cityObjects[i] = temp;
            }

            // --- Unknown3 --- //
            fs.Seek(100 * chunk.unknownCount3, SeekOrigin.Current);

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // --- Unknown4 --- //
            fs.Seek(52 * chunk.unknownCount4, SeekOrigin.Current);

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // --- Unknown5 --- //
            chunk.unknownCount5 = br.ReadUInt32();
            // 12B
            fs.Seek(12 * chunk.unknownCount5, SeekOrigin.Current);

            // --- Unknown6 --- //
            chunk.unknownCount6 = br.ReadUInt32();
            // 3B
            fs.Seek(3 * chunk.unknownCount6, SeekOrigin.Current);

            // --- Unknown7 --- //
            chunk.unknownCount7 = br.ReadUInt32();
            // 4B
            fs.Seek(4 * chunk.unknownCount7, SeekOrigin.Current);

            // --- Unknown8 --- //
            chunk.unknownCount8 = br.ReadUInt32();
            // 12B
            fs.Seek(12 * chunk.unknownCount8, SeekOrigin.Current);

            // byte align
            while ((fs.Position & 0xf) != 0)
            {
                fs.Seek(1, SeekOrigin.Current);
            }

            // --- Havok Mopp Collision tree --- //
            chunk.moppSize = br.ReadUInt32();

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            fs.Seek(chunk.moppSize, SeekOrigin.Current);
            fs.Seek(24, SeekOrigin.Current);

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // --- Model Buffer Header --- //
            chunk.modelBuffers = new ModelBuffer[chunk.modelBufferCount];
            // 20B
            for (int i = 0; i < chunk.modelBufferCount; i++)
            {
                ModelBuffer temp = new ModelBuffer();
                temp.type = br.ReadUInt16();
                temp.vertBufferCount = br.ReadUInt16();
                temp.indexCount = br.ReadUInt32();
                fs.Seek(12, SeekOrigin.Current);
                chunk.modelBuffers[i] = temp;
            }
            // Vertex buffer header
            chunk.g_chunkVBufCount = 0;
            for (int i = 0; i < chunk.modelBufferCount; i++)
            {
                chunk.modelBuffers[i].vertBuffers = new VertBuffer[chunk.modelBuffers[i].vertBufferCount];
                // 16B
                for (int ii = 0; ii < chunk.modelBuffers[i].vertBufferCount; ii++)
                {
                    VertBuffer temp = new VertBuffer();
                    fs.Seek(2, SeekOrigin.Current);
                    temp.vertSize = br.ReadUInt16();
                    temp.vertCount = br.ReadUInt32();
                    fs.Seek(8, SeekOrigin.Current);
                    chunk.modelBuffers[i].vertBuffers[ii] = temp;

                    if (chunk.modelBuffers[i].type == 0)
                    {
                        chunk.g_chunkVBufCount += 1;
                    }
                }
            }
            chunk.g_chunkVBufCount = 0;
            for (int i = 0; i < chunk.modelBufferCount; i++)
            {
                if (chunk.modelBuffers[i].type == 0)
                {
                    chunk.g_chunkVBufCount += 1;
                }
            }

            // --- Physics Models --- //
            for (int i = 0; i < chunk.modelBufferCount; i++)
            {
                if (chunk.modelBuffers[i].type == 7)
                {
                    // Vertices
                    fs.Seek(12 * chunk.modelBuffers[i].vertBuffers[0].vertCount, SeekOrigin.Current);

                    // byte align
                    while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

                    // Indices
                    fs.Seek(2 * chunk.modelBuffers[i].indexCount, SeekOrigin.Current);

                    // byte align
                    while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }
                }
            }

            // --- Materials --- //
            chunk.materialCount = br.ReadUInt32();

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            chunk.shaderParamCount = br.ReadUInt32();
            fs.Seek(8, SeekOrigin.Current);
            chunk.matUnk2Count = br.ReadUInt32();
            fs.Seek(4, SeekOrigin.Current);

            chunk.materials = new Material[chunk.materialCount];
            // 24B
            for (int i = 0; i < chunk.materialCount; i++)
            {
                Material temp = new Material();
                fs.Seek(12, SeekOrigin.Current);
                temp.shaderFlagCount = br.ReadUInt16();
                temp.textureCount = br.ReadUInt16();
                fs.Seek(8, SeekOrigin.Current);
                chunk.materials[i] = temp;
            }

            // Bit flags, maybe? Messing with these toggled uv repeat on for ultor flags.
            for (int i = 0; i < chunk.materialCount; i++)
            {
                for (int ii = 0; ii < chunk.materials[i].shaderFlagCount; ii++)
                {
                    fs.Seek(6, SeekOrigin.Current);
                    if (chunk.materials[i].shaderFlagCount % 2 != 0)
                    {
                        fs.Seek(2, SeekOrigin.Current);
                    }
                }
            }

            // Messing with these break shaders.
            // 16B
            for (int i = 0; i < chunk.materialCount; i++)
            {
                fs.Seek(16, SeekOrigin.Current);
            }

            // byte align
            while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

            // Shader Floats
            // Mostly colors, sometimes affects scrolling texture speed and probably more things.
            // 4B single float
            for (int i = 0; i < chunk.shaderParamCount; i++)
            {
                fs.Seek(4, SeekOrigin.Current);
            }

            // Material Texture IDs
            // 64B
            for (int i = 0; i < chunk.materialCount; i++)
            {
                fs.Seek(64, SeekOrigin.Current);
            }

            // Material Unknown 2
            chunk.matUnk3Count = 0;
            // 16B
            for (int i = 0; i < chunk.matUnk2Count; i++)
            {
                fs.Seek(8, SeekOrigin.Current);
                chunk.matUnk3Count += br.ReadUInt16();
                fs.Seek(6, SeekOrigin.Current);
            }

            // Material Unknown 3
            // 4B
            for (int i = 0; i < chunk.matUnk3Count; i++)
            {
                fs.Seek(4, SeekOrigin.Current);
            }

            // --- Render Models --- //
            chunk.renderModels = new RenderModel[chunk.renderModelCount];
            GD.Print(chunk.renderModelCount);
            for (int i = 0; i < chunk.renderModelCount; i++)
            {
                RenderModel temp = new RenderModel();

                temp.bboxMin = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                temp.bboxMax = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                fs.Seek(8, SeekOrigin.Current);

                if (br.ReadUInt32() == 0) { temp.submeshCount = 0; }
                if (br.ReadUInt32() == 0) { temp.unk2_count = 0; }

                // byte align
                while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }

                // Get Submesh counts
                if (temp.submeshCount != 0)
                {
                    fs.Seek(2, SeekOrigin.Current);
                    temp.submeshCount = br.ReadUInt16();
                    fs.Seek(8, SeekOrigin.Current);
                    // byte align
                    while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }
                }
                if (temp.unk2_count != 0)
                {
                    fs.Seek(2, SeekOrigin.Current);
                    temp.unk2_count = br.ReadUInt16();
                    fs.Seek(8, SeekOrigin.Current);
                    // byte align
                    while ((fs.Position & 0xf) != 0) { fs.Seek(1, SeekOrigin.Current); }
                }

                // Get Submeshes
                temp.submeshes = new RenderModelSubmesh[temp.submeshCount];
                for (int ii = 0; ii < temp.submeshCount; ii++)
                {
                    RenderModelSubmesh tempSubmesh = new RenderModelSubmesh();
                    tempSubmesh.vertBuffer = br.ReadUInt32();
                    tempSubmesh.indexOffset = br.ReadUInt32();
                    tempSubmesh.vertOffset = br.ReadUInt32();
                    tempSubmesh.indexCount = br.ReadUInt16();
                    tempSubmesh.material = br.ReadUInt16();
                    temp.submeshes[ii] = tempSubmesh;
                }
                for (int ii = 0; ii < temp.unk2_count; ii++)
                {
                    fs.Seek(16, SeekOrigin.Current);
                }
                chunk.renderModels[i] = temp;
            }
            GD.Print("end: ", fs.Position.ToString("X"));


            return chunk;
        }
    }
    public void LoadGPUChunk(CPUChunk chunk, string filepath)
    {
        if (!System.IO.File.Exists(filepath))
        {
            GD.PushWarning("ChunkLoader.LoadGPUChunk(): File doesn't exist! " + filepath);
            return;// null;
        }

        using (FileStream fs = System.IO.File.OpenRead(filepath))
        {
            BinaryReader br = new BinaryReader(fs);

            for (int i = 0; i < chunk.renderModelCount; i++)
            {
                RenderModel model = chunk.renderModels[i];
                SurfaceTool st = new SurfaceTool();
                st.Begin(Mesh.PrimitiveType.Triangles);

                // Buffer Offsets
                uint iBufOffset = 0;
                uint[] vBufOffsets = new uint[chunk.modelBuffers[0].vertBufferCount];
                for (i = 0; i < chunk.modelBuffers[0].vertBufferCount; i++)
                {
                    vBufOffsets[i] = iBufOffset;
                    uint vertCount = chunk.modelBuffers[0].vertBuffers[i].vertCount;
                    uint vertSize = chunk.modelBuffers[0].vertBuffers[i].vertSize;
                    iBufOffset += vertCount * vertSize;
                    // byte align
                    while ((iBufOffset & 0xf) != 0) { iBufOffset += 1; }
                }

                // Buffers
                for (int ii = 0; ii < model.submeshCount; ii++)
                {
                    // Get vertCount
                    fs.Seek(iBufOffset + 2 * model.submeshes[ii].indexOffset, SeekOrigin.Begin);

                    uint tempVertCount = 0;
                    for (int iii = 0; iii < model.submeshes[ii].indexCount; iii++)
                    {
                        tempVertCount = (uint)Math.Max(br.ReadInt16(), tempVertCount);
                    }
                    model.submeshes[ii].vertCount = tempVertCount;

                    // Get Indices
                    fs.Seek(iBufOffset + 2 * model.submeshes[ii].indexOffset, SeekOrigin.Begin);
                    for (int iii = 0; iii < model.submeshes[ii].indexCount - 2; iii++)
                    {
                        uint i0 = br.ReadUInt16();
                        uint i1 = br.ReadUInt16();
                        uint i2 = br.ReadUInt16();
                        if ((iii % 2) == 0)
                        {
                            st.AddIndex((int)i0);
                            st.AddIndex((int)i1);
                            st.AddIndex((int)i2);
                        }
                        else
                        {
                            st.AddIndex((int)i2);
                            st.AddIndex((int)i1);
                            st.AddIndex((int)i0);
                        }
                    }
                    GD.Print("got here");

                    // Get Vertices
                    uint vertBufID = model.submeshes[ii].vertBuffer;
                    uint vertSize = chunk.modelBuffers[0].vertBuffers[vertBufID].vertSize;
                    fs.Seek(vBufOffsets[vertBufID] + model.submeshes[ii].vertOffset, SeekOrigin.Begin);
                    for (int iii = 0; iii < model.submeshes[ii].vertCount; iii++)
                    {
                        Vector3 pos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        GD.Print(pos);
                        st.AddVertex(pos);
                        fs.Seek(vertSize - 12, SeekOrigin.Current);
                    }
                }

                Mesh mesh = st.Commit();

                MeshInstance meshInstance = new MeshInstance();
                meshInstance.Mesh = mesh;
                meshInstance.Name = i.ToString();
                GetNode("/root/main/world").AddChild(meshInstance);
                return;
            }

            //for (int i = 0; i < chunk.modelBuffers[0].vertBufferCount; i++) {
            //    VertBuffer vbuf = chunk.modelBuffers[0].vertBuffers[i];
            //    for (int ii = 0; ii < vbuf.vertCount; ii++) {
            //    
            //}
            //}
        }
    }
}
