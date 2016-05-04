using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameController : MonoBehaviour
{
	public GameObject enemy;
	public GameObject background;

	public int enemyStart = 40;
	public int enemyMax = 100;
	public float waitSpawnTime = 3.0f;
	public float bufferSpace = 10.0f;
	public float massDensity = 4.0f / Mathf.PI;
	public float enemyScaleMin = 0.01f;
	public float enemyScaleMax = 3.0f;
	public float winSizeAsPercent = 0.5f;

	public Text gameOverText;
	public Text restartText;
	public Text scoreText;
	public Text highScoreText;
	public Text newHighScore;

	public bool debug = false;

	private GameObject player;
	private CircleCollider2D playerCollider;
	private float highScore;
	private float score;
	private int count;
	private bool gameOver;
	private int enemyCounter;
	private float mapBorder;
	private bool flashHighScore;

	// Use this for initialization
	void Start ()
	{
		score = 0;

		flashHighScore = false;
		newHighScore.text = "";
		highScore = PlayerPrefs.GetFloat ("High Score", 0.0f);
		gameOverText.text = restartText.text = scoreText.text = "";
		scoreText.text = "Score: " + score;

		if (debug)
			Debug.Log ("Current High Score: " + highScore);

		if (highScore == 0.0f)
			highScoreText.text = "";
		else
			highScoreText.text = "High Score: " + highScore;

		player = GameObject.FindGameObjectWithTag ("Player");
		playerCollider = player.GetComponent<CircleCollider2D> ();
		enemyCounter = 0;
		count = 1;
		gameOver = false;

		// SpawnPlayer (); -- Removed due to need for player tracking by enemies and camera

		// Ensure that the background components are equal (should be if a circle)
		if (debug)
			Debug.Assert (background.transform.localScale.x == background.transform.localScale.y);

		mapBorder = background.transform.localScale.x * background.GetComponent<CircleCollider2D> ().radius;

		DebugLogEnemies ();

		StartCoroutine (spawnEnemies ());
	}

	/*
	// Spawn the player at the origin and set their mass to be automatic based on given density value
	void SpawnPlayer ()
	{
		Vector3 origin = new Vector3 (0.0f, 0.0f);
		var spawnedPlayer = Instantiate (player, origin, Quaternion.identity) as GameObject;
		spawnedPlayer.GetComponent<Rigidbody2D> ().useAutoMass = true;
		spawnedPlayer.GetComponent<Collider2D> ().density = massDensity;
	}
	*/

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

			if (gameOver)
				break;
		}
	}

	void StartSpawn ()
	{
		for (int i = 0; i < enemyStart; i++)
			RandomlyGenerateEnemy ();
	}

	// Randomly select a position, scale, velocity, and rotation to spawn an enemy inside of the map and a certain distance from the player
	void RandomlyGenerateEnemy ()
	{
		Vector3 playerScale = player.transform.localScale;
		Vector3 playerPosition = player.transform.position;
		CircleCollider2D enemyCollider = enemy.GetComponent<CircleCollider2D> ();

		float playerRadius = playerScale.x * playerCollider.radius;
		float enemyRadius;
		bool playerCheck = false;
		bool mapCheck = false;
		//bool enemyCheck = false;
		Vector2 spawnPoint;
		Vector3 enemyScale = new Vector3 ();
		Vector3 enemyPosition = new Vector3 ();
		//GameObject[] enemies = GameObject.FindGameObjectsWithTag ("Enemy");

		// Randomly select a spawn point in 2D space inside the spherical map
		do {
			spawnPoint = Random.insideUnitCircle * mapBorder;
			enemyPosition.x = spawnPoint.x;
			enemyPosition.y = spawnPoint.y;

			// Randomly select a scale that an enemy will generate at and use it for a circle
			float scaleValue = Random.Range (enemyScaleMin, enemyScaleMax);
			enemyScale.x = enemyScale.y = scaleValue;
			enemyRadius = scaleValue * enemyCollider.radius;

			if (debug)
				Debug.Log ("Enemy Spawn Attempt" + "\n" + "Position: " + enemyPosition + "\n" + "Scale: " + enemyScale + "\n");

			// Check to see if the spawn point is overlapping with the player
			float playerEnemyDistance = Vector3.Distance (enemyPosition, playerPosition);
			playerCheck = playerEnemyDistance > playerRadius + enemyRadius + bufferSpace;

			if (!playerCheck) {
				if (debug)
					Debug.LogWarning ("Player Spawn Failure" + "\n" + "Distance from Enemy: " + playerEnemyDistance + "\n" + "Player Radius: " + playerRadius + "\n" +
					"Enemy Radius: " + enemyRadius + "\n" + "Buffer Space: " + bufferSpace);
				continue;
			}
			/*
			 * Checking for enemy distance was too heavy on processing per frame and caused severe lag, removed since out of player scope more often than not
			 * 
			float spawnToEnemyDistance;
			if (enemies.Length <= 0 || enemies == null)
				enemyCheck = true;
			else {
				
				// Check for proximity to other enemies
				foreach (GameObject e in enemies) {
					spawnToEnemyDistance = Vector3.Distance (enemyPosition, e.transform.position);
					enemyCheck = spawnToEnemyDistance > (e.transform.localScale.x * e.GetComponent<CircleCollider2D> ().radius) + enemyRadius + bufferSpace;

					if (!enemyCheck) {
						if (debug)
							Debug.LogWarning ("Enemy Spawn Failure" + "\n" + "Distance from Enemy: " + spawnToEnemyDistance + "\n" + 
							"Old Enemy Radius: " + (e.transform.localScale.x * e.GetComponent<CircleCollider2D> ().radius) + "\n" +
							"Spawn Enemy Radius: " + enemyRadius + "\n" + "Buffer Space: " + bufferSpace);
						break;
					}
				}

				if (!enemyCheck)
					continue;
			}
			*/

			// Check to see if the enemy will be spawned outside the border of the map due to its scale
			float enemyOriginDistance = Vector3.Distance (enemyPosition, background.transform.position);
			mapCheck = enemyOriginDistance + enemyRadius < mapBorder;

			if (!mapCheck) {
				if (debug)
					Debug.LogWarning ("Map Spawn Failure" + "\n" + "Distance from Origin: " + enemyOriginDistance + "\n" + "Enemy Radius: " + enemyRadius + "\n" + "Map Border: " + mapBorder);
				continue;
			}

			if (WinCondition ())
				break;

			// Select a new random spawn point if this one is too near the player or the edge of the map (includes the size of the enemy)
		} while (!playerCheck /*|| !enemyCheck */ || !mapCheck);

		// Do not generate an enemy if the player has won
		if (WinCondition ()) {
			ScoreIncrease (player.GetComponent<Rigidbody2D> ().mass / 100.0f);
			GameOver ();
			gameOverText.text = "You Win!";
		} else {
			// Randomly generate the rotation of the enemy
			Quaternion enemyRotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f));

			GenerateEnemy (enemyPosition, enemyRotation, enemyScale);
		}
	}

	// Create a publicly accessible function (for enemy Mitosis) that spawns an enemy
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

	// Control enemy counter to ensure that spawn count maximum is never exceeded
	public void EnemyDestroyed ()
	{
		enemyCounter--;
		DebugLogEnemies ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		if (gameOver) {
			// If game is over and space is pressed, restart the game
			if (Input.GetKeyDown (KeyCode.Space))
				SceneManager.LoadScene (SceneManager.GetActiveScene ().name);

			// If game is over and enter is pressed, load main menu
			if (Input.GetKeyDown (KeyCode.Return))
				SceneManager.LoadScene ("MainMenu");
		}

		// If escape is pressed at any time, return to main menu
		if (Input.GetKeyDown (KeyCode.Escape))
			SceneManager.LoadScene ("MainMenu");

		NewHighScore ();
	}

	// Change the high score if the player surprasses it in the game
	void NewHighScore ()
	{
		if (score > highScore) {
			highScore = score;
			PlayerPrefs.SetFloat ("High Score", highScore);
			PlayerPrefs.Save ();
			highScoreText.text = "High Score: " + score;
			newHighScore.text = "New High Score!";

			if (!flashHighScore)
				StartCoroutine (FlashScore ());

			flashHighScore = true;
		}
	}

	// Call this function elsewhere when the game is over for any reason
	public void GameOver ()
	{
		gameOver = true;
		gameOverText.text = "Game Over";

		NewHighScore ();
		StartCoroutine (EndGame ());
	}

	IEnumerator FlashScore ()
	{
		yield return new WaitForEndOfFrame ();

		while (flashHighScore) {

			newHighScore.color = Color.red;
			yield return new WaitForSeconds (1.0f);
			newHighScore.color = Color.white;
			yield return new WaitForSeconds (1.0f);
		}
			
	}

	IEnumerator EndGame ()
	{
		yield return new WaitForSeconds (3.0f);
		restartText.text = "Press Space to restart or Enter for Main Menu";

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

	// Adjust score, should be triggered when player consumes enemy
	public void ScoreIncrease (float val)
	{
		if (debug)
			Debug.Log ("Score Increase" + "\n" + "Previous Score: " + score + "\n" + "Increase: " + Mathf.Round (val));

		score += Mathf.Round (val);
		scoreText.text = "Score: " + score;
	}

	// Win if there is no more room to spawn enemies
	bool WinCondition ()
	{
		float playerRadius = player.transform.localScale.x * player.GetComponent<CircleCollider2D> ().radius;
		float backgroundRadius = background.transform.localScale.x * background.GetComponent<CircleCollider2D> ().radius;

		if (playerRadius >= (backgroundRadius - bufferSpace) * winSizeAsPercent || (enemyCounter == 0 && count != 1))
			return true;
		else
			return false;
	}
}
