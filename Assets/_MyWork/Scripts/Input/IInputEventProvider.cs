using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IInputEventProvider
{
    public IReadOnlyReactiveProperty<bool> IsTurretGrab { get; }

    public TurretGrabbedHand TurretGrabbed { get; }

    public IReadOnlyReactiveProperty<bool> IsTriggerGrab { get; }
}
