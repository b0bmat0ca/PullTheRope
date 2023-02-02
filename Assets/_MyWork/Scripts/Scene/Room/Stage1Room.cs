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

public class Stage1Room : PassthroughRoom
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
}
