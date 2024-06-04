using UnityEngine;

namespace _Scripts
{
    [CreateAssetMenu(fileName = "MeshSettings", menuName = "MeshSettings", order = 0)]
    public class MeshSettings : ScriptableObject
    {
        public Material Material;
        [Range(1, 1024)] public int Resolution = 250;
        [Range(0, 1)] public float Scale = 0.25f;

        [Range(0, 100)] public float param1 = 10;
        [Range(0, 2)] public float param2 = 10;
        [Range(0, 100)] public float param3 = 10;
        [Range(0, 5)] public float param4;
        [Range(0, 100)] public float FadeTime;
        [Range(0, 100)] public float Speed;
    }
}