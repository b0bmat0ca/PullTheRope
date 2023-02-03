using UnityEngine;

/// <summary>
/// 的データ
/// </summary>
[CreateAssetMenu(menuName = "ScriptableObject/TargetData")]
public class TargetData : ScriptableObject
{
    public string Name => _name;
    [Header("名前"), SerializeField] private string _name;

    public int Point => point;
    [Header("点数"), SerializeField] private int point;

    public bool EnableLookAt => enableLookAt;
    [Header("プレイヤーの方向を常に向く"), SerializeField] private bool enableLookAt;

    public bool EnableRigidBody => enableRigidBody;
    [Header("RigidBodyが付与されたターゲット"), SerializeField] private bool enableRigidBody;
}
