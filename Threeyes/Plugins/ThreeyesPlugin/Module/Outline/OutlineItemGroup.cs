using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Outline
{
    using static OutlineItem;

    /// <summary>
    /// 生成一组OutlineItem，并管理其激活、禁用、销毁
    /// 
    /// ToTest
    /// </summary>
    public class OutlineItemGroup : ElementGroupBase<OutlineItem>
    {
        /// <summary>
        /// 为该物体及子物体增加高亮Item
        /// </summary>
        public void CreateChilds(ItemInfo itemInfo)
        {
            Clear();

            //ToAdd：参考TransformGizmo，搜索所有Renderer物体并创建，然后添加到listElement中

            List<Renderer> childRenderers = transform.FindComponentsInChild<Renderer>(true, true);

            foreach (var renderer in childRenderers)
            {
                OutlineItem outlineItem = renderer.AddComponentOnce<OutlineItem>();
                outlineItem.Init(itemInfo);
                listElement.Add(outlineItem);
            }
        }

        /// <summary>
        /// 销毁所有的高亮物体
        /// </summary>
        public void DeleteChilds()
        {
            foreach (var e in listElement)
            {
                e.Destroy();
            }
            Clear();
        }
    }
}