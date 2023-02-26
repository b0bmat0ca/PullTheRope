using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class SpawnBoxTarget : MonoBehaviour
{
    [SerializeField] private float minSpawnTime = 5;
    [SerializeField] private float maxSpawnTime = 15;

    [SerializeField] private Transform spawnParent;

    // ターゲットリスト
    [SerializeField] private List<TargetMap> targetList;
    [System.Serializable]
    private class TargetMap
    {
        [SerializeField] private GameObject target;
        [SerializeField] private float offsetY;
        [SerializeField] private float targetScale;

        public GameObject Target { get { return target; } }
        public float OffsetY { get { return offsetY; } }
        public float TargetScale { get { return targetScale;} }
    }

    private AudioSource audioSource;
    private StageModel model;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        model = GameStateManager.Instance.model;
        if (spawnParent == null)
        {
            spawnParent = GameObject.FindGameObjectWithTag("SpawnParent").transform;
        }
        SpawnTarget().Forget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async  UniTaskVoid SpawnTarget()
    {
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        while (true)
        {
            // 制限時間外は、生成しない
            if (model.Time.Value <= 0)
            {
                await UniTask.WaitUntil(() => model.Time.Value > 0, cancellationToken: token);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(Random.Range(minSpawnTime, maxSpawnTime)), cancellationToken: token);
            TargetMap target = targetList[Random.Range(0, targetList.Count)];
            GameObject obj = Instantiate(target.Target, transform.position + new Vector3(0, target.OffsetY, 0), Quaternion.identity);
            obj.SetActive(false);
            obj.transform.localScale *= target.TargetScale;
            obj.transform.SetParent(spawnParent);
            obj.SetActive(true);
            audioSource.Play();
            await UniTask.WaitUntil(() => obj == null, cancellationToken: token);
        }
    }
}
