using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerPrefSetterInterface<TValue>
{
    string Key { get; set; }
    bool HasKey { get; }
    bool IsSave { get; set; }
    bool IsLoadOnAwake { get; set; }

    void LoadValue();
    TValue GetValue();
    void SetDefaultValue(TValue defaultValue);
}
