using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IndexedDictionary<TKey, TValue>
{
    [SerializeField] private TKey[] keys;
    [SerializeField] private TValue[] values;
    [SerializeField] private int count;

    public IndexedDictionary(int initialCapacity = 4)
    {
        keys = new TKey[initialCapacity];
        values = new TValue[initialCapacity];
        count = 0;
    }

    public int Count => count;
    public TKey[] Keys => keys;
    public TValue[] Values => values;

    // Adds a new key-value pair
    public void Add(TKey key, TValue value)
    {
        if (ContainsKey(key))
            throw new ArgumentException($"Key '{key}' already exists in the dictionary.");

        EnsureCapacity();

        keys[count] = key;
        values[count] = value;
        count++;
    }

    // Removes a key-value pair by key
    public bool Remove(TKey key)
    {
        int index = IndexOfKey(key);
        if (index == -1)
            return false;

        // Shift elements to keep arrays contiguous
        for (int i = index; i < count - 1; i++)
        {
            keys[i] = keys[i + 1];
            values[i] = values[i + 1];
        }

        count--;
        keys[count] = default;
        values[count] = default;
        return true;
    }

    // Attempts to get a value by key
    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = IndexOfKey(key);
        if (index != -1)
        {
            value = values[index];
            return true;
        }

        value = default;
        return false;
    }

    // Allows getting/setting values by key
    public TValue this[TKey key]
    {
        get
        {
            int index = IndexOfKey(key);
            if (index == -1)
                throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");

            return values[index];
        }
        set
        {
            int index = IndexOfKey(key);
            if (index == -1)
                Add(key, value);
            else
                values[index] = value;
        }
    }

    // Retrieves a value by index
    public bool TryGetValueByIndex(int index, out TValue value)
    {
        if (index >= 0 && index < count)
        {
            value = values[index];
            return true;
        }

        value = default;
        return false;
    }

    // Retrieves a key by index
    public bool TryGetKeyByIndex(int index, out TKey key)
    {
        if (index >= 0 && index < count)
        {
            key = keys[index];
            return true;
        }

        key = default;
        return false;
    }

    // Checks if a key exists in the dictionary
    public bool ContainsKey(TKey key) => IndexOfKey(key) != -1;

    // Clears all entries
    public void Clear()
    {
        Array.Clear(keys, 0, count);
        Array.Clear(values, 0, count);
        count = 0;
    }

    // Gets the index of a key, or -1 if not found
    public int IndexOfKey(TKey key)
    {
        for (int i = 0; i < count; i++)
        {
            if (Equals(keys[i], key))
                return i;
        }

        return -1;
    }

    // Ensures the arrays have enough capacity to add new entries
    private void EnsureCapacity()
    {
        if (count >= keys.Length)
        {
            int newCapacity = keys.Length * 2;
            Array.Resize(ref keys, newCapacity);
            Array.Resize(ref values, newCapacity);
        }
    }
}