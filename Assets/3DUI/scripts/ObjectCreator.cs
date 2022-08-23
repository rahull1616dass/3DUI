using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject BlockPrefab;
    [SerializeField] private GameObject CylinderPrefab;
    private List<GameObject> BlocksCreated = new List<GameObject>();

    public void CreateBlockPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(BlockPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        BlocksCreated.Add(blockInstance);
    }

    public void CreateCylinderPrefab(Transform Where)
    {
        GameObject blockInstance = Instantiate(CylinderPrefab, Where.position, Quaternion.identity);

        blockInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        BlocksCreated.Add(blockInstance);
    }

    public void DestroyAllCreatedBlocks()
    {
        foreach (var b in BlocksCreated)
        {
            Destroy(b);
        }
    }
}