using UnityEngine;
using System.Collections;

public class AutoWaypointTester : MonoBehaviour {
	public Transform destination;
	public static float ENDPOINT_RADIUS=2.0f;
	private ArrayList path=null;
	
	[ContextMenu ("Rebuild Waypoint Paths")]
	void RebuildWaypointPaths() {
		PathFinder.ConnectAllWaypoints();
	}
	
	[ContextMenu ("Find path to destination")]
	void FindPath() {
		if (destination!=null) {
			path = PathFinder.AStar(this.gameObject, destination.gameObject);
			//if (path==null) { UnityEngine.Debug.Log("Path could not be found from :" + transform.position + " to:" + destination.position); }
		} else {
			UnityEngine.Debug.Log("Please drag a gameobject into destination.");
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;		
		
		if (destination!=null) {
			Vector3 src=transform.position;
			FindPath();
			if (path!=null) {
				//Debug.Log("Path was: " + path.Count);
				Vector3 dst;			
				for (int i=0; (i<path.Count); ++i) {
					dst = ((AutoWaypoint)path[i]).transform.position;
					Gizmos.color = Color.blue; // Blue
					Gizmos.DrawLine(src, dst);					
					src.x+=0.01f;
					dst.z+=0.01f;
					Gizmos.DrawLine(src, dst);
					src.x+=0.01f;
					dst.z+=0.01f;
					Gizmos.DrawLine(src, dst);
					src.y+=0.01f;
					dst.y+=0.01f;
					Gizmos.DrawLine(src, dst);
					src=((AutoWaypoint)path[i]).transform.position;
				}
				Gizmos.DrawLine(src, destination.position);
			}
			
			// Draw endpoints in proper color (blue is good, red means we couldn't make it):
			Gizmos.color=Color.green;
			Gizmos.DrawWireSphere(transform.position, ENDPOINT_RADIUS);
			Gizmos.color = (path!=null && path.Count>0) ? Color.blue : Color.red;			
			Gizmos.DrawWireSphere(destination.position, ENDPOINT_RADIUS);
		} else {
			Gizmos.color=Color.yellow;
			Gizmos.DrawWireSphere(transform.position, ENDPOINT_RADIUS);
		}
		
	}
}
