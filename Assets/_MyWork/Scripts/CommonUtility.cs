using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public sealed class CommonUtility : MonoBehaviour
{
    public static CommonUtility Instance { get; private set; }

    [SerializeField] private GameObject debugDialog;
    [SerializeField] private TextMeshProUGUI debugText;

    [SerializeField] private MeshRenderer fadeSphere;
    [SerializeField] private OVRScreenFade2 screenFade;

    [Header("制限時間、スコアを管理するモデル")] public StageModel model;

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

    /// <summary>
    /// VR空間上へのDebugメッセージ表示
    /// </summary>
    /// <param name="message"></param>
    public void Debug(string message)
    {
        debugText.text += $"{message}\n";
    }

    /// <summary>
    /// OVRScreenFadeの解除
    /// </summary>
    public void ExitExplicitFade()
    {
        screenFade.SetExplicitFade(0);
    }

    /// <summary>
    /// 即座にフェードイン
    /// </summary>
    public void FadeIn()
    {
        fadeSphere.gameObject.SetActive(false);
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
    }

    /// <summary>
    /// 指定時間をかけてフェードイン
    /// </summary>
    /// <param name="token"></param>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    public async UniTask<bool> FadeIn(CancellationToken token, float fadeTime = 2)
    {
        await fadeSphere.sharedMaterial.DOFade(0, fadeTime)
            .OnComplete(() =>
            {
                FadeIn();
            }).ToUniTask(cancellationToken: token);

        return true;
    }

    /// <summary>
    /// 即座にフェードアウト
    /// </summary>
    public void FadeOut()
    {
        fadeSphere.sharedMaterial.SetColor("_Color", Color.black);
        fadeSphere.gameObject.SetActive(true);
    }

    /// <summary>
    /// 指定時間をかけてフェードアウト
    /// </summary>
    /// <param name="token"></param>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    public async UniTask<bool> FadeOut(CancellationToken token, float fadeTime = 2)
    {
        fadeSphere.sharedMaterial.SetColor("_Color", new Color(0, 0, 0, 0));
        fadeSphere.gameObject.SetActive(true);

        await fadeSphere.sharedMaterial.DOFade(1, fadeTime).ToUniTask(cancellationToken: token);

        return true;
    }

    /// <summary>
    /// アプリケーションの再起動
    /// </summary>
    public void RestartAndroid()
    {
        if (Application.isEditor)
        {
            return;
        }

        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
            const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            var intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);

            intent.Call<AndroidJavaObject>("setFlags", kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
            currentActivity.Call("startActivity", intent);
            currentActivity.Call("finish");
            var process = new AndroidJavaClass("android.os.Process");
            int pid = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }
    }
}
