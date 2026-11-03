using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/ChargeShotgun")]
public class ChargeShotgun : WeaponData
{
    [Header("Shotgun Properties")]
    public int pelletCount;
    public float spreadAngle;

    [Header("Line Properties")]
    public Gradient halfChargeGradient;
    public Gradient fullChargeGradient;
    public float lineWidth;
    public float lineDuration;
    public int zigzagPoints;
    public float zigzagStrength;
    public float halfChargeRangeModifier;

    public override void HandleFire(Transform spawnPoint, WeaponController owner)
    { /* Do nothing */ }


    public override void HandleFire(Transform spawnPoint, WeaponController owner, float charge)
    {
        if (!owner.bulletOverlapChecker.canFire)
        { return; }

        bool halfCharge = charge < 1f;

        float usedRange = halfCharge ? range * halfChargeRangeModifier : range;
        float usedZigzag = halfCharge ? 0.01f : zigzagStrength;
        Gradient usedGradient = halfCharge ? halfChargeGradient : fullChargeGradient;

        float halfSpread = spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)
        {
            float offset = Random.Range(-halfSpread, halfSpread);
            Quaternion rotation = spawnPoint.rotation * Quaternion.Euler(0, 0, offset);
            Vector2 direction = rotation * Vector2.right;

            RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, direction, usedRange, LayerMask.GetMask("Ground", "Walls"));
            Vector2 endPoint = hit ? hit.point : (Vector2)spawnPoint.position + direction * usedRange;
            DrawLine(spawnPoint.position, endPoint, usedZigzag, usedGradient);
        }
    }


    private void DrawLine(Vector2 start, Vector2 end, float zigzag, Gradient gradient)
    {
        GameObject lineObj = new GameObject("ShotgunPellet");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        int count = Mathf.Max(2, zigzagPoints);

        lr.useWorldSpace = true;
        lr.positionCount = count;
        lr.widthMultiplier = lineWidth;
        lr.colorGradient = gradient;
        lr.material = new Material(Shader.Find("Sprites/Default"));

        Vector2 direction = (end - start).normalized;
        Vector2 perp = new Vector2(-direction.y, direction.x);

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector2 basePos = Vector2.Lerp(start, end, t);

            float noise = Random.Range(-zigzag, zigzag);
            Vector2 finalPos = basePos + perp * noise;

            lr.SetPosition(i, finalPos);
        }

        Object.Destroy(lineObj, lineDuration);
    }
}
