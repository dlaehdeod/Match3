using UnityEngine;
using UnityEngine.UI;

public class PuzzleObject : MonoBehaviour
{
    public int colorType;
    public IconType iconType;
    public bool isUsing;
    public bool isSwapping;

    private Image colorImage;
    private Sprite circleSprite;
    private Sprite starSprite;
    private Sprite polySprite;
    
    private void Awake()
    {
        colorImage = transform.GetComponent<Image>();
        iconType = IconType.Common;
    }

    public void SetCircleSprite (Sprite circleSprite)
    {
        this.circleSprite = circleSprite;
    }

    public void SetStarSprite (Sprite starSprite)
    {
        this.starSprite = starSprite;
    }

    public void SetPolySprite (Sprite polySprite)
    {
        this.polySprite = polySprite;
    }

    public void SetColor (Color color)
    {
        colorImage.color = color;
    }

    public void SetColorType (int type)
    {
        colorType = type;
    }

    public void ReadyToReuse (Vector3 startPosition)
    {
        transform.localScale = Vector3.one;
        transform.localPosition = startPosition;
        colorImage.sprite = circleSprite;

        gameObject.SetActive(true);
    }

    public void FadeOut (float ratio)
    {
        if (ratio >= 1)
        {
            if (iconType == IconType.Four)
            {
                colorImage.sprite = polySprite;
                transform.localScale = Vector3.one;
            }
            else if (iconType == IconType.Five)
            {
                colorImage.color = Color.cyan;
                colorImage.sprite = starSprite;
                transform.localScale = Vector3.one;
            }
            else
            {
                gameObject.SetActive(false);
            }

            return;
        }

        transform.localScale = Vector3.one * (1.1f - ratio);
    }
}
