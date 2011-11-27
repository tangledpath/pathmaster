using UnityEngine;
using System.Collections;

public class AutoWaypoint : MonoBehaviour {
	// True to always show connectors.  Otherwise, show when selected:
	private const float WAYPOINT_SIDE_LEN = 0.25f;
	private const float RAYCASTCHECKRADIUS = 0.25f;	
	
	
	
	// All connected waypoints:	
	public AutoWaypoint[] waypoints;
	// Same number of elements as above.  Contains distances 	
	public IDictionary waypointDistances; 
	private bool noPointsWarned=false;
	
	// Use this for initialization
	void Start () {
		RebuildWaypointPaths();
		noPointsWarned=false;
	}
	
	void Awake() {
		RebuildWaypointPaths();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	[ContextMenu ("Rebuild Waypoint Paths")]
	void RebuildWaypointPaths() {
		PathFinder.ConnectAllWaypoints();
	}
	
	void OnDrawGizmos() {
		DrawGizmos(AutoWaypointOptions.Instance.waypointColor);
	}
	
	void OnDrawGizmosSelected () {
		// Draw connectors unless we already draw them:
		DrawGizmos(AutoWaypointOptions.Instance.selectedWaypointColor);
		if (AutoWaypointOptions.Instance.drawConnectors) { DrawConnectors(); }
	}		   
	
	private void DrawGizmos(Color waypointColor) {		
		Gizmos.color = waypointColor;
		Vector3 pos = transform.position;
		
		// Draw a square around waypoint:
		Vector3 fromVec = new Vector3(pos.x-WAYPOINT_SIDE_LEN, pos.y, pos.z+WAYPOINT_SIDE_LEN);
		Vector3 toVec = new Vector3(pos.x+WAYPOINT_SIDE_LEN, pos.y, pos.z+WAYPOINT_SIDE_LEN);
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.z = pos.z-WAYPOINT_SIDE_LEN;
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.x = pos.x-WAYPOINT_SIDE_LEN;
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.z = pos.z+WAYPOINT_SIDE_LEN;
		Gizmos.DrawLine(fromVec, toVec);
		
		// Draw a diagonal square around waypoint:
		Vector2 diag=new Vector2(WAYPOINT_SIDE_LEN, WAYPOINT_SIDE_LEN);
		float wpDiag = diag.magnitude;
		fromVec = new Vector3(pos.x-wpDiag, pos.y, pos.z);
		toVec = new Vector3(pos.x, pos.y, pos.z+wpDiag);
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.x=pos.x+wpDiag;
		toVec.z = pos.z;
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.x = pos.x;
		toVec.z = pos.z-wpDiag;
		Gizmos.DrawLine(fromVec, toVec);
		
		fromVec=toVec;
		toVec.x = pos.x-wpDiag;
		toVec.z = pos.z;
		Gizmos.DrawLine(fromVec, toVec);		
		
		// Draw a sphere at center.  Make it look good:
		Gizmos.DrawWireSphere(transform.position, WAYPOINT_SIDE_LEN/2);
		if (AutoWaypointOptions.Instance.drawConnectors) { DrawConnectors(); }
	}
	
	private void DrawConnectors() {
		if (waypoints.Length==0) { PathFinder.ConnectAllWaypoints(); }
		if (waypoints.Length==0 && !noPointsWarned) { 
			noPointsWarned=true;
		}
		
		try {
			foreach (AutoWaypoint wp in waypoints) {
				if (wp==null) {
					Debug.Log("A waypoint was removed...rebuilding.");
					PathFinder.ConnectAllWaypoints();
					return;
				}
				Gizmos.color = (CanSee(wp.gameObject)) ? AutoWaypointOptions.Instance.connectorColor : AutoWaypointOptions.Instance.badConnectorColor;
				Gizmos.DrawLine(transform.position, wp.transform.position);
			}
		} catch (MissingReferenceException) {
			Debug.Log("A waypoint was removed...rebuilding.");
			PathFinder.ConnectAllWaypoints();
		}
	}

	public void ConnectWaypoint(AutoWaypoint[] allPoints) {
		ArrayList found = new ArrayList(10);
		foreach (AutoWaypoint wp in allPoints) {
			if (wp==this) continue;  // Don't connect to me!
			if (CanSee(wp.gameObject)) {
				found.Add(wp);
			}
		}
		
		waypointDistances=new Hashtable(found.Count);
		foreach (AutoWaypoint f in found) {
			waypointDistances[f] = (transform.position - f.transform.position).magnitude;
		}
		found.Sort(new WaypointComparer(this));
		
		waypoints=(AutoWaypoint[])found.ToArray(typeof(AutoWaypoint));		
	}		
	
	public bool CanSee(GameObject obj) {
		// A simple check for non-collider objects (which includes waypoints):
		if (CanSeePos(obj.transform.position)) {
			return true;
		}
		
		// A more complicated check for collider objects.  The simple check above
		// won't work in some cases since the collider blocks the line-of-sight:
		RaycastHit[] hits;
		Vector3 dir = (obj.transform.position-this.transform.position).normalized;
		
		hits = Physics.RaycastAll(this.transform.position, dir, Mathf.Infinity);
		return (hits.Length > 0 && GetClosestHit(hits).transform==obj.transform);		
	}
	
	
	// Get closest transform (to `this` object) of hit amongst given hits:
	private RaycastHit GetClosestHit(RaycastHit[] hits) {
		float closest = Mathf.Infinity;
		RaycastHit closestHit=new RaycastHit();
		foreach(RaycastHit h in hits) {
			if ((h.transform.position - this.transform.position).sqrMagnitude < closest) {
				closestHit = h;
			}
		}
		return closestHit;
	}
	
	private bool CanSeePos(Vector3 pos) {
		Vector3 p1 = new Vector3(pos.x, pos.y+RAYCASTCHECKRADIUS+0.1f, pos.z);
		Vector3 p2 = new Vector3(transform.position.x, transform.position.y+RAYCASTCHECKRADIUS+0.1f, transform.position.z);
		return !Physics.CheckCapsule(p1, p2, RAYCASTCHECKRADIUS);
	}	
}

