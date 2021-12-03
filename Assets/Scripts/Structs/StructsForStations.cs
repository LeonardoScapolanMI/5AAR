using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct location {
	public float lat;
	public float lng;
}
[System.Serializable]
public struct viewport {
	public location northeast;
	public location southwest;
}
[System.Serializable]
public struct geometry {
	public location location;
	public viewport viewport;
}
[System.Serializable]
public struct result {
	public geometry geometry;
	public string name;
}
[System.Serializable]
public struct attr {
	
}
[System.Serializable]
public struct attribution {
	public List<attr> attrs;
	public List<result> results;
}