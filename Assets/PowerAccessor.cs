using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public class PowerAccessor : DynamicObject
{
    const BindingFlags ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public object Value { get; } 
    public PowerAccessor(object value) => Value = value;

    public override bool Equals(object obj) => Value.Equals(obj);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();

    public override IEnumerable<string> GetDynamicMemberNames() => GetMembers().Select(m => m.Name);
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        => ExceptionToBool(out result, () => GetMembers().OfType<PropertyInfo>().First(p => p.Name == "Item").GetValue(Value, indexes));
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        => ExceptionToBool(() => GetMembers().OfType<PropertyInfo>().First(p => p.Name == "Item").SetValue(Value, value, indexes));

    public override bool TryGetMember(GetMemberBinder binder, out object result)
        => ExceptionToBool(out result, () => GetValue(GetMembers().First(p => p.Name == binder.Name)));
    public override bool TrySetMember(SetMemberBinder binder, object value)
        => ExceptionToBool(() => SetValue(GetMembers().First(p => p.Name == binder.Name), value));

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        => ExceptionToBool(out result, () => GetMembers().OfType<MethodInfo>().First(p => p.Name == binder.Name).Invoke(Value, args));

    IEnumerable<MemberInfo> GetMembers()
    {
        var type = Value.GetType();
        while(type != null)
        {
            foreach (var member in type.GetMembers(ALL_ACCESS))
            {
                yield return member;
            }
            type = type.BaseType;
        }
    }

    object GetValue(MemberInfo member)
    {
        if (member is FieldInfo f)
            return f.GetValue(Value);
        if (member is PropertyInfo p)
            return p.GetValue(Value);
        throw new Exception();
    }
    
    void SetValue(MemberInfo member, object value)
    {
        if (member is FieldInfo f)
            f.SetValue(Value, value);
        if (member is PropertyInfo p)
            p.SetValue(Value, value);
        throw new Exception();
    }

    static bool ExceptionToBool(Action action) => ExceptionToBool(out var _, () => { action(); return true; });
    
    static bool ExceptionToBool(out object result, Func<object> action)
    {
        try
        {
            result = action();
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

}