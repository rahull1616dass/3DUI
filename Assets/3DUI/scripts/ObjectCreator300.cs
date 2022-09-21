using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR;



public class ObjectCreator300 : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject CylinderPrefab;
    public GameObject CapsulePrefab;
    public GameObject SpherePrefab;
    private List<GameObject> createdObjects = new List<GameObject>();
    
    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);
 
        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        createdObjects.Add(blockInstance);
    }
    
    public void CreateCylinderPrefab(Transform Where)
    {
        GameObject cylinderInstance = Instantiate(CylinderPrefab, Where.position, Quaternion.identity);
 
        cylinderInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        createdObjects.Add(cylinderInstance);
    }
    public void CreateCapsulePrefab(Transform Where)
    {
        GameObject capsuleInstance = Instantiate(CapsulePrefab, Where.position, Quaternion.identity);
 
        capsuleInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        createdObjects.Add(capsuleInstance);
    }
    public void CreateSpherePrefab(Transform Where)
    {
        GameObject sphereInstance = Instantiate(SpherePrefab, Where.position, Quaternion.identity);
 
        sphereInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        createdObjects.Add(sphereInstance);
    }
    
    public void DestroyCreatedobjects()
    {
        foreach (GameObject gameObject in createdObjects)
        {
            Destroy(gameObject);
        }
    }
    
}
