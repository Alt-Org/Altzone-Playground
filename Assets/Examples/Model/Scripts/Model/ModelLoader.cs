namespace Examples.Model.Scripts.Model
{
    /// <summary>
    /// Utility class to load all models for runtime.
    /// </summary>
    public static class ModelLoader
    {
        public static void LoadModels()
        {
            add(Defence.Desensitisation.ToString(), new DefenceModel(Defence.Desensitisation));
            add(Defence.Deflection.ToString(), new DefenceModel(Defence.Deflection));
            add(Defence.Introjection.ToString(), new DefenceModel(Defence.Introjection));
            add(Defence.Projection.ToString(), new DefenceModel(Defence.Desensitisation));
            add(Defence.Retroflection.ToString(), new DefenceModel(Defence.Retroflection));
            add(Defence.Egotism.ToString(), new DefenceModel(Defence.Egotism));
            add(Defence.Confluence.ToString(), new DefenceModel(Defence.Confluence));

            add("Koulukiusaaja", new CharacterModel(
                "Koulukiusaaja", Defence.Desensitisation, 3, 9, 6, 2));
            add("Vitsiniekka", new CharacterModel(
                "Vitsiniekka", Defence.Deflection, 9, 2, 4, 5));
            add("Hissukkasihteeri", new CharacterModel(
                "Hissukkasihteeri", Defence.Introjection, 5, 5, 5, 5));
            add("Taiteilija", new CharacterModel(
                "Taiteilija", Defence.Projection, 2, 2, 8, 8));
            add("Hodariläski", new CharacterModel(
                "Hodariläski", Defence.Retroflection, 3, 6, 2, 9));
            add("Älykkö", new CharacterModel(
                "Älykkö", Defence.Egotism, 6, 2, 6, 6));
            add("Unikeko", new CharacterModel(
                "Unikeko", Defence.Confluence, 6, 7, 1, 6));
        }

        private static void add(string name, AbstractModel model)
        {
            Models.Add(model, name);
        }
    }
}