using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
                    //sceneAnchor.gameObject.SetActive(false);
                }
            }
        }
        CullForegroundObjects();

        // 砲台の位置調整
        InitializeCannon();
    }
}
