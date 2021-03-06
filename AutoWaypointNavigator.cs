using UnityEngine;
using System.Collections;

// Navigates among specified waypoints:
public class AutoWaypointNavigator : MonoBehaviour {
	public ArrayList waypoints=new ArrayList(20);
	public GameObject currentTarget=null;//Vector3.zero;
	public float updateInterval=0.4f;
	public bool activated=false;
	public float rotationSpeed = 0.5f;
	public float speed = 4.0f;
	GameObject lastCalcedTarget=null;//Vector3.zero;
	ArrayList currentPath;
	// This is set initially to false so our behaviour script can turn it on at will:
	static float TARGET_CLOSE_DISTANCE_SQR = 1.0f;
	//private static float SEARCH_TIMEOUT = 30.0f;
	//private float searchTime=0.0f;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
		if (activated && currentTarget != null) {
		 	//lastUpdate=Time.time;
			if (lastCalcedTarget==currentTarget) {
				// We've already calculated a path for current target.
				if (IsTargetClose(currentTarget.transform)) {
					// We are here:
					this.rigidbody.velocity=Vector3.zero;
				} else if (currentPath.Count==0) {
					this.rigidbody.velocity=(currentTarget.transform.position-this.transform.position).normalized * speed;
				} else if (IsTargetClose(((AutoWaypoint)currentPath[0]).transform)) {
					currentPath.RemoveAt(0);
				} else {
					this.rigidbody.velocity=(((AutoWaypoint)currentPath[0]).transform.position-this.transform.position).normalized * speed;
				}
			} else {
				//Debug.Log("Calculating path.");
				// We need to calculate a path:
				this.rigidbody.velocity=Vector3.zero;
				CalculatePathToTarget(currentTarget);
			}
		} else {
			// Not activated / no target:
			this.rigidbody.velocity=Vector3.zero;
		}
	}

	// Calculates path to target & repurposes navigator if successful, in
	// which case we return true.  If we can't do it, we deactivate the
	// navigator and return false:
	public bool CalculatePathToTarget(GameObject targ) {
		activated=false;
		ArrayList path = PathFinder.AStar(this.gameObject, targ);

		if (path.Count==0) {
			// Happens when path can't be found.  This is currently occuring as the objects never
			// die.  We also need to put the "Unit" objects in a layer to not be considered for
			// waypoint calculation:
			Debug.Log("A* path was null.");
		}

		currentPath=path;
		currentTarget = targ;
		lastCalcedTarget = currentTarget;
		//searchTime=Time.time;
		activated=true;
		return activated;
	}

	// Return random waypoint from waypoints.  Return null if no waypoints:
	public AutoWaypoint RandomWaypoint() {
		if (waypoints==null || waypoints.Count==0) { return null; }
		int rindex=Mathf.FloorToInt(Random.value * (float)waypoints.Count);
		rindex=Mathf.Clamp(rindex, 0, waypoints.Count-1); // Make sure we don't get caught with exactly count.
		if ((AutoWaypoint)waypoints[rindex]==waypoints[0]) {
			// Random was same as current..Try again once more (let it through after that):
			return RandomWaypoint();
		} else {
		      return (AutoWaypoint)waypoints[rindex];
		}
	}

	// Is target close to this object (as defined by TARGET_CLOSE_DISTANCE_SQR):
	private bool IsTargetClose(Transform target) {
		return ((this.transform.position-target.transform.position).sqrMagnitude <= TARGET_CLOSE_DISTANCE_SQR);
	}

}
