using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    Text text;

    private void Start()
    {
        text = GetComponent<Text>();

        InvokeRepeating("displayFPS", 0.2f, 0.1f);
    }

    void displayFPS()
    {
        text.text = System.Convert.ToInt32(1f / Time.smoothDeltaTime).ToString();
    }
}
