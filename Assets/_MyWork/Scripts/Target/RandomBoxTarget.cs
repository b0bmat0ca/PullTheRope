using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomBoxTarget : MonoBehaviour
{
    [SerializeField] private CollisionEventSubject subject;
    [SerializeField] private GameObject boxPattern;
    [SerializeField] private Transform itemSpaawnPoint;
    [SerializeField] private GameObject[] items;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 弾丸と衝突したかを購読
        subject.OnCollisionEnterAsync
            .Subscribe(_ =>
            {
                boxPattern.SetActive(false);
                GameObject obj = Instantiate(items[Random.Range(0, items.Length -1)], itemSpaawnPoint.position, Quaternion.identity);
                obj.SetActive(false);
                obj.transform.localScale = transform.localScale;
                obj.SetActive(true);
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
