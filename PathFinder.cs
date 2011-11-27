using UnityEngine;
using System;
using System.Collections;

public class PathFinder
{
	private static float A_STAR_GOAL_TOLERANCE_SQR=10000; // 100 meters
	
	static public void ConnectAllWaypoints() {
		AutoWaypoint[] points = AllWayPoints();
		foreach (AutoWaypoint wp in points) {
			wp.ConnectWaypoint(points);
		}	    
	}
	
	public static AutoWaypoint[] AllWayPoints() {
		return UnityEngine.Object.FindObjectsOfType(typeof(AutoWaypoint)) as AutoWaypoint[];
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
		ConnectAllWaypoints();
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
