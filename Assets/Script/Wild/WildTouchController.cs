using UnityEngine;

public class WildTouchController : TouchController
{
    protected override void OnTouch(Collider collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            var enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnClick();
            }
        }
    }
}
