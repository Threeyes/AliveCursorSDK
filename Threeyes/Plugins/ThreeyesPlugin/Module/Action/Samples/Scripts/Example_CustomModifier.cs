using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Action;
using Threeyes.Core;
namespace Threeyes.Action.Example
{
    public class Example_CustomModifier : ActionModifierBase, IActionModifier_Config
    {
        public void ModifyConfig(IActionConfig config, ActionState actionState, object value)
        {
            if (!IsStateActived(actionState))//Check if the specified type is activated
                return;

            if (config.BaseEndValue is string)//Check if the EndValue is of type string
            {
                if (actionState == ActionState.Enter)
                {
                    //Warning: You need to ensure that the value assigned to BaseEndValue
                    //is of the same type or convertible, otherwise an error may occur.
                    config.BaseEndValue = System.DateTime.Now.ToString();//Change endvalue to current Time
                }
                else if (actionState == ActionState.Exit)
                {
                    config.BaseEndValue = System.DateTime.Now.ToString("dddd");//Change endvalue to current week
                }
            }
        }
    }
}