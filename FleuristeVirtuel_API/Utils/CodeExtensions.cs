using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FleuristeVirtuel_API.Utils
{
    public static class CodeExtensions
    {
        //public static void SetValue(this MemberInfo mi, object? obj, object? value)
        //{
        //    if(mi is FieldInfo fi)
        //    {
        //        fi.SetValue(obj, value);
        //        return;
        //    }
        //    else if (mi is PropertyInfo pi)
        //    {
        //        pi.SetValue(obj, value);
        //        return;
        //    }

        //    throw new FieldAccessException("Cannot set member " + mi.Name + " value on type " + mi.DeclaringType?.Name + ", this is not a property nor a field!");
        //}

        //public static object? GetValue(this MemberInfo mi, object? obj)
        //{
        //    if(mi is FieldInfo fi)
        //    {
        //        return fi.GetValue(obj);
        //    }
        //    else if(mi is PropertyInfo pi)
        //    {
        //        return pi.GetValue(obj);
        //    }

        //    throw new FieldAccessException("Cannot access member value, this is not a property nor a field!");
        //}

        //public static Type GetVariableType(this MemberInfo mi)
        //{
        //    if (mi is FieldInfo fi)
        //    {
        //        return fi.FieldType;
        //    }
        //    else if (mi is PropertyInfo pi)
        //    {
        //        return pi.PropertyType;
        //    }

        //    throw new FieldAccessException("Cannot access member type, this is not a property nor a field!");
        //}

        public static void DeleteAndRemove<T>(this List<T> liste, T item, string tableName, DbConnection connection) where T : DbRecord, new()
        {
            item.DeleteFrom(tableName, connection);
            liste.Remove(item);
        }
    }
}
