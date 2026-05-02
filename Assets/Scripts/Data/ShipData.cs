using UnityEngine;

[CreateAssetMenu(fileName = "Ship Data", menuName = "Scriptable Objects/Ship Data")]
public class ShipData : ScriptableObject
{
    [Header("Ship Basic Info")]
    public string shipName;
    public int level;

    [Header("Visual Settings")]
    public Sprite shipSprite;
    public float scale = 1f;


    [Header("Merge Settings")]
    public int scoreOnMerge;
}