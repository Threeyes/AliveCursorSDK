using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !USE_SimpleSQL
public class PrimaryKeyAttribute : Attribute
{
}
public class AutoIncrementAttribute : Attribute
{
}

public class MaxLengthAttribute : Attribute
{
    public int Value
    {
        get;
        private set;
    }

    public MaxLengthAttribute(int length)
    {
        Value = length;
    }
}

public class IgnoreAttribute : Attribute
{
}
#endif
