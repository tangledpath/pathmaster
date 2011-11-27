using UnityEngine;
using System;
using System.Collections;

public class NavigationUtil
{
	public NavigationUtil ()
	{
	}
	
	// Turns given transform towards target, so that forward movements will be towards target, back is exactly the opposite, left is strafe-left, etc.
	public static Vector3 TurnTowardsTarget(Transform transform, Vector3 target, float rotationSpeed) {
		Vector3 targetDir = target - transform.position;		
		targetDir.y=0;
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(targetDir), rotationSpeed * Time.deltaTime);
		transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
		
		// TODO:  Hook up speed:
		Vector3 fwdVector = transform.TransformDirection(Vector3.forward);
		float speedMod = Vector3.Dot(fwdVector, targetDir.normalized);
		speedMod = Mathf.Clamp01(speedMod);
		fwdVector = fwdVector * speedMod;
		return fwdVector;
	}
	
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
	    T temp;
	    temp = lhs;
	    lhs = rhs;
	    rhs = temp;
	}

}


