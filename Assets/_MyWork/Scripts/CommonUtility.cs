using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UniRx;
using Cysharp.Threading.Tasks.Triggers;

public sealed class CommonUtility : MonoBehaviour
{
    public static CommonUtility Instance { get; private set; }

    [SerializeField] private GameObject debugDialog;
    [SerializeField] private TextMeshProUGUI debugText;

    [SerializeField] private MeshRenderer fadeSphere;

    [SerializeField] private OVRScreenFade2 screenFade;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Debug(string message)
    {
        debugText.text += $"{message}\n";
    }

    public void ExitExplicitFade()
    {
        screenFade.SetExplicitFade(0);
    }

    public void FadeIn()
    {
        fadeSphere.gameObject.SetActive(false);
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
    }

    public async UniTask<bool> FadeIn(CancellationToken token, float fadeTime = 2)
    {
        await fadeSphere.sharedMaterial.DOFade(0, fadeTime)
            .OnComplete(() =>
            {
                FadeIn();
            }).ToUniTask(cancellationToken: token);

        return true;
    }


    public void FadeOut()
    {
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
        fadeSphere.gameObject.SetActive(true);
    }

    public async UniTask<bool> FadeOut(CancellationToken token, float fadeTime = 2)
    {
        fadeSphere.sharedMaterial.SetColor("_Color", new Color(0, 0, 0, 0));
        fadeSphere.gameObject.SetActive(true);

        await fadeSphere.sharedMaterial.DOFade(1, fadeTime).ToUniTask(cancellationToken: token);

        return true;
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
