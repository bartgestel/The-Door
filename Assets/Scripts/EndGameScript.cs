using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class EndGameScript : MonoBehaviour
{
    [Header("Pre-Credits UI")]
    [Tooltip("Optional panel to show before the credits roll (e.g., 'Congratulations!', 'You Won!', etc.)")]
    public GameObject preCreditsPanel;

    [Tooltip("How long (seconds, unscaled) to display the pre-credits panel before showing credits.")]
    public float preCreditsDisplayDuration = 3f;

    [Tooltip("Optional text component in the pre-credits panel to customize the message.")]
    public TextMeshProUGUI preCreditsText;

    [Tooltip("If provided, this message will be set to preCreditsText on start.")]
    [TextArea]
    public string preCreditsMessage = "Congratulations!\nYou've completed the game!";

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
        // Hide the pre-credits panel at start
        if (preCreditsPanel != null)
            preCreditsPanel.SetActive(false);

        // If the panel is assigned, ensure it's hidden at start
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        // Set pre-credits message if provided
        if (preCreditsText != null && !string.IsNullOrEmpty(preCreditsMessage))
        {
            preCreditsText.text = preCreditsMessage;
        }

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
        
        // Unlock and show cursor so the player can interact with UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGame)
            Time.timeScale = 0f; // freeze the game's physics and updates (UI still can animate via unscaled time)

        // Start the full end game sequence (pre-credits then credits)
        StartCoroutine(PlayEndGameSequence());
    }

    IEnumerator PlayEndGameSequence()
    {
        // Make sure credits panel is hidden initially
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }

        // Step 1: Show pre-credits panel if assigned
        if (preCreditsPanel != null)
        {
            Debug.Log("EndGameScript: Showing pre-credits panel.");
            
            // Ensure proper Canvas rendering order
            Canvas preCanvas = preCreditsPanel.GetComponent<Canvas>();
            Canvas creditsCanvas = endGamePanel != null ? endGamePanel.GetComponent<Canvas>() : null;
            
            if (preCanvas != null)
            {
                // Make sure pre-credits canvas renders on top
                preCanvas.overrideSorting = true;
                preCanvas.sortingOrder = 1000;
                Debug.Log($"EndGameScript: Pre-credits Canvas sort order set to {preCanvas.sortingOrder}");
                
                if (creditsCanvas != null)
                {
                    creditsCanvas.overrideSorting = true;
                    creditsCanvas.sortingOrder = 500;
                    Debug.Log($"EndGameScript: Credits Canvas sort order set to {creditsCanvas.sortingOrder}");
                }
            }
            else
            {
                Debug.LogWarning("EndGameScript: Pre-credits panel doesn't have a Canvas component! It should be a Canvas object.");
            }
            
            preCreditsPanel.SetActive(true);
            
            // Verify it's actually active and check for child objects
            if (preCreditsPanel.activeSelf)
            {
                Debug.Log("EndGameScript: Pre-credits panel is now active and visible.");
                Debug.Log($"EndGameScript: Pre-credits panel has {preCreditsPanel.transform.childCount} child objects.");
                
                // Check if children are active
                for (int i = 0; i < preCreditsPanel.transform.childCount; i++)
                {
                    Transform child = preCreditsPanel.transform.GetChild(i);
                    Debug.Log($"EndGameScript: Child '{child.name}' is active: {child.gameObject.activeSelf}");
                }
            }
            else
            {
                Debug.LogWarning("EndGameScript: Pre-credits panel failed to activate!");
            }
            
            yield return new WaitForSecondsRealtime(preCreditsDisplayDuration);
            preCreditsPanel.SetActive(false);
            Debug.Log("EndGameScript: Pre-credits panel hidden.");
        }
        else
        {
            Debug.Log("EndGameScript: No pre-credits panel assigned, skipping to credits.");
        }

        // Step 2: Show credits panel and play credits animation
        if (endGamePanel != null)
        {
            Debug.Log("EndGameScript: Showing credits panel.");
            endGamePanel.SetActive(true);
            yield return StartCoroutine(PlayCreditsUnscaled());
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu_Screen");
    }
}
