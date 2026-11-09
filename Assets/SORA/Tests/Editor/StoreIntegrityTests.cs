using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Sora.AfterTheRain.Demo.Tests
{
    public sealed class StoreIntegrityTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private SoraDemoCore core;

        private T Component<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            objects.Add(go);
            return go.AddComponent<T>();
        }

        private void Call(string name, params object[] args)
        {
            var method = typeof(SoraDemoCore).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Prototype method changed; update this behavioral test.");
            method.Invoke(core, args);
        }

        private void Interact(InteractKind kind)
        {
            var target = Component<SoraInteractable>("Test interaction");
            target.kind = kind;
            Call("Interact", target);
        }

        [SetUp]
        public void SetUp()
        {
            core = Component<SoraDemoCore>("Test store");
            core.RegisterCharacter(Component<SoraDemoCharacter>("Test Toshiharu"));
            core.RegisterCharacter(Component<SoraDemoCharacter>("Test Akiko"));
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = objects.Count - 1; i >= 0; i--) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void DraftDoesNotChangeStockOrCashAndCancelsOnSwitch()
        {
            Interact(InteractKind.Register);
            Assert.That(core.HasPendingSale, Is.True);
            Assert.That(core.State.shelfStock, Is.EqualTo(8));
            Assert.That(core.State.storeCash, Is.Zero);
            Call("SwitchCharacter");
            Assert.That(core.HasPendingSale, Is.False);
            Assert.That(core.State.shelfStock, Is.EqualTo(8));
            Assert.That(core.State.sales, Is.Zero);
        }

        [Test]
        public void PaidSaleSurvivesOneHundredSwitchesWithoutDuplicateCharge()
        {
            Interact(InteractKind.Register);
            Interact(InteractKind.Register);
            for (int i = 0; i < 100; i++) Call("SwitchCharacter");
            Assert.That(core.State.shelfStock, Is.EqualTo(7));
            Assert.That(core.State.storeCash, Is.EqualTo(380));
            Assert.That(core.State.sales, Is.EqualTo(1));
            Assert.That(core.State.activeIndex, Is.Zero);
            Assert.That(core.HasPendingSale, Is.False);
        }

        [Test]
        public void EmptyShelfCannotCreateSaleDraft()
        {
            core.State.shelfStock = 0;
            Interact(InteractKind.Register);
            Assert.That(core.HasPendingSale, Is.False);
            Assert.That(core.State.storeCash, Is.Zero);
        }

        [Test]
        public void DuplicateDeliveryAndRestockingPreserveTotalInventory()
        {
            Interact(InteractKind.Delivery);
            Interact(InteractKind.Delivery);
            Assert.That(core.State.deliveries, Is.EqualTo(1));
            Assert.That(core.State.backStock, Is.EqualTo(6));
            Interact(InteractKind.Shelf);
            Assert.That(core.State.shelfStock, Is.EqualTo(11));
            Assert.That(core.State.backStock, Is.EqualTo(3));
            Assert.That(core.State.shelfStock + core.State.backStock, Is.EqualTo(14));
        }

        [Test]
        public void DialogueRelationsRemainSeparateForEachTwin()
        {
            Interact(InteractKind.Customer);
            Call("ChooseDialogue", 1);
            Call("SwitchCharacter");
            Assert.That(core.State.familiarity[0], Is.EqualTo(1));
            Assert.That(core.State.familiarity[3], Is.Zero);
            Interact(InteractKind.Customer);
            Call("ChooseDialogue", 2);
            Assert.That(core.State.familiarity[0], Is.EqualTo(1));
            Assert.That(core.State.familiarity[3], Is.EqualTo(2));
            Assert.That(core.State.trust[0], Is.EqualTo(1));
            Assert.That(core.State.trust[3], Is.Zero);
        }

        [Test]
        public void ModelMenuCancelsDraftWithoutChangingSharedProgress()
        {
            Interact(InteractKind.Register);
            core.State.familiarity[3] = 4;
            core.SetModelSelectionOpen(true);
            Assert.That(core.IsChoosingModels, Is.True);
            Assert.That(core.HasPendingSale, Is.False);
            Assert.That(core.State.shelfStock, Is.EqualTo(8));
            Assert.That(core.State.sales, Is.Zero);
            Assert.That(core.State.familiarity[3], Is.EqualTo(4));
            Assert.That(core.SelectCharacterModel(0, "Chisa"), Is.False);
            Assert.That(core.SelectCharacterModel(1, "Zhongli"), Is.False);
            Assert.That(core.SelectCharacterModel(-1, "MaleRover"), Is.False);
            Assert.That(core.SelectCharacterModel(1, "../../unexpected"), Is.False);
        }

        [Test]
        public void SaveRestoresSelectedModelsAndLegacySavesKeepTheirProgress()
        {
            if (SoraCharacterModels.All.Any(m => m.Prefab == null)) Assert.Ignore("Local owner models absent.");
            foreach (var character in core.Characters) character.InitializeAppearance(core.Characters.IndexOf(character));
            var controllers = core.Characters.Select(c => c.GetComponent<CharacterController>()).ToArray();
            foreach (var model in SoraCharacterModels.All)
            {
                Assert.That(core.SelectCharacterModel(model.TwinIndex, model.Id), Is.True);
                Assert.That(core.Characters[model.TwinIndex].Height, Is.EqualTo(model.Height).Within(.001f));
                Assert.That(core.Characters[model.TwinIndex].GetComponent<CharacterController>(), Is.SameAs(controllers[model.TwinIndex]));
            }
            core.State.storeCash = 380;
            core.State.sales = 1;
            core.State.shelfStock = 7;
            core.State.familiarity[3] = 2;
            core.State.activeIndex = 1;
            core.State.characterPositions = new[] { 1f, 0f, 2f, 3f, 0f, 4f };
            var saved = JsonUtility.FromJson<SoraSaveData>(JsonUtility.ToJson(core.State));
            Assert.That(core.SelectCharacterModel(0, "MaleRover"), Is.True);
            Assert.That(core.SelectCharacterModel(1, "Chisa"), Is.True);
            Call("RestoreSave", saved);
            Assert.That(core.Characters[0].ModelId, Is.EqualTo("Zhongli"));
            Assert.That(core.Characters[1].ModelId, Is.EqualTo("Lynae"));
            Assert.That(core.State.storeCash, Is.EqualTo(380));
            Assert.That(core.State.sales, Is.EqualTo(1));
            Assert.That(core.State.shelfStock, Is.EqualTo(7));
            Assert.That(core.State.familiarity[3], Is.EqualTo(2));
            Assert.That(core.Characters[1].transform.position, Is.EqualTo(new Vector3(3, 0, 4)));
            saved.toshiharuModel = saved.akikoModel = null;
            Call("RestoreSave", saved);
            Assert.That(core.Characters.All(c => c.ModelId == null), Is.True);
            Assert.That(core.State.storeCash, Is.EqualTo(380));
        }

        [Test]
        public void InvalidSavedModelCannotReplaceCurrentState()
        {
            var before = core.State;
            var invalid = new SoraSaveData { toshiharuModel = "Denia", storeCash = 12345 };
            Call("RestoreSave", invalid);
            Assert.That(core.State, Is.SameAs(before));
            invalid.toshiharuModel = null;
            invalid.characterPositions[0] = float.NaN;
            Call("RestoreSave", invalid);
            Assert.That(core.State, Is.SameAs(before));
        }
    }
}
