using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

public class PassthroughTarget : Target
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
    protected override void Update()
    {
        base.Update();
        transform.LookAt(mainCamera.transform);
    }
}
