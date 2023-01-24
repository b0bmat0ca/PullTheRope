using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UniRx;

public class InputEventProviderGrabbable : MonoBehaviour, IInputEventProvider
{

    #region IInputEventProvider
    public IReadOnlyReactiveProperty<bool> IsTurretGrab => isTurretGrab;

    public IReadOnlyReactiveProperty<bool> IsTriggerGrab => isTriggerGrab;
    #endregion

    [Header("砲塔"), SerializeField] private Grabbable turret;
    private ReactiveProperty<bool> isTurretGrab = new(false);

    [Header("トリガー"), SerializeField] private Grabbable trigger;
    private ReactiveProperty<bool> isTriggerGrab = new(false);


    // Start is called before the first frame update
    void Start()
    {
        isTurretGrab.AddTo(this);
        isTriggerGrab.AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        isTurretGrab.Value = turret.GrabPoints.Count > 0;
        isTriggerGrab.Value = trigger.GrabPoints.Count > 0;
    }
}
