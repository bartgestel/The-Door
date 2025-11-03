using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Animation")]
    public Animator doorAnimator;
    public string openTriggerName = "Open";
    public string closeTriggerName = "Close";
    public string closedStateName = "Closed"; // Name of the closed state in animator
    
    [Header("Audio (Optional)")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    private AudioSource audioSource;
    
    private bool isOpen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Animator component if not assigned
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();
            
        // Get AudioSource component if we have door sounds
        if (doorOpenSound != null || doorCloseSound != null)
            audioSource = GetComponent<AudioSource>();
            
        if (doorAnimator == null)
            Debug.LogError("DoorScript: No Animator component found! Please assign one or add an Animator to this GameObject.");
        
        // Ensure door starts in closed state
        InitializeDoorState();
    }

    void InitializeDoorState()
    {
        if (doorAnimator != null)
        {
            // Force door to closed state at start
            isOpen = false;
            doorAnimator.SetTrigger(closeTriggerName);
            Debug.Log("Door initialized to closed state");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDoor()
    {
        if (doorAnimator != null && !isOpen)
        {
            doorAnimator.SetTrigger(openTriggerName);
            isOpen = true;
            
            // Play open sound if available
            if (audioSource != null && doorOpenSound != null)
                audioSource.PlayOneShot(doorOpenSound);
                
            Debug.Log("Door opened!");
        }
    }
    
    public void CloseDoor()
    {
        if (doorAnimator != null && isOpen)
        {
            doorAnimator.SetTrigger(closeTriggerName);
            isOpen = false;
            
            // Play close sound if available
            if (audioSource != null && doorCloseSound != null)
                audioSource.PlayOneShot(doorCloseSound);
                
            Debug.Log("Door closed!");
        }
    }
    
    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
    
    // Method to force door to specific state (useful for initialization)
    public void SetDoorState(bool shouldBeOpen)
    {
        if (doorAnimator != null)
        {
            if (shouldBeOpen && !isOpen)
            {
                OpenDoor();
            }
            else if (!shouldBeOpen && isOpen)
            {
                CloseDoor();
            }
        }
    }
    
    // Method to set any trigger by name
    public void SetAnimationTrigger(string triggerName)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(triggerName);
        }
    }
    
    // Method to set bool parameters
    public void SetAnimationBool(string boolName, bool value)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(boolName, value);
        }
    }
    
    // Method to set float parameters
    public void SetAnimationFloat(string floatName, float value)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetFloat(floatName, value);
        }
    }
    
    // Method to set int parameters
    public void SetAnimationInt(string intName, int value)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetInteger(intName, value);
        }
    }
    
    // Public getter for door state
    public bool IsOpen()
    {
        return isOpen;
    }
}
