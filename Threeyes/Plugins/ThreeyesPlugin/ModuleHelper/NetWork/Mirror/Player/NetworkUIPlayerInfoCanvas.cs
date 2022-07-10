using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIPlayerInfoCanvas : MonoBehaviour
{
    public Text textName;
    public void SetName(string name)
    {
        textName.text = name;
    }
}
