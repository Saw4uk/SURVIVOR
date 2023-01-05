using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Model.GameEntity;

public class FightCharacter : MonoBehaviour
{
    public Entity Entity { get; private set; }
    public CharacterType Type { get; private set; }

    public void ApplyProperties(Entity target, CharacterType type, bool alive = true)
    {
        target.Body.Died += OnDied;
        target.Body.Died += FightSceneController.Instance.DrawAreas;
        target.Body.Died += FightSceneController.Instance.SetNearestNodeToCurrentCharacter;
        Entity = target;
        Type = type;
        Alive = alive;
        RemainingEnergy = Energy;
    }

    public float Initiative => Entity.Initiative;
    public int Energy => Entity.SpeedInFightScene;
    public int RemainingEnergy {get; set;}
    public bool Alive = true;
    public GameObject TargetToHit;
    public readonly float radius = 1.5f;
    
    // public void MakeShoot(GameObject targetObj, string targetName)
    // {
    //     transform.LookAt(targetObj.transform);
    //     //RangeWeapon.Shoot(gameObject, targetObj, "Head");
    // }
    //
    // public void MakeHit()
    // {
    //     transform.LookAt(TargetToHit.transform);
    //     //MeleeWeapon.Hit(TargetToHit);
    // }

    public void MakeShoot(GameObject targetObj)
    {
        var shootDirection = targetObj.transform.position - transform.position;
        var ray = Physics.Raycast(transform.position, shootDirection, out var hit);
        if (hit.transform.gameObject.GetComponent<FightCharacter>())
        {
            TargetToHit = targetObj;
            Attack();
        }
        else
        {
            Debug.Log("Попал в препятствие");
        }
    }

    public void Attack()
    {
        var lookPoint = new Vector3(TargetToHit.transform.position.x, transform.position.y,
            TargetToHit.transform.position.z);
        transform.LookAt(lookPoint);
        var bodyParts = TargetToHit.GetComponent<FightCharacter>().Entity.Body.BodyParts;
        Entity.Attack(bodyParts, Vector3.Distance(gameObject.transform.position,
                                                             TargetToHit.transform.position));
    }

    public void ResetEnergy()
    {
        RemainingEnergy = Energy;
    }

    public void OnDisable()
    {
        Entity.Body.Died -= OnDied;
        Entity.Body.Died -= FightSceneController.Instance.DrawAreas;
        Entity.Body.Died -= FightSceneController.Instance.SetNearestNodeToCurrentCharacter;
    }

    private void OnDied()
    {
        FightSceneController.Instance.DeleteDeathCharacterFromQueue(this);
        UIController.Instance.DeleteDeathCharacterCard(this);
        Destroy(gameObject);
    }
}
