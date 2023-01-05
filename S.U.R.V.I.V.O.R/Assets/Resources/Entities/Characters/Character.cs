using System;
using System.Collections.Generic;
using System.Linq;
using Model.GameEntity;
using Model.GameEntity.Skills;
using UnityEngine;

public class Character : Entity
{
    public readonly ManBody body = new ();
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
    
    public void Eat(EatableFood food)
    {
        body.Energy += food.Data.DeltaEnergy;
        body.Water += food.Data.DeltaWater;
        body.Hunger += food.Data.DeltaHunger;
    }
    
    public IEnumerable<GameObject> Cook(CookableFood food)
    {
        var cookedObjects = food.ObjectToSpawnAfterCook.Select(Instantiate);
        Destroy(food.gameObject);
        return cookedObjects;
    }
    
    public MeleeWeapon MeleeWeapon { get; set; }
    public readonly Skills skills;

    public Character()
    {
        skills = new Skills(this);
    }

    public event Action OnGunsChanged; 

    public override Body Body => body;
    public int Mobility => throw new NotImplementedException(); //Скорость передвижения на глобальной карте
    
    public override void Attack(IEnumerable<BodyPart> targets, float distance)
    {
        targets.First().TakeDamage(new DamageInfo(15100f));
    }
}