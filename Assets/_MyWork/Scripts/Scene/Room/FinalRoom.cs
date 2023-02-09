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

public class FinalRoom : PassthroughRoom
{
    [SerializeField] RankingInfoPresenter rankingInfoPresenter;
    [SerializeField] GameObject rankingDialog;

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
        await UniTask.WaitUntil(() => rankingLoaded, cancellationToken: this.GetCancellationTokenOnDestroy());
        await EnablePassthrough();

        rankingDialog.transform.SetPositionAndRotation(GetPlayerForwardPosition(0.8f, 1f),
            Quaternion.Euler(new(rankingDialog.transform.rotation.eulerAngles.x, player.eulerAngles.y, 0)));
    }

    public override async UniTask<bool> EndRoom()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());

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
            text.text += sceneAnchorClassification.classification;
            foreach (OVRSceneAnchor sceneAnchor in sceneAnchorClassification.anchors)
            {
                sceneAnchor.gameObject.SetActive(!sceneAnchor.gameObject.activeSelf);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
