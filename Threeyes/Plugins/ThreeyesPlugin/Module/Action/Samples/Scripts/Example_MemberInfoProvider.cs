using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.Action.Example
{
    /// <summary>
    /// This class contains non-public member information that can be obtained through reflection
    /// </summary>
    public class Example_MemberInfoProvider : MonoBehaviour
    {
        /// <summary>
        /// 
        /// Note: Usually, the protected attribute cannot be accessed through UnityEvent, but you can read and write it through ReflectionValue Holdeer_XXX
        /// </summary>
        protected int ProtectedProperty { get { return privateValue; } set { privateValue = value; } }
        [SerializeField] int privateValue = 0;//Mark as [SerializeField] to view its current value on the Inspector window

        protected int GetCurValue() { return privateValue; }
        protected void SetCurValue(int input) { privateValue = input; }


        public Text displayText;
        private void Update()
        {
            ///ToAdd:
            ///-通过引用Text，打印当前的值（模拟用户的情况）
            displayText.text = privateValue.ToString();
        }
    }
}