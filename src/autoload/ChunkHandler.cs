using Godot;
using System;
using System.IO;

public class ChunkHandler : Node
{
    ChunkLoader chunkLoader;

    public override void _Ready()
    {
        chunkLoader = GetNode("/root/ChunkLoader") as ChunkLoader;
    }

    public void LoadChunk(string input_filepath)
    {
        string cpu_chunk_filepath = "";
        string gpu_chunk_filepath = "";
        // string peg_filepath;

        GD.Print(input_filepath);

        // File Exist
        if (!System.IO.File.Exists(input_filepath))
        {
            GD.Print("Error: Input File Doesn't Exist! " + input_filepath);
            return;
        }

        // File Extension
        if (System.IO.Path.GetExtension(input_filepath) == ".chunk_pc")
        {
            cpu_chunk_filepath = input_filepath;
            gpu_chunk_filepath = System.IO.Path.ChangeExtension(input_filepath, ".g_chunk_pc");
        }
        else if (System.IO.Path.GetExtension(input_filepath) == ".g_chunk_pc")
        {
            cpu_chunk_filepath = System.IO.Path.ChangeExtension(input_filepath, ".chunk_pc");
            gpu_chunk_filepath = input_filepath;
        }
        else if (System.IO.Path.GetExtension(input_filepath) == ".g_peg_pc")
        {
            cpu_chunk_filepath = System.IO.Path.ChangeExtension(input_filepath, ".chunk_pc");
            gpu_chunk_filepath = System.IO.Path.ChangeExtension(input_filepath, ".g_chunk_pc");
        }
        else
        {
            GD.Print("Error: Unknown extension!");
            return;
        }

        // Chunkfile Exist
        if (!System.IO.File.Exists(cpu_chunk_filepath))
        {
            GD.Print("Error: " + cpu_chunk_filepath + " doesn't exist!");
            return;
        }
        if (!System.IO.File.Exists(gpu_chunk_filepath))
        {
            GD.Print("Error: " + gpu_chunk_filepath + " doesn't exist!");
            return;
        }

        CPUChunk chunk = chunkLoader.LoadChunk(cpu_chunk_filepath);
        if (chunk != null)
        {
            chunkLoader.LoadGPUChunk(chunk, gpu_chunk_filepath);
			//ImportChunkToScene(chunk);
        }
        else
        {
            GD.PushWarning("ChunkLoader returned null.");
        }
    }

    public void ImportChunkToScene(CPUChunk chunk)
    {
        Node world = GetNode("/root/main/world");

        for (int i = 0; i < chunk.cityobjectCount; i++)
        {
            CityObject temp = chunk.cityObjects[i];
            CityObjectNode cityObjectNode = new CityObjectNode();
            cityObjectNode.Translation = temp.pos;
            cityObjectNode.SetModel(temp.model);
            world.AddChild(cityObjectNode);
        }
    }
}
