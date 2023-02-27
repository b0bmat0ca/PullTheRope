using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CreditTitle : MonoBehaviour
{
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject subTitlePrefab;
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private List<CreditContent> contentList;
    [SerializeField] private double fadeInTime = 2;

    [Serializable]
    private class CreditContent
    {
        public string subTitle;
        public List<string> content;
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
    /// CreditTitleの表示
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async UniTask<bool> DisplayCredit(CancellationToken token)
    {
        bool developSection = false;

        title.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear)
            .ToUniTask(cancellationToken: token).Forget();
        await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime * 2), cancellationToken: token);

        foreach (CreditContent contentItem in contentList)
        {
            GameObject subTitle = Instantiate(contentPrefab, this.transform);
            subTitle.GetComponent<TextMeshProUGUI>().text = contentItem.subTitle;

            if (contentItem != contentList.Last())
            {
                subTitle.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: token).Forget();
            }
            else
            {
                developSection = true;
                subTitle.GetComponent<RectTransform>().DOAnchorPos(new(0, -400), 6).SetEase(Ease.Linear)
                    .ToUniTask(cancellationToken: token).Forget();
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);

            foreach (string content in contentItem.content)
            {
                GameObject obj = Instantiate(contentPrefab, this.transform);
                obj.GetComponent<TextMeshProUGUI>().text = content;

                if (!developSection)
                {
                    obj.GetComponent<RectTransform>().DOAnchorPos(new(0, 100), 11).SetEase(Ease.Linear)
                        .ToUniTask(cancellationToken: token).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);
                }
                else
                {
                    await obj.GetComponent<RectTransform>().DOAnchorPos(new(0, -500), 5).SetEase(Ease.Linear)
                        .ToUniTask(cancellationToken: token);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(fadeInTime), cancellationToken: token);
        }

        return true;
    }
}
