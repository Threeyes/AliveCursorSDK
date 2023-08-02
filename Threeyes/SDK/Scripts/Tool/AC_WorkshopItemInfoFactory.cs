using Newtonsoft.Json;
using Steamworks.Ugc;
using System.IO;
using System.Linq;
using Threeyes.Steamworks;
using UnityEngine;

//ToUpdate:改为非静态类
public class AC_WorkshopItemInfoFactory : WorkshopItemInfoFactory<AC_WorkshopItemInfoFactory, AC_SOWorkshopItemInfo, AC_WorkshopItemInfo>
{
    public AC_WorkshopItemInfoFactory() : base()
    {
    }
}