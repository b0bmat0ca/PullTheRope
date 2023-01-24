using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IInputEventProvider
{
    IReadOnlyReactiveProperty<bool> IsTurretGrab { get; }

    IReadOnlyReactiveProperty<bool> IsTriggerGrab { get; }
    
}
