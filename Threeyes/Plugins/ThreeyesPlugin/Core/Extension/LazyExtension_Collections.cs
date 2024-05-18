using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Threeyes.Core
{
    /// <summary>
    /// Collections的相关扩展方法
    /// </summary>
    public static class LazyExtension_Collections
    {
        #region 乱序

        /// <summary>
        /// 尝试获取新的随机值（排除特定值）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listObj"></param>
        /// <param name="objToIgnore"></param>
        /// <param name="forceExclde">True：会强制排除objToIgnore；False：如果List只有objToIgnore这一个元素，则返回该元素</param>
        /// <returns></returns>
        public static T GetNewRandom<T>(this IEnumerable<T> listObj, T objToIgnore, bool forceExclde = false)
        {
            if (objToIgnore != null && listObj.Count() == 1 && listObj.Contains(objToIgnore))//list仅包含objToIgnore
            {
                if (forceExclde)
                    return default;
                else
                    return objToIgnore;
            }
            var newCollection = listObj.Where(o => !Equals(o, objToIgnore));//排除objToIgnore
            return newCollection.GetRandom();
        }

        public static T GetRandom<T>(this IEnumerable<T> listObj)
        {
            if (listObj.Count() > 0)
            {
                //return listObj[UnityEngine.Random.Range(0, listObj.Count())];//注意：Range会排除上限
                return listObj.ElementAt(UnityEngine.Random.Range(0, listObj.Count()));//注意：Range会排除上限
            }
            else
            {
                UnityEngine.Debug.LogWarning("List is Null!");
                return default;
            }
        }

        /// <summary>
        /// Returns a random element from a list, or null if the list is empty.
        /// </summary>
        /// <typeparam name="T">The type of object being enumerated</typeparam>
        /// <param name="rand">An instance of a random number generator</param>
        /// <returns>A random element from a list, or null if the list is empty</returns>
        public static T Random<T>(this IEnumerable<T> list, Random rand = null)
        {
            if (rand == null)
                rand = new Random();
            if (list != null && list.Count() > 0)
                return list.ElementAt(rand.Next(list.Count()));

            return default;
        }

        /// <summary>
        /// 洗牌/乱序
        /// Returns a shuffled IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of object being enumerated</typeparam>
        /// <returns>A shuffled shallow copy of the source items</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        /// <summary>
        /// Returns a shuffled IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of object being enumerated</typeparam>
        /// <param name="rand">An instance of a random number generator</param>
        /// <returns>A shuffled shallow copy of the source items</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rand)
        {
            var list = source.ToList();
            list.Shuffle(rand);
            return list;
        }

        /// <summary>
        /// Shuffles an IList in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        public static void Shuffle<T>(this IList<T> list)
        {
            list.Shuffle(new Random(UnityEngine.Random.Range(0, 1000)));
        }

        /// <summary>
        /// Shuffles an IList in place.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="rand">An instance of a random number generator</param>
        public static void Shuffle<T>(this IList<T> list, Random rand)
        {
            int count = list.Count;
            while (count > 1)
            {
                int i = rand.Next(count--);
                T temp = list[count];
                list[count] = list[i];
                list[i] = temp;
            }
        }

        #endregion

        #region Compare
        /// <summary>
        /// 获取新增的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">旧列表</param>
        /// <param name="newSource">新列表</param>
        /// <returns></returns>
        public static IEnumerable<T> GetAddedElements<T>(this IEnumerable<T> source, IEnumerable<T> newList)
        {
            var listSource = source.ToList();
            var listNewList = newList.ToList();
            return listNewList.FindAll(d => !listSource.Contains(d));//ToUpdate:默认比较相同元素，后期提供自定义判断相同的方法
        }
        /// <summary>
        /// 获取被删除的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">旧列表</param>
        /// <param name="newSource">新列表</param>
        /// <returns></returns>
        public static IEnumerable<T> GetDeletedElements<T>(this IEnumerable<T> source, IEnumerable<T> newList)
        {
            var listSource = source.ToList();
            var listNewList = newList.ToList();
            return listSource.FindAll(d => !listNewList.Contains(d));//ToUpdate:默认比较相同元素，后期提供自定义判断相同的方法
        }

        /// <summary>
        /// 检查两个列表元素是否相同，考虑排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsSequenceEqual<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == list2)//引用相同（包括都为null）
                return true;
            if (list1 == null || list2 == null)
                return false;

            if (list1.Count() != list2.Count())
                return false;
            for (int i = 0; i != list1.Count(); i++)
            {
                if (!Equals(list1.ElementAt(i), list2.ElementAt(i)))//调用每个元素的Equals(object)或IEquatable<T>方法进行比较。使用Equals(a,b)可避免引用为空导致报错
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if two lists have the same element(忽略排序) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsElementEqual<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        {
            //参考:https://stackoverflow.com/questions/12795882/quickest-way-to-compare-two-list
            if (list1.Count() != list2.Count())
                return false;
            var firstNotSecond = list1.Except(list2).ToList();
            var secondNotFirst = list2.Except(list1).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }

        /// <summary>
        /// 判断list1是否与list2相同或者为其父集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool IsContainAll<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
            where T : class
        {
            return list2.All(t => list1.Any(b => b == t));
        }

        #endregion

        #region GetOrSet
        public static void AddIfNotNull<T>(this ICollection<T> list, T item)
        {
            if (item != null)
                list.Add(item);
        }
        public static void AddOnce<T>(this ICollection<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        //#以下List的方法默认都会循环获取（todo：加一个bool isLoop=false）

        //——Get List Item——

        /// <summary>
        /// 根据item的位置，算出它的下一位数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T GetNext<T>(this IList<T> list, T item)
        {
            return list.GetDelta(item, +1);
        }
        public static T GetDelta<T>(this IList<T> list, T item, int delta)
        {
            int targetIndex = list.GetDeltaIndex(item, delta);//这里如果有错会返回-1
            if (targetIndex >= 0)
                return list[targetIndex];
            return default;
        }

        /// <summary>
        /// 根据item的位置，算出它的下一位数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T GetNextByIndex<T>(this IList<T> list, int curIndex)
        {
            return list.GetDeltaByIndex(curIndex, +1);
        }

        /// <summary>
        /// 根据item的位置，算出它的指定位移的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T GetDeltaByIndex<T>(this IList<T> list, int curIndex, int delta)
        {
            int targetIndex = curIndex.GetDeltaIndex(delta, list.Count);
            if (targetIndex >= 0)
            {
                return list[targetIndex];
            }
            return default;
        }


        //——Index——

        public static int GetIndex<T>(this IList<T> list, int targetIndex)
        {
            return targetIndex.GetIndex(list.Count);
        }

        public static int GetNextIndex<T>(this IList<T> list, T item)
        {
            return list.GetDeltaIndex(item, +1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="delta">retun -1 if item not exist on list</param>
        /// <returns></returns>
        public static int GetDeltaIndex<T>(this IList<T> list, T item, int delta)
        {
            int curIndex = list.IndexOf(item);
            if (curIndex < 0)
            {
                //UnityEngine.Debug.LogError(string.Format("The list {0} doesn't contain item {1}", list, item));
                return -1;
            }
            return curIndex.GetDeltaIndex(delta, list.Count);
        }

        /// <summary>
        /// 循环获取元素的位置，（如果index越界则从头开始计算）
        /// </summary>
        /// <param name="targetIndex">允许越界</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int GetIndex(this int targetIndex, int length)
        {
            return targetIndex.GetDeltaIndex(0, length);
        }
        public static int GetNextIndex(this int curIndex, int length)
        {
            return curIndex.GetDeltaIndex(+1, length);
        }
        public static int GetDeltaIndex(this int curIndex, int delta, int length)
        {
            if (length > 0)
                return (int)UnityEngine.Mathf.Repeat(curIndex + delta, length);

            UnityEngine.Debug.LogError("The Total Length is 0！");
            return -1;
        }

        //Todo
        //public static int GetPingPongIndex(this int curIndex, int delta, int length)
        //{
        //    //PS:Mathf.PingPong的取值为[0,length]，而且会0和length在边界会出现两次
        //    if (length > 0)
        //        return (int)UnityEngine.Mathf.PingPong(curIndex + delta, length);

        //    UnityEngine.Debug.LogError("The Total Length is 0！");
        //    return -1;
        //}

        /// <summary>
        /// 序号是否可用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsIndexValid<T>(this IList<T> list, int index)
        {
            if (list.Count > 0)
                return index >= 0 && index <= list.Count - 1;
            return false;
        }

        #endregion

        #region Others

        public static List<T> SimpleClone<T>(this List<T> list)
        {

            List<T> newList = list.GetRange(0, list.Count);
            return newList;
        }

        /// <summary>
        /// 将列表连接成string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="strInterval"></param>
        /// <returns></returns>
        public static string ConnectToString<T>(this IEnumerable<T> list, string strInterval = "\r\n")
            where T : class
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var item in list)
            {
                if (i > 0)
                {
                    sb.Append(strInterval);
                }
                sb.Append(item.ToString());
                i++;
            }
            return sb.ToString();
        }

        #endregion

        #region Dictionary

        public static TKey GetKey<TKey, TValue>(this Dictionary<TKey, TValue> keyValuePairs, TValue value)
        {
            if (keyValuePairs != null && keyValuePairs.ContainsValue(value))
            {
                foreach (var keyPair in keyValuePairs)
                {
                    if (Equals(keyPair.Value, value))
                        return keyPair.Key;
                }
            }
            return default;
        }
        #endregion
    }
}