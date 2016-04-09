using UnityEngine;
using System.Collections;

public class DestroyByBoundary : MonoBehaviour
{
	public GameController gameController;

	public bool debug = true;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnTriggerExit2D (Collider2D other)
	{
		if (debug)
			Debug.Log ("Object Leaving Boundary" + "\n" + other.gameObject + "\n" + "At Position: " + other.gameObject.transform.position);

		GameObject gobj = other.gameObject;
		if (gobj.tag == "Enemy") {
			gameController.EnemyDestroyed ();
			Destroy (gobj);
		} else {
			gameController.GameOver ();
			gobj.SetActive (false);
		}
	}

}
