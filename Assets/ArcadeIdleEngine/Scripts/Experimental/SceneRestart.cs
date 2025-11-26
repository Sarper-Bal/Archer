using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    public class SceneRestart : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(1);
            }
        }
    }
}
