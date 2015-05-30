// from http://docs.unity3d.com/Manual/BuildingAssetBundles5x.html
#if UNITY_EDITOR
using UnityEditor;

public class CreateAssetBundles
{
	[MenuItem ("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles ()
	{
		BuildPipeline.BuildAssetBundles ("AssetBundles");
	}
}
#endif