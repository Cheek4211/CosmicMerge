using UnityEngine;

[CreateAssetMenu(fileName = "Ship Data", menuName = "Scriptable Objects/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Ship Basic Info")]
    public string shipName;
    public int level;

    [Header("Visual Settings")]
    public Sprite shipSprite;   // 우주선 이미지
    public float scale = 1f;  // 우주선 크기


    [Header("Merge Settings")]
    public int scoreOnMerge;
}