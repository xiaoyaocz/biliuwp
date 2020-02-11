using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliBili.UWP
{
    public class ReturnJObject
    {
        public int code { get; set; }
        public string message { get; set; }
        public string msg { get; set; }
        public JToken json { get; set; } 
    }

    ///// <summary>
    ///// 将Jobject转换为dynamic,可以像JS一样操作JSON
    ///// 就是效率有点低,比直接操作JObject低大概7倍...，但是写起来爽就行
    ///// </summary>
    //public class DynamicJObject : DynamicObject
    //{
    //    JToken obj;
    //    public DynamicJObject(string json)
    //    {
    //        this.obj = JObject.Parse(json);
    //    }
    //    public DynamicJObject(JToken obj)
    //    {
    //        this.obj = obj;
    //    }

    //    public override bool TryGetMember(GetMemberBinder binder, out object result)
    //    {
    //        result = null;

    //        if (obj == null) return false;

    //        var val = obj[binder.Name];

    //        if (val == null)
    //        {
    //            (obj as JObject).Add(new JProperty(binder.Name, "undefined"));
    //            val = obj[binder.Name];
    //        }
    //        result = Populate(val);
    //        return true;
    //    }
    //    public override bool TrySetMember(SetMemberBinder binder, object value)
    //    {

    //        if (obj == null) return false;
    //        var val = obj[binder.Name];
    //        if (val == null)
    //        {
    //            (obj as JObject).Add(new JProperty(binder.Name, value));
    //        }
    //        else
    //        {
    //            var jval = val as JValue;
    //            jval.Value = value;

    //        }
    //        return true;
    //    }

    //    private object Populate(JToken token)
    //    {

    //        var jval = token as JValue;
    //        if (jval != null)
    //        {
    //            return jval.Value;
    //        }
    //        else if (token.Type == JTokenType.Array)
    //        {
    //            var objectAccessors = new List<object>();
    //            foreach (var item in token as JArray)
    //            {
    //                objectAccessors.Add(Populate(item));
    //            }
    //            return objectAccessors;
    //        }
    //        else
    //        {
    //            return new DynamicJObject(token);
    //        }
    //    }


    //    public string ToJsonString()
    //    {

    //        return obj.ToString();
    //    }
    //}



    ///// <summary>
    ///// 将Jobject转换为dynamic,可以像JS一样操作JSON
    ///// 就是效率有点低，但是写起来爽就行
    ///// </summary>
    //[Serializable]
    //public class DynamicJObject : DynamicObject, IDictionary<string, object>, IPclCloneable, INotifyPropertyChanged
    //{
    //    public static DynamicJObject Create(string json)
    //    {

    //        return Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicJObject>(json);
    //    }


    //    private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

    //    #region IDictionary<String, Object> 接口实现

    //    public object this[string key]
    //    {
    //        get { return _values[key]; }

    //        set
    //        {
    //            _values[key] = value;


    //            OnPropertyChanged(key);
    //        }
    //    }

    //    public int Count
    //    {
    //        get { return _values.Count; }
    //    }

    //    public bool IsReadOnly
    //    {
    //        get { return _values.IsReadOnly; }
    //    }

    //    public ICollection<string> Keys
    //    {
    //        get { return _values.Keys; }
    //    }

    //    public ICollection<object> Values
    //    {
    //        get { return _values.Values; }
    //    }

    //    public void Add(KeyValuePair<string, object> item)
    //    {
    //        _values.Add(item);
    //    }

    //    public void Add(string key, object value)
    //    {
    //        _values.Add(key, value);
    //    }

    //    public void Clear()
    //    {
    //        _values.Clear();
    //    }

    //    public bool Contains(KeyValuePair<string, object> item)
    //    {
    //        return _values.Contains(item);
    //    }

    //    public bool ContainsKey(string key)
    //    {
    //        return _values.ContainsKey(key);
    //    }

    //    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    //    {
    //        _values.CopyTo(array, arrayIndex);
    //    }

    //    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    //    {
    //        return _values.GetEnumerator();
    //    }

    //    public bool Remove(KeyValuePair<string, object> item)
    //    {
    //        return _values.Remove(item);
    //    }

    //    public bool Remove(string key)
    //    {
    //        return _values.Remove(key);
    //    }

    //    public bool TryGetValue(string key, out object value)
    //    {
    //        return _values.TryGetValue(key, out value);
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return _values.GetEnumerator();
    //    }

    //    #endregion

    //    #region ICloneable 接口实现

    //    public object Clone()
    //    {
    //        var clone = new DynamicJObject() as IDictionary<string, object>;

    //        foreach (var key in _values.Keys)
    //        {
    //            clone[key] = _values[key] is IPclCloneable ? ((IPclCloneable)_values[key]).Clone() : _values[key];
    //        }

    //        return clone;
    //    }

    //    #endregion

    //    #region INotifyPropertyChanged 接口实现

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    #endregion-

    //    /// <summary>  
    //    /// 获取属性值  
    //    /// </summary>  
    //    /// <param name="propertyName"></param>  
    //    /// <returns></returns>  
    //    public object GetPropertyValue(string propertyName)
    //    {
    //        if (_values.ContainsKey(propertyName) == true)
    //        {
    //            return _values[propertyName];
    //        }
    //        return null;
    //    }

    //    /// <summary>  
    //    /// 设置属性值  
    //    /// </summary>  
    //    /// <param name="propertyName"></param>  
    //    /// <param name="value"></param>  
    //    public void SetPropertyValue(string propertyName, object value)
    //    {
    //        if (_values.ContainsKey(propertyName) == true)
    //        {
    //            _values[propertyName] = value;
    //        }
    //        else
    //        {
    //            _values.Add(propertyName, value);
    //        }
    //    }

    //    /// <summary>  
    //    /// 实现动态对象属性成员访问的方法，得到返回指定属性的值  
    //    /// </summary>  
    //    /// <param name="binder"></param>  
    //    /// <param name="result"></param>  
    //    /// <returns></returns>  
    //    public override bool TryGetMember(GetMemberBinder binder, out object result)
    //    {
    //        result = GetPropertyValue(binder.Name);
    //        return result != null;
    //    }

    //    /// <summary>  
    //    /// 实现动态对象属性值设置的方法。  
    //    /// </summary>  
    //    /// <param name="binder"></param>  
    //    /// <param name="value"></param>  
    //    /// <returns></returns>  
    //    public override bool TrySetMember(SetMemberBinder binder, object value)
    //    {
    //        SetPropertyValue(binder.Name, value);
    //        return true;
    //    }

    //    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    //    {
    //        return base.TryInvoke(binder, args, out result);
    //    }
    //}
    //public interface IPclCloneable
    //{
    //    object Clone();
    //}

}
