using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(AudioSource))]
public class VirtualDoorMove : MonoBehaviour
{
    public IReadOnlyReactiveProperty<bool> OnDoorOpen => onDoorOpen;
    private ReactiveProperty<bool> onDoorOpen = new(false);

    public MeshRenderer depthOccluder;
    public float doorOpenAngle = -90;

    [SerializeField] private Transform door;
    [SerializeField] private BoxCollider handleCollider;

    private AudioSource audioSource;

    private void Awake()
    {
        onDoorOpen.AddTo(this);
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        depthOccluder.gameObject.SetActive(false);
        onDoorOpen.Value = false;
        door.localRotation = Quaternion.Euler(Vector3.zero);
    }

    // Start is called before the first frame update
    void Start()
    {
        handleCollider.OnTriggerEnterAsObservable()
                .Where(other => other.CompareTag("Hand"))
                .Subscribe(async _ =>
                {
                    if (onDoorOpen.Value)
                    {
                        return;
                    }
                    onDoorOpen.Value = true;
                    depthOccluder.gameObject.SetActive(true);
                    audioSource.Play();
                    await door.DOLocalRotate(new(0, doorOpenAngle, 0), audioSource.clip.length);
                }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
