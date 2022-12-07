﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : Entity
{
    
    [SerializeField] private Sprite sprite; 
    [SerializeField] private string firstName;
    [SerializeField] private string surname;

    public Sprite Sprite => sprite;
    public string FirstName => firstName;
    public string Surname => surname;

    private Gun primaryGun;

    public Gun PrimaryGun
    {
        get => primaryGun;
        set
        {
            primaryGun = value;
            OnGunsChanged?.Invoke();
        }
    }
    
    private Gun secondaryGun;
    public Gun SecondaryGun
    {
        get => secondaryGun;
        set
        {
            secondaryGun = value;
            OnGunsChanged?.Invoke();
        }
    }
    
    public MeleeWeapon MeleeWeapon { get; set; }
    public readonly Skills skills = new Skills();
    public readonly ManBody body = new ManBody();

    public event Action OnGunsChanged; 

    public override Body Body => body;
    public int Mobility => throw new NotImplementedException(); //Скорость передвижения на глобальной карте
    
    public override void Attack(List<BodyPart> targets, float distance)
    {
        throw new NotImplementedException();
    }
}