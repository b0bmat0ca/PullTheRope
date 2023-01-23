using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBoxTarget : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float offsetY;
    [SerializeField] private float targetScale;


    // Start is called before the first frame update
    void Start()
    {
        GameObject obj =  Instantiate(target, transform.position + new Vector3(0, offsetY, 0), Quaternion.identity);
        GameObject child = obj.transform.GetChild(0).gameObject;
        child.transform.localScale *= targetScale;
        obj.GetComponent<BoxCollider>().size *= targetScale;
        child.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
