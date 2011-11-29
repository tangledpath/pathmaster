using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

// Make this static:
public class AutoWaypointOptions : MonoBehaviour {	
	private static AutoWaypointOptions instance;
	
	public Color waypointColor = new Color(0.3f, 0, 0.4f, 1.0f); // a Nice purple
	public Color selectedWaypointColor = Color.cyan;
	public Color connectorColor = new Color(0.0f, 0.3f, 0.01f, 1.0f); // a Nice green
	public Color badConnectorColor = new Color(0.3f, 0.0f, 0.01f, 1.0f); // a Nice red	
	public bool  drawConnectors = true;
	public string lastWaypointFind="nada";
	public string[] ignoreLayers = new string[]{};
	private int layerMask = 0xff;
	
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
	
	public int PathLayerMask() {
		if (layerMask==-1) {
			// Calculate once, as we are starting from strings:
			int mask=0;
			foreach (string layerName in ignoreLayers) {
				mask = mask | (1 << LayerMask.NameToLayer(layerName));
			}
			layerMask = ~mask;
			Debug.Log("Layer mask was: " + layerMask);
		}
		return layerMask;
	}
	
	[ContextMenu ("Rebuild Waypoint Paths")]
	public void RebuildWaypointPaths() {
		layerMask=-1; // Effect reset
		PathFinder.ConnectAllWaypoints();
	}
	
	[ContextMenu ("Add Waypoint")]
	void AddWaypoint() {
		
		AutoWaypoint[] waypoints = PathFinder.AllWayPoints();
		string waypointName = "Waypoint XXX";
		for (int i=0; i<1000; ++i) {
			waypointName = String.Format("Waypoint{0:0##}", i);
			bool found = false;
			foreach (AutoWaypoint wp in waypoints) {
				if (wp.name==waypointName) {
					found = true;
					break;					
				}
			}
			if (!found) {
				break;
			}
		}
		
		GameObject waypoint = new GameObject(waypointName, new Type[]{typeof(AutoWaypoint)});		
		waypoint.transform.position = new Vector3(waypoint.transform.position.x, 0.0f, waypoint.transform.position.z);
		waypoint.transform.parent=this.transform;
		UnityEditor.Selection.activeGameObject=waypoint;
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
