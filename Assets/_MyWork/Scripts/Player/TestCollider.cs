using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx.Triggers;
using UnityEngine;
using UniRx;

public class TestCollider : MonoBehaviour
{
    public SphereCollider trigger;

    // Start is called before the first frame update
    void Start()
    {
        trigger.OnCollisionEnterAsObservable()
            .Subscribe(collision =>
            {
                CommonUtility.Instance.Debug(collision.gameObject.name);
                CommonUtility.Instance.Debug(collision.gameObject.transform.parent.name);
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
