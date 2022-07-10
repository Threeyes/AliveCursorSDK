using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.ValueHolder.Test
{
    public class ReflectionValueHolder_EnumTest : ReflectionValueHolder_Enum
    {
        #region Testing
        [Header("Testing")]
        public CameraType cameraType;
        public CameraType CameraTyp2 { set { cameraType = value; } }
        public TextAlignment textAlignment;
        public TextAlignment TextAlignment { set { textAlignment = value; } }
        public CameraType ReadonlyCamerType { get { return CameraType.Game; } }
        public void TestSetTextAlignment(TextAlignment textAlignment)
        {
            this.TextAlignment = textAlignment;
        }
        #endregion
    }
}