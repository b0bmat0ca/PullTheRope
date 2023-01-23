using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

[RequireComponent(typeof(AudioSource))]
public class Target : MonoBehaviour
{
    [SerializeField] protected TargetData targetData;
    [SerializeField] protected Collider colider;
    [SerializeField] protected RayfireRigid rayFireRigid;
    [SerializeField] protected float destroyTime = 5.0f;

    protected MeshRenderer[] shatterMesh;   // RayFire Shutter で事前に粉砕されたオブジェクトの配列

    protected AudioSource audioSource;

    protected string _name;
    protected int point;

    protected virtual void Awake()
    {
        _name = targetData.Name;
        point = targetData.Point;
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        shatterMesh = rayFireRigid.gameObject.GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // 粉砕する
            rayFireRigid.Demolish();

            // コライダーを無効化する
            colider.enabled = false;

            // 粉砕後、{destroyTime}秒後にDestroyする
            foreach (MeshRenderer mesh in shatterMesh)
            {
                Destroy(mesh.gameObject, destroyTime);
            }
            Destroy(gameObject, destroyTime);
        }
    }
}
