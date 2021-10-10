namespace Examples.Model.Scripts.Model
{
    public enum Defence
    {
        Desensitisation,
        Deflection,
        Introjection,
        Projection,
        Retroflection,
        Egotism,
        Confluence,
    }

    /// <summary>
    /// Player Defence model.
    /// </summary>
    public class DefenceModel : AbstractModel
    {
        public readonly Defence Defence;

        public DefenceModel(Defence defence)
        {
            Defence = defence;
        }

        public override string sortValue()
        {
            return Defence.ToString();
        }

        public override string ToString()
        {
            return $"{nameof(Defence)}: {Defence}";
        }
    }
}