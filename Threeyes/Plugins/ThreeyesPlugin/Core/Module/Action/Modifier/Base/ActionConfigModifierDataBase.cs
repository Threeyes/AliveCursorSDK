using UnityEngine;

namespace Threeyes.Action
{
    [System.Serializable]
    public class ActionConfigModifierDataBase
    {
    }

    /// <summary>
    /// Modify param from ActionConfigBase
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public class ActionConfigModifierDataBase<TParam> : ActionConfigModifierDataBase
    {
        public virtual object ObjEndValueScale { get { return EndValueScale; } }

        public TParam EndValueScale { get { return endValueScale; } set { endValueScale = value; } }
        [SerializeField] protected TParam endValueScale = default(TParam);
    }


    //！！Common！！

    [System.Serializable]
    public class ActionConfigModifierData_Bool : ActionConfigModifierDataBase<bool>
    {
        public ActionConfigModifierData_Bool() : base()//PS: Inher from base() structer, so when the developer AddComponent or Reset, the data will reset to the right default value
        {
            endValueScale = true;//Setup default value
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_Int : ActionConfigModifierDataBase<int>
    {
        public ActionConfigModifierData_Int() : base()
        {
            endValueScale = 1;
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_String : ActionConfigModifierDataBase<string>
    {
        public ActionConfigModifierData_String() : base()
        {
            endValueScale = null;
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_Float : ActionConfigModifierDataBase<float>
    {
        public ActionConfigModifierData_Float() : base()
        {
            endValueScale = 1;
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_Vector2 : ActionConfigModifierDataBase<Vector2>
    {
        public ActionConfigModifierData_Vector2() : base()
        {
            endValueScale = Vector2.one;
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_Vector3 : ActionConfigModifierDataBase<Vector3>
    {
        public ActionConfigModifierData_Vector3() : base()
        {
            endValueScale = Vector3.one;
        }
    }
    [System.Serializable]
    public class ActionConfigModifierData_Color : ActionConfigModifierDataBase<Color>
    {
        public ActionConfigModifierData_Color() : base()
        {
            endValueScale = Color.white;
        }
    }
}