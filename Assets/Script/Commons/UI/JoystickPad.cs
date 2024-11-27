using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform stickOuter;
    [SerializeField] RectTransform stickInner;
    [SerializeField] Image stickArrowImage;
    [SerializeField] Vector2 paddingMin;
    [SerializeField] Vector2 paddingMax;

    public bool lockOuterOnTap;
    public bool moveOuterOnDrag;

    bool isTouched;

    RectTransform rectTransform;
    Vector2 originOuterAnchoredPos;

    public Vector2 Value; // { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originOuterAnchoredPos = stickOuter.anchoredPosition;
        stickInner.SetParent(transform);
        stickArrowImage.transform.SetParent(transform);
    }

    private void OnEnable()
    {
        RefreshStickColors();
    }

    private void OnDisable()
    {
        isTouched = false;
        Value = Vector2.zero;
        stickOuter.anchoredPosition = originOuterAnchoredPos;
        stickInner.localPosition = stickOuter.localPosition;
    }

    private void Update()
    {
        if (!isTouched)
        {
            stickOuter.anchoredPosition =
                Vector3.Lerp(stickOuter.anchoredPosition, originOuterAnchoredPos, Time.deltaTime * 25);
            stickInner.localPosition =
                Vector3.Lerp(stickInner.localPosition, stickOuter.localPosition, Time.deltaTime * 15);
        }

        if (stickArrowImage.gameObject.activeSelf)
        {
            stickArrowImage.transform.localPosition = stickOuter.localPosition;
            stickArrowImage.transform.rotation =
                Quaternion.Euler(0, 0, Mathf.Atan2(Value.y, Value.x) * Mathf.Rad2Deg - 90);
            stickArrowImage.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, .5f), Value.magnitude);
        }

        RefreshStickColors();
    }

    private Color tempColor = Color.clear;

    void RefreshStickColors()
    {
        var color = new Color(1, 1, 1, isTouched ? .5f : .1f);
        if (tempColor == color) return;
        tempColor = color;
        stickOuter.GetComponent<Image>().color = color;
        stickInner.GetComponent<Image>().color = color;
    }

    bool TryGetPointerLocal(PointerEventData eventData, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
            eventData.pressEventCamera, out localPoint);
    }

    Vector2 LimitInnerPointInsideOuter(Vector2 innerPoint)
    {
        Vector2 outerPoint = stickOuter.localPosition;
        Vector2 direction = innerPoint - outerPoint;
        float distance = direction.magnitude;
        float maxDistance = stickOuter.rect.width / 2f - stickInner.rect.width / 2f;

        if (distance > maxDistance)
        {
            direction = direction.normalized * maxDistance;
            innerPoint = outerPoint + direction;
        }

        return innerPoint;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId > 0) return;

        isTouched = true;

        if (TryGetPointerLocal(eventData, out Vector2 localPoint))
        {
            if (!lockOuterOnTap)
                stickOuter.localPosition = LimitOuterPointInsideRect(localPoint);
            stickInner.localPosition = LimitInnerPointInsideOuter(localPoint);

            UpdateValue();
        }
    }

    Vector2 LimitOuterPointInsideRect(Vector2 outerPoint)
    {
        Vector2 rectSize = rectTransform.rect.size;
        Vector2 halfRectSize = rectSize / 2f;
        Vector2 halfOuterSize = stickOuter.rect.size / 2f;

        Vector2 min = -halfRectSize + halfOuterSize;
        Vector2 max = halfRectSize - halfOuterSize;

        outerPoint.x = Mathf.Clamp(outerPoint.x, min.x + paddingMin.x, max.x - paddingMax.x);
        outerPoint.y = Mathf.Clamp(outerPoint.y, min.y + paddingMin.y, max.y - paddingMax.y);

        return outerPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId > 0) return;

        if (TryGetPointerLocal(eventData, out Vector2 localPoint))
        {
            if (moveOuterOnDrag)
            {
                Vector2 outerPoint = stickOuter.localPosition;
                Vector2 direction = localPoint - outerPoint;

                float distance = direction.magnitude;
                float maxDistance = stickOuter.rect.width / 2f - stickInner.rect.width / 2f;

                if (distance > maxDistance)
                {
                    direction = direction.normalized * maxDistance;
                    outerPoint = LimitOuterPointInsideRect(localPoint - direction);
                    stickOuter.localPosition = outerPoint;
                }
            }

            stickInner.localPosition = LimitInnerPointInsideOuter(localPoint);

            UpdateValue();
        }
    }

    void UpdateValue()
    {
        var stickInnerPosLocal = stickInner.localPosition - stickOuter.localPosition;
        Value = stickInnerPosLocal / (stickOuter.rect.width / 2f - stickInner.rect.width / 2f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId > 0) return;

        isTouched = false;
        Value = Vector2.zero;
    }
}