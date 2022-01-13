using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
	public InputField rowInputField;
	public InputField columnInputField;
	public InputField colorTypeInputField;

	public void StartButtonDown ()
	{
		int rowCount, columnCount, colorCount;

		if (!int.TryParse(rowInputField.text, out rowCount))
		{
			rowCount = 7;
		}

		if (!int.TryParse(columnInputField.text, out columnCount))
		{
			columnCount = 7;
		}

		if (!int.TryParse(colorTypeInputField.text, out colorCount))
		{
			colorCount = 3;
		}

		PuzzleManager.gameBoardRow = rowCount;
		PuzzleManager.gameBoardColumn = columnCount;
		PuzzleManager.colorCount = colorCount;

		SceneManager.LoadScene ("Play");
	}

	public void GameQuitButtonDown()
	{
		Application.Quit();
	}
}