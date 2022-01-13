using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    public struct QueueNode
    {
        public QueueNode(List<PuzzleObject> matchedList, List<FillNode> moveList)
        {
            this.matchedList = matchedList;
            this.moveList = moveList;
        }

        public List<PuzzleObject> matchedList;
        public List<FillNode> moveList;
    }

    public struct FillNode
    {
        public FillNode(Transform targetTransform, Vector3 startPosition, Vector3 endPosition)
        {
            this.targetTransform = targetTransform;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
        }

        public void Move(float time)
        {
            targetTransform.localPosition = Vector3.Lerp(startPosition, endPosition, time);
        }

        public Transform targetTransform;
        public Vector3 startPosition;
        public Vector3 endPosition;
    }

    public struct MatchingInfo
    {
        public MatchingInfo(int row, int column, MatchedType matchedType, SearchType searchType = SearchType.None)
        {
            this.row = row;
            this.column = column;
            this.matchedType = matchedType;
            this.searchType = searchType;
        }

        public int row;
        public int column;
        public MatchedType matchedType;
        public SearchType searchType;
    }

    public static int moveCount = 0;
    public static int colorCount = 3;
    public static int gameBoardRow = 7;
    public static int gameBoardColumn = 7;

    public PlayerController playerController;
    public PuzzleObjectPool puzzleObjectPool;

    public Color[] iconColors;

    public GameObject puzzleGrid;
	public GameObject puzzleObject;

    public RectTransform border;
    public RectTransform topBlind;

    public float swapSpeed = 7;
    public float moveDownSpeed = 5;
    public float fadeOutSpeed = 3;

    public Sprite starSprite;
    public Sprite circleSprite;
    public Sprite polySprite;

    public Text scoreText;
    public Text comboText;
    public Animator comboAnimator;

    private readonly int[] moveRow = new int[] { -1, 0, 1, 0 };
    private readonly int[] moveColumn = new int[] { 0, 1, 0, -1 };

    private Queue<QueueNode> matchingQueue;
    private Queue<MatchingInfo> bombQueue;
    private List<MatchingInfo> swapMatchingList;

    private PuzzleObject[,] puzzleObjects;
    private MatchedType[,] matchedTypes;
    private SearchType[,] searchTypes;
    
    private float iconSize;
    private float leftPosition;
    private float topPosition;

    private bool isMatching;
    private int matchingPass;

    private long totalScore;
    private int combo;
    private float comboTime;

    private void Start()
    {
        gameBoardRow = Mathf.Clamp(gameBoardRow, 3, 10);
        gameBoardColumn = Mathf.Clamp(gameBoardColumn, Mathf.Max(3, gameBoardRow - 3), 10);
        colorCount = Mathf.Clamp(colorCount, 3, iconColors.Length);

        matchingQueue = new Queue<QueueNode>();
        bombQueue = new Queue<MatchingInfo>();
        swapMatchingList = new List<MatchingInfo>();

        puzzleObjects = new PuzzleObject[gameBoardRow, gameBoardColumn];
        matchedTypes = new MatchedType[gameBoardRow, gameBoardColumn];
        searchTypes = new SearchType[gameBoardRow, gameBoardColumn];

        matchingPass = -1;
        Initialize();
    }

    private void Initialize ()
    {
        CanvasScaler canvasScaler = transform.parent.GetComponent<CanvasScaler>();
        RectTransform iconRect = puzzleObject.GetComponent<RectTransform>();

        Vector2 scale = canvasScaler.referenceResolution;
        iconSize = scale.x / (gameBoardColumn + 2);

        iconRect.sizeDelta = new Vector2(iconSize, iconSize);

        RectTransform gridRect = puzzleGrid.GetComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(iconSize, iconSize);

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

        playerController.Initialize(iconSize);
        
        CreateGrids();
        CreateIcons();
        SetIconColors();
        SetBorderSize();
        SetBlindPosition();
    }

    private Vector2 GetCurrentPosition(int row, int column)
    {
        return new Vector2(leftPosition + iconSize * column, topPosition - iconSize * row);
    }

    private void CreateGrids ()
    {
        Transform gridParent = transform.Find("GridParent");

        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                Vector2 newPos = GetCurrentPosition(row, column);

                GameObject newGrid = Instantiate(puzzleGrid, gridParent);
                newGrid.transform.localPosition = newPos;

                PuzzleGrid newPuzzleGrid = newGrid.GetComponent<PuzzleGrid>();
                newPuzzleGrid.row = row;
                newPuzzleGrid.column = column;
            }
        }
    }

    private void CreateIcons()
	{
        for (int row = 0; row < gameBoardRow; ++row)
		{
			for (int column = 0; column < gameBoardColumn; ++column)
			{
                Vector2 newPos = GetCurrentPosition(row, column);

                GameObject newObject = Instantiate(puzzleObject, transform);
                newObject.transform.localPosition = newPos;

                PuzzleObject newPuzzleObject = newObject.GetComponent<PuzzleObject>();
                newPuzzleObject.SetCircleSprite(circleSprite);
                newPuzzleObject.SetStarSprite(starSprite);
                newPuzzleObject.SetPolySprite(polySprite);

                puzzleObjects[row, column] = newPuzzleObject;
                puzzleObjectPool.Add(newPuzzleObject);
            }
		}
	}

    private void SetIconColors()
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

        CheckCanSwapPuzzle();
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

    private void SetBorderSize()
    {
        RectTransform puzzleRect = puzzleObject.GetComponent<RectTransform>();
        float iconWidth = puzzleRect.rect.width;
        float iconHeight = puzzleRect.rect.height;

        border.sizeDelta = new Vector2(gameBoardColumn * iconWidth, gameBoardRow * iconHeight);
    }

    private void SetBlindPosition ()
    {
        topBlind.GetComponent<Image>().color = Camera.main.backgroundColor;
        topBlind.localPosition = new Vector2(0.0f, border.localPosition.y + border.sizeDelta.y);
        topBlind.sizeDelta = new Vector2(topBlind.sizeDelta.x, border.sizeDelta.y);
    }

    private void Update()
    {
        if (matchingQueue.Count > 0 && isMatching == false)
        {
            QueueNode queueNode = matchingQueue.Dequeue();

            isMatching = true;
            StartCoroutine(MatchingAndFill(queueNode.matchedList, queueNode.moveList));
        }
        else if (comboTime > 0.0f)
        {
            comboTime -= Time.deltaTime;
        }
        else
        {
            combo = 0;
        }
    }

    private IEnumerator MatchingAndFill (List<PuzzleObject> matchedList, List<FillNode> moveList)
    {
        yield return new WaitForSeconds(0.1f);
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
        yield return null;
        
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

        CheckNextMatching();
        yield return null;
    }

    private void CheckNextMatching()
    {
        List<MatchingInfo> matchingInfoList = new List<MatchingInfo>();
        ResetSearchTypesAndUsingLine();

        if (swapMatchingList.Count > 0)
        {
            UpdateMatching(swapMatchingList);
            swapMatchingList.Clear();
        }

        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                Matching(matchingInfoList, row, column);
            }
        }

        while (bombQueue.Count > 0)
        {
            MatchingInfo bombInfo = bombQueue.Dequeue();
            matchingInfoList.AddRange(GetBombMatching(bombInfo.row, bombInfo.column));
        }

        UpdateMatching(matchingInfoList);
        List<PuzzleObject> matchedList = CrashPuzzle();

        if (matchedList.Count == 0)
        {
            CheckCanSwapPuzzle();

            isMatching = false;
            return;
        }

        List<FillNode> moveList = FillPuzzle();
        isMatching = false;
        matchingQueue.Enqueue(new QueueNode(matchedList, moveList));
    }
    
    private void CheckCanSwapPuzzle()
    {
        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                if (SwapIsPossible(row, column))
                {
                    return;
                }
            }
        }

        SetIconColors();
    }    

    private bool SwapIsPossible(int row, int column)
    {
        PuzzleObject firstPuzzleObject = puzzleObjects[row, column];

        for (int i = 0; i < 4; ++i)
        {
            int newRow = moveRow[i] + row;
            int newColumn = moveColumn[i] + column;

            if (IsValidRange(newRow, newColumn))
            {
                PuzzleObject secondPuzzleObject = puzzleObjects[newRow, newColumn];

                puzzleObjects[row, column] = secondPuzzleObject;
                puzzleObjects[newRow, newColumn] = firstPuzzleObject;

                int rowStart = row;
                int rowEnd = row + 1;
                int columnStart = column;
                int columnEnd = column + 1;
                int baseColorType = puzzleObjects[row, column].colorType;
                
                SetRowMatchingRange(baseColorType, column, ref rowStart, ref rowEnd);
                SetColumnMatchingRange(baseColorType, row, ref columnStart, ref columnEnd);

                puzzleObjects[row, column] = firstPuzzleObject;
                puzzleObjects[newRow, newColumn] = secondPuzzleObject;

                if (rowEnd - rowStart >= 3 || columnEnd - columnStart >= 3)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ResetSearchTypesAndUsingLine()
    {
        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                searchTypes[row, column] = SearchType.None;
                puzzleObjects[row, column].isUsing = false;
            }
        }
    }

    private void UpdateMatching(List<MatchingInfo> matchingInfoList)
    {
        int count = matchingInfoList.Count;

        for (int i = 0; i < count; ++i)
        {
            int row = matchingInfoList[i].row;
            int column = matchingInfoList[i].column;
            MatchedType matchedType = matchingInfoList[i].matchedType;
            SearchType searchType = matchingInfoList[i].searchType;

            searchTypes[row, column] |= searchType;
            puzzleObjects[row, column].isUsing = true;

            if (puzzleObjects[row, column].iconType == IconType.Four)
            {
                List<MatchingInfo> subMatchingInfoList = GetCrossMatching(row, column);
                UpdateMatching(subMatchingInfoList);
            }
            else if (puzzleObjects[row, column].iconType == IconType.Five)
            {
                List<MatchingInfo> subMatchingInfoList = GetBombMatching(row, column);
                UpdateMatching(subMatchingInfoList);
            }

            if (CheckOverrideType(row, column, matchedType))
            {
                matchedTypes[row, column] = matchedType;
            }
        }
    }

    private bool CheckOverrideType(int row, int column, MatchedType type)
    {
        return (int)matchedTypes[row, column] < (int)type;
    }

    private void Matching(List<MatchingInfo> matchingInfo, int row, int column)
    {
        int baseColor = puzzleObjects[row, column].colorType;
        int nextRow = row + 1;
        int nextColumn = column + 1;

        SearchType currentType = searchTypes[row, column];

        if (currentType == SearchType.Both)
        {
            return;
        }

        if (((int)currentType & (int)SearchType.Row) == 0)
        {
            while (nextRow < gameBoardRow && baseColor == puzzleObjects[nextRow, column].colorType)
            {
                nextRow++;
            }
        }

        if (((int)currentType & (int)SearchType.Column) == 0)
        {
            while (nextColumn < gameBoardColumn && baseColor == puzzleObjects[row, nextColumn].colorType)
            {
                nextColumn++;
            }
        }

        int rowMatchedCount = nextRow - row;
        int columnMatchedCount = nextColumn - column;

        if (rowMatchedCount >= 3)
        {
            for (int r = row; r < nextRow; ++r)
            {
                matchingInfo.Add(new MatchingInfo(r, column, MatchedType.Crash));
                searchTypes[r, column] |= SearchType.Row;
            }
        }

        if (columnMatchedCount >= 3)
        {
            for (int c = column; c < nextColumn; ++c)
            {
                matchingInfo.Add(new MatchingInfo(row, c, MatchedType.Crash));
                searchTypes[row, c] |= SearchType.Column;
            }
        }

        if (rowMatchedCount >= 3 || columnMatchedCount >= 3)
        {
            MatchedType type = GetMatchedType(rowMatchedCount, columnMatchedCount);
            matchingInfo.Add(new MatchingInfo(row, column, type));
        }
    }

    private List<PuzzleObject> CrashPuzzle()
    {
        int score = 0;
        List<PuzzleObject> crashList = new List<PuzzleObject>();

        for (int row = 0; row < gameBoardRow; ++row)
        {
            for (int column = 0; column < gameBoardColumn; ++column)
            {
                switch (matchedTypes[row, column])
                {
                    case MatchedType.Blank:

                        break;

                    case MatchedType.None:

                        break;

                    case MatchedType.Crash:

                        crashList.Add(puzzleObjects[row, column]);
                        matchedTypes[row, column] = MatchedType.Blank;
                        score++;
                        break;

                    case MatchedType.MakeFour:

                        puzzleObjects[row, column].iconType = IconType.Four;
                        matchedTypes[row, column] = MatchedType.None;
                        crashList.Add(puzzleObjects[row, column]);
                        score++;
                        break;

                    default:

                        puzzleObjects[row, column].colorType = matchingPass--;
                        puzzleObjects[row, column].iconType = IconType.Five;
                        matchedTypes[row, column] = MatchedType.None;
                        crashList.Add(puzzleObjects[row, column]);
                        score++;
                        break;
                }
            }
        }

        UpdateScore(score);

        return crashList;
    }

    private List<FillNode> FillPuzzle()
    {
        List<FillNode> moveList = new List<FillNode>();

        for (int column = 0; column < gameBoardColumn; ++column)
        {
            int topPositionOffset = 0;

            for (int row = gameBoardRow - 1; row >= 0; --row)
            {
                int upRow = row - 1;

                if (matchedTypes[row, column] == MatchedType.Blank)
                {
                    puzzleObjects[row, column].isUsing = true;

                    while (upRow >= 0 && matchedTypes[upRow, column] == MatchedType.Blank)
                    {
                        upRow--;
                    }

                    PuzzleObject target;
                    Vector2 startPosition;

                    if (upRow < 0)
                    {
                        topPositionOffset++;
                        startPosition = new Vector2(leftPosition + iconSize * column, topPosition + iconSize * topPositionOffset);

                        int newColorType = Random.Range(0, colorCount);

                        target = puzzleObjectPool.GetPuzzleObject();
                        target.ReadyToReuse(startPosition);
                        target.SetColorType(newColorType);
                        target.SetColor(iconColors[newColorType]);
                    }
                    else
                    {
                        startPosition = GetCurrentPosition(upRow, column);

                        target = puzzleObjects[upRow, column];
                        matchedTypes[upRow, column] = MatchedType.Blank;
                    }

                    Vector2 endPosition = GetCurrentPosition(row, column);

                    puzzleObjects[row, column] = target;
                    matchedTypes[row, column] = MatchedType.None;

                    moveList.Add(new FillNode(target.transform, startPosition, endPosition));
                }
            }
        }

        return moveList;
    }

    private void UpdateScore (int score)
    {
        if (score == 0)
        {
            return;
        }

        UpdateCombo();
        totalScore += score * combo;
        scoreText.text = string.Format("Score: {0}", totalScore);
    }

    private void UpdateCombo()
    {
        comboTime = 1.5f;
        
        combo++;
        comboText.text = string.Format("{0} Combo!", combo);
        comboAnimator.SetTrigger("Show");
    }

    public bool IsBombIconClick (int row, int column)
    {
        if (puzzleObjects[row, column].iconType == IconType.Five)
        {
            if (!isMatching)
            {
                List<MatchingInfo> matchingInfoList = GetBombMatching(row, column);

                UpdateMatching(matchingInfoList);

                List<PuzzleObject> matchedList = CrashPuzzle();
                List<FillNode> moveList = FillPuzzle();

                matchingQueue.Enqueue(new QueueNode(matchedList, moveList));
            }
            else
            {
                puzzleObjects[row, column].isUsing = true;
                puzzleObjects[row, column].iconType = IconType.Common;
                bombQueue.Enqueue(new MatchingInfo(row, column, MatchedType.Crash));
            }

            return true;
        }
        
        return false;
    }

    public bool SwapToDirection(int row, int column, int swapRow, int swapColumn)
    {
        if (IsValidRange(swapRow, swapColumn) == false)
        {
            return false;
        }

        TrySwap(row, column, swapRow, swapColumn);
        return true;
    }

    private bool IsValidRange(int row, int column)
    {
        return 0 <= row && row < gameBoardRow && 0 <= column && column < gameBoardColumn;
    }

    public SwapResult TrySwap(int firstRow, int firstColumn, int secondRow, int secondColumn)
    {
        if (IsUsingLine(firstRow, firstColumn) || IsUsingLine(secondRow, secondColumn))
        {
            return SwapResult.Using;
        }

        if (IsAdjacent(firstRow, firstColumn, secondRow, secondColumn) == false)
        {
            return SwapResult.NotAdjacent;
        }

        IconSwap(firstRow, firstColumn, secondRow, secondColumn);
        return SwapResult.Success;
    }

    public bool IsUsingLine(int row, int column)
    {
        if (puzzleObjects[row, column].isSwapping)
        {
            return true;
        }

        for (int upRow = row; upRow < gameBoardRow; ++upRow)
        {
            if (puzzleObjects[upRow, column].isUsing)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAdjacent (int firstRow, int firstColumn, int secondRow, int secondColumn)
    {
        return Mathf.Abs(firstRow - secondRow) + Mathf.Abs(firstColumn - secondColumn) == 1;
    }

    private void IconSwap (int firstRow, int firstColumn, int secondRow, int secondColumn)
    {
        Vector2 firstSelectedPosition = GetCurrentPosition(firstRow, firstColumn);
        Vector2 secondSelectedPosition = GetCurrentPosition(secondRow, secondColumn);

        PuzzleObject firstPuzzleObject = puzzleObjects[firstRow, firstColumn];
        PuzzleObject secondPuzzleObject = puzzleObjects[secondRow, secondColumn];
        puzzleObjects[firstRow, firstColumn] = secondPuzzleObject;
        puzzleObjects[secondRow, secondColumn] = firstPuzzleObject;
        
        List<MatchingInfo> matchingInfoList = CheckSwapMatching(firstRow, firstColumn);
        List<MatchingInfo> matchingInfoList2 = CheckSwapMatching(secondRow, secondColumn);

        matchingInfoList.AddRange(matchingInfoList2);
        bool successMatching = matchingInfoList.Count > 0;

        StartCoroutine(SwapMoving(firstPuzzleObject, firstSelectedPosition, secondPuzzleObject, secondSelectedPosition, successMatching));

        if (successMatching)
        {
            if (matchingQueue.Count == 0 && isMatching == false)
            {
                UpdateMatching(matchingInfoList);

                List<PuzzleObject> matchedList = CrashPuzzle();
                List<FillNode> moveList = FillPuzzle();

                matchingQueue.Enqueue(new QueueNode(matchedList, moveList));
            }
            else
            {
                swapMatchingList = matchingInfoList;
            }
        }
        else
        {
            puzzleObjects[firstRow, firstColumn] = firstPuzzleObject;
            puzzleObjects[secondRow, secondColumn] = secondPuzzleObject;
        }
    }

    private List<MatchingInfo> GetBombMatching (int row, int column)
    {
        List<MatchingInfo> bombMatchingInfo = new List<MatchingInfo>();

        puzzleObjects[row, column].iconType = IconType.Common;

        int rowStart = Mathf.Max(0, row - 2);
        int rowEnd = Mathf.Min(gameBoardRow, row + 3);
        int columnStart = Mathf.Max(0, column - 2);
        int columnEnd = Mathf.Min(gameBoardColumn, column + 3);

        for (int r = rowStart; r < rowEnd; ++r)
        {
            for (int c = columnStart; c < columnEnd; ++c)
            {
                bombMatchingInfo.Add(new MatchingInfo(r, c, MatchedType.Crash));
            }
        }

        return bombMatchingInfo;
    }

    private List<MatchingInfo> GetCrossMatching (int row, int column)
    {
        List<MatchingInfo> crossMatchingInfo = new List<MatchingInfo>();

        puzzleObjects[row, column].iconType = IconType.Common;

        for (int r = 0; r < gameBoardRow; ++r)
        {
            crossMatchingInfo.Add(new MatchingInfo(r, column, MatchedType.Crash));
        }

        for (int c = 0; c < gameBoardColumn; ++c)
        {
            crossMatchingInfo.Add(new MatchingInfo(row, c, MatchedType.Crash));
        }

        return crossMatchingInfo;
    }

    private IEnumerator SwapMoving (PuzzleObject firstPuzzleObject, Vector2 firstSelectedPosition, PuzzleObject secondPuzzleObject, Vector2 secondSelectedPosition, bool successMatching)
    {
        float time = 0.0f;
        firstPuzzleObject.isSwapping = true;
        secondPuzzleObject.isSwapping = true;

        while (time < 1.0f)
        {
            time += Time.deltaTime * swapSpeed;
            firstPuzzleObject.transform.localPosition = Vector2.Lerp(firstSelectedPosition, secondSelectedPosition, time);
            secondPuzzleObject.transform.localPosition = Vector2.Lerp(secondSelectedPosition, firstSelectedPosition, time);
            yield return null;
        }

        firstPuzzleObject.transform.localPosition = secondSelectedPosition;
        secondPuzzleObject.transform.localPosition = firstSelectedPosition;

        yield return null;

        if (successMatching == false)
        {
            StartCoroutine(SwapMoving(firstPuzzleObject, secondSelectedPosition, secondPuzzleObject, firstSelectedPosition, true));
        }
        else
        {
            firstPuzzleObject.isSwapping = false;
            secondPuzzleObject.isSwapping = false;
        }
    }

    private List<MatchingInfo> CheckSwapMatching (int row, int column)
    {
        List<MatchingInfo> matchingInfoList = new List<MatchingInfo>();

        int rowStart = row;
        int rowEnd = row + 1;
        int columnStart = column;
        int columnEnd = column + 1;
        int baseColorType = puzzleObjects[row, column].colorType;

        SetRowMatchingRange(baseColorType, column, ref rowStart, ref rowEnd);
        SetColumnMatchingRange(baseColorType, row, ref columnStart, ref columnEnd);

        int rowMatchedCount = rowEnd - rowStart;
        int columnMatchedCount = columnEnd - columnStart;

        if (rowMatchedCount >= 3)
        {
            while (rowStart < rowEnd)
            {
                searchTypes[rowStart, column] |= SearchType.Row;
                matchingInfoList.Add(new MatchingInfo(rowStart, column, MatchedType.Crash, SearchType.Row));
                rowStart++;
            }
        }

        if (columnMatchedCount >= 3)
        {
            while (columnStart < columnEnd)
            {
                searchTypes[row, columnStart] |= SearchType.Column;
                matchingInfoList.Add(new MatchingInfo(row, columnStart, MatchedType.Crash, SearchType.Column));
                columnStart++;
            }
        }

        if (rowMatchedCount >= 3 || columnMatchedCount >= 3)
        {
            MatchedType type = GetMatchedType(rowMatchedCount, columnMatchedCount);
            matchingInfoList.Add(new MatchingInfo(row, column, type));
        }

        return matchingInfoList;
    }

    private void SetRowMatchingRange (int baseColorType, int column, ref int rowStart, ref int rowEnd)
    {
        while (0 <= rowStart - 1 && baseColorType == puzzleObjects[rowStart - 1, column].colorType)
        {
            rowStart--;
        }

        while (rowEnd < gameBoardRow && baseColorType == puzzleObjects[rowEnd, column].colorType)
        {
            rowEnd++;
        }
    }

    private void SetColumnMatchingRange (int baseColorType, int row, ref int columnStart, ref int columnEnd)
    {
        while (0 <= columnStart - 1 && baseColorType == puzzleObjects[row, columnStart - 1].colorType)
        {
            columnStart--;
        }

        while (columnEnd < gameBoardColumn && baseColorType == puzzleObjects[row, columnEnd].colorType)
        {
            columnEnd++;
        }
    }

    private MatchedType GetMatchedType(int rowCount, int columnCount)
    {
        int baseCount = Mathf.Max(rowCount, columnCount);

        switch (baseCount)
        {
            case 0:
            case 1:
            case 2:
                return MatchedType.None;
            case 3:
                return MatchedType.Crash;
            case 4:
                return MatchedType.MakeFour;
            default:
                return MatchedType.MakeFive;
        }
    }
}