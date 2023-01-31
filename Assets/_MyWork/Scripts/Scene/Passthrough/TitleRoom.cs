using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using Cysharp.Threading.Tasks;

public class TitleRoom : PassthroughRoom
{
    [SerializeField] private GameObject randomBox;

    protected override void Awake()
    {
        base.Awake();
        randomBox.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomBoxPosition = CannonPosition(randomBox.transform.position.y);
        Vector3 randomBoxRotation = new(0, mainCamera.transform.eulerAngles.y, 0);

        randomBox.transform.SetPositionAndRotation(randomBoxPosition, Quaternion.Euler(randomBoxRotation));
        randomBox.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region PassthroughRoom
    public override void InitializRoom()
    {
        OVRSceneAnchor[] sceneAnchors = FindObjectsOfType<OVRSceneAnchor>();
        OVRSceneAnchor floorAnchor = null;

        if (sceneAnchors != null)
        {
            foreach (OVRSceneAnchor sceneAnchor in sceneAnchors)
            {
                OVRSemanticClassification classification = sceneAnchor.GetComponent<OVRSemanticClassification>();

                if (classification.Contains(OVRSceneManager.Classification.Ceiling) ||
                    classification.Contains(OVRSceneManager.Classification.DoorFrame) ||
                    classification.Contains(OVRSceneManager.Classification.WallFace) ||
                    classification.Contains(OVRSceneManager.Classification.WindowFrame))
                {
                    sceneAnchor.gameObject.SetActive(false); // Passthrough で現実世界が「見えなくなる」
                    
                }
                else if (classification.Contains(OVRSceneManager.Classification.Floor))
                {
                    floorAnchor = sceneAnchor;

                    if (envRoot)
                    {
                        Vector3 envPos= envRoot.position;
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
                    sceneAnchor.gameObject.SetActive(false);    // Passthrough で現実世界が「見えなくなる」
                }
                else if (classification.Contains(OVRSceneManager.Classification.Desk) ||
                         classification.Contains(OVRSceneManager.Classification.Other) ||
                         classification.Contains(OVRSceneManager.Classification.Couch))
                {
                    sceneAnchor.gameObject.SetActive(false);
                }
            }
        }
        CullForegroundObjects();

        cannonBase.SetActive(false);
    }
    #endregion

    public void InstantiateTitle()
    {
        InitializeTitleAndCannon().Forget();
    }

    public async UniTask InitializeTitleAndCannon()
    {
        randomBox.GetComponent<RandomBoxTarget>().DestroyBox();
        await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());

        InitializeCannon(false);

        //onClearAsyncSubject.OnNext(true);
        //onClearAsyncSubject.OnCompleted();
    }
}
