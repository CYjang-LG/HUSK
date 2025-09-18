using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image backgroundImage;
    public Image joystickImage;
    private Vector2 inputVector = Vector2.zero;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public bool IsFiring { get; private set; }
    public bool IsJumping { get; private set; }

    void Start()
    {
        if (backgroundImage == null || joystickImage == null)
            Debug.LogError("VirtualJoystick: 이미지를 할당해주세요!");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            backgroundImage.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos);

        pos.x /= backgroundImage.rectTransform.sizeDelta.x;
        pos.y /= backgroundImage.rectTransform.sizeDelta.y;

        inputVector = new Vector2(pos.x * 2, pos.y * 2);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
        joystickImage.rectTransform.anchoredPosition = new Vector2(
            inputVector.x * (backgroundImage.rectTransform.sizeDelta.x / 2),
            inputVector.y * (backgroundImage.rectTransform.sizeDelta.y / 2));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        joystickImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnFireButtonDown() => IsFiring = true;
    public void OnFireButtonUp() => IsFiring = false;

    public void OnJumpButtonDown()
    {
        IsJumping = true;
        Invoke("ResetJump", 0.1f);
    }



    public void ResetJump()
    {
        IsJumping = false;
    }
}
