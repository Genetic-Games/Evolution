using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{

	public float speed;
	public Text countText;
	public Text winText;

	private int count;
	private Canvas canv;
	private Rigidbody2D rb2d;

	void Start ()
	{
		rb2d = GetComponent<Rigidbody2D> ();
		count = 0;
		SetCountText ();
	}

	void FixedUpdate ()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		Vector2 movement = new Vector2 (speed * moveHorizontal, speed * moveVertical);
		rb2d.AddForce (movement);
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (other.gameObject.tag == "Pick Up") {
			other.gameObject.SetActive (false);
			count++;
			SetCountText ();
		}

		if (count == 8) {
			winText.text = "You Win!";
		}
			
	}

	void SetCountText ()
	{
		countText.text = "Score: " + count.ToString ();
	}

}
