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

public class FinalRoom : PassthroughRoom
{
    #region PassthroughRoom
    public override void InitializRoom()
    {
        return;
    }

    public override async UniTask StartRoom()
    {
        await EnablePassthrough();
    }

    public override async UniTask EndRoom()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {

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
