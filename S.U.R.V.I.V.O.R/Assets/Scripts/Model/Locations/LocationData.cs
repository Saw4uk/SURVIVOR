using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New LocationData", menuName = "Data/Location Data", order = 50)]
public class LocationData: ScriptableObject
{
    public List<ItemChance> chancesList;
}