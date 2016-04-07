using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	public GameObject player;
	public float zoomSmooth = 1.0f;

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
		// offset.z = player.transform.localScale.x * -10.0f; -- Removed to be replaced by Zoom function with Linear Interpolation (Lerp)
		Zoom ();

		// player = GameObject.FindGameObjectWithTag ("Player"); -- Removed after player is back to being main starting target at first frame of game
		// transform.position = player.transform.position + offset; -- Removed to be replace by Zoom function with Linear Interpolation (Lerp)

		DebugCameraPosition ();
	}

	void DebugCameraPosition ()
	{
		if (debug)
			Debug.Log ("Camera Settings" + "\n" + "Player Position: " + player.transform.position + "\n" + "Camera Position: " + transform.position + "\n" + "Camera Offset: " + offset);
	}

	// Zoom out smoothly using linear interpolation when the player changes in size
	void Zoom ()
	{
		offset.z = Mathf.Lerp (offset.z, player.transform.localScale.x * -10.0f, Time.deltaTime * zoomSmooth);
		transform.position = player.transform.position + offset;
	}
}
