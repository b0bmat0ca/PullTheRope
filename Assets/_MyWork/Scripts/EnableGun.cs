using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableGun : MonoBehaviour
{
    [SerializeField] private Transform leftHandAnchor;
    [SerializeField] private Transform rightHandAnchor;

    private void Awake()
    {
        //gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.transform.position = leftHandAnchor.position;
        //gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
