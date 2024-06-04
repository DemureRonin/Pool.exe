using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class DontDestroyOnSelectedScenes : MonoBehaviour
    {
        [SerializeField] private List<string> _sceneNames;

        [SerializeField] private string _instanceName;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckForDuplicateInstances();

            CheckIfSceneInList();
        }

        private void CheckForDuplicateInstances()
        {
            var collection = FindObjectsByType<DontDestroyOnSelectedScenes>(FindObjectsSortMode.None);

            foreach (var obj in collection)
            {
                if (obj == this) continue;
                if (obj._instanceName == _instanceName)
                {
                    DestroyImmediate(obj.gameObject);
                }
            }
        }

        private void CheckIfSceneInList()
        {
            var currentScene = SceneManager.GetActiveScene().name;

            if (_sceneNames.Contains(currentScene))
            {
            }
            else
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;

                DestroyImmediate(gameObject);
            }
        }
    }
}