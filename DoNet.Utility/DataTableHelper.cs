using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace DoNet.Utility
{
    public static class DataTableHelper
    {
        public static T DataRowToEntity<T>(DataRow tableRow) where T : new()
        {
            // Create a new type of the entity I want
            var t = typeof (T);
            var returnObject = new T();

            foreach (DataColumn col in tableRow.Table.Columns)
            {
                var colName = col.ColumnName;

                // Look for the object's property with the columns name, ignore case
                var pInfo = t.GetProperty(colName.ToLower(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // did we find the property ?
                if (pInfo != null)
                {
                    var val = tableRow[colName];

                    // is this a Nullable<> type
                    var isNullable = (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
                    if (isNullable)
                    {
                        if (val is DBNull)
                        {
                            val = null;
                        }
                        else
                        {
                            // Convert the db type into the T we have in our Nullable<T> type
                            val = Convert.ChangeType(val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                        }
                    }
                    else
                    {
                        try
                        {
                            // Convert the db type into the type of the property in our entity
                            val = Convert.ChangeType(val, pInfo.PropertyType);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    try
                    {
                        // Set the value of the property with the value from the db
                        pInfo.SetValue(returnObject, val, null);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            // return the entity object with values
            return returnObject;
        }

        public static List<T> DataTableToList<T>(DataTable table) where T : new()
        {
            // Create a new type of the entity I want
            var result = new List<T>();

            foreach (DataRow tableRow in table.Rows)
            {
                var t = typeof (T);
                var returnObject = new T();
                foreach (DataColumn col in table.Columns)
                {
                    var colName = col.ColumnName;

                    // Look for the object's property with the columns name, ignore case
                    var pInfo = t.GetProperty(colName.ToLower(),
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    // did we find the property ?
                    if (pInfo != null)
                    {
                        var val = tableRow[colName];

                        // is this a Nullable<> type
                        var isNullable = (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
                        if (isNullable)
                        {
                            if (val is DBNull)
                            {
                                val = null;
                            }
                            else
                            {
                                // Convert the db type into the T we have in our Nullable<T> type
                                val = Convert.ChangeType
                                    (val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                            }
                        }
                        else
                        {
                            // Convert the db type into the type of the property in our entity
                            val = Convert.ChangeType(val, pInfo.PropertyType);
                        }
                        // Set the value of the property with the value from the db
                        pInfo.SetValue(returnObject, val, null);
                    }
                }
                result.Add(returnObject);
            }

            // return the entity object with values
            return result;
        }

        public static DataTable GetDataTableSchema<T>()
        {
            var props =
                TypeDescriptor.GetProperties(typeof (T));
            var table = new DataTable();
            for (var i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                var pt = prop.PropertyType;
                if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof (Nullable<>))
                    pt = Nullable.GetUnderlyingType(pt);
                table.Columns.Add(prop.Name, pt);
            }
            return table;
        }

        public static DataTable ListToDataTable<T>(List<T> data)
        {
            var props =
                TypeDescriptor.GetProperties(typeof (T));
            var table = GetDataTableSchema<T>();
            var values = new object[props.Count];
            foreach (var item in data)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            table.AcceptChanges();
            return table;
        }

        /// <summary>
        ///     把customRow的dbnull的字段，用默认值填充
        /// </summary>
        public static void RemoveDbNullColumn<T>(T customRow) where T : class
        {
            var pis =
                typeof (T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                         BindingFlags.NonPublic | BindingFlags.SetProperty);

            foreach (var pi in pis)
            {
                try
                {
                    var value = pi.GetValue(customRow, null);
                }
                catch (Exception)
                {
                    pi.SetValue(customRow, GetDefaultValue(pi.PropertyType.FullName.ToLower()), null);
                }
            }
        }

        /// <summary>
        ///     比较新旧customRow的内容，新customRow中为dbnull的字段，用旧customRow中的字段来填充
        /// </summary>
        public static void RemoveDbNullColumn<T>(T newCustomRow, T oldCustomRow) where T : class
        {
            var pis =
                typeof (T).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                         BindingFlags.NonPublic | BindingFlags.SetProperty);

            foreach (var pi in pis)
            {
                try
                {
                    var value = pi.GetValue(newCustomRow, null);
                }
                catch (Exception)
                {
                    try
                    {
                        pi.SetValue(newCustomRow, pi.GetValue(oldCustomRow, null), null);
                    }
                    catch (Exception)
                    {
                        pi.SetValue(newCustomRow, GetDefaultValue(pi.PropertyType.FullName.ToLower()), null);
                    }
                }
            }
        }

        /// <summary>
        ///     供RemoveDbNullColumn方法使用，返回每种类型的默认值
        /// </summary>
        private static object GetDefaultValue(string type)
        {
            object defaultValue;
            switch (type)
            {
                case "system.string":
                    defaultValue = string.Empty;
                    break;

                case "system.datetime":
                    defaultValue = DateTime.Parse("1900-01-01");
                    break;

                case "system.int16":
                case "system.int32":
                case "system.int64":
                    defaultValue = int.MinValue;
                    break;

                case "system.single":
                    defaultValue = float.MinValue;
                    break;

                case "system.double":
                    defaultValue = double.MinValue;
                    break;

                case "system.char":
                    defaultValue = char.MinValue;
                    break;

                case "system.guid":
                    defaultValue = Guid.Empty;
                    break;

                case "system.boolean":
                    defaultValue = false;
                    break;

                default:
                    defaultValue = new object();
                    break;
            }

            return defaultValue;
        }
    }
}