using System;
using UnityEngine;
using System.Collections;

public class MainMenuEnemyController : MonoBehaviour
{
	public float speedMin = 1.0f;
	public float speedMax = 10.0f;
	public float randomMitosisChance = 1.0f;
	public int lifetime = 10000;

	public bool debug = false;
	public bool debugLifetime = true;

	private Rigidbody2D rbody;
	private float speed;

	private Vector3 origin = new Vector3 (0.0f, 0.0f);
	private MainMenuController gameController;
	private GameObject background;
	private int lifetimeCounter = 0;

	// Use this for initialization, determine enemy growth factor and speed factor
	void Start ()
	{
		speed = UnityEngine.Random.Range (speedMin, speedMax);

		rbody = GetComponent<Rigidbody2D> ();
		background = GameObject.FindGameObjectWithTag ("Background");
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
		lifetimeCounter++;

		MoveBasedOnTarget ();

		// Ensure that no one leaves the boundary and if they do, they are destroyed
		if (Vector3.Distance (transform.position, origin) > background.GetComponent<CircleCollider2D> ().radius * background.transform.localScale.x) {
			gameController.EnemyDestroyed ();
			Destroy (gameObject);
		}

		// Perform Mitosis (splitting of cells) randomly above a minimum size and once a maximum size is reached
		if (UnityEngine.Random.Range (0.0f, 100.0f) <= randomMitosisChance && transform.localScale.x > gameController.enemyScaleMin)
			Mitosis ();

		if (lifetimeCounter == lifetime) {

			if (debugLifetime)
				Debug.Log ("Enemy: " + gameObject.name + " reached lifetime limit.");

			gameController.EnemyDestroyed ();
			Destroy (gameObject);
		}
	}

	void MoveBasedOnTarget ()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");

		if (debug)
			Debug.Log ("Number of Enemies Found: " + enemies.Length);

		float thisEnemyRadius = transform.localScale.x * gameObject.GetComponent<CircleCollider2D> ().radius;

		GameObject target;
		float distanceToTarget;

		// Player does not exist on main menu, set the target to the first enemy
		float firstEnemyRadius = enemies [0].transform.localScale.x * enemies [0].GetComponent<CircleCollider2D> ().radius;
		distanceToTarget = Vector3.Distance (transform.position, enemies [0].transform.position) - firstEnemyRadius - thisEnemyRadius;
		target = enemies [0];

		// Search through all targets to find the closest one out of all enemies
		foreach (GameObject enemy in enemies) {

			// Prevent the enemy from selecting itself as the closest target
			if (Vector3.Equals (transform.position, enemy.transform.position))
				continue;

			// Find closest point distance between target and this enemy
			float targetRadius = enemy.transform.localScale.x * enemy.GetComponent<CircleCollider2D> ().radius;
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

		if (target.GetComponent<Rigidbody2D> ().mass < rbody.mass)
			moveTowards = true;
		else
			moveTowards = false;

		// Log base 2 of mass + 2.5 chosen as most balanced value for both low mass and high mass speed to give a good sense of weight (could not use force function)
		float massSpeedFactor = Mathf.Log (rbody.mass) + 2.5f;

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

					// If enemies have equal mass (and thus size), do nothing
					// If other enemy has greater or equal mass (and thus size), then destroy enemy and grow other enemy (handled with other enemy script)
				}
			} catch (NullReferenceException e) {
				Debug.LogWarning ("Enemy Spawn Exception: " + e.ToString ());
			}
		}
	}

	// Split an enemy that is too large into two smaller enemies of equal size
	void Mitosis ()
	{
		if (debug)
			Debug.Log ("Mitosis Triggered" + "\n" + "Enemy: " + gameObject + "\n" + "Position: " + transform.position + "\n" + "Scale: " + transform.localScale.x);

		// Set the spawn origin and radius (their spawn zone is a circle) for the two cells as the middle of the enemy before it is split
		Vector3 spawnOrigin = transform.position;

		float area = Mathf.PI * Mathf.Pow (transform.localScale.x, 2.0f) * Mathf.Pow (GetComponent<CircleCollider2D> ().radius, 2.0f);
		float newArea = area / 2.0f;
		float newScale = Mathf.Sqrt (newArea / (Mathf.PI * Mathf.Pow (GetComponent<CircleCollider2D> ().radius, 2.0f)));

		float spawnDistance = newScale * GetComponent<CircleCollider2D> ().radius / 2.0f;
		Vector3 newScaleVector = new Vector3 (newScale, newScale);

		// Randomly choose where to spawn inside the old enemy's radius, choose the exact opposite way for the second spawn
		Vector2 firstVector = UnityEngine.Random.insideUnitCircle.normalized * spawnDistance;
		Vector3 firstVector3 = new Vector3 (firstVector.x, firstVector.y);
		Vector3 secondVector3 = new Vector3 (firstVector.x * -1.0f, firstVector.y * -1.0f);

		// Shrink the original before deleting it or spawning others so there are not any race conditions or collisions happening
		transform.localScale = new Vector3 (gameController.enemyScaleMin / 100.0f, gameController.enemyScaleMin / 100.0f);

		// Randomize enemy rotation orientation
		Quaternion enemyRotation = Quaternion.Euler (0.0f, 0.0f, UnityEngine.Random.Range (0.0f, 360.0f));

		// Generate the two enemies (simulating a mitosis split) and delete the old one
		gameController.GenerateEnemy (spawnOrigin + firstVector3, enemyRotation, newScaleVector);
		gameController.GenerateEnemy (spawnOrigin + secondVector3, enemyRotation, newScaleVector);
		gameController.EnemyDestroyed ();
		Destroy (gameObject);
	}
}
