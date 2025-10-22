using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LaserBeam
{
    Vector3 pos, dir;

    private GameObject laserObj;
    private LineRenderer laser;
    List<Vector3> laserIndices = new List<Vector3>();
    private PuzzleManager puzzleManager;
    
    public LaserBeam(Vector3 pos, Vector3 dir, Material material)
    {
        this.laser = new LineRenderer();
        this.laserObj = new GameObject();
        this.laserObj.name = "Laser Beam";
        this.pos = pos;
        this.dir = dir;
        
        this.laser = this.laserObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
        this.laser.startWidth = 0.1f;
        this.laser.endWidth = 0.1f;
        this.laser.material = material;
        this.laser.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        this.laser.startColor = Color.green;
        this.laser.endColor = Color.green;
        
        // Find PuzzleManager in the scene
        puzzleManager = Object.FindFirstObjectByType<PuzzleManager>();
        
        CastRay(pos, dir, laser);
    }

    void CastRay(Vector3 pos, Vector3 dir, LineRenderer laser)
    {
        laserIndices.Add(pos);
        Ray ray = new Ray(pos, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 30, 1))
        {
            CheckHit(hit, dir, laser);
        }
        else
        {
            laserIndices.Add(ray.GetPoint(30));
            UpdateLaser();
        }
    }

    void UpdateLaser()
    {
        int count = 0;
        laser.positionCount = laserIndices.Count;
        
        foreach (Vector3 v in laserIndices)
        {
            laser.SetPosition(count, v);
            count++;
        }
    }

    void CheckHit(RaycastHit hitInfo, Vector3 direction, LineRenderer laser)
    {
        if (hitInfo.collider.gameObject.tag == "Mirror")
        {
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);
            
            CastRay(pos, dir, laser);
        }
        else if (hitInfo.collider.gameObject.tag == "LaserReceiver")
        {
            laserIndices.Add(hitInfo.point);
            UpdateLaser();
            if (puzzleManager != null)
            {
                puzzleManager.NotifyLaserReceiverHit(hitInfo.collider.gameObject);
            }
        }
        else
        {
            laserIndices.Add(hitInfo.point);
            UpdateLaser();
        }
    }
}
