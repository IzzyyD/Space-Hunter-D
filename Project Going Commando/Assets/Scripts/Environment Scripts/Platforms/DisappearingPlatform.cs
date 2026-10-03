using UnityEngine;
using System.Collections;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Timers")]
    [SerializeField] private float timerOffset;
    [SerializeField] private float appearTime;
    [SerializeField] private float disappearTime;
    [SerializeField] private float warningTime;

    [Header("Warning")]
    [SerializeField] private string warningColorHex;
    [SerializeField] private float flashInterval;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Color originalColor;
    private Color warningColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        originalColor = spriteRenderer.color;

        if(!ColorUtility.TryParseHtmlString(warningColorHex, out warningColor))
        {
            warningColor = Color.red;
            Debug.LogWarning("Invalid color code. Defaulting to red");
        }
    }

    private void Start()
    { StartCoroutine(PlatformCycle()); }

    private System.Collections.IEnumerator PlatformCycle()
    {
        // wait before starting the cycle
        yield return new WaitForSeconds(timerOffset);

        while (true)
        {
            // platform is visible and solid
            spriteRenderer.enabled = true;
            platformCollider.enabled = true;
            spriteRenderer.color = originalColor;

            // normal visible period - warningTime should always be shorter than appearTime to avoid errors
            yield return new WaitForSeconds(appearTime - warningTime);

            // warning flash
            float timer = 0f;

            while (timer < warningTime)
            {
                spriteRenderer.color = warningColor;
                yield return new WaitForSeconds(flashInterval);
                
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);

                timer += flashInterval * 2f;
            }

            // make sure it's hidden
            spriteRenderer.color = originalColor;
            spriteRenderer.enabled = false;
            platformCollider.enabled = false;

            // wait before reappearing
            yield return new WaitForSeconds(disappearTime);


        }
    }

}
