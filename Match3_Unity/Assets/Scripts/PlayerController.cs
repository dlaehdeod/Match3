using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public GameObject focusBorder;
    public RectTransform iconRectTransform;
    public bool gameOver;

    private float detectMovementDistance;

    private PointerEventData pointer;
    private List<RaycastResult> raycastResults;
    private EventSystem eventSystem;

    private Transform firstSelectedTransform;
    private bool isDrag;

    public void Initialize (float iconSize)
    {
        detectMovementDistance = iconSize / 2 + iconSize / 10;
        focusBorder.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
    }

    private void Start()
    {
        eventSystem = EventSystem.current;
        pointer = new PointerEventData(eventSystem);
    }

    private void Update()
    {
        if (gameOver)
        {
            ResetFocus();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            MouseButtonDown();
        }

        if (isDrag && Input.GetMouseButton(0))
        {
            SwapToDargDirection();
        }
    }

    private void MouseButtonDown ()
    {
        raycastResults = new List<RaycastResult>();
        pointer.position = Input.mousePosition;
        eventSystem.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count == 0)
        {
            return;
        }

        GameObject currentSelectedObject = raycastResults[0].gameObject;

        if (currentSelectedObject.CompareTag("BackToMain"))
        {
            return;
        }

        if (firstSelectedTransform == null)
        {
            PuzzleGrid selectedPuzzleGrid = currentSelectedObject.GetComponent<PuzzleGrid>();

            if (puzzleManager.IsUsingLine(selectedPuzzleGrid.row, selectedPuzzleGrid.column))
            {
                return;
            }

            if (puzzleManager.IsBombIconClick (selectedPuzzleGrid.row, selectedPuzzleGrid.column))
            {
                return;
            }

            SetFocus(currentSelectedObject);
        }
        else if (firstSelectedTransform == currentSelectedObject.transform)
        {
            ResetFocus();
        }
        else
        {
            PuzzleGrid firstPuzzleGrid = firstSelectedTransform.GetComponent<PuzzleGrid>();
            PuzzleGrid secondPuzzleGrid = currentSelectedObject.GetComponent<PuzzleGrid>();

            SwapResult swapResult = puzzleManager.TrySwap(firstPuzzleGrid.row, firstPuzzleGrid.column, secondPuzzleGrid.row, secondPuzzleGrid.column);

            switch (swapResult)
            {
                case SwapResult.NotAdjacent:
                    SetFocus(currentSelectedObject);
                    break;
                case SwapResult.Success:
                    ResetFocus();
                    break;
                case SwapResult.Using:
                    break;
                default:
                    break;
            }
        }
    }
    
    private void SetFocus (GameObject currentSelectedObject)
    {
        firstSelectedTransform = currentSelectedObject.transform;
        focusBorder.transform.position = currentSelectedObject.transform.position;
        focusBorder.SetActive(true);
        isDrag = true;
    }

    private void ResetFocus ()
    {
        firstSelectedTransform = null;
        focusBorder.SetActive(false);
        isDrag = false;
    }

    private void SwapToDargDirection ()
    {
        Vector2 moveDistance = Input.mousePosition - firstSelectedTransform.position;
        PuzzleGrid selectPuzzleGrid = firstSelectedTransform.GetComponent<PuzzleGrid>();
        int row = selectPuzzleGrid.row;
        int column = selectPuzzleGrid.column;

        bool moveSuccess = false;

        if (moveDistance.x >= detectMovementDistance)
        {
            moveSuccess = puzzleManager.SwapToDirection(row, column, row, column + 1);
        }
        else if (moveDistance.x <= -detectMovementDistance)
        {
            moveSuccess = puzzleManager.SwapToDirection(row, column, row, column - 1);
        }
        else if (moveDistance.y >= detectMovementDistance)
        {
            moveSuccess = puzzleManager.SwapToDirection(row, column, row - 1, column);
        }
        else if (moveDistance.y <= -detectMovementDistance)
        {
            moveSuccess = puzzleManager.SwapToDirection(row, column, row + 1, column);
        }

        if (moveSuccess)
        {
            ResetFocus();
        }
    }

    public void BackToMainButtonDown ()
    {
        SceneManager.LoadScene("_Main");
    }

    public void ReGame ()
    {
        SceneManager.LoadScene("Play");
    }
}