﻿using System;
using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
	public float speedMin = 1.0f;
	public float speedMax = 10.0f;
	//public float growthMin = 0.0001f;
	//public float growthMax = 0.0005f;

	public bool debug = true;
	//public bool debugGrowth = false;
	//public bool debugMovement = false;
	//public bool debugCollision = true;

	private GameObject player;
	private Rigidbody2D rbody;
	//private float growth;
	private float speed;
	//private Vector3 growthFactor;

	private GameController gameController;

	// Use this for initialization, determine enemy growth factor and speed factor
	void Start ()
	{
		
		speed = UnityEngine.Random.Range (speedMin, speedMax);

		//growth = UnityEngine.Random.Range (growthMin, growthMax);
		//growthFactor = new Vector3 (growth, growth);

		player = GameObject.FindGameObjectWithTag ("Player");
		rbody = GetComponent<Rigidbody2D> ();

		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");

		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent <GameController> ();
		} else {
			Debug.LogError ("Cannot find 'GameController' script.");
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		//Growth (); -- Removed due to growth being too large in combination with absorbing
		MoveBasedOnTarget ();
	}

	// Grow (increase the scale and thus mass automatically) of the enemy each frame based on a random growth factor
	/*
	void Growth ()
	{
		Vector3 currentScale = transform.localScale;
		Vector3 growthScale = currentScale + growthFactor;
		transform.localScale = growthScale;

		bool currentScaleCheck = currentScale.x == currentScale.y;
		bool growthScaleCheck = growthScale.x == growthScale.y;

		float currentScaleParameter = currentScale.x;
		//collide.radius = currentScaleParameter;

		if (debugGrowth) {
			Debug.Log ("Enemy Growth" + "\n" + "Current Scale Parameter: " + currentScaleParameter + "\n" + "Current Scale: " + currentScale + "\n" +
			"Growth Parameter: " + growth + "\n" + "Growth Vector: " + growthFactor + "\n" + "Scale After Growth: " + growthScale + "\n");

			// Log warnings to console if they are not currently scaled and growing uniformly
			if (!currentScaleCheck)
				Debug.LogWarning ("Enemy Current Scale Non-Uniform" + "\n" + "Current Scale Uniform: " + currentScaleCheck);

			if (!growthScaleCheck)
				Debug.LogWarning ("Enemy Growth Scale Non-Uniform" + "\n" + "Growth Scale Uniform" + growthScaleCheck);
		}
	}
	*/

	void MoveBasedOnTarget ()
	{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");

		if (debug)
			Debug.Assert (enemies.Length > 0);

		float thisEnemyRadius = transform.localScale.x / 2.0f;

		GameObject target;
		float distanceToTarget;

		// If player is not active or has been destroyed, then set the target to the Origin
		if (player == null || !player.activeInHierarchy) {
			
			Vector3 origin = new Vector3 (0.0f, 0.0f);
			distanceToTarget = Vector3.Distance (transform.position, origin) - thisEnemyRadius;
			target = GameObject.FindGameObjectWithTag ("Background");
		} else {
			// Initialize targeting variables to start with base case of closest point on player object
			float playerRadius = player.transform.localScale.x / 2.0f;
			target = player;
			distanceToTarget = Vector3.Distance (transform.position, player.transform.position) - playerRadius - thisEnemyRadius;
		}

		// Search through all targets to find the closest one out of the player and all enemies
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