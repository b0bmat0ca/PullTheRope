using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassthroughTarget : EnableDestroyTarget
{
    private Camera mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        transform.LookAt(mainCamera.transform);
    }
}
