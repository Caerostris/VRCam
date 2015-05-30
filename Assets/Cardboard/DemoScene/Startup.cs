using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections;

public class Startup : MonoBehaviour {
	public static Config config = null;

	public void Start () {
		// read config or create if it does not exist
		Config config;
		try {
			StreamReader r = new StreamReader (Application.persistentDataPath + "/config.json");
			string json = r.ReadToEnd ();
			config = JsonConvert.DeserializeObject<Config> (json);
		} catch(FileNotFoundException exc) {
			Debug.Log ("Config not found. Creating. " + exc.ToString ());
			config = Startup.createConfig ();
		}

		// update model
		StartCoroutine(LoadBundle(config.model.assetURL, 1));

		Startup.config = config;
	}

	public IEnumerator LoadBundle(string url, int version) {
		using(WWW www = WWW.LoadFromCacheOrDownload(url, version)) {
			yield return www;

			AssetBundle assetBundle = www.assetBundle;
			GameObject gameObject = assetBundle.LoadAsset<GameObject> 	("robot");
			gameObject = Instantiate(gameObject);

			gameObject.name = "Object";
			gameObject.transform.eulerAngles = new Vector3(config.model.rotationX, config.model.rotationY, config.model.rotationZ);
			gameObject.transform.localScale = new Vector3 (config.model.scaleX, config.model.scaleY, config.model.scaleZ);
			gameObject.transform.parent = GameObject.Find ("Screen").transform;
			gameObject.transform.localPosition = new Vector3 (0, 0, config.model.distance);

			Startup.config.currentObject = gameObject;
			assetBundle.Unload(false);
		}
	}

	public static Config createConfig() {
		Config config = new Config ();
		config.analysis = new AnalysisConfig ();
		config.model = new ModelConfig ();

		config.model.assetURL = "file://" + Application.dataPath + "/object.unity3d"; // 3D model file path
		config.model.scaleX = 0.0015f; // scaling
		config.model.scaleY = 0.0015f; // scaling
		config.model.scaleZ = 0.0015f; // scaling
		config.model.distance = 3f; // distance from screen
		config.model.rotationX = 0;
		config.model.rotationY = 0;
		config.model.rotationZ = 0;

		config.analysis.markerColor = new Color32 (235, 125, 35, 1); // orange
		config.analysis.deviation = 80;
		config.analysis.scaleFactor = 0.15f;
		config.analysis.binaryThreshold = 20;
		config.analysis.edgeScale = 3;
		config.analysis.markerMinWidth = 5;
		config.analysis.markerMinHeight = 5;

		string json = JsonConvert.SerializeObject (config, Formatting.Indented);
		StreamWriter w = new StreamWriter (Application.persistentDataPath + "/config.json");
		w.Write (json);
		w.Close ();

		return config;
	}
}

public class Config {
	public AnalysisConfig analysis;
	public ModelConfig model;
	public GameObject currentObject = null;
}

public class ModelConfig {
	public string assetURL;
	public float scaleX;
	public float scaleY;
	public float scaleZ;
	public float distance;
	public float rotationX;
	public float rotationY;
	public float rotationZ;
}

public class AnalysisConfig {
	public Color32 markerColor;
	public byte deviation;
	public float scaleFactor;
	public byte binaryThreshold;
	public int edgeScale;
	public int markerMinWidth;
	public int markerMinHeight;
}