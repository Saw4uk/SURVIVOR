using UnityEngine;

public class Gun: Weapon
{
    [SerializeField] private Magazine currentMagazine;
    [SerializeField] private Bullet chamber;
    [SerializeField] private GunModule gunModule;
    /* 
     * После патронника в реализации абстракных классов будут идти слоты для GunModule.
     * У каждого оружия будет свой допустимый набор возможных GunModules
     */

    public Shoot GiveShoot()
    {
        //TODO реализовать метод который возвращает служебный класс выстрела
        return default;
    }
}

