using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private float _delayTime = 4f;
        private WaitForSeconds _delay;


        public void LoadScene()
        {
            StartCoroutine(WaitForReload());
        }

        private IEnumerator WaitForReload()
        {
            yield return _delay;
            
            SceneManager.LoadScene(_sceneName);
        }
        
    }
}