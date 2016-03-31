using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public GameObject player;

	public bool debug = false;

	private Vector3 offset;

	// Use this for initialization
	void Start ()
	{
		offset = player.transform.position + transform.position;

		DebugCameraPosition ();
	}
	
	// LateUpdate is called once per frame as the last function
	void LateUpdate ()
	{
		offset.z = player.transform.localScale.x * -10.0f;

		// player = GameObject.FindGameObjectWithTag ("Player"); -- Removed after player is back to being main starting target at first frame of game
		transform.position = player.transform.position + offset;

		DebugCameraPosition ();
	}

	void DebugCameraPosition ()
	{
		if (debug)
			Debug.Log ("Camera Settings" + "\n" + "Player Position: " + player.transform.position + "\n" + "Camera Position: " + transform.position + "\n" + "Camera Offset: " + offset);
	}
}
