using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-1000)]
public class TouchController : MonoBehaviour
{
    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0) && !CameraController.Current.isMoving && !CameraController.Current.isZooming && !HomeUIController.Current.HasUIEnabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                OnTouch(hit.collider);
            }
        }
    }

    protected virtual void OnTouch(Collider collider)
    {

    }
}
