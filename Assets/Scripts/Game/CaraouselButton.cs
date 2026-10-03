
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CarouselButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Button Sprite")]
    [SerializeField] private Image buttonImage;

    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite selectedSprite;

    private bool isSelected;
    private bool isHover;
    private bool isPressed;

    private void Awake()
    {
        FindImage();
        UpdateSprite();
    }

    private void Start()
    {
        FindImage();
        UpdateSprite();
    }

    private void FindImage()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (buttonImage == null)
            buttonImage = GetComponentInChildren<Image>();
    }

    private void OnValidate()
    {
        FindImage();
        UpdateSprite();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHover = true;
        UpdateSprite();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHover = false;
        isPressed = false;
        UpdateSprite();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateSprite();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateSprite();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (buttonImage == null)
            return;

        if (isPressed)
        {
            if (pressedSprite != null)
                buttonImage.sprite = pressedSprite;
        }
        else if (isHover)
        {
            if (hoverSprite != null)
                buttonImage.sprite = hoverSprite;
        }
        else if (isSelected)
        {
            if (selectedSprite != null)
                buttonImage.sprite = selectedSprite;
        }
        else
        {
            if (normalSprite != null)
                buttonImage.sprite = normalSprite;
        }
    }
}

