using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AC_BehaviourCollectionBase<TSOActionCollection> : MonoBehaviour
where TSOActionCollection : SOCollectionBase
{
	protected const string foldoutName_SOAction = "[SO Action]";//PS：因为SO可以折叠，所以要统一增加特殊标志[]
	protected const string foldoutName_UnityEvent = "[Unity Event]";
	protected const string foldoutName_ActionTarget = "[Action Target]";

	[Foldout(foldoutName_SOAction)] [Expandable] public TSOActionCollection soActionCollection;
}
