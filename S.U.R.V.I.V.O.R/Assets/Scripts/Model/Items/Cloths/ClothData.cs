﻿using UnityEngine;

[CreateAssetMenu(fileName = "New ClothData", menuName = "Data/Cloth Data", order = 50)]
public class ClothData : ScriptableObject
{
    [SerializeField] private int maxArmor;
    [SerializeField] private Size inventorySize;
    [SerializeField] private float warm;

    public int MaxArmor => maxArmor;
    public Size InventorySize => inventorySize;

    public float Warm => warm;
}