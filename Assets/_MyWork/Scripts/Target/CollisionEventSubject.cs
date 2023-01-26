using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class CollisionEventSubject : MonoBehaviour
{
    public IObservable<bool> OnCollisionEnterAsync => onCollisionEnterAsyncSubject;
    private readonly AsyncSubject<bool> onCollisionEnterAsyncSubject = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            onCollisionEnterAsyncSubject.OnNext(true);
            onCollisionEnterAsyncSubject.OnCompleted();
        }
    }
}
