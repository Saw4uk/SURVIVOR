namespace Model.GameEntity
{
    public interface IAlive: ITakingDamage, IHealing
    {
        public float Hp { get; }
    }
}