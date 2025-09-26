using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject cameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(cameraPos){
            Vector3 newPos = cameraPos.transform.position;
            transform.position = newPos;
        }
    }
}
