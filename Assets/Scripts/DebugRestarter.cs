using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugRestarter : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.P))
            OnRestart();
    }

    void OnRestart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
