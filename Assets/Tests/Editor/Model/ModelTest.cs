using Examples.Model.Scripts.Model;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor.Model
{
    [TestFixture]
    public class ModelTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp start");
            Models.Clear();
            ModelLoader.LoadModels();
            Debug.Log("OneTimeSetUp done");
        }

        [Test]
        public void GetByName()
        {
            var model1 = Models.GetByName<DefenceModel>("Confluence");
            Assert.That(model1, Is.Not.Null);
            var model2 = Models.GetByName<CharacterModel>("Vitsiniekka");
            Assert.That(model2, Is.Not.Null);
            var model3 = Models.GetByName<CharacterModel>("Unikeko");
            Assert.That(model3, Is.Not.Null);
        }

        [Test]
        public void GetById()
        {
            var model1 = Models.GetById<DefenceModel>(1);
            Assert.That(model1.Id, Is.EqualTo(1));
            var model2 = Models.GetById<CharacterModel>(2);
            Assert.That(model2.Id, Is.EqualTo(2));
            var model3 = Models.GetById<CharacterModel>(3);
            Assert.That(model3.Id, Is.EqualTo(3));
        }

        [Test]
        public void GetBySelector()
        {
           var model1 = Models.Get<DefenceModel>(x => x.Defence == Defence.Egotism);
            Assert.That(model1.Id, Is.EqualTo((int)Defence.Egotism));
            var model2 = Models.Get<CharacterModel>(x => x.Speed == 6);
            Assert.That(model2.Speed, Is.EqualTo(6));
            var model3 = Models.Get<CharacterModel>(x => x.Name == "Unikeko");
            Assert.That(model3.Name, Is.EqualTo("Unikeko"));
        }

        [Test]
        public void GetAll()
        {
            var characterModels = Models.GetAll<CharacterModel>();
            const int expectedCharacters = 7;
            Assert.That(characterModels.Count, Is.EqualTo(expectedCharacters));
            var defenceModels = Models.GetAll<DefenceModel>();
            const int expectedDefences = 7;
            Assert.That(defenceModels.Count, Is.EqualTo(expectedDefences));
        }
    }
}