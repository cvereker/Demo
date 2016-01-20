using SystemExtensions.Primitives;
using System;
using System.Collections.Generic;

namespace SystemExtensions
{
    public struct ValuePair<T>
    {
        public ValuePair(T d1, T d2)
        {
            Value1 = d1;
            Value2 = d2;
        }

        public T Value1;
        public T Value2;
    }

    public struct DoubleKeyValuePair<TKey, TValue>
    {
        public DoubleKeyValuePair(TKey key1, TKey key2, TValue value)
        {
            _key = new ValuePair<TKey>(key1, key2);
            _value = value;
        }

        private ValuePair<TKey> _key;

        public ValuePair<TKey> Key { get { return _key; } }

        public TKey Key1 { get { return _key.Value1; } }

        public TKey Key2 { get { return _key.Value2; } }

        private TValue _value;

        public TValue Value { get { return _value; } }

        public override string ToString()
        {
            return "Key1={0}, Key2={1}, Value={2}".FormatString(Key1, Key2, Value);
        }
    }

    public static class DictionaryExtensions
    {
        public static void Clear<T>(this Dictionary<string, T> me, params string[] args)
        {
            foreach (string arg in args)
            {
                if (me.ContainsKey(arg))
                    me.Remove(arg);
            }
        }

        public static void Add<T1, T2>(this Dictionary<T1, ValuePair<T2>> me, T1 Key, T2 Value1, T2 Value2)
        {
            var value = new ValuePair<T2>(Value1, Value2);
            me.Add(Key, value);
        }

        public static void Add<T1, T2>(this Dictionary<ValuePair<T1>, T2> me, T1 Key1, T1 Key2, T2 Value)
        {
            var key = new ValuePair<T1>(Key1, Key2);
            me.Add(key, Value);
        }

        public static T2 GetValue<T1, T2>(this Dictionary<ValuePair<T1>, T2> me, T1 Key1, T1 Key2)
        {
            return me[new ValuePair<T1>(Key1, Key2)];
        }

        /// <summary>
        /// Trys to retrieve TKey from the dictionary. If it is not found then return defaultValue.
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key</typeparam>
        /// <typeparam name="TValue">Type of the dictionary values</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">Key to look up in the dictionary</param>
        /// <param name="defaultValue">Default value to return if the key is not found in the dictionary</param>
        /// <returns></returns>
        public static TValue GetValueOrDefault<TKey, TValue>
           (this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        /// <summary>
        /// Trys to retrieve TKey from the dictionary. If it is not found then return defaultValue (provided by the lambda expression).
        /// </summary>
        /// <typeparam name="TKey">Type of the dictionary key</typeparam>
        /// <typeparam name="TValue">Type of the dictionary values</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">Key to look up in the dictionary</param>
        /// <param name="defaultValueProvider">Lambda expression which returns the default value.</param>
        /// <returns></returns>
        public static TValue GetValueOrDefault<TKey, TValue>
            (this IDictionary<TKey, TValue> dictionary,
             TKey key,
             Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value
                 : defaultValueProvider();
        }

        //public static TValue GetValueOrSetDefault<TKey, TValue>
        //    (this IDictionary<TKey, TValue> dictionary,
        //     TKey key,
        //     TValue defaultValue)
        //{
        //    TValue value;
        //    if (dictionary.TryGetValue(key, out value))
        //        return value;
        //    else
        //    {
        //        dictionary.Add(key, defaultValue);
        //        return defaultValue;
        //    }
        //}
    }
}