using UnityEngine;
using UnityEngine.UI;

public class PuzzleObject : MonoBehaviour
{
    public int colorType;

    private Transform iconTransform;
    private int row;
    private int column;

    private Image colorImage;
    private Sprite circleSprite;
    private Sprite starSprite;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private void Awake()
    {
        iconTransform = transform.GetChild(0);
        colorImage = iconTransform.GetComponent<Image>();
    }

    public bool IsAdjacent (PuzzleObject targetPuzzleObject)
    {
        int targetRow = targetPuzzleObject.GetRow();
        int targetColumn = targetPuzzleObject.GetColumn();

        return Mathf.Abs(row - targetRow) + Mathf.Abs(column - targetColumn) <= 1;
    }

    public void SetCircleSprite (Sprite circleSprite)
    {
        this.circleSprite = circleSprite;
    }

    public void SetStarSprite (Sprite starSprite)
    {
        this.starSprite = starSprite;
    }

    public void SetColor (Color color)
    {
        colorImage.color = color;
    }

    public void SetColorType (int type)
    {
        colorType = type;
    }

    public void SetRow (int row)
    {
        this.row = row;
    }

    public void SetColumn (int column)
    {
        this.column = column;
    }
    public void SetStartPosition (Vector2 startPosition)
    {
        this.startPosition = startPosition;
    }

    public void SetEndPosition (Vector2 endPosition)
    {
        this.endPosition = endPosition;
    }

    public int GetRow ()
    {
        return row;
    }

    public int GetColumn ()
    {
        return column;
    }

    public void ReadyToReuse ()
    {
        transform.localScale = Vector3.one;
        transform.localPosition = startPosition;
        colorImage.sprite = circleSprite;

        gameObject.SetActive(true);
    }

    public void Move (float ratio)
    {
        transform.localPosition = Vector2.Lerp(startPosition, endPosition, ratio);
    }

    public void FadeOut (float ratio)
    {
        if (ratio >= 1)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.localScale = Vector3.one * (1.0f - ratio);
    }
}
