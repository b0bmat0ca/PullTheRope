using UnityEngine;

/// <summary>
/// 的データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/TargetData")]
public class TargetData : ScriptableObject
{
    public string Name => _name;    // 名前
    [SerializeField] private string _name;

    public int Point => point;    // 点数
    [SerializeField] private int point;

    public bool EnableRigidBody => enableRigidBody; // RigidBodyが付与されたターゲットか
    [SerializeField] private bool enableRigidBody;
}
