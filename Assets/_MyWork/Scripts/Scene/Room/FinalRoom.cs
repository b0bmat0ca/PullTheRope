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

    public override async UniTask StartRoom(float fadeTime)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(fadeTime), cancellationToken: this.GetCancellationTokenOnDestroy());
        EnablePassthrough().Forget();
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
    private async UniTaskVoid EnablePassthrough()
    {
        foreach (SceneAnchormap map in sceneAnchormap)
        {

            map.anchor.gameObject.SetActive(!map.anchor.gameObject.activeSelf);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }


}
