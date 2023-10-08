using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Log
{
    public static class LogManagerHolder
    {
        public static ILogManager LogManager { get; internal set; }
    }
}