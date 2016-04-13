using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
	public GameObject enemy;
	public GameObject background;

	public int enemyStart = 200;
	public int enemyMax = 1000;
	public float waitSpawnTime = 3.0f;
	public float massDensity = 4.0f / Mathf.PI;
	public float enemyScaleMin = 0.01f;
	public float enemyScaleMax = 3.0f;

	public Text highScoreText;
	public Text startText;

	public bool debug = false;
	public bool debugUI = true;

	GameObject[] startObjects;
	GameObject[] creditObjects;

	private float highScore;
	private int count;
	private int enemyCounter;
	private float mapBorder;

	// Use this for initialization
	void Start ()
	{
		startText.text = "";
		highScore = PlayerPrefs.GetFloat ("High Score", 0.0f);

		if (debug)
			Debug.Log ("Current High Score: " + highScore);

		if (highScore == 0.0f)
			highScoreText.text = "";
		else
			highScoreText.text = "High Score: " + highScore;

		enemyCounter = 0;
		count = 1;

		startObjects = GameObject.FindGameObjectsWithTag ("Start");
		creditObjects = GameObject.FindGameObjectsWithTag ("Credits");

		Credits (false);

		// Background should be a circle
		mapBorder = background.GetComponent<CircleCollider2D> ().radius * background.transform.localScale.x;

		DebugLogEnemies ();

		StartCoroutine (spawnEnemies ());
		StartCoroutine (startGame ());
	}

	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space))
			SceneManager.LoadScene ("Game");
	}

	IEnumerator startGame ()
	{
		yield return new WaitForSeconds (5.0f);

		startText.text = "Press Space to Start!";

	}

	// Log how many enemies there currently are in the debug log
	void DebugLogEnemies ()
	{
		if (debug)
			Debug.Log ("Enemies: " + enemyCounter);
	}

	// Spawn enemies every so often, depending on wait time, the number of enemies, and if the game is over or not
	IEnumerator spawnEnemies ()
	{
		StartSpawn ();

		while (true) {

			yield return new WaitForSeconds (waitSpawnTime);

			if (enemyCounter < enemyMax)
				RandomlyGenerateEnemy ();

		}
	}

	void StartSpawn ()
	{
		for (int i = 0; i < enemyStart; i++)
			RandomlyGenerateEnemy ();
	}

	// Randomly determine spawn position, scale, and speed for new enemy, checking to make sure they are within the map and not on the player
	void RandomlyGenerateEnemy ()
	{
		float enemyRadius;
		bool mapCheck = false;
		Vector2 spawnPoint;
		Vector3 enemyScale = new Vector3 ();
		Vector3 enemyPosition = new Vector3 ();

		// Randomly select a spawn point in 2D space inside the spherical map
		do {
			spawnPoint = Random.insideUnitCircle * mapBorder;
			enemyPosition.x = spawnPoint.x;
			enemyPosition.y = spawnPoint.y;

			// Randomly select a scale that an enemy will generate at and use it for a circle
			float scaleValue = Random.Range (enemyScaleMin, enemyScaleMax);
			enemyScale.x = enemyScale.y = scaleValue;
			enemyRadius = scaleValue * enemy.GetComponent<CircleCollider2D> ().radius;

			if (debug)
				Debug.Log ("Enemy Spawn Attempt" + "\n" + "Position: " + enemyPosition + "\n" + "Scale: " + enemyScale + "\n");

			// Check to see if the enemy will be spawned outside the border of the map due to its scale
			float enemyOriginDistance = Vector3.Distance (enemyPosition, background.transform.position);
			mapCheck = enemyOriginDistance + enemyRadius < mapBorder;

			if (!mapCheck) {
				if (debug)
					Debug.LogWarning ("Map Spawn Failure" + "\n" + "Distance from Origin: " + enemyOriginDistance + "\n" + "Enemy Radius: " + enemyRadius + "\n" + "Map Border: " + mapBorder);
				continue;
			}

			// Select a new random spawn point if this one is too near the player or the edge of the map (includes the size of the enemy)
		} while (!mapCheck);

		// Generate the enemy and set its position, scale (rotation is 0), and mass automatically as based on density
		GenerateEnemy (enemyPosition, Quaternion.identity, enemyScale);
	}

	// Public function of generate enemy, allows for Mitosis in enemy controller calculation
	public void GenerateEnemy (Vector3 spawnPosition, Quaternion spawnRotation, Vector3 spawnScale)
	{
		// Generate the enemy and set its position, scale (rotation is 0), and mass automatically as based on density
		var spawnedEnemy = Instantiate (enemy, spawnPosition, spawnRotation) as GameObject;
		spawnedEnemy.transform.localScale = spawnScale;

		Rigidbody2D rbody = spawnedEnemy.GetComponent<Rigidbody2D> ();
		rbody.useAutoMass = true;
		spawnedEnemy.GetComponent<CircleCollider2D> ().density = massDensity;
		spawnedEnemy.name = "Enemy " + count;

		count++;
		enemyCounter++;

		if (debug)
			Debug.Log ("New Enemy Spawned" + "\n" + "Position: " + spawnPosition + "\n" + "Scale: " + spawnScale + "\n" + "Mass: " + rbody.mass);

		DebugLogEnemies ();
	}

	public void EnemyDestroyed ()
	{
		enemyCounter--;
		DebugLogEnemies ();
	}

	// Growth calculates the amount that source should grow by based on target's size
	public void AbsorbGrowth (GameObject source, GameObject target)
	{
		float sourceColliderRadius = source.GetComponent<CircleCollider2D> ().radius;
		float sourceRadius = source.transform.localScale.x * sourceColliderRadius;
		float targetRadius = target.transform.localScale.x * target.GetComponent<CircleCollider2D> ().radius;

		float sourceArea = Mathf.PI * Mathf.Pow (sourceRadius, 2.0f);
		float targetArea = Mathf.PI * Mathf.Pow (targetRadius, 2.0f);
		float totalArea = sourceArea + targetArea;

		float newRadius = Mathf.Sqrt (totalArea / Mathf.PI);

		Vector3 scale = new Vector3 (newRadius / sourceColliderRadius, newRadius / sourceColliderRadius);
		source.transform.localScale = scale;

		if (debug)
			Debug.Log ("Collision Growth Detected" + "\n" + "Winner Radius Before: " + sourceRadius + "\n" + "Loser Radius: " + targetRadius + "\n" +
			"Winner Area Before: " + sourceArea + "\n" + "Loser Area: " + targetArea + "\n" + "Winner Area After: " + totalArea + "\n" + "Winner Radius After: " + newRadius);
	}

	// Toggle whether to show the main menu screen or the credits screen based on user input (click on credits button), requires that all elements start as active so they can be found and toggled
	public void Credits (bool displayCredits)
	{
		if (debugUI)
			Debug.Log (displayCredits ? "Displaying Credits" : "Displaying Main Menu");

		if (displayCredits) {

			foreach (GameObject s in startObjects)
				s.SetActive (false);

			foreach (GameObject c in creditObjects)
				c.SetActive (true);

		} else {

			foreach (GameObject c in creditObjects)
				c.SetActive (false);

			foreach (GameObject s in startObjects)
				s.SetActive (true);
		}
	}
		
}
