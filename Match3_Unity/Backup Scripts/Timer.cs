using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour 
{
	public Transform orderChangeButton;
	public Transform bombButton;

	public GameObject gameOverObject;
	public GameObject reGameObject;

	public Transform readyBackground;
	public Text readyText;

	private Text timerText;
	private int gameTimer;

	private void Start ()
	{
		timerText = transform.GetComponent<Text>();
		gameTimer = 60;

		//StartCoroutine (StartMessage ());

		readyBackground.gameObject.SetActive(false);
		//StartCoroutine(CountDown());
	}

	private IEnumerator StartMessage ()
	{
		readyText.text = "Ready";
		yield return new WaitForSeconds (0.5f);

		for (int count = 3; count > 0; --count)
        {
			readyText.text = count.ToString();
			readyBackground.localScale = Vector3.one;
			float time = 0.0f;
			while (time < 1.0f)
            {
				time += Time.deltaTime;
				readyBackground.localScale += Vector3.one * (0.5f * Time.deltaTime);
				yield return null;
            }
        }

        readyText.text = "Start!";
		yield return new WaitForSeconds (0.5f); //0.5f

		readyBackground.gameObject.SetActive (false);
		StartCoroutine (CountDown ());

	}

	private IEnumerator CountDown()
	{
		bombButton.gameObject.SetActive (true);
		orderChangeButton.gameObject.SetActive (true);

		timerText.text = gameTimer.ToString ();
		
		while (gameTimer > 0)
		{
			yield return new WaitForSeconds (1f);
			gameTimer--;
			timerText.text = gameTimer.ToString ();
		}

		yield return new WaitForSeconds (0.01f);
		StartCoroutine(GameOver());
	}

	private IEnumerator GameOver()
	{
		gameOverObject.SetActive (true);
		yield return new WaitForSeconds (2f);
	}
}
