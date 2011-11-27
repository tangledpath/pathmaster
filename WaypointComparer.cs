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
