using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Dynamic
{
    public class PowerAccessor : DynamicObject
    {
        public static Action<Exception> ErrorHandler { internal get; set; } = e => Debug.LogError(e);


        public object @this { get; }

        public PowerAccessor(object value) => @this = value;

        // Indexer
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            => Utility.ExceptionToBool(out result, ()
                => @this.GetIndexProperty(indexes).GetValue(@this, indexes));
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            => Utility.ExceptionToBool(()
                => @this.GetIndexProperty(indexes).SetValue(@this, value, indexes));
        // Field/Property
        public override bool TryGetMember(GetMemberBinder binder, out object result)
            => Utility.ExceptionToBool(out result, ()
                => @this.EnumerateAllMembers(binder.Name).First().GetValue(@this));
        public override bool TrySetMember(SetMemberBinder binder, object value)
            => Utility.ExceptionToBool(()
                => @this.EnumerateAllMembers(binder.Name).First().SetValue(@this, value));
        // Method
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            => Utility.ExceptionToBool(out result, ()
                => @this.EnumerateAllMembers(binder.Name, args).OfType<MethodInfo>().First().Invoke(@this, args));
        // Delegate
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            => Utility.ExceptionToBool(out result, ()
                => ((Delegate)@this).DynamicInvoke(args));
        // 演算子
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
            => Utility.ExceptionToBool(out result, ()
                => @this.ExpressionMethod(binder.Operation, arg));
        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
            => Utility.ExceptionToBool(out result, ()
                => @this.ExpressionMethod(binder.Operation, Array.Empty<object>()));

        public override DynamicMetaObject GetMetaObject(Expression parameter) => base.GetMetaObject(parameter);
        public override IEnumerable<string> GetDynamicMemberNames() => @this.EnumerateAllMembers().Select(m => m.Name);

        public override bool Equals(object obj) => @this.Equals(obj);
        public override int GetHashCode() => @this.GetHashCode();
        public override string ToString() => @this.ToString();


        public override bool TryConvert(ConvertBinder binder, out object result) => throw new NotImplementedException(nameof(TryConvert));
        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result) => throw new NotImplementedException(nameof(TryCreateInstance));
        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes) => throw new NotImplementedException(nameof(TryDeleteIndex));
        public override bool TryDeleteMember(DeleteMemberBinder binder) => throw new NotImplementedException(nameof(TryDeleteMember));
    }

    internal static class Utility
    {
        const BindingFlags ALL_ACCESS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        public static IEnumerable<MemberInfo> EnumerateAllMembers(this object @this, string nameFilter = null, object[] arguments = null)
        {
            var type = @this.GetType();
            var argumentsTypes = arguments?.Select(a => a.GetType())?.ToArray();
            while(type != null)
            {
                foreach (var member in type.GetMembers(ALL_ACCESS))
                {
                    if(nameFilter != null && member.Name != nameFilter) continue;
                    if(arguments != null && !member.MatchParameter(argumentsTypes)) continue;
                    yield return member;
                }
                type = type.BaseType;
            }
        }

        public static bool MatchParameter(this MemberInfo member, Type[] arguments)
        {
            ParameterInfo[] parameters;
            switch(member)
            {
                case PropertyInfo property:
                    parameters = property.GetIndexParameters();
                    break;
                case MethodInfo method:
                    parameters = method.GetParameters();
                    break;
                default:
                    parameters = Array.Empty<ParameterInfo>();
                    break;
            }
            if(arguments.Length != parameters.Length)
            {
                return false;
            }
            if(arguments.Zip(parameters, (a, p) => (a, p: p.ParameterType))
                // .Any(t => !(t.a.IsSubclassOf(t.p) || t.a.GetInterfaces().Contains(t.p))))
                .Any(t =>
                {
                    bool v2 = t.a == t.p;
                    bool v = t.a.IsSubclassOf(t.p);
                    bool v1 = t.a.GetInterfaces().Contains(t.p);
                    return !(v2 || v || v1);
                }))
            {
                return false;
            }
            return true;
        }

        public static PropertyInfo GetIndexProperty(this object @this, object[] indexes)
        {
            var explicitlyIndexProperty = @this.EnumerateAllMembers(arguments: indexes).OfType<PropertyInfo>().FirstOrDefault(p => p.GetCustomAttribute<IndexerNameAttribute>() != null);
            if (explicitlyIndexProperty != null)
            {
                return explicitlyIndexProperty;
            }
            var implicitlyIndexProperty = @this.EnumerateAllMembers("Item", indexes).OfType<PropertyInfo>().FirstOrDefault(p => p.Name == "Item");
            if (implicitlyIndexProperty != null)
            {
                return implicitlyIndexProperty;
            }
            throw new NotImplementedException($"Not implimented Indexer in {@this.GetType().Name}");
        }

        public static object GetValue(this MemberInfo fieldOrProperty, object @this, params object[] indexes)
        {
            switch(fieldOrProperty)
            {
                case FieldInfo f:
                    return f.GetValue(@this);
                case PropertyInfo p:
                    return p.GetValue(@this, indexes);
                default:
                    throw new AccessViolationException($"{@this.GetType().Name}{fieldOrProperty.Name} expected field|property, but {fieldOrProperty.GetType()}");
            }
        }
        
        public static void SetValue(this MemberInfo fieldOrProperty, object @this, object value, params object[] indexes)
        {
            switch(fieldOrProperty)
            {
                case FieldInfo f:
                    f.SetValue(@this, value);
                    break;
                case PropertyInfo p:
                    p.SetValue(@this, value, indexes);
                    break;
                default:
                    throw new AccessViolationException($"{@this.GetType().Name}{fieldOrProperty.Name} expected field|property, but {fieldOrProperty.GetType()}");
            }
        }

        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second) => new HashSet<T>(first).SetEquals(new HashSet<T>(second));

        public static bool ExceptionToBool(Action action) => ExceptionToBool(out var _, () => { action(); return true; });
        public static bool ExceptionToBool(out object result, Func<object> action)
        {
            try
            {
                result = action();
                return true;
            }
            catch(Exception e)
            {
                PowerAccessor.ErrorHandler?.Invoke(e);
                result = null;
                return false;
            }
        }
    
        public static object ExpressionMethod(this object @this, ExpressionType type, object arg)
        {
            if(arg is PowerAccessor pa) arg = pa.@this;
            switch(type)
            {   
                // +,-
                case ExpressionType.Negate: return @this.EnumerateAllMembers("op_UnaryPlus").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.UnaryPlus: return @this.EnumerateAllMembers("op_UnaryNegation").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                // ==,!=,>,>=,<,<=,
                case ExpressionType.Equal: return @this.EnumerateAllMembers("op_Equality").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.NotEqual: return @this.EnumerateAllMembers("op_Inequality").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.GreaterThan: return @this.EnumerateAllMembers("op_GreaterThan").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.GreaterThanOrEqual: return @this.EnumerateAllMembers("op_GreaterThanOrEqual").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.LessThan: return @this.EnumerateAllMembers("op_LessThan").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.LessThanOrEqual: return @this.EnumerateAllMembers("op_LessThanOrEqual").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                // ++,--
                case ExpressionType.Increment: return @this.EnumerateAllMembers("op_Increment").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Decrement: return @this.EnumerateAllMembers("op_Decrement").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                // +,-,*,/,%,<<,>>
                case ExpressionType.Add: return @this.EnumerateAllMembers("op_Addition").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Subtract: return @this.EnumerateAllMembers("op_Subtraction").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Multiply: return @this.EnumerateAllMembers("op_Multiply").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Divide: return @this.EnumerateAllMembers("op_Division").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Modulo: return @this.EnumerateAllMembers("op_Modulus").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.LeftShift: return @this.EnumerateAllMembers("op_LeftShift").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.RightShift: return @this.EnumerateAllMembers("op_RightShift").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                // &,|
                case ExpressionType.And: return @this.EnumerateAllMembers("op_BitwiseAnd").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.Or: return @this.EnumerateAllMembers("op_BitwiseOr").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                case ExpressionType.ExclusiveOr: return @this.EnumerateAllMembers("op_ExclusiveOr").OfType<MethodInfo>().First().Invoke(null, new []{ @this, arg });
                default:
                    throw new NotImplementedException($"{@this.GetType().Name}.{type}");
            }
        }
    }

}
