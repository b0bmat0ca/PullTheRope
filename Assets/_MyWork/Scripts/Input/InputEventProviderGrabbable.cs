using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UniRx;

public class InputEventProviderGrabbable : MonoBehaviour, IInputEventProvider
{

    #region IInputEventProvider
    public IReadOnlyReactiveProperty<bool> IsGrab => isGrab;
    #endregion

    [Header("トリガー"), SerializeField] private Grabbable trigger;
    private ReactiveProperty<bool> isGrab = new(false);


    // Start is called before the first frame update
    void Start()
    {
        isGrab.AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        isGrab.Value = trigger.GrabPoints.Count > 0;
    }
}
