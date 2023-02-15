using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UniRx.Triggers;

public class VisibleCamera : MonoBehaviour
{
    public IReadOnlyReactiveProperty<bool> OnVisibleCamera => onVisibleCamera;
    private ReactiveProperty<bool> onVisibleCamera = new(true);

    private void Awake()
    {
        onVisibleCamera.AddTo(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        // カメラの範囲内を購読
        this.OnBecameVisibleAsObservable()
            .Subscribe(_ => onVisibleCamera.Value = true)
            .AddTo(this);

        // カメラの範囲外を購読
        this.OnBecameInvisibleAsObservable()
            .Subscribe(_ => onVisibleCamera.Value = false)
            .AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
