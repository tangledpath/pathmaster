using UnityEngine;
using System.Collections;

public class AutoWaypoint : MonoBehaviour {
	// True to always show connectors.  Otherwise, show when selected:
	private const float WAYPOINT_SIDE_LEN = 0.25f;
	private const float RAYCASTCHECKRADIUS = 0.25f;	
	private static float A_STAR_GOAL_TOLERANCE_SQR=10000; // 100 meters
	
	
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
		ConnectAllWaypoints();
	}
	
	void OnDrawGizmos() {
		DrawGizmos(AutoWaypointOptions.Instance.waypointColor);
	}
	
	void OnDrawGizmosSelected () {
		// Draw connectors unless we already draw them:
		//Gizmos.color=Color.cyan;
		DrawGizmos(Color.cyan);
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
		if (waypoints.Length==0) { ConnectAllWaypoints(); }
		if (waypoints.Length==0 && !noPointsWarned) { 
			noPointsWarned=true;
		}
		
		try {
			foreach (AutoWaypoint wp in waypoints) {
				if (wp==null) {
					Debug.Log("A waypoint was removed...rebuilding.");
					ConnectAllWaypoints();
					return;
				}
				Gizmos.color = (CanSee(wp.gameObject)) ? AutoWaypointOptions.Instance.connectorColor : AutoWaypointOptions.Instance.badConnectorColor;
				Gizmos.DrawLine(transform.position, wp.transform.position);
			}
		} catch (MissingReferenceException x) {
			Debug.Log("A waypoint was removed...rebuilding.");
			ConnectAllWaypoints();
		}
	}

	static public void ConnectAllWaypoints() {
		AutoWaypoint[] points = AllWayPoints();
		AutoWaypointOptions.Instance.allWaypoints= points; // TODO; DELETE THIS
		foreach (AutoWaypoint wp in points) {
			wp.ConnectWaypoint(points);
		}	    
	}
	
	private void ConnectWaypoint(AutoWaypoint[] allPoints) {
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
	
	private bool CanSee(GameObject obj) {
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
	
	private bool CanSeePos(Vector3 pos) {
		Vector3 p1 = new Vector3(pos.x, pos.y+RAYCASTCHECKRADIUS+0.1f, pos.z);
		Vector3 p2 = new Vector3(transform.position.x, transform.position.y+RAYCASTCHECKRADIUS+0.1f, transform.position.z);
		return !Physics.CheckCapsule(p1, p2, RAYCASTCHECKRADIUS);
	}
	
	// Compare two waypoints, identifying the lesser one
	// as closer.  Does not support null waypoints:
	public class WaypointComparer : System.Collections.IComparer  {
		private AutoWaypoint srcPnt;
		private float dist1; // To avoid many allocations/deletes
		private float dist2; // To avoid many allocations/deletes
		public WaypointComparer(AutoWaypoint sourcePoint) {
			srcPnt=sourcePoint;			
		}
			
	    public int Compare(object x, object y) {
			dist1=(float)srcPnt.waypointDistances[x];// this.   (((Waypoint) y).transform.position-srcPos).sqrMagnitude;
			dist2=(float)srcPnt.waypointDistances[y];// (((Waypoint) y).transform.position-srcPos).sqrMagnitude;
			if (dist1 < dist2) {
				return -1;
			} else if (dist2 > dist1) {
				return 1;
			} else {
				return -1;
			}
	    }
   }
	
	public static AutoWaypoint[] AllWayPoints() {
		return FindObjectsOfType(typeof(AutoWaypoint)) as AutoWaypoint[];
	}
	
	public static AutoWaypoint ClosestWaypoint(Vector3 goal) {
		AutoWaypoint closestPnt=null;
		float closestDist = float.MaxValue;
		float dist;
		AutoWaypoint[] allPnts=AllWayPoints();
		for (int i=0; i<allPnts.Length; ++i) {
			dist = (allPnts[i].transform.position-goal).sqrMagnitude;
			if (dist < closestDist) {
				closestDist = dist;
				closestPnt = allPnts[i];
			}
		}
		return closestPnt;
	}
	
	public static ArrayList AStar(GameObject start, GameObject goal) {
		//UnityEngine.Debug.Log("Finding path from " + start.transform.position.ToString() + " to " + goal.ToString());
		Vector3 startPos = start.transform.position;
		AutoWaypoint.ConnectAllWaypoints();
		AutoWaypoint[] all= AllWayPoints();
		float tm = Time.realtimeSinceStartup;
		ArrayList closedSet = new ArrayList(all.Length);
		ArrayList openSet = new ArrayList(all.Length);
		AutoWaypoint startWaypoint = ClosestWaypoint(startPos);
		if (startWaypoint==null) {
			Debug.LogWarning("No waypoints!");
			return new ArrayList();
		}
		
		openSet.Add(startWaypoint);
		IDictionary cameFrom = new Hashtable(20, 0.75f);
		IDictionary g_score = new Hashtable(20, 0.75f);
		IDictionary h_score = new Hashtable(20, 0.75f);
		IDictionary f_score = new Hashtable(20, 0.75f);
		g_score[startWaypoint] = 0.0f;
		h_score[startWaypoint] = heuristicCostEstimate(startPos, goal.transform.position);
		
		while(openSet.Count !=0) {
			AutoWaypoint candidate=(AutoWaypoint)openSet[0];
			if (((candidate.transform.position - goal.transform.position).sqrMagnitude <= A_STAR_GOAL_TOLERANCE_SQR) && candidate.CanSee(goal)) {
				// Pretty close & can see:
				AutoWaypointOptions.Instance.lastWaypointFind = "" + (Time.realtimeSinceStartup-tm);
				return ReconstructPath(cameFrom, candidate);
			}
			
			// Transfer waypoint to closed set:
			closedSet.Add(candidate);
			openSet.RemoveAt(0);
			
			// Check neighbors (connected waypoints):
			AutoWaypoint neighborCandidate;
			float tentativeGScore;
			bool tentativeIsBetter;
			for (int i=0; i<candidate.waypoints.Length; ++i) {
				neighborCandidate = candidate.waypoints[i];
				if (closedSet.Contains(neighborCandidate)) {
					continue;
				}
				
				// Calc scores & compare:
				if (candidate.waypointDistances==null) { UnityEngine.Debug.Log(candidate + " has no distances."); }
				tentativeGScore = ((g_score[candidate]==null) ? 0.0f : (float)g_score[candidate]);
				tentativeGScore +=  (float)(candidate.waypointDistances[neighborCandidate]);						
					
				if (!openSet.Contains(neighborCandidate)) {
					openSet.Add(neighborCandidate);
					tentativeIsBetter=true;
				} else if (tentativeGScore < ((g_score[neighborCandidate]==null) ? 0.0f : (float)g_score[neighborCandidate])) {
					tentativeIsBetter=true;
				} else {
					tentativeIsBetter=false;
				}
				
				if (tentativeIsBetter) {
					cameFrom[neighborCandidate] = candidate;
					g_score[neighborCandidate] = tentativeGScore;
					h_score[neighborCandidate] = heuristicCostEstimate(neighborCandidate.transform.position, goal.transform.position);
					f_score[neighborCandidate] = ((g_score[neighborCandidate]==null) ? 0.0f : (float)g_score[neighborCandidate]) 
											   + ((h_score[neighborCandidate]==null) ? 0.0f : (float)h_score[neighborCandidate]);
				}
			}
		}	
		AutoWaypointOptions.Instance.lastWaypointFind = "" + (Time.realtimeSinceStartup-tm);
		return null;
	}
	
	public static ArrayList ReconstructPath(IDictionary cameFrom, AutoWaypoint currentNode) {
		ArrayList path;
		if (cameFrom.Contains(currentNode)) {
			path = ReconstructPath(cameFrom, (AutoWaypoint)cameFrom[currentNode]);
			path.Add(currentNode);			
		} else {
			path = new ArrayList(10);
			path.Add(currentNode);			
		}
		return path;		
	}
	
	private static float heuristicCostEstimate(Vector3 start, Vector3 end) {
		return (start-end).magnitude;
	}
}

