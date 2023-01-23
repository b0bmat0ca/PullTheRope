using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;


public sealed class CommonUtility : MonoBehaviour
{
    public static CommonUtility Util { get; private set; }

    private void Awake()
    {
        if (Util == null)
        {
            Util = this;
        }
        else
        {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// シーンのリロード
    /// </summary>
    public void TransitionScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        TransitionScene(currentScene);
    }

    /// <summary>
    /// 指定シーンのロード
    /// </summary>
    /// <param name="sceneNo"></param>
    public void TransitionScene(int sceneNo)
    {
        SceneManager.LoadSceneAsync(sceneNo);
    }
}
