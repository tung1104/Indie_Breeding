using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AP_RaysCone : AbilityPresenter
{
    public int raysCount = 100; // Tổng số lượng ray
    public float angle = 30; // Góc mở hình cone (độ)
    public float range = 10; // Tầm xa của ray
    public float originScale = 1; // Tỉ lệ chiều dài ray
    public Vector2 scale = Vector2.one; // Tỉ lệ chiều rộng ray

    public Vector3[] directions;

    List<Unit> tmpUnitsList = new();
    RaycastHit[] hits = new RaycastHit[5];

    void CalculateDirections()
    {
        directions = new Vector3[raysCount];
        float angleInRadians = angle * Mathf.Deg2Rad; // Chuyển sang radian
        //Vector3 coneDirection = Vector3.forward; // Hướng trung tâm của cone

        int layersCount = Mathf.CeilToInt(Mathf.Sqrt(raysCount)); // Số lớp trong cone
        int index = 0;

        for (int layer = 0; layer < layersCount; layer++)
        {
            // Tính góc theta cho lớp này (cao độ)
            float t = (float)layer / layersCount;
            float theta = t * (angleInRadians / 2); // Từ đỉnh cone xuống đáy
            float radius = Mathf.Tan(theta); // Bán kính tại lớp này

            // Số ray trong vòng tròn này (phân bổ tăng dần)
            int pointsInLayer = Mathf.CeilToInt(2 * Mathf.PI * radius * raysCount / layersCount);

            for (int j = 0; j < pointsInLayer; j++)
            {
                // Tính góc phi (phương vị) cho ray
                float phi = 2 * Mathf.PI * j / pointsInLayer;

                // Chuyển từ tọa độ cực sang tọa độ Decartes
                float x = Mathf.Cos(phi) * radius * scale.x;
                float y = Mathf.Sin(phi) * radius * scale.y;
                float z = Mathf.Cos(theta); // Chiều cao từ đỉnh cone

                Vector3 point = new Vector3(x, y, z).normalized;

                // Chuyển hướng ray sang hệ trục của cone
                directions[index] = point; //transform.rotation * point;
                index++;

                // Dừng nếu đã đủ số lượng ray
                if (index >= raysCount)
                    return;
            }
        }
    }

    private void OnValidate()
    {
        CalculateDirections();
    }

    private void OnDrawGizmosSelected()
    {
        //if (Application.isPlaying) return;
        Gizmos.color = Color.magenta;
        foreach (var direction in directions)
        {
            Gizmos.DrawRay(transform.TransformPoint(new Vector3(direction.x, direction.y, 0) * originScale),
                transform.TransformDirection(direction) * range);
        }
    }

    protected override void Appearance()
    {
    }

    protected override void Presenting(float deltaTime)
    {
        tmpUnitsList.Clear();
        for (int i = 0; i < raysCount; i++)
        {
            var direction = directions[i];
            var rayOrigin = transform.TransformPoint(new Vector3(direction.x, direction.y, 0) * originScale);
            var rayDirection = transform.TransformDirection(direction);
            Debug.DrawRay(rayOrigin, rayDirection * range, Color.red);
            var hitCount = Physics.RaycastNonAlloc(rayOrigin, rayDirection, hits, range,
                1 << abInstance.owner.gameObject.layer);
            for (int j = 0; j < hitCount; j++)
            {
                var col = hits[j].collider;
                if (col.TryGetComponent(out Unit unit) && CheckTargetIsValid(unit))
                {
                    if (!tmpUnitsList.Contains(unit))
                    {
                        tmpUnitsList.Add(unit);
                        unit.TakeDamage(abInstance.owner, new DamageInfo()
                        {
                            damage = damage,
                            damageType = damageType,
                            impactForce = impactForce,
                            impactDirection = rayDirection,
                            impactPoint = hits[j].point
                        });
                        foreach (var buff in buffs)
                        {
                            unit.runtimeStats.AddBuff(buff);
                        }
                    }
                }
            }
        }
    }

    protected override void Disappearance()
    {
    }
}