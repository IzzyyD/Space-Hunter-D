using UnityEngine;
using System.Collections;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Timers")]
    [SerializeField] private float timerOffset;
    [SerializeField] private float appearTime;
    [SerializeField] private float disappearTime;
    [SerializeField] private float warningTime;


    [Header("Appearance Flash")]
    [SerializeField] private float appearFlashTime;
    [SerializeField] private float appearFlashInterval;

    [Header("Disappearance Warning")]
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
    {
        // start invisible and non-solid
        spriteRenderer.enabled = false;
        platformCollider.enabled = false;

        StartCoroutine(PlatformCycle()); 
    }

    private System.Collections.IEnumerator PlatformCycle()
    {
        // wait before starting the cycle
        yield return new WaitForSeconds(timerOffset);

        while (true)
        {

            // flashes without changing color before it appears
            spriteRenderer.color = originalColor;
            platformCollider.enabled = false;

            float appearanceTimer = 0f;

            while (appearanceTimer < appearFlashTime)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;

                yield return new WaitForSeconds(appearFlashInterval);

                appearanceTimer += appearFlashInterval;
            }

            // platform is now visible and solid
            spriteRenderer.enabled = true;
            platformCollider.enabled = true;
            spriteRenderer.color = originalColor;

            // stay visible until the warning begins
            yield return new WaitForSeconds(Mathf.Max(0f, appearTime - warningTime));

            // warning flash
            float warningTimer = 0f;

            while (warningTimer < warningTime)
            {
                spriteRenderer.color = warningColor;
                yield return new WaitForSeconds(flashInterval);
                
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);

                warningTimer += flashInterval * 2f;
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
