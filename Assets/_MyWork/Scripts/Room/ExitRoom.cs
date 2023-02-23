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
using System.Threading;
using DG.Tweening;

public class ExitRoom : PassthroughRoom
{
    [SerializeField] RankingInfoPresenter rankingInfoPresenter;

    [Header("案内柱"), SerializeField] private Transform information;
    [SerializeField] private GameObject endCredit;

    private bool rankingLoaded = false;

    #region PassthroughRoom
    public override void InitializRoom()
    {
        // 初期化完了通知
        onInitializeAsyncSubject.OnNext(true);
        onInitializeAsyncSubject.OnCompleted();
    }

    public override async UniTask StartRoom()
    {
        await UniTask.WaitUntil(() => rankingLoaded, cancellationToken: tokenSource.Token);
        information.position = GetPlayerInitialForwardPosition(1f, 0);
        information.LookAt(new Vector3(player.position.x, 0, player.position.z));

        await EnablePassthrough();
        //await UniTask.Delay(TimeSpan.FromSeconds(10),cancellationToken: tokenSource.Token);

        onClearAsyncSubject.OnNext(true);
        onClearAsyncSubject.OnCompleted();
    }

    public override async UniTask<bool> EndRoom()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: tokenSource.Token);

        BGMPlay();
        rankingInfoPresenter.gameObject.SetActive(false);
        endCredit.SetActive(true);

        return false;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rankingInfoPresenter.OnLodedRankingAsync
            .Subscribe(_ => rankingLoaded = true).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// パススルー表示への切り替え
    /// </summary>
    /// <returns></returns>
    private async UniTask EnablePassthrough()
    {
        foreach (SceneAnchorClassification sceneAnchorClassification in sceneAnchorClassifications)
        {
            if (sceneAnchorClassification.classification == OVRSceneManager.Classification.Couch ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Desk ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.Other ||
                sceneAnchorClassification.classification == OVRSceneManager.Classification.DoorFrame)
            {
                foreach (OVRSceneAnchor sceneAnchor in sceneAnchorClassification.anchors)
                {
                    sceneAnchor.gameObject.SetActive(false);
                }
            }
            else
            {
                foreach (OVRSceneAnchor sceneAnchor in sceneAnchorClassification.anchors)
                {
                    sceneAnchor.gameObject.SetActive(true);    // 現実世界を見えるようにする
                }
            }
        }

        await CommonUtility.Instance.FadeIn(tokenSource.Token);
    }
}
