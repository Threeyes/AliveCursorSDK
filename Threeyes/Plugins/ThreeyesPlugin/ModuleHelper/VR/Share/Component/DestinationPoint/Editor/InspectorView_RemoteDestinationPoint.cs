#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Threeyes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RemoteDestinationPoint))]
    public class InspectorView_RemoteDestinationPoint : UnityEditor.Editor
    {
        private RemoteDestinationPoint _target;

        void OnEnable()
        {
            _target = (RemoteDestinationPoint)target;
        }

        public override void OnInspectorGUI()
        {
            _target.LocationHeight = EditorGUILayout.FloatField("LocationHeight", _target.LocationHeight);

            base.OnInspectorGUI();

        }
    }
}
#endif