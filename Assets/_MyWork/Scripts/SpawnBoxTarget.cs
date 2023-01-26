using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBoxTarget : MonoBehaviour
{
    // ターゲットリスト
    [SerializeField] protected List<TargetMap> targetList;
    [System.Serializable]
    protected class TargetMap
    {
        [SerializeField] private GameObject target;
        [SerializeField] private float offsetY;
        [SerializeField] private float targetScale;

        public GameObject Target { get { return target; } }
        public float OffsetY { get { return offsetY; } }
        public float TargetScale { get { return targetScale;} }
    }


    // Start is called before the first frame update
    void Start()
    {
        TargetMap target = targetList[Random.Range(0, targetList.Count)];
        GameObject obj = Instantiate(target.Target, transform.position + new Vector3(0, target.OffsetY, 0), Quaternion.identity);
        GameObject child = obj.transform.GetChild(0).gameObject;
        child.transform.localScale *= target.TargetScale;
        //obj.GetComponent<BoxCollider>().size *= targetScale;
        child.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
