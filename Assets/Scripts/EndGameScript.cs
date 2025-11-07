using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class EndGameScript : MonoBehaviour
{
    [Header("End Game UI")]
    [Tooltip("Assign the panel (Canvas or GameObject) that contains the credits UI. It will be enabled when the player triggers the end.")]
    public GameObject endGamePanel;

    [Tooltip("A UI Text component inside the panel that will display the credits (or leave empty and fill Credits Lines).")]
    public TextMeshProUGUI  creditsText;

    [Tooltip("If provided, these lines will be joined into the creditsText on start.")]
    [TextArea]
    public string[] creditsLines;

    [Tooltip("How long (seconds, unscaled) the credits scroll animation should take.")]
    public float creditsDuration = 12f;

    [Tooltip("If true, Time.timeScale will be set to 0 when showing credits. The coroutine uses unscaled time so the animation still plays.")]
    public bool pauseGame = true;

    [Tooltip("Tag used to identify the player object. Change this if your player uses a different tag.")]
    public string playerTag = "Player";

    bool _hasTriggered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If the panel is assigned, ensure it's hidden at start
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        // If there are lines provided, join them into the text component (if assigned)
        if (creditsText != null && creditsLines != null && creditsLines.Length > 0)
        {
            creditsText.text = string.Join("\n", creditsLines);
        }

        // NOTE: Removed fallback proximity checks. Detection now only occurs via OnTriggerEnter/OnCollisionEnter/OnControllerColliderHit
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("EndGameScript: OnTriggerEnter with " + (other != null ? other.name + " (tag=" + other.tag + ")" : "null"));

        // Only react once
        if (_hasTriggered)
            return;

        // If the other object matches the specified player tag, start
        if (other != null && other.CompareTag(playerTag))
        {
            _hasTriggered = true;
            StartEndSequence();
            return;
        }

        // Helpful hint if tags don't match
        Debug.LogFormat("EndGameScript: Triggered by '{0}' with tag '{1}', but expected tag '{2}'.", other != null ? other.name : "null", other != null ? other.tag : "null", playerTag);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Sometimes your trigger setup may be a non-trigger collider; handle collisions as well
        Debug.Log("EndGameScript: OnCollisionEnter with " + (collision != null ? collision.gameObject.name + " (tag=" + collision.gameObject.tag + ")" : "null"));
        if (_hasTriggered) return;
        if (collision != null && collision.gameObject.CompareTag(playerTag))
        {
            _hasTriggered = true;
            StartEndSequence();
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Note: OnControllerColliderHit is invoked on the Controller's gameobject, not on other objects.
        Debug.Log("EndGameScript: OnControllerColliderHit (note: this runs only if this object has a Controller) with " + (hit != null ? hit.gameObject.name + " (tag=" + hit.gameObject.tag + ")" : "null"));
        if (_hasTriggered) return;
        if (hit != null && hit.gameObject.CompareTag(playerTag))
        {
            _hasTriggered = true;
            StartEndSequence();
        }
    }

    [ContextMenu("Trigger End Sequence (Inspector)")]
    void StartEndSequence()
    {
        Debug.Log("EndGameScript: Player triggered end game sequence.");
        // Show panel and start credits coroutine
        if (endGamePanel != null)
        {
            // Ensure panel active so UI elements can be measured/animated
            endGamePanel.SetActive(true);

            // Unlock and show cursor so the player can interact with UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseGame)
                Time.timeScale = 0f; // freeze the game's physics and updates (UI still can animate via unscaled time)

            StartCoroutine(PlayCreditsUnscaled());
        }
        else
        {
            Debug.LogWarning("EndGameScript: endGamePanel is not assigned. Assign a panel in the inspector to show credits.");
        }
    }

    IEnumerator PlayCreditsUnscaled()
    {
        // If there's no creditsText, just wait for the duration then return
        if (creditsText == null)
        {
            yield return new WaitForSecondsRealtime(creditsDuration);
            OnCreditsComplete();
            yield break;
        }

        RectTransform rt = creditsText.rectTransform;

        // Record original position so we can restore or calculate from it
        Vector2 originalPos = rt.anchoredPosition;

        // Start below the visible area
        float startY = -Screen.height;
        float endY = Screen.height + rt.rect.height;

        float elapsed = 0f;
        while (elapsed < creditsDuration)
        {
            float t = elapsed / creditsDuration;
            float y = Mathf.Lerp(startY, endY, t);
            rt.anchoredPosition = new Vector2(originalPos.x, y);

            elapsed += Time.unscaledDeltaTime;
            yield return null; // runs in real time because we use unscaledDeltaTime
        }

        // Ensure final position
        rt.anchoredPosition = new Vector2(originalPos.x, endY);

        // Wait a little at the end so final lines are readable
        yield return new WaitForSecondsRealtime(1f);

        OnCreditsComplete();
    }

    void OnCreditsComplete()
    {
        // Optional: keep panel active so the player can see credits, or provide buttons on the panel to quit/restart.
        // For now, we'll simply log and leave the panel visible. If the game was paused, we keep it paused so timeScale stays 0.
        Debug.Log("EndGameScript: Credits finished.");
        
    }
}
