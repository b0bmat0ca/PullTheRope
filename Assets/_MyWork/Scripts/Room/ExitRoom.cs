using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class ExitRoom : PassthroughRoom
{
    [SerializeField] private RankingInfoPresenter rankingInfoPresenter;

    [Header("案内柱"), SerializeField] private Transform information;
    [SerializeField] private CreditTitle creditTitle;
    [Header("ランキング表示時間"), SerializeField] private float rankingFadeTime = 10f;
    [Header("エンドクレジット表示終了時間"), SerializeField] private float creditFadeTime = 20f;

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

        onClearAsyncSubject.OnNext(true);
        onClearAsyncSubject.OnCompleted();
    }

    public override async UniTask<bool> EndRoom()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(rankingFadeTime), cancellationToken: tokenSource.Token);

        BGMPlay();
        rankingInfoPresenter.gameObject.SetActive(false);
        creditTitle.transform.parent.gameObject.SetActive(true);
        await creditTitle.DisplayCredit(tokenSource.Token);

        await UniTask.Delay(TimeSpan.FromSeconds(creditFadeTime), cancellationToken: tokenSource.Token);

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
