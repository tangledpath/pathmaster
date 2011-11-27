using UnityEngine;
using System.Collections;

// Make this static:
public class AutoWaypointOptions : MonoBehaviour {	
	private static AutoWaypointOptions instance;
	
	public Color waypointColor = new Color(0.3f, 0, 0.4f, 1.0f); // a Nice purple
	public Color connectorColor = new Color(0.0f, 0.3f, 0.01f, 1.0f); // a Nice green
	public Color badConnectorColor = new Color(0.3f, 0.0f, 0.01f, 1.0f); // a Nice red	
	public bool  drawConnectors = true;
	public string lastWaypointFind="nada";
	public AutoWaypoint[] allWaypoints;
	
	public static AutoWaypointOptions Instance { 
		get {
			if (instance==null) {
				instance=(AutoWaypointOptions)FindObjectOfType(typeof(AutoWaypointOptions));
			}
            if (instance == null)
            {
                Debug.Log("instantiate");
                GameObject go = new GameObject();
                instance = go.AddComponent<AutoWaypointOptions>();
                go.name = "AutoWayPointOptions";
            }

            return instance; 
        } 
    }
	
	[ContextMenu ("Rebuild Waypoint Paths")]
	void RebuildWaypointPaths() {
		AutoWaypoint.ConnectAllWaypoints();
	}
	
    void Awake(){
        instance=(AutoWaypointOptions)FindObjectOfType(typeof(AutoWaypointOptions));
    }
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
