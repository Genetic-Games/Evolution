using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	public float speedFactor = 20.0f;
	public float massDensity = 4.0f / Mathf.PI;
	public AudioSource eat;

	public bool debug = false;

	private Rigidbody2D rbody;
	private GameController gameController;

	// Use this for initialization
	void Start ()
	{
		rbody = GetComponent<Rigidbody2D> ();
		rbody.useAutoMass = true;
		GetComponent<CircleCollider2D> ().density = massDensity;

		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");

		if (gameControllerObject != null) {
			gameController = gameControllerObject.GetComponent <GameController> ();
		} else {
			Debug.LogError ("Cannot find 'GameController' script.");
		}

		if (debug)
			Debug.Log ("Player Settings" + "\n" + "Rigidbody2D: " + rbody + "\n" + "Speed: " + speedFactor);
	}
	
	/* Update is called once per frame */
	void Update ()
	{
		// Ensure that player components are equal (should be if a circle)
		if (debug)
			Debug.Assert (transform.localScale.x == transform.localScale.y);
	}

	// FixedUpdate is called once per frame at the end of processing all other calculations
	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 movement = new Vector3 (moveHorizontal, moveVertical);

		// Deemed as a good smooth function from low mass to high mass to represent the force applied for speed
		float speed = Mathf.Pow (rbody.mass, (2.0f / 3.0f));

		if (!gameController.paused) {
			rbody.WakeUp ();
			rbody.AddForce (speedFactor * speed * movement);
			rbody.AddTorque (-1.0f * (moveHorizontal + moveVertical) / (speedFactor * rbody.mass));
		} else {
			rbody.Sleep ();
		}

		if (debug) {
			if (moveHorizontal != 0.0f)
				Debug.Log ("Player Input" + "\n" + "Horizontal: " + moveHorizontal);

			if (moveVertical != 0.0f)
				Debug.Log ("Player Input" + "\n" + "Vertical: " + moveVertical);
			
			if (movement.x != 0.0f || movement.y != 0.0f)
				Debug.Log ("Player Movement" + "\n" + "Movement: " + movement);
		}
			
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		if (debug)
			Debug.Log ("Collision With Player Detected" + "\n" + "Other Object:" + "\n" + other.gameObject + "\n" +
			"Other Tag: " + other.gameObject.tag + "\n" + "Other Mass: " + other.gameObject.GetComponent<Rigidbody2D> ().mass + "\n" + "Player Mass: " + rbody.mass);

		// If an enemy is touched, see who is bigger
		if (other.gameObject.tag == "Enemy") {

			// If player has higher mass (and thus size), grow player and destroy enemy
			if (rbody.mass > other.gameObject.GetComponent<Rigidbody2D> ().mass) {

				gameController.AbsorbGrowth (gameObject, other.gameObject);
				gameController.EnemyDestroyed ();
				gameController.ScoreIncrease (other.gameObject.GetComponent<Rigidbody2D> ().mass * 100.0f);
				eat.Play ();
				Destroy (other.gameObject);

				// If enemy has higher mass or equal mass (and thus size), grow enemy and destroy player
			} else {
				gameController.AbsorbGrowth (other.gameObject, gameObject);
				gameController.GameOver ();
				other.gameObject.GetComponent<AudioSource> ().Play ();
				gameObject.SetActive (false);
				//Destroy (gameObject);  -- Removed because it threw an error upon destroying the primary player
			}
		}
	}
}
