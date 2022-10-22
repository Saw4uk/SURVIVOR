﻿using UnityEngine;

[CreateAssetMenu(fileName = "New GunData", menuName = "Gun Data", order = 52)]
public class GunData: ScriptableObject
{
    [SerializeField] private int fireRate;
    [SerializeField] private float accuracy;
    [SerializeField] private float extraDamage;
    [SerializeField] private float fireDistance;
    [SerializeField] private float ergonomics; //Чем выше, тем больше негативное влияние на Mobility класса персонажа
    
     public int FireRate => fireRate;
     public float Accuracy => accuracy;
     public float ExtraDamage => extraDamage;
     public float FireDistance => fireDistance;
     public float Ergonomics => ergonomics; //Чем выше, тем больше негативное влияние на Mobility класса персонажа
    
}