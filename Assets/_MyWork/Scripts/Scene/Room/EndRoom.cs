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

public class EndRoom : PassthroughRoom
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public override void InitializRoom()
    {
        DisableVirtualWorld().Forget();

        
    }

    private async UniTaskVoid DisableVirtualWorld()
    {
        foreach (SceneAnchormap map in sceneAnchormap)
        {
            map.anchor.gameObject.SetActive(!map.anchor.gameObject.activeSelf);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }
}
