using UnityEngine;
using System.Collections;

public class DestroyByBoundary : MonoBehaviour
{
	public GameController gameController;

	public bool debug = true;

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
