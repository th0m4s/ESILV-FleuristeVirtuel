using FleuristeVirtuel_API.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

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

        public static void ExportToJson<T>(this List<T> liste, Type originalType, string fileName) where T : DbRecord
        {
            // Create a new JSON document
            using var stream = new FileStream(fileName, FileMode.Create);
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartArray();

            foreach (var element in liste)
            {
                writer.WriteStartObject();

                foreach (PropertyInfo pi in originalType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = pi.GetCustomAttributes<DbColumnAttribute>();
                    if (attr != null && attr.Count() > 0)
                    {
                        string columnName = attr.ToArray()[0].ColumnName;
                        if (columnName == "") columnName = pi.Name;
                        writer.WriteString(columnName, pi.GetValue(element)?.ToString() ?? "");
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        public static void ExportToXml<T>(this List<T> liste, Type originalType, string fileName, string rootName = "data", string? elementName = null) where T : DbRecord
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDocument.InsertBefore(xmlDeclaration, xmlDocument.DocumentElement);
            XmlElement xmlElement = xmlDocument.CreateElement(rootName);

            string typeName = elementName ?? originalType.Name.Substring(1).ToLower();

            foreach(var element in liste)
            {
                XmlElement instanceElement = xmlDocument.CreateElement(typeName);
                foreach(PropertyInfo pi in originalType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attr = pi.GetCustomAttributes<DbColumnAttribute>();
                    if(attr != null && attr.Count() > 0)
                    {
                        string columnName = attr.ToArray()[0].ColumnName;
                        if (columnName == "") columnName = pi.Name;
                        XmlElement columnElement = xmlDocument.CreateElement(columnName);
                        columnElement.InnerText = pi.GetValue(element)?.ToString() ?? "";
                        instanceElement.AppendChild(columnElement);
                    }
                }
                xmlElement.AppendChild(instanceElement);
            }


            xmlDocument.AppendChild(xmlElement);
            xmlDocument.Save(fileName);
        }

        public static object? ChangeNullableType(object? value, Type conversionType)
        {
            if (conversionType == null)
                throw new ArgumentNullException("conversionType");

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;

                conversionType = Nullable.GetUnderlyingType(conversionType) ?? throw new Exception("Invalid conversionType!");
            }

            return Convert.ChangeType(value, conversionType);
        }
    }
}
