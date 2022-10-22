using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class Location: MonoBehaviour
{
    public Random rnd = new();
    [SerializeField] private LocationData data; 
    private List<Item> chancesList = new ();
    

    private void Awake()
    {
        foreach (var itemChance in data.chancesList)
            for (var i = 0; i < itemChance.weigthChance; i++)
                chancesList.Add(itemChance.item);
    }
    
    public Item GetLoot() => chancesList[rnd.Next(chancesList.Count)];

}

/*
public static List<Tuple<Item, int>> list = new()
{
    Tuple.Create(new Item("ПКП Печенег",1),1),
    Tuple.Create(new Item("Магазин ПКП Печенег",2),2),
    Tuple.Create(new Item("Магазин СВД",3),3),
    Tuple.Create(new Item("СВД",4),4),
    Tuple.Create(new Item("ИРП",5),5),
    Tuple.Create(new Item("Магазин 5.45 белый",6),6),
    Tuple.Create(new Item("На вас напал Яо-Гай",7),7),
    Tuple.Create(new Item("Армейские штаны",8),8),
    Tuple.Create(new Item("Армейский шлем",9),9),
    Tuple.Create(new Item("Магазин АС-Вал",10),10),
    Tuple.Create(new Item("Ак-74",11),11),
    Tuple.Create(new Item("Магазин 7.62 для АКМ",12),12),
    Tuple.Create(new Item("Коробка патронов 7.62 50 штук",13),13),
    Tuple.Create(new Item("Оптика х8",14),14),
    Tuple.Create(new Item("ТТ",15),15),
    Tuple.Create(new Item("Армейская куртка",16),16),
    Tuple.Create(new Item("Ничего",17),17),
    Tuple.Create(new Item("На вас напал зомби",18),18),
    Tuple.Create(new Item("АКМ",19),19),
    Tuple.Create(new Item("Наган",21),20),
    Tuple.Create(new Item("Мaгазин 5.45 черный",22),19),
    Tuple.Create(new Item("Граната",23),18),
    Tuple.Create(new Item("Металлолом",24),17),
    Tuple.Create(new Item("Микросхема",25),16),
    Tuple.Create(new Item("Магазин Кедр",26),15),
    Tuple.Create(new Item("Магазин СВД",27),14),
    Tuple.Create(new Item("АС-ВАЛ",28),13),
    Tuple.Create(new Item("На вас напал залутанный выживший",29),12),
    Tuple.Create(new Item("Коробка патронов 7.62 х 39 30 штук",30),11),
    Tuple.Create(new Item("Ботинки армейские",31),10),
    Tuple.Create(new Item("Магазин 5.45 белый",32),9),
    Tuple.Create(new Item("Кедр",33),8),
    Tuple.Create(new Item("Коробка патронов 7.62 30 штук",34),7),
    Tuple.Create(new Item("Коробка патронов 12 калибра 30 штук",35),6),
    Tuple.Create(new Item("Ничего",36),5),
    Tuple.Create(new Item("Армейский рюкзак",37),4),
    Tuple.Create(new Item("Туристический рюкзак",38),3),
    Tuple.Create(new Item("Противогаз",39),2),
    Tuple.Create(new Item("АК-74-М",40),1)
};
*/
