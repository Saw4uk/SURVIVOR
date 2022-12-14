using System.Collections.Generic;

namespace Model.GameEntity.Health
{
    public abstract class Health
    {
        public abstract IAlive Target { get; }
        public abstract ICollection<HealthProperty> HealthProperties { get; }
    
        public void AddProperty(HealthProperty property)
        {
            if (HealthProperties.Contains(property))
                return;
            HealthProperties.Add(property);
            property.InitialAction(this);
        }

        public void DeleteProperty(HealthProperty property)
        {
            HealthProperties.Remove(property);
            property.FinalAction(this);
        }

        public void OnTurnEnd()
        {
            foreach (var healthProperty in HealthProperties)
                healthProperty.OnTurnEnd(this);
        }
    }
}