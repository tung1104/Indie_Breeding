using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TopdownCameraController : StandardSingleton<TopdownCameraController>
{
    [SerializeField] new Camera camera;
    [SerializeField] TopdownCameraScroller scroller;
    public Bounds worldBounds;
    public Bounds screenBounds;
    public bool followTarget;
    public GameObject target;
    public Bounds targetBounds;
    public float maxOrthographicSize;
    public float minOrthographicSize = 5;

    Bounds cachedWorldBounds;

    private float orthographicSize;

    private void Reset()
    {
        camera = GetComponent<Camera>();
        scroller = FindObjectOfType<TopdownCameraScroller>();
    }

    private void Awake()
    {
        // Set camera orthographic size based on world bounds
        var aspectRatio = camera.aspect;
        maxOrthographicSize = worldBounds.size.x / (2 * aspectRatio);
        camera.orthographicSize = orthographicSize = maxOrthographicSize;

        scroller.UpdateContentSize(camera, worldBounds);
        scroller.SetCameraPosition(camera, worldBounds, screenBounds, Vector3.zero, 0);
    }

    public void SetWorldBounds(Bounds bounds)
    {
        worldBounds = bounds;
        Update();
        UpdateCameraPosition(0);
    }

    private void Update()
    {
        orthographicSize = Mathf.Clamp(orthographicSize - Input.mouseScrollDelta.y * .5f, minOrthographicSize,
            maxOrthographicSize);
        if (cachedWorldBounds != worldBounds || camera.orthographicSize != orthographicSize)
        {
            cachedWorldBounds = worldBounds;

            camera.orthographicSize = orthographicSize;
            scroller.UpdateContentSize(camera, worldBounds);
        }
    }

    private void UpdateCameraPosition(float deltaTime)
    {
        if (!target) return;
        var targetPosition = target.transform.position;
        scroller.SetCameraPosition(camera, worldBounds, screenBounds, targetPosition, deltaTime);
    }

    private void LateUpdate()
    {
        if (followTarget && !scroller.gameObject.activeInHierarchy)
        {
            UpdateCameraPosition(Time.deltaTime);
        }

        transform.position = scroller.GetCameraPosition(camera, worldBounds);
        // transform.position = Vector3.Lerp(transform.position,
        //     scroller.GetCameraPosition(camera, worldBounds), Time.deltaTime * 20);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
    }
}