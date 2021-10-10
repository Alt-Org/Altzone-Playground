namespace Examples.Model.Scripts.Model
{
    /// <summary>
    /// Player character model.
    /// </summary>
    public class CharacterModel : AbstractModel
    {
        public readonly string Name;
        public readonly Defence MainDefence;
        public readonly int Speed;
        public readonly int Resistance;
        public readonly int Attack;
        public readonly int Defence;

        public CharacterModel(string name, Defence mainDefence, int speed, int resistance, int attack, int defence)
        {
            Name = name;
            MainDefence = mainDefence;
            Speed = speed;
            Resistance = resistance;
            Attack = attack;
            Defence = defence;
        }

        public override string ToString()
        {
            return
                $"{nameof(Name)}: {Name}, {nameof(MainDefence)}: {MainDefence}, {nameof(Speed)}: {Speed}, {nameof(Resistance)}: {Resistance}, {nameof(Attack)}: {Attack}, {nameof(Defence)}: {Defence}";
        }
    }
}