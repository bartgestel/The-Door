using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    public float rotationSpeed = 200f;
    private bool isGrabbed = false;

    void Update()
    {
        if (!isGrabbed)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && hit.transform == transform)
                {
                    isGrabbed = true;
                }
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                isGrabbed = false;
            }
            else
            {
                float mouseX = Input.GetAxis("Mouse X");
                if (Mathf.Abs(mouseX) > 0.01f)
                {
                    transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);
                }
            }
        }
    }
}