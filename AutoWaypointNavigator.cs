using UnityEngine;
using System.Collections;

// Navigates among specified waypoints:
public class AutoWaypointNavigator : MonoBehaviour {
	public ArrayList waypoints=new ArrayList(20);
	public GameObject currentTarget=null;//Vector3.zero;
	private GameObject lastCalcedTarget=null;//Vector3.zero;
	public float updateInterval=0.4f;
	//private float lastUpdate=0.0f;
	private ArrayList currentPath;
	// This is set initially to false so our behaviour script can turn it on at will:
	public bool activated=false;
	//public CharacterMotor motor;
	private static float TARGET_CLOSE_DISTANCE_SQR = 1.0f;
	private static float SEARCH_TIMEOUT = 30.0f;	
	public float rotationSpeed = 0.5f;
	private float searchTime=0.0f;
	
	
	// Use this for initialization
	void Start () {
		
		// TODO: Handle character motor and Constant Force:
//		motor = GetComponent(typeof(CharacterMotor)) as CharacterMotor;
//		if (motor==null) {
//			UnityEngine.Debug.LogError("Character Motor is a required component.", this);
//		}						
		
		if (this.constantForce==null) {
			UnityEngine.Debug.LogError("Must have a constant force attached.", this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (activated && currentTarget) { // && (Time.time-lastUpdate) > updateInterval // Do every frame for now:
		 	//lastUpdate=Time.time;
			if (lastCalcedTarget==currentTarget) {
				// We've already calculated a path for current target.
				if (IsTargetClose(currentTarget.transform)) {
					// We are here:
					this.rigidbody.velocity=Vector3.zero;
				} else if (currentPath.Count==0) {
					this.rigidbody.velocity=(currentTarget.transform.position-this.transform.position).normalized * 2;
				} else if (IsTargetClose(((AutoWaypoint)currentPath[0]).transform)) {
					currentPath.RemoveAt(0);
				} else {
					this.rigidbody.velocity=(((AutoWaypoint)currentPath[0]).transform.position-this.transform.position).normalized * 2;
				}
				

			} else {
				Debug.Log("Calculating path.");
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
		//ArrayList path = AutoWaypoint.AStar(transform.position, targ.transform.position);
		ArrayList path = AutoWaypoint.AStar(this.gameObject, targ);
		
		if (path.Count==0) {
			Debug.Log("A* path was null.");		
		}
		
		currentPath=path;
		currentTarget = targ;		
		lastCalcedTarget = currentTarget;
		searchTime=Time.time;
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
