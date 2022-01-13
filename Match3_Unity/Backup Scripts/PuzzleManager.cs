using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public struct QueueNode
    {
        public QueueNode (List<PuzzleObject> matchedList, List<PuzzleObject> moveList)
        {
            this.matchedList = matchedList;
            this.moveList = moveList;
        }

        public List<PuzzleObject> matchedList;
        public List<PuzzleObject> moveList;
    }

    public static int moveCount = 0;
    public static int colorCount = 4;

    public PlayerController playerController;
    public PuzzleObjectPool puzzleObjectPool;
    public Color[] iconColors;

	public GameObject puzzleObject;
    public GameObject testTextObject;

    public RectTransform border;
    public RectTransform topBlind;

    public float swapSpeed = 7;
    public float moveDownSpeed = 5;
    public float fadeOutSpeed = 3;

    public int gameBoardRow = 10;
    public int gameBoardColumn = 10;

    public Sprite starSprite;
    public Sprite circleSprite;

    private Queue<QueueNode> queue;

    private PuzzleObject[,] puzzleObjects;
    private MatchedType[,] matchedTypes;
    private Text[,] testTexts;
    private int[] usingLine;

    private float iconSize;
    private float leftPosition;
    private float topPosition;

    private bool isMatching;

    private void Awake()
    {
        queue = new Queue<QueueNode>();

        puzzleObjects = new PuzzleObject[gameBoardRow, gameBoardColumn];
        matchedTypes = new MatchedType[gameBoardRow, gameBoardColumn];
        testTexts = new Text[gameBoardRow, gameBoardColumn];
        usingLine = new int[gameBoardColumn];

        for (int i = 0; i < gameBoardColumn; ++i)
        {
            usingLine[i] = -1;
        }

        CreateIcon();
        SetBorderSize();
        SetBlindTransform();
        SetIconColor();

        CreateTestText();

        playerController.Initialize(iconSize);
    }

    private void Update ()
    {
        if (queue.Count > 0 && isMatching == false)
        {
            QueueNode queueNode = queue.Dequeue();

            isMatching = true;
            StartCoroutine(MatchingAndFill(queueNode.matchedList, queueNode.moveList));
        }
    }

    private void CreateIcon()
	{
        RectTransform iconRect = puzzleObject.GetComponent<RectTransform>();
        iconSize = Screen.width / (gameBoardColumn + 2);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        leftPosition = -gameBoardColumn / 2 * iconSize;
		topPosition = gameBoardRow / 2 * iconSize;

        if (gameBoardRow % 2 == 0)
        {
            topPosition -= iconSize / 2;
        }

        if (gameBoardColumn % 2 == 0)
        {
            leftPosition += iconSize / 2;
        }

        for (int row = 0; row < gameBoardRow; ++row)
		{
			for (int column = 0; column < gameBoardColumn; ++column)
			{
                Vector2 newPos = GetCurrentPosition(row, column);

                GameObject newObject = Instantiate(puzzleObject, transform);
                newObject.transform.localPosition = newPos;

                PuzzleObject newPuzzleObject = newObject.GetComponent<PuzzleObject>();
                newPuzzleObject.SetRow(row);
                newPuzzleObject.SetColumn(column);
                newPuzzleObject.SetCircleSprite(circleSprite);
                newPuzzleObject.SetStarSprite(starSprite);

                puzzleObjects[row, column] = newPuzzleObject;
                puzzleObjectPool.Add(newPuzzleObject);
            }
		}
	}

    private Vector2 GetCurrentPosition (int row, int column)
    {
        return new Vector2(leftPosition + iconSize * column, topPosition - iconSize * row);
    }

    private void CreateTestText ()
    {
        Transform textParent = transform.Find("TextParent");

        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                Vector2 newPos = GetCurrentPosition(row, column);

                Text newText = Instantiate(testTextObject, textParent).GetComponent<Text>();
                newText.text = puzzleObjects[row, column].colorType.ToString();
                newText.transform.localPosition = newPos;
                testTexts[row, column] = newText;
            }
        }

        textParent.SetAsLastSibling();
    }

    private void ShowTestTextState (int row, int column, string state)
    {
        testTexts[row, column].text = state;
    }

    private void SetBorderSize()
    {
        RectTransform puzzleRect = puzzleObject.GetComponent<RectTransform>();
        float iconWidth = puzzleRect.rect.width;
        float iconHeight = puzzleRect.rect.height;

        border.sizeDelta = new Vector2(gameBoardColumn * iconWidth, gameBoardRow * iconHeight);
    }

    private void SetBlindTransform ()
    {
        topBlind.GetComponent<Image>().color = Camera.main.backgroundColor;
        topBlind.localPosition = new Vector2(0.0f, border.localPosition.y + border.sizeDelta.y);
        topBlind.sizeDelta = new Vector2(topBlind.sizeDelta.x, border.sizeDelta.y);
    }

	private void SetIconColor()
	{
		for (int row = 0; row < gameBoardRow; ++row)
		{
			for (int column = 0; column < gameBoardColumn; ++column)
			{
				int colorType = GetNotOverlapedRandomColorType(row, column);
                
                puzzleObjects[row, column].SetColorType(colorType);
                puzzleObjects[row, column].SetColor(iconColors[colorType]);
			}
		}
	}

    private int GetNotOverlapedRandomColorType(int row, int column)
    {
        int randomColorType = Random.Range(0, colorCount);
        
        if (row > 1 && column > 1)
        {
            while ((randomColorType == puzzleObjects[row - 1, column].colorType && randomColorType == puzzleObjects[row - 2, column].colorType) ||
                   (randomColorType == puzzleObjects[row, column - 1].colorType && randomColorType == puzzleObjects[row, column - 2].colorType))
            {
                randomColorType = Random.Range(0, colorCount);
            }
        }
        else if (row > 1)
        {
            while (randomColorType == puzzleObjects[row - 1, column].colorType && randomColorType == puzzleObjects[row - 2, column].colorType)
            {
                randomColorType = Random.Range(0, colorCount);
            }
        }
        else if (column > 1)
        {
            while (randomColorType == puzzleObjects[row, column - 1].colorType && randomColorType == puzzleObjects[row, column - 2].colorType)
            {
                randomColorType = Random.Range(0, colorCount);
            }
        }

        return randomColorType;
    }

    private void CheckNextMatching()
    {
        List<PuzzleObject> matchedList = new List<PuzzleObject>();
        
        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                if (matchedTypes[row, column] == MatchedType.None)
                {
                    if (CheckMatching(row, column))
                    {
                        List<PuzzleObject> subList = CrashPuzzle();

                        matchedList.AddRange(subList);
                    }
                }
            }
        }

        if (matchedList.Count == 0)
        {
            return;
        }

        List<PuzzleObject> moveList = FillPuzzle();
        queue.Enqueue(new QueueNode(matchedList, moveList));
    }

    private IEnumerator MatchingAndFill (List<PuzzleObject> matchedList, List<PuzzleObject> moveList)
    {
        float time = 0.0f;
        
        while (true)
        {
            time += Time.deltaTime * fadeOutSpeed;

            for (int i = 0; i < matchedList.Count; ++i)
            {
                matchedList[i].FadeOut(time);
            }

            yield return null;

            if (time > 1)
            {
                break;
            }
        }

        matchedList.Clear();
        yield return new WaitForSeconds(0.1f);
        
        time = 0.0f;

        while (true)
        {
            time += Time.deltaTime * moveDownSpeed;

            for (int i = 0; i < moveList.Count; ++i)
            {
                moveList[i].Move(time);
            }

            yield return null;

            if (time > 1)
            {
                break;
            }
        }
        
        moveList.Clear();
        isMatching = false;
        yield return null;
        
        for (int column = 0; column < gameBoardColumn; ++column)
        {
            ResetUsingLine(column);
        }

        CheckNextMatching();
    }

    public bool IsUsingLine (PuzzleObject selectedPuzzleObject)
    {
        int row = selectedPuzzleObject.GetRow();
        int column = selectedPuzzleObject.GetColumn();

        return usingLine[column] >= row;
    }

    public SwapType TrySwap(PuzzleObject firstPuzzleObject, PuzzleObject secondPuzzleObject)
    {
        if (IsUsingLine(secondPuzzleObject))
        {
            return SwapType.Using;
        }

        if (firstPuzzleObject.IsAdjacent(secondPuzzleObject) == false)
        {
            return SwapType.Fail;
        }

        IconSwap(firstPuzzleObject, secondPuzzleObject);
        return SwapType.Success;
    }

    private void IconSwap (PuzzleObject firstPuzzleObject, PuzzleObject secondPuzzleObject)
    {
        Transform firstSelectedTransform = firstPuzzleObject.transform;
        Transform secondSelectedTransform = secondPuzzleObject.transform;

        int firstRow = firstPuzzleObject.GetRow();
        int firstColumn = firstPuzzleObject.GetColumn();
        int secondRow = secondPuzzleObject.GetRow();
        int secondColumn = secondPuzzleObject.GetColumn();

        Vector2 firstSelectedPosition = GetCurrentPosition(firstRow, firstColumn);
        Vector2 secondSelectedPosition = GetCurrentPosition(secondRow, secondColumn);

        puzzleObjects[firstRow, firstColumn] = secondPuzzleObject;
        puzzleObjects[secondRow, secondColumn] = firstPuzzleObject;

        bool successMatching = CheckMatching(firstRow, firstColumn) | CheckMatching(secondRow, secondColumn);
        
        StartCoroutine(SwapMoving(firstSelectedTransform, firstSelectedPosition, secondSelectedTransform, secondSelectedPosition, successMatching));

        if (successMatching)
        {
            firstPuzzleObject.SetRow(secondRow);
            firstPuzzleObject.SetColumn(secondColumn);
            secondPuzzleObject.SetRow(firstRow);
            secondPuzzleObject.SetColumn(firstColumn);

            List<PuzzleObject> matchedList = CrashPuzzle();
            List<PuzzleObject> moveList = FillPuzzle();

            queue.Enqueue(new QueueNode(matchedList, moveList));
        }
        else
        {
            puzzleObjects[firstRow, firstColumn] = firstPuzzleObject;
            puzzleObjects[secondRow, secondColumn] = secondPuzzleObject;
        }
    }
    private void UpdateUsingLine (int row, int column)
    {
        if (usingLine[column] < row)
        {
            usingLine[column] = row;
        }
    }

    private void ResetUsingLine (int column)
    {
        usingLine[column] = -1;
    }

    private IEnumerator SwapMoving (Transform firstSelectedTransform, Vector2 firstSelectedPosition, Transform secondSelectedTransform, Vector2 secondSelectedPosition, bool successMatching)
    {
        float time = 0.0f;
        
        while (time < 1.0f)
        {
            time += Time.deltaTime * swapSpeed;
            firstSelectedTransform.localPosition = Vector2.Lerp(firstSelectedPosition, secondSelectedPosition, time);
            secondSelectedTransform.localPosition = Vector2.Lerp(secondSelectedPosition, firstSelectedPosition, time);
            yield return null;
        }

        firstSelectedTransform.localPosition = secondSelectedPosition;
        secondSelectedTransform.localPosition = firstSelectedPosition;

        yield return null;

        if (successMatching == false)
        {
            StartCoroutine(SwapMoving(firstSelectedTransform, secondSelectedPosition, secondSelectedTransform, firstSelectedPosition, true));
        }
    }

    private bool CheckMatching (int row, int column)
    {
        bool isMatched = false;

        List<int> rowList = GetRowMatchedPosition(row, column);
        List<int> columnList = GetColumnMatchedPosition(row, column);

        int rowMatchedCount = rowList.Count;
        int columnMatchedCount = columnList.Count;

        MatchedType type = GetMatchedType(rowMatchedCount, columnMatchedCount);

        if (rowMatchedCount >= 3)
        {
            isMatched = true;
            for (int index = 0; index < rowMatchedCount; ++index)
            {
                int matchedRow = rowList[index];
                matchedTypes[matchedRow, column] = type;

                UpdateUsingLine(matchedRow, column);
            }
        }

        if (columnMatchedCount >= 3)
        {
            isMatched = true;
            for (int index = 0; index < columnMatchedCount; ++index)
            {
                int matchedColumn = columnList[index];

                matchedTypes[row, matchedColumn] = type;

                UpdateUsingLine(row, matchedColumn);
            }
        }

        return isMatched;
    }

    private MatchedType GetMatchedType (int rowCount, int columnCount)
    {
        MatchedType type = MatchedType.None;

        switch (rowCount)
        {
            case 3:
                type |= MatchedType.Row_Three;
                break;
            case 4:
                type |= MatchedType.Row_Four;
                break;
            case 5:
                type |= MatchedType.Row_Five;
                break;
            default:
                break;
        }

        switch (columnCount)
        {
            case 3:
                type |= MatchedType.Column_Three;
                break;
            case 4:
                type |= MatchedType.Column_Four;
                break;
            case 5:
                type |= MatchedType.Column_Five;
                break;
            default:
                break;
        }

        return type;
    }

    private List<PuzzleObject> CrashPuzzle ()
    {
        List<PuzzleObject> matchedList = new List<PuzzleObject>();

        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                switch (matchedTypes[row, column])
                {
                    //case MatchedType.Row_Three:
                    //case MatchedType.Column_Three:

                    //    break;
                    //case MatchedType.Row_Four:
                    //case MatchedType.Column_Four:

                    //    break;
                    //case MatchedType.Row_Five:
                    //case MatchedType.Column_Five:

                    //    break;
                    //case MatchedType.Cross_Three_Three:

                    //    break;
                    //case MatchedType.Cross_Three_Four:
                    //case MatchedType.Cross_Four_Three:

                    //    break;
                    //case MatchedType.Cross_Three_Five:
                    //case MatchedType.Cross_Five_Three:

                    //    break;
                    //case MatchedType.Cross_Four_Four:

                    //    break;
                    //case MatchedType.Cross_Four_Five:
                    //case MatchedType.Cross_Five_Four:

                    //    break;
                    //case MatchedType.Cross_Five_Five:

                    //    break;
                    case MatchedType.None:
                        break;
                    default:
                        matchedList.Add(puzzleObjects[row, column]);
                        matchedTypes[row, column] = MatchedType.EmptySpace;
                        ShowTestTextState(row, column, puzzleObjects[row, column].colorType.ToString());
                        break;
                }
            }
        }

        return matchedList;
    }

    private List<PuzzleObject> FillPuzzle ()
    {
        List<PuzzleObject> moveList = new List<PuzzleObject>();

        for (int column = 0; column < gameBoardColumn; ++column)
        {
            int topPositionOffset = 0;
            
            for (int row = gameBoardRow - 1; row >= 0; --row)
            {
                int notEmptyRow = row - 1;

                if (matchedTypes[row, column] == MatchedType.EmptySpace)
                {
                    while (notEmptyRow >= 0 && matchedTypes[notEmptyRow, column] == MatchedType.EmptySpace)
                    {
                        notEmptyRow--;
                    }

                    PuzzleObject target;

                    if (notEmptyRow < 0)
                    {
                        topPositionOffset++;
                        Vector2 startPosition = new Vector2(leftPosition + iconSize * column, topPosition + iconSize * topPositionOffset);
                        int newColorType = Random.Range(0, colorCount);

                        target = puzzleObjectPool.GetPuzzleObject();
                        target.SetStartPosition(startPosition);
                        target.ReadyToReuse();
                        target.SetColorType(newColorType);
                        target.SetColor(iconColors[newColorType]);
                    }
                    else
                    {
                        Vector2 startPosition = new Vector2(leftPosition + iconSize * column, topPosition - iconSize * notEmptyRow);

                        target = puzzleObjects[notEmptyRow, column];
                        target.SetStartPosition(startPosition);
                        matchedTypes[notEmptyRow, column] = MatchedType.EmptySpace;
                    }

                    Vector2 endPosition = new Vector2(leftPosition + iconSize * column, topPosition - iconSize * row);

                    target.SetEndPosition(endPosition);
                    target.SetRow(row);
                    target.SetColumn(column);

                    puzzleObjects[row, column] = target;
                    matchedTypes[row, column] = MatchedType.None;
                    ShowTestTextState(row, column, target.colorType.ToString());

                    moveList.Add(target);
                }
            }
        }

        return moveList;
    }

    private List<int> GetRowMatchedPosition (int row, int column)
    {
        List<int> rowList = new List<int>();
        int baseColorType = puzzleObjects[row, column].colorType;

        rowList.Add(row);

        for (int up = row - 1; up >= 0; --up)
        {
            if (baseColorType == puzzleObjects[up, column].colorType)
            {
                rowList.Add(up);
            }
            else
            {
                break;
            }
        }

        for (int down = row + 1; down < gameBoardRow; ++down)
        {
            if (baseColorType == puzzleObjects[down, column].colorType)
            {
                rowList.Add(down);
            }
            else
            {
                break;
            }
        }

        return rowList;
    }

    private List<int> GetColumnMatchedPosition (int row, int column)
    {
        List<int> columnList = new List<int>();

        int baseColorType = puzzleObjects[row, column].colorType;

        columnList.Add(column);

        for (int left = column - 1; left >= 0; --left)
        {
            if (baseColorType == puzzleObjects[row, left].colorType)
            {
                columnList.Add(left);
            }
            else
            {
                break;
            }
        }

        for (int right = column + 1; right < gameBoardColumn; ++right)
        {
            if (baseColorType == puzzleObjects[row, right].colorType)
            {
                columnList.Add(right);
            }
            else
            {
                break;
            }
        }

        return columnList;
    }

    public bool MoveToDirection (Transform firstSelectedTransform, int directionRow, int directionColumn)
    {
        PuzzleObject firstPuzzleObject = firstSelectedTransform.GetComponent<PuzzleObject>();
        
        int moveRow = firstPuzzleObject.GetRow() + directionRow;
        int moveColumn = firstPuzzleObject.GetColumn() + directionColumn;

        if (IsValidRange(moveRow, moveColumn) == false)
        {
            return false;
        }

        TrySwap(firstPuzzleObject, puzzleObjects[moveRow, moveColumn]);

        return true;
    }

    public bool IsValidRange (int row, int column)
    {
        return 0 <= row && row < gameBoardRow && 0 <= column && column < gameBoardColumn;
    }
}