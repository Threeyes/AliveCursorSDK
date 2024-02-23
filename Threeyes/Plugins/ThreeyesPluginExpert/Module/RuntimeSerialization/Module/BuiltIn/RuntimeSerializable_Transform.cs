using Threeyes.Core;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    [DisallowMultipleComponent]
    public class RuntimeSerializable_Transform : RuntimeSerializableComponent<Transform, TransformPropertyBag>
    {
    }

    #region Define
    /// <summary>
    /// Ref:  Unity.RuntimeSceneSerialization.Internal.TransformPropertyBagDefinition
    /// </summary>
    [System.Serializable]
    public class TransformPropertyBag : ComponentPropertyBag<Transform>
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public TransformPropertyBag() { }

        public override void Init(Transform container)
        {
            base.Init(container);
            localPosition = container.localPosition;
            localRotation = container.localRotation;
            localScale = container.localScale;
        }

        public override void Accept(ref Transform container)
        {
            base.Accept(ref container);

            container.SetProperty(localPosition, localRotation, localScale);
        }
    }
    #endregion
}
