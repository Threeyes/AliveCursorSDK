#if UNITY_EDITOR// 仅作调试用
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class PCCarInput : MonoBehaviour
    {
        ///——ToDelete——
        private const string HORIZONTAL = "Horizontal";
        private const string VERTICAL = "Vertical";

        public CarController carController;

        private void Update()
        {
            carController.SetSteering(InputTool.GetAxis(HORIZONTAL));
            carController.SetAccelerate(InputTool.GetAxis(VERTICAL));
            carController.SetBrake(InputTool.GetKey(KeyCode.Space));
            carController.SetBoost(InputTool.GetKey(KeyCode.V));
        }
    }
}
#endif