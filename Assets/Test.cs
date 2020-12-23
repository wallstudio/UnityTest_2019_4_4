using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // DynamicTest();
        CS2ILCompileResult();
    }

    object Execute()
    {
        Debug.Log("ExecuteLog!");
        return this;
    }

    public object DynamicTest()
    {
        dynamic value = (this);
        return value.Execute();
    }
    
    static class Cache
    {
        public static CallSite<Func<CallSite, object, object>> Delegate;
    }

    public object CS2ILCompileResult()
    {
        if (Cache.Delegate == null)
        {
            Type typeFromHandle = typeof(Test);
            CSharpArgumentInfo[] array = new CSharpArgumentInfo[1];
            array[0] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null);
            CallSiteBinder binder = Binder.InvokeMember(CSharpBinderFlags.None, "Execute", null, typeFromHandle, array);
            Cache.Delegate = CallSite<Func<CallSite, object, object>>.Create(binder);
        }
        return Cache.Delegate.Target(Cache.Delegate, this);
    }
}
