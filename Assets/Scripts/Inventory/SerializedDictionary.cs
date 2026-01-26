using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    
    [SerializeField]
    private List<TValue> values = new List<TValue>();
    
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        
        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    
    public void OnAfterDeserialize()
    {
        this.Clear();
        
        if (keys.Count != values.Count)
        {
            Debug.LogError($"Mismatch in key/value counts: {keys.Count} keys, {values.Count} values");
        }
        
        for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
        {
            this[keys[i]] = values[i];
        }
    }
}
