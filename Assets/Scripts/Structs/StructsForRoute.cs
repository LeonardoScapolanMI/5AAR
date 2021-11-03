using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct loc {
	public float lat;
	public float lng;
}
[System.Serializable]
public struct distance {
	public string text;
	public int value;
}

[System.Serializable]
public struct duration {
	public string text;
	public int value;
}
[System.Serializable]
public struct step {
	public loc end_location;
	public loc start_location;
	public string travel_mode;
	public distance dis;
	public duration dur;
	public string maneuver;
}
[System.Serializable]
public struct leg {
	public distance dis;
	public duration dur;
	public string end_address;
	public loc end_location;
	public string start_address;
	public loc start_location;
	public List<step> steps;
}
[System.Serializable]
public struct route {
	public List<leg> legs;
	public string warnings;
}
[System.Serializable]
public struct geoCoded {
	public List<route> routes;
}