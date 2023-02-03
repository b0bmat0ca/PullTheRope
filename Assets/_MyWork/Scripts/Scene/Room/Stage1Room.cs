using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Cysharp.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UniRx;
using Cysharp.Threading.Tasks.Linq;
using Cysharp.Threading.Tasks.Triggers;
using Cysharp.Threading.Tasks.CompilerServices;

public class Stage1Room : PassthroughRoom
{
    [Header("制限時間"), SerializeField] private int time = 60;
    [Header("制限時間、スコアを表示するUI"), SerializeField] private GameObject playInfoUI;
    [Header("ターゲット生成位置"), SerializeField] private GameObject spawnPoint;

    private float currentTime = 0;
    private StageModel model;

    private bool roomStart = false;

    #region PassthroughRoom
    public override void  InitializRoom()
    {
        OVRSceneAnchor floorAnchor = null;

        foreach (SceneAnchormap map in sceneAnchormap)
        {
            OVRSceneAnchor sceneAnchor = map.anchor;

            if (map.name == OVRSceneManager.Classification.Couch ||
                map.name == OVRSceneManager.Classification.Desk ||
                map.name == OVRSceneManager.Classification.Other)
            {
                map.anchor.gameObject.SetActive(true);
            }
            else if (map.name == OVRSceneManager.Classification.Floor)
            {
                floorAnchor = sceneAnchor;

                if (envRoot)
                {
                    Vector3 envPos = envRoot.position;
                    float groundHeight = sceneAnchor.transform.position.y - groundDelta;
                    envRoot.position = new Vector3(envPos.x, groundHeight, envPos.z);

                    if (OVRPlugin.GetSpaceBoundary2D(sceneAnchor.Space, out Vector2[] boundary))
                    {
                        cornerPoints = boundary.ToList()
                            .ConvertAll<Vector3>(corner => new Vector3(-corner.x, corner.y, 0.0f));

                        cornerPoints.Reverse();
                        for (int i = 0; i < cornerPoints.Count; i++)
                        {
                            cornerPoints[i] = sceneAnchor.transform.TransformPoint(cornerPoints[i]);
                        }
                    }
                }
            }
        }
        CullForegroundObjects();

        // 砲台の位置調整
        cannonBase.SetActive(false);
        InitializeCannon();
    }

    public override async UniTask StartRoom(float fadeTime)
    {
        playInfoUI.transform.SetParent(cannon.transform);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), cancellationToken: this.GetCancellationTokenOnDestroy());
        roomStart = true;
    }

    public override async UniTask EndRoom()
    {
        playInfoUI.transform.SetParent(cannonBase.transform);
        cannon.transform.position = new Vector3(0, -10, 0);
        spawnPoint.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: this.GetCancellationTokenOnDestroy());
        cannonBase.SetActive(false);
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        model = GameStateManager.Instance.model;

        // 制限時間を設定
        model.Time.Value = this.time;

        // 制限時間を購読
        model.Time
            .Where(x => x == 0)
            .Subscribe( _=>
            {
                onClearAsyncSubject.OnNext(true);
                onClearAsyncSubject.OnCompleted();
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (roomStart)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1)
            {
                if (model.Time.Value > 0)
                {
                    model.Time.Value -= 1;
                }
                currentTime -= 1;
            }

        }
    }
}
