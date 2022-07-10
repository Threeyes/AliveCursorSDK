#if USE_VRTK
using UnityEngine;
using VRTK;
/// <summary>
/// 初始化VR的全局设置:
/// ——高亮颜色
/// </summary>
public class VRModelInitSetting : MonoBehaviour
{
    public Color touchHighlightColor = Color.cyan;

    private void Awake()
    {
        transform.Recursive((tf) =>
        {
            if (tf.GetComponent<VRTK_InteractableObject>())
            {
                if (tf.GetComponent<VRTK_InteractableObject>().touchHighlightColor == Color.black)
                    tf.GetComponent<VRTK_InteractableObject>().touchHighlightColor = touchHighlightColor;
            }
        });
    }
}
#endif