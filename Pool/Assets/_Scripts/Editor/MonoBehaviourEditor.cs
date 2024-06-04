using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviourEditor : UnityEditor.Editor
    {
    }
}