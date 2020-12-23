using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
using Debug = UnityEngine.Debug;
using Dynamic;
using System.Collections.Generic;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;
using UnityEngine;
using Object = UnityEngine.Object;

public class Test
{
    class PublicHoge
    {
        public readonly string Value;
        public PublicHoge(string value) => Value = value;
        public string Field = "FIELD";
        public string Property { get; set; } = "PROPERTY";
        public string Method() => "METHOD";
        public string Method(IEnumerable<string> args) => $"METHOD_{string.Join("|", args)}";
        public string Method(Object arg) => $"METHOD_{arg.name}";
        public static string operator+(PublicHoge a, PublicHoge b) => $"ADD_{a.Value}_{b.Value}";
    }

    [Test]
    public void TestDynamic()
    {
        dynamic dyHoge = new PublicHoge("Fuga");
        Assert.AreEqual((string)(dyHoge.Value), "Fuga");
        Assert.AreEqual((string)(dyHoge.Field), "FIELD");
        Assert.AreEqual((string)(dyHoge.Property), "PROPERTY");
        dyHoge.Method();
        Assert.AreEqual((string)(dyHoge.Method(new []{ "Piyo1", "Piyo2", })), "METHOD_Piyo1|Piyo2");
        Assert.AreEqual((string)(dyHoge.Method(new GameObject("GO"))), "METHOD_GO");
        Assert.AreEqual((string)(dyHoge + dyHoge), "ADD_Fuga_Fuga");
        dynamic dyFunc = new PowerAccessor(new Func<string, string>(s => s.ToUpper()));
        Assert.AreEqual((string)(dyFunc("www")), "WWW");
    }


    class PrivateHoge
    {
        readonly string Value;
        public PrivateHoge(string value) => Value = value;
        string Field = "FIELD";
        string Property { get; set; } = "PROPERTY";
        string Method() => "METHOD";
        string Method(IEnumerable<string> args) => $"METHOD_{string.Join("|", args)}";
        string Method(Object arg) => $"METHOD_{arg.name}";
        public static string operator+(PrivateHoge a, PrivateHoge b) => $"ADD_{a.Value}_{b.Value}";
    }

    [Test]
    public void TestPowerAccessor()
    {
        Debug.Log(Process.GetCurrentProcess().MainModule.FileName);
        // C:\Program Files\Unity\Hub\Editor\2019.4.4f1\Editor\Unity.exe
        Debug.Log(typeof(CallSite).Assembly.Location);
        // C:\Program Files\Unity\Hub\Editor\2019.4.4f1\Editor\Data\MonoBleedingEdge\lib\mono\unityjit\System.Core.dll
        Debug.Log(typeof(CSharpArgumentInfo).Assembly.Location);
        // C:\Program Files\Unity\Hub\Editor\2019.4.4f1\Editor\Data\MonoBleedingEdge\lib\mono\unityjit\Microsoft.CSharp.dll
        
        var hoge = new PowerAccessor(new PrivateHoge("Fuga"));
        // File.WriteAllLines("hoge", hoge.GetDynamicMemberNames());
        dynamic dyHoge = hoge;
        Assert.AreEqual((string)(dyHoge.Value), "Fuga");
        Assert.AreEqual((string)(dyHoge.Field), "FIELD");
        Assert.AreEqual((string)(dyHoge.Property), "PROPERTY");
        dyHoge.Method();
        Assert.AreEqual((string)(dyHoge.Method(new []{ "Piyo1", "Piyo2", })), "METHOD_Piyo1|Piyo2");
        Assert.AreEqual((string)(dyHoge.Method(new GameObject("GO"))), "METHOD_GO");
        Assert.AreEqual((string)(dyHoge + dyHoge), "ADD_Fuga_Fuga");
        dynamic dyFunc = new PowerAccessor(new Func<string, string>(s => s.ToUpper()));
        Assert.AreEqual((string)(dyFunc("www")), "WWW");
    }


    static class Cache
    {
        public static CallSite<Action<CallSite, object>> Delegate;
    }

    [Test]
    public void TestCS2ILCompileResult()
    {
        // if (Cache.Delegate == null)
        {
            Type typeFromHandle = typeof(PublicHoge);
            CSharpArgumentInfo[] array = new CSharpArgumentInfo[1];
            array[0] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null);
            CallSiteBinder binder = Binder.InvokeMember(CSharpBinderFlags.None, "Method", null, typeFromHandle, array);
            Cache.Delegate = CallSite<Action<CallSite, object>>.Create(binder);
        }
        Cache.Delegate.Target(Cache.Delegate, new PublicHoge("Moo"));
    }
}
