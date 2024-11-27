using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class TopdownCameraScroller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Vector2 sizeRatio;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        scrollRect.viewport = transform as RectTransform;
        scrollRect.scrollSensitivity = 0;
        if (!scrollRect.content)
        {
            scrollRect.content = (RectTransform)new GameObject("Content",
                typeof(RectTransform)).transform;
            scrollRect.content.SetParent(scrollRect.transform);
            scrollRect.content.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            scrollRect.content.localScale = Vector3.one;
        }
    }

    private void Awake()
    {
    }

    const float groundHeight = 0;

    public Vector3 GetCameraPosition(Camera cam, Bounds worldBounds)
    {
        var content = scrollRect.content;
        // Calculate the camera's height above the ground
        var cameraHeight = cam.transform.position.y - groundHeight;
        var cameraForward = cam.transform.forward;
        var tiltAngle = Vector3.Angle(Vector3.down, -cameraForward);
        var xzOffset = cameraHeight * Mathf.Tan(Mathf.Deg2Rad * tiltAngle);
        return new Vector3(-content.localPosition.x / sizeRatio.x + worldBounds.center.x,
            cam.transform.position.y,
            -content.localPosition.y / sizeRatio.x + worldBounds.center.z + xzOffset);
    }

    public void SetCameraPosition(Camera cam, Bounds worldBounds, Bounds screenBounds, Vector3 worldPosition,
        float deltaTime = 0)
    {
        var content = scrollRect.content;
        var localPosX = -(worldPosition.x - worldBounds.center.x) * sizeRatio.x;
        var localPosY = -(worldPosition.z - worldBounds.center.z) * sizeRatio.x;
        localPosX = Mathf.Clamp(content.localPosition.x, localPosX + screenBounds.min.x,
            localPosX + screenBounds.max.x);
        localPosY = Mathf.Clamp(content.localPosition.y, localPosY + screenBounds.min.y,
            localPosY + screenBounds.max.y);
        var viewportRect = scrollRect.viewport.rect;
        var contentRect = scrollRect.content.rect;
        localPosX = Mathf.Clamp(localPosX, viewportRect.width / 2f - contentRect.width / 2f,
            contentRect.width / 2f - viewportRect.width / 2f);
        if (worldBounds.size.z > 0)
            localPosY = Mathf.Clamp(localPosY, viewportRect.height / 2f - contentRect.height / 2f,
                contentRect.height / 2f - viewportRect.height / 2f);
        var targetLocalPosition = new Vector3(localPosX, localPosY, content.localPosition.z);
        content.localPosition = deltaTime <= 0
            ? targetLocalPosition
            : Vector3.Lerp(content.localPosition, targetLocalPosition, deltaTime * 1.5f);
    }

    public void UpdateContentSize(Camera cam, Bounds worldBounds)
    {
        // Calculate size ratio
        var orthographicSize = cam.orthographicSize;
        var canvasHeight = scrollRect.viewport.rect.height;
        var canvasWidth = scrollRect.viewport.rect.width;
        sizeRatio = new Vector2(canvasWidth / (orthographicSize * 2 * cam.aspect),
            canvasHeight / (orthographicSize * 2));
        //Debug.Log($"Size ratio: {sizeRatio}");
        // Calculate content size
        scrollRect.content.sizeDelta = new Vector2(worldBounds.size.x * sizeRatio.x,
            worldBounds.size.z * sizeRatio.x);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
    }
}