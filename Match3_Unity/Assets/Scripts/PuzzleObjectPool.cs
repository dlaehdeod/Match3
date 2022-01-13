using System.Collections.Generic;
using UnityEngine;

public class PuzzleObjectPool : MonoBehaviour
{
    public GameObject puzzleObjectPrefab;
    public Transform puzzleManagerTransform;

    public Sprite circleSprite;
    public Sprite starSprite;
    public Sprite polySprite;

    private List<PuzzleObject> puzzleObjectList;

    public void Add (PuzzleObject target)
    {
        if (puzzleObjectList == null)
        {
            puzzleObjectList = new List<PuzzleObject>();
        }

        puzzleObjectList.Add(target);
    }

    public PuzzleObject GetPuzzleObject ()
    {
        for (int i = 0; i < puzzleObjectList.Count; ++i)
        {
            if (puzzleObjectList[i].gameObject.activeSelf == false)
            {
                return puzzleObjectList[i];
            }
        }

        PuzzleObject newPuzzleObject = Instantiate(puzzleObjectPrefab, puzzleManagerTransform).GetComponent<PuzzleObject>();
        newPuzzleObject.SetCircleSprite(circleSprite);
        newPuzzleObject.SetStarSprite(starSprite);
        newPuzzleObject.SetPolySprite(polySprite);
        newPuzzleObject.gameObject.SetActive(false);
        newPuzzleObject.transform.SetAsFirstSibling();
        
        puzzleObjectList.Add(newPuzzleObject);
        return newPuzzleObject;
    }
}