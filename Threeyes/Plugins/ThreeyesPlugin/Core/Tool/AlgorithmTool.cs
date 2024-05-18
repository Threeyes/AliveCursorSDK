using System;
using System.Collections;
using UnityEngine.Events;

namespace Threeyes.Core
{
    public static class AlgorithmTool
    {
        /// <summary>
        /// Recursive execute function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <param name="getEnum"></param>
        /// <param name="action"></param>
        /// <param name="includeSelf"></param>
        /// <param name="maxDepth">null for endless seek&invoke (Warning: will get StackOverFlow if the seek never end! (eg: Nested self Classes)</param>
        public static void Recursive<T>(T element, Func<T, IEnumerable> getEnum, UnityAction<T> action, bool includeSelf = true, int? maxDepth = null)
        {
            if (includeSelf)
                action.Execute(element);

            if (maxDepth.HasValue)
            {
                maxDepth--;
                if (maxDepth == -1)
                    return;
            }

            if (getEnum == null)
                return;
            foreach (T subElement in getEnum.Invoke(element))//遍历子元素列表
            {
                AlgorithmTool.Recursive(subElement, getEnum, action, maxDepth: maxDepth.HasValue ? --maxDepth : null);//The child circle will be includeSelf=true so it will get set via action
            }
        }
    }
}