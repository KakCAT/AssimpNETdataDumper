using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;								// note: install AssimpNET 4.1 via nuget



namespace assimpDataDump
{
static class Program
{
static List<Node> assimpNodes;

//=============================================================================
/// <summary></summary>
static void Main(string[] args)
{
	if (args.Length!=1) { Console.WriteLine ("assimpDataDump <model-file>"); return; } 

	// load model
	Scene scene=loadAssimpFile (args[0]);
	if (scene==null) { Console.WriteLine ($"Couldn't load {args[0]}"); return; }

	// dump data
	dumpAssimpData (scene);
}

//=============================================================================
/// <summary>Loads a model into assimp</summary>
static Scene loadAssimpFile(string fn)
{
	try
	{
		var importer = new AssimpContext();
		Scene scene=importer.ImportFile (fn,
								  PostProcessSteps.FlipUVs					 // So far appears necessary
								| PostProcessSteps.JoinIdenticalVertices
								| PostProcessSteps.Triangulate
								| PostProcessSteps.FindInvalidData 
								| PostProcessSteps.ImproveCacheLocality
								| PostProcessSteps.FindDegenerates 
								| PostProcessSteps.OptimizeMeshes 
								| PostProcessSteps.RemoveRedundantMaterials
								);

		return scene;
	}
	catch (Exception e)
	{
		Console.WriteLine (e.Message);
		return null;
	}
}

//=============================================================================
/// <summary>Simple example to get data from an assimp scene</summary>
static void dumpAssimpData (Scene scene)
{
	// create a flattened node list + print hierarchy
	assimpNodes=new List<Node> ();
	dumpNodesRecursive (scene.RootNode,0);

	printNodeData (assimpNodes);

	printMaterialData (scene);

	printMeshData (scene);

	printAnimData (scene);
}

//=============================================================================
/// <summary></summary>
static string tabify (int nTabs)
{
int i;
string str="";

	for (i=0;i<nTabs;i++) str+="\t";
	return str;
}

//=============================================================================
/// <summary></summary>
static void dumpNodesRecursive (Node node,int tabLevel)
{
string str="";

	assimpNodes.Add (node);

	str+=tabify (tabLevel) + node.Name;

	Console.WriteLine (str);

	foreach (var child in node.Children) dumpNodesRecursive (child,tabLevel+1);
}

//=============================================================================
/// <summary></summary>
static void printNodeData (List<Node> nodeList)
{
string str;

	foreach (var node in nodeList)
	{
		str=node.Name+"\n";
		str+=String.Format ("  transform: {0}\n",node.Transform.ToString());
		str+=String.Format ("  nChilds: {0}\n",node.ChildCount);
		str+=String.Format ("  nMeshes: {0} : ",node.MeshCount);

		foreach (var nMesh in node.MeshIndices)
		{
			str+=$"#{nMesh} ";
		}
		str+="\n\n";

		Console.WriteLine (str);
	}
}


//=============================================================================
/// <summary></summary>
static void printMaterialData (Scene scene)
{

	Console.WriteLine ("\n==== Materials ====\n\n");

	for (int i=0;i<scene.MaterialCount;i++)
	{
		var material=scene.Materials[i];

		Console.WriteLine ($"Material #{i} name: {material.Name}");

		var textures=material.GetAllMaterialTextures ();
		foreach (var tex in textures)
		{
			Console.WriteLine ($"  Texture {tex.TextureType.ToString()} {tex.FilePath}  ");
		}
	}
}

//=============================================================================
/// <summary></summary>
static void printMeshData (Scene scene)
{
	Console.WriteLine ("\n==== Meshes ====\n\n");

	for (int i=0;i<scene.MeshCount;i++)
	{
		var mesh=scene.Meshes[i];

		Console.WriteLine ($"Mesh #{i} name: {mesh.Name}");
		Console.WriteLine ($"  vertices: {mesh.VertexCount}");		// access via mesh.Vertices[0..N]
		Console.WriteLine ($"  normals: {mesh.Normals.Count}");		// access via mesh.Normals[0..N]
		Console.WriteLine ($"  faces: {mesh.FaceCount}");			// ""
		Console.WriteLine ($"  uv:");								// access mesh.TextureCoordinateChannels[#uvChannel]
		for (int j=0;j<mesh.TextureCoordinateChannelCount;j++)
		{
			Console.WriteLine ($"    type: {mesh.UVComponentCount[j]}D");
			Console.WriteLine ($"    elements: {mesh.TextureCoordinateChannels[j].Count}");
		}
		Console.WriteLine ($"  vertexColorChannelCount: {mesh.VertexColorChannelCount}");
		Console.WriteLine ($"  bones: {mesh.BoneCount}");
	}
}

//=============================================================================
/// <summary></summary>
static void printAnimData (Scene scene)
{
int i;

	Console.WriteLine ("\n==== Anim data ====\n\n");

	for (i=0;i<scene.Animations.Count;i++)
	{
		var anim=scene.Animations[i];
		Console.WriteLine ($"Anim #{i} #{anim.Name}");
		Console.WriteLine ($"  Duration: {anim.DurationInTicks} / {anim.TicksPerSecond} sec.");

		Console.WriteLine ($"  Node Channels: {anim.NodeAnimationChannelCount}");

		foreach (var chan in anim.NodeAnimationChannels)
		{
			Console.WriteLine ($"  Channel {chan.NodeName}");		// the node name has to be used to tie this channel to the originally printed hierarchy. For some reason. BTW, node names must be unique.
			Console.WriteLine ($"    Position Keys: {chan.PositionKeyCount}");		// access via chan.PositionKeys
			Console.WriteLine ($"    Rotation Keys: {chan.RotationKeyCount}");		// 
			Console.WriteLine ($"    Scaling  Keys: {chan.ScalingKeyCount}");		// 
		}
	}
}

}
}
