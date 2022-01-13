using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
	private Image rank;
	private int childCount;

	public Text[] nameText;
	public Text[] scoreText;

	void Start ()
	{
		rank = transform.Find("Rank").GetComponent<Image> ();
		childCount = rank.transform.childCount;

		for (int i = 0; i < 9; ++i)
		{
			string prefsName = "Name" + i.ToString ();
			string prefsScore = "Score" + i.ToString ();

			nameText [i].text = PlayerPrefs.GetString (prefsName, "none");
			scoreText [i].text = PlayerPrefs.GetInt (prefsScore, 0).ToString ();
		}
	}

	public void EasyButtonDown ()
	{
		PuzzleManager.colorCount = 4;
		SceneManager.LoadScene ("Play");
	}

	public void NormalButtonDown ()
	{
		PuzzleManager.colorCount = 5;
		SceneManager.LoadScene ("Play");
	}

	public void HardButtonDown ()
	{
		PuzzleManager.colorCount = 7;
		SceneManager.LoadScene ("Play");
	}

	public void GameQuitButtonDown()
	{
		Application.Quit();
	}

	public void RankButtonDown ()
	{
		StartCoroutine (ShowRank ());
	}

	private IEnumerator ShowRank()
	{
		float alpha = 0f;
		rank.color = new Color(1, 1, 1, alpha);
		rank.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.01f);
		while (alpha < 1f)
		{
			alpha += 0.025f;
			yield return new WaitForSeconds(0.01f);
			rank.color = new Color(1, 1, 1, alpha);
		}

		for (int i = 0; i < childCount; ++i)
		{
			rank.transform.GetChild(i).gameObject.SetActive(true);
			yield return new WaitForSeconds(0.02f);
		}
	}

	public void RankResetButtonDown ()
	{
		PlayerPrefs.DeleteAll ();

		for (int i = 0; i < 9; ++i)
		{
			nameText[i].text = "none";
			scoreText[i].text = "0";
		}
	}

	public void RankQuitButtonDown ()
	{
		rank.gameObject.SetActive (false);

		for (int i = 0; i < childCount; ++i)
		{
			rank.transform.GetChild (i).gameObject.SetActive (false);
		}
	}
}