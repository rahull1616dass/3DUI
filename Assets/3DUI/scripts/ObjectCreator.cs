using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



public class ObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private GameObject CylinderPrefab;
    private List<GameObject> BlocksCreated = new List<GameObject>();
    [SerializeField] private GameObject CapsulePrefab;
    [SerializeField] private GameObject SpherePrefab;

    private RaySelecting raySelectingInstance;

    private void Start()
    {
        raySelectingInstance = GetComponent<RaySelecting>();
    }

    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }

    public void CreateCylinderPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(CylinderPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }
    public void CreateCapsulePrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(CapsulePrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }
    public void CreateSpherePrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(SpherePrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

    }

}