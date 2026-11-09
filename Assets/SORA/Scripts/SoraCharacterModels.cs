using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sora.AfterTheRain.Demo
{
    public static class SoraCharacterModels
    {
        public const string ResourceFolder = "SoraCharacters/";

        public sealed class Model
        {
            public readonly string Id;
            public readonly int TwinIndex;
            public readonly float Height;
            public string Label => (Id == "MaleRover" ? "Male Rover" : Id) + " | " + Mathf.RoundToInt(Height * 100) + " cm";
            public GameObject Prefab => Resources.Load<GameObject>(ResourceFolder + Id);

            internal Model(string id, int twinIndex, float height) { Id = id; TwinIndex = twinIndex; Height = height; }
        }

        public static readonly IReadOnlyList<Model> All = Array.AsReadOnly(new[]
        {
            new Model("Chisa", 1, 1.65f), new Model("Citlali", 1, 1.52f),
            new Model("Denia", 1, 1.58f), new Model("Lynae", 1, 1.65f),
            new Model("MaleRover", 0, 1.80f), new Model("Zhongli", 0, 1.80f)
        });

        public static Model Find(string id)
        {
            foreach (var model in All) if (model.Id == id) return model;
            return null;
        }

        public static bool IsAllowed(int twinIndex, string id) => Find(id)?.TwinIndex == twinIndex;
    }
}
