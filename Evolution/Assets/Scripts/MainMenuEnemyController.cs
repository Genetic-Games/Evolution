using System;
using UnityEngine;
using System.Collections;

public class MainMenuEnemyController : MonoBehaviour
{
	public float speedMin = 1.0f;
	public float speedMax = 10.0f;

	public bool debug = false;

	private Rigidbody2D rbody;
	private float speed;

	private MainMenuController gameController;

	// Use this for initialization, determine enemy growth factor and speed factor
	void Start ()
	{

		speed = UnityEngine.Random.Range (speedMin, speedMax);

		rbody = GetComponent<Rigidbody2D> ();

		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");

		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent <MainMenuController> ();
		} else {
			Debug.LogError ("Cannot find 'MainMenuController' script.");
		}
	}

	// Update is called once per frame
	void Update ()
	{
		MoveBasedOnTarget ();
	}

	void MoveBasedOnTarget ()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");

		if (debug)
			Debug.Assert (enemies.Length > 0);

		float thisEnemyRadius = transform.localScale.x / 2.0f;

		GameObject target;
		float distanceToTarget;

		// Player does not exist on main menu, set the target to the Origin
		Vector3 origin = new Vector3 (0.0f, 0.0f);
		distanceToTarget = Vector3.Distance (transform.position, origin) - thisEnemyRadius;
		target = GameObject.FindGameObjectWithTag ("Background");

		// Search through all targets to find the closest one out of all enemies
		foreach (GameObject enemy in enemies) {

			// Prevent the enemy from selecting itself as the closest target
			if (Vector3.Equals (transform.position, enemy.transform.position))
				continue;

			// Find closest point distance between target and this enemy
			float targetRadius = enemy.transform.localScale.x / 2.0f;
			float distanceBetweenObjects = Vector3.Distance (transform.position, enemy.transform.position) - thisEnemyRadius - targetRadius;

			// If this object is closer than the target, make it the new target and update the distance to its closest point
			if (distanceBetweenObjects < distanceToTarget) {
				distanceToTarget = distanceBetweenObjects;
				target = enemy;
			}
		}

		if (debug)
			Debug.Log ("Closest Target" + "\n" + "Target Distance: " + distanceToTarget + "\n" + "Target Position: " + target.transform.position + "\n" + "Target Scale: " + target.transform.localScale);

		// Determine if this enemy should move toward or away from the target based on size, approach if target is smaller, otherwise retreat
		bool moveTowards;
		if (target.transform.localScale.x < transform.localScale.x)
			moveTowards = true;
		else
			moveTowards = false;

		float mass = rbody.mass;
		float massSpeedFactor = Mathf.Log (mass) + 2.5f;

		// If larger, move away, otherwise move towards
		transform.position = Vector2.MoveTowards (transform.position, target.transform.position, (Time.deltaTime * speed * (moveTowards ? 1.0f : -1.0f)) / massSpeedFactor);
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		if (debug) {
			try {
				Debug.Log ("Collision With Other Enemy Detected" + "\n" + "Other Object:" + "\n" + other.gameObject + "\n" +
				"Other Tag: " + other.gameObject.tag + "\n" + "Other Mass: " + other.gameObject.GetComponent<Rigidbody2D> ().mass + "\n" + "Enemy Mass: " + rbody.mass);

			} catch (NullReferenceException e) {
				Debug.LogWarning ("Enemy Spawn Exception: " + e.ToString ());
			}
		}

		// If an enemy is touched, see who is bigger
		if (other.gameObject.tag == "Enemy") {

			try {
				// If enemy has higher mass (and thus size) than other enemy, grow enemy and destroy other enemy
				if (rbody.mass > other.gameObject.GetComponent<Rigidbody2D> ().mass) {

					gameController.AbsorbGrowth (gameObject, other.gameObject);
					gameController.EnemyDestroyed ();
					Destroy (other.gameObject);

					// If enemies have equal mass (and thus size), destroy both, because why not
				} else if (rbody.mass == other.gameObject.GetComponent<Rigidbody2D> ().mass) {

					gameController.EnemyDestroyed ();
					gameController.EnemyDestroyed ();
					Destroy (other.gameObject);
					Destroy (gameObject);

					// If other enemy has greater mass (and thus size), then destroy enemy and grow other enemy (handled with other enemy script)
				}
			} catch (NullReferenceException e) {
				Debug.LogWarning ("Enemy Spawn Exception: " + e.ToString ());
			}
		}
	}
}
