using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorEngine
{
    [CreateAssetMenu(fileName = "Main_Hub", menuName = "Main_Hub")]
    public class SceneReference : ScriptableObject
    {
        public string Name;

        public bool IsLoaded()
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                if (SceneManager.GetSceneAt(i).name == Name)
                    return true;
            }

            return false;
        }
    }
}