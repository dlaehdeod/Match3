using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour 
{
	public PlayerController playerController;

	public GameObject gameOverObject;
	public GameObject reGameObject;
	public GameObject tipObject;
	
	public Transform readyBackground;
	public Text readyText;


	private Text timerText;
	private int gameTimer;

	private void Start ()
	{
		timerText = transform.GetChild(0).GetComponent<Text>();
		gameTimer = 60;
		playerController.gameOver = false;

		StartCoroutine (StartMessage ());
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

		tipObject.SetActive(false);
		readyBackground.gameObject.SetActive (false);
		StartCoroutine (CountDown ());

	}

	private IEnumerator CountDown()
	{
		timerText.text = gameTimer.ToString ();
		
		while (gameTimer > 0)
		{
			yield return new WaitForSeconds (1f);
			gameTimer--;
			timerText.text = gameTimer.ToString();
		}

		yield return new WaitForSeconds (0.01f);
		StartCoroutine(GameOver());
	}

	private IEnumerator GameOver()
	{
		gameOverObject.SetActive (true);
		playerController.gameOver = true;
		yield return new WaitForSeconds(1.0f);
		reGameObject.SetActive(true);

		while (true)
        {
			if (Input.GetMouseButtonDown(0))
            {
				playerController.ReGame();
            }
			yield return null;
        }
	}
}
