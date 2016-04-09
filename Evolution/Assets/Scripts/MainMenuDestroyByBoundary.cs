using UnityEngine;
using System.Collections;

public class MainMenuDestroyByBoundary : MonoBehaviour
{
	public MainMenuController mainController;

	public bool debug = false;

	void OnTriggerExit2D (Collider2D other)
	{
		if (debug)
			Debug.Log ("Object Leaving Boundary" + "\n" + other.gameObject + "\n" + "At Position: " + other.gameObject.transform.position);

		GameObject gobj = other.gameObject;
		if (gobj.tag == "Enemy") {
			mainController.EnemyDestroyed ();
			Destroy (gobj);

		}

	}

}