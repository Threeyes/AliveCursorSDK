using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.Action.Example
{
    public class Example_TextController : MonoBehaviour
    {
        public Text target;

        public void SetText(Vector2 value)
        {
            SetTextFunc(value);
        }
        public void SetText(Vector3 value)
        {
            SetTextFunc(value);
        }
        public void SetText(float value)
        {
            SetTextFunc(value);
        }

        public void SetText(int value)
        {
            SetTextFunc(value);
        }

        public void SetText(string value)
        {
            SetTextFunc(value);
        }

        public void SetText(Color value)
        {
            SetTextFunc(value);
        }

        void SetTextFunc(object value)
        {
            target.text = value.ToString();
        }
    }
}