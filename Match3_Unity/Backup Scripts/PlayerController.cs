using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public PuzzleManager puzzleManager;
    public GameObject focusBorder;
    public RectTransform iconRectTransform;

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
        if (Input.GetMouseButtonDown(0))
        {
            MouseButtonDown();
        }

        if (isDrag && Input.GetMouseButton(0))
        {
            MoveToDargDirection();
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

        if (currentSelectedObject.CompareTag("Button"))
        {
            return;
        }

        if (firstSelectedTransform == null)
        {
            PuzzleObject selectedPuzzleObject = currentSelectedObject.GetComponent<PuzzleObject>();

            if (puzzleManager.IsUsingLine(selectedPuzzleObject))
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
            Transform secondSelectedTransform = currentSelectedObject.transform;
            PuzzleObject firstPuzzleObject = firstSelectedTransform.GetComponent<PuzzleObject>();
            PuzzleObject secondPuzzleObject = secondSelectedTransform.GetComponent<PuzzleObject>();

            SwapType swapType = puzzleManager.TrySwap(firstPuzzleObject, secondPuzzleObject);

            switch (swapType)
            {
                case SwapType.Fail:
                    SetFocus(currentSelectedObject);
                    break;
                case SwapType.Success:
                    ResetFocus();
                    break;
                case SwapType.Using:
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

    private void MoveToDargDirection ()
    {
        Vector2 moveDistance = Input.mousePosition - firstSelectedTransform.position;

        bool moveSuccess = false;

        if (moveDistance.x >= detectMovementDistance)
        {
            moveSuccess = puzzleManager.MoveToDirection(firstSelectedTransform, 0, 1);
        }
        else if (moveDistance.x <= -detectMovementDistance)
        {
            moveSuccess = puzzleManager.MoveToDirection(firstSelectedTransform, 0, -1);
        }
        else if (moveDistance.y >= detectMovementDistance)
        {
            moveSuccess = puzzleManager.MoveToDirection(firstSelectedTransform, -1, 0);
        }
        else if (moveDistance.y <= -detectMovementDistance)
        {
            moveSuccess = puzzleManager.MoveToDirection(firstSelectedTransform, 1, 0);
        }

        if (moveSuccess)
        {
            ResetFocus();
        }
    }
}