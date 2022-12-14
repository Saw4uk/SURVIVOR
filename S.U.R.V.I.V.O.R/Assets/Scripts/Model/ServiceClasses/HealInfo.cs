using System;

public class HealInfo
{
    private float heal;
    public float Heal
    {
        get => heal;
        private set
        {
            if (heal < 0)
                throw new ArgumentException("Лечение не может быть меньше нуля!");
            heal = value;
        }
    }

    public HealInfo(float heal)
    {
        Heal = heal;
    }
}