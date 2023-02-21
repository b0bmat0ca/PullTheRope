using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using UniRx;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using UnityEngine.Experimental.XR.Interaction;

public class InputEventProviderGrabbable : MonoBehaviour, IInputEventProvider
{

    #region IInputEventProvider
    public IReadOnlyReactiveProperty<bool> IsTurretGrab => isTurretGrab;

    public TurretGrabbedHand TurretGrabbed
    {
        get
        {
            if (leftHandInteractor.Interactable
                && leftHandInteractor.Interactable.transform.parent.gameObject.name == "Turret")
            {
                return TurretGrabbedHand.Left;
            }
            else if (rightHandInteractor.Interactable
                && rightHandInteractor.Interactable.transform.parent.gameObject.name == "Turret")
            {
                return TurretGrabbedHand.Right;
            }
            return TurretGrabbedHand.None;
        }
    }

    public IReadOnlyReactiveProperty<bool> IsTriggerGrab => isTriggerGrab;
    #endregion

    public HandGrabInteractor leftHandInteractor;
    public HandGrabInteractor rightHandInteractor;

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
        
    }

    public void TurretGrab()
    {
        isTurretGrab.Value = true;
    }

    public void TurretRelease()
    {
        isTurretGrab.Value = false;
    }

    public void TriggerGrab()
    {
        isTriggerGrab.Value = true;
    }

    public void TriggerRelease()
    {
        isTriggerGrab.Value = false;
    }
}
