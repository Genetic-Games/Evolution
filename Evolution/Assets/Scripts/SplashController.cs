using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
	public SpriteRenderer logo;
	public float duration = 4.0f;
	public float wait = 2.0f;

	public bool debug = false;

	private Color invisible;
	private Color visible;
	private bool fadeStart;
	private bool fadedIn;
	private bool fadedOut;
	private bool fadeComplete;
	private float startTime;
	private float fadeInTime;

	// Use this for initialization
	void Start ()
	{
		fadeStart = false;
		fadedIn = false;
		fadedOut = false;
		fadeComplete = false;

		invisible = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		visible = new Color (1.0f, 1.0f, 1.0f, 1.0f);

		// Ensure that the logo starts off hidden so it can fade in and out in the right order
		logo.color = invisible;

		if (debug)
			Debug.Log ("Starting Splash Color: " + logo.color);

		StartCoroutine (WaitThenStart ());
	}
	
	// FixedUpdate is called once per frame
	void Update ()
	{
		// Use floats as value step when used with timestamp difference and max range value of duration
		float t = (Time.time - startTime) / duration;
		float tprime = (Time.time - fadeInTime) / duration;

		if (debug)
			Debug.Log ("Current Splash Color: " + logo.color);

		// Smoothly fade in if logo has not been faded in yet
		if (!fadedIn && fadeStart)
			logo.color = new Color (1.0f, 1.0f, 1.0f, Mathf.SmoothStep (0.0f, 1.0f, t));

		// Smoothly fade out if logo has already been faded in and has not faded out yet
		else if (fadedIn && !fadedOut && fadeStart)
			logo.color = new Color (1.0f, 1.0f, 1.0f, Mathf.SmoothStep (1.0f, 0.0f, tprime));

		if (Color.Equals (visible, logo.color)) {
			fadedIn = true;
			fadeInTime = Time.time;
				
			if (debug)
				Debug.Log ("Splash Screen successfully faded in.");

		}

		if (fadedIn && !fadedOut && Color.Equals (invisible, logo.color)) {
			fadedOut = true;
			StartCoroutine (WaitThenComplete ());

			if (debug)
				Debug.Log ("Splash Screen successfully faded out.");
		}

		// Load main menu once splash screen fade process is completed
		if (fadeComplete) {

			if (debug)
				Debug.Log ("Loading scene: Main Menu");

			SceneManager.LoadScene ("MainMenu");
		}
		
	}

	// Once logo fades out completely, briefly wait before going to main menu
	IEnumerator WaitThenComplete ()
	{
		yield return new WaitForSeconds (wait);
		fadeComplete = true;
	}

	// Briefly wait until starting to fade in the logo, purely aesthetic
	IEnumerator WaitThenStart ()
	{
		yield return new WaitForSeconds (wait);
		fadeStart = true;
		startTime = Time.time;
	}
		
}
