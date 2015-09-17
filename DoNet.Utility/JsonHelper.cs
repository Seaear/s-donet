using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DoNet.Utility
{
    public class JsonHelper
    {
        /// <summary>
        ///     dataTable转换成Json格式
        /// </summary>
        /// <returns>
        ///     [
        ///     { "firstName": "Brett", "lastName":"McLaughlin", "email": "aaaa" },
        ///     { "firstName": "Jason", "lastName":"Hunter", "email": "bbbb" },
        ///     { "firstName": "Elliotte", "lastName":"Harold", "email": "cccc" }
        ///     ]
        /// </returns>
        public static string ToJson(DataTable dt)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            try
            {
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("[");
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(dt.Columns[j].ColumnName);
                        jsonBuilder.Append("\":\"");
                        jsonBuilder.Append(dt.Rows[i][j]);
                        jsonBuilder.Append("\",");
                    }
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    jsonBuilder.Append("},");
                }
                if (dt.Rows.Count > 0)
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]");
                return jsonBuilder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     dataTable转换成Json格式
        /// </summary>
        /// <returns>
        ///     { "total":28,"rows": [
        ///     { "firstName": "Brett", "lastName":"McLaughlin", "email": "aaaa" },
        ///     { "firstName": "Jason", "lastName":"Hunter", "email": "bbbb" },
        ///     { "firstName": "Elliotte", "lastName":"Harold", "email": "cccc" }
        ///     ]}
        /// </returns>
        public static string ToJson(DataTable dt, int rowCount)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            try
            {
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{\"total\":" + rowCount + ",\"rows\":[");
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(dt.Columns[j].ColumnName);
                        jsonBuilder.Append("\":\"");
                        jsonBuilder.Append(dt.Rows[i][j]);
                        jsonBuilder.Append("\",");
                    }
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    jsonBuilder.Append("},");
                }
                if (dt.Rows.Count > 0)
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]}");
                return jsonBuilder.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     dataTable转换成Json格式
        /// </summary>
        /// <returns>
        ///     { "total":28,"rows": [
        ///     { "firstName": "Brett", "lastName":"McLaughlin", "email": "aaaa" },
        ///     { "firstName": "Jason", "lastName":"Hunter", "email": "bbbb" },
        ///     { "firstName": "Elliotte", "lastName":"Harold", "email": "cccc" }
        ///     ]}
        /// </returns>
        public static string ToJson(DataTable dt, int rowCount, Dictionary<string, string> dic)
        {
            if (dt == null)
                throw new ArgumentNullException("dt");

            if (dic == null)
                throw new ArgumentNullException("rowCount");

            try
            {
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{\"total\":" + rowCount + ",\"rows\":[");
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(dt.Columns[j].ColumnName);
                        jsonBuilder.Append("\":\"");
                        jsonBuilder.Append(dt.Rows[i][j]);
                        jsonBuilder.Append("\",");
                    }

                    foreach (var valuePair in dic)
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(valuePair.Key);
                        jsonBuilder.Append("\":\"");
                        jsonBuilder.Append(valuePair.Value);
                        jsonBuilder.Append("\",");
                    }

                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                    jsonBuilder.Append("},");
                }
                if (dt.Rows.Count > 0)
                    jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("]}");
                return jsonBuilder.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     dataset转换成Json格式
        /// </summary>
        /// <param name="ds"></param>
        /// <returns>
        ///     成功(JSON) 异常99998|信息
        ///     返回的格式
        ///     { "programmers": [
        ///     { "firstName": "Brett", "lastName":"McLaughlin", "email": "aaaa" },
        ///     { "firstName": "Jason", "lastName":"Hunter", "email": "bbbb" },
        ///     { "firstName": "Elliotte", "lastName":"Harold", "email": "cccc" }
        ///     ],
        ///     "authors": [
        ///     { "firstName": "Isaac", "lastName": "Asimov", "genre": "science fiction" },
        ///     { "firstName": "Tad", "lastName": "Williams", "genre": "fantasy" },
        ///     { "firstName": "Frank", "lastName": "Peretti", "genre": "christian fiction" }
        ///     ],
        ///     "musicians": [
        ///     { "firstName": "Eric", "lastName": "Clapton", "instrument": "guitar" },
        ///     { "firstName": "Sergei", "lastName": "Rachmaninoff", "instrument": "piano" }
        ///     ] }
        /// </returns>
        public static string ToJson(DataSet ds)
        {
            try
            {
                var jsonBuilder = new StringBuilder();
                jsonBuilder.Append("{");
                foreach (DataTable dt in ds.Tables)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.TableName);
                    jsonBuilder.Append("\":");
                    jsonBuilder.Append(ToJson(dt));
                    jsonBuilder.Append(",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("}");
                return jsonBuilder.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     List转成json
        /// </summary>
        /// <returns>成功(JSON) 异常99998|信息</returns>
        public static string ToJson<T>(IList<T> list)
        {
            try
            {
                var json = new StringBuilder();
                json.Append("[");
                if (list.Count > 0)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var obj = Activator.CreateInstance<T>();
                        var type = obj.GetType();
                        var pis = type.GetProperties();
                        json.Append("{");
                        for (var j = 0; j < pis.Length; j++)
                        {
                            json.Append("\"" + pis[j].Name + "\":\"" + pis
                                [j].GetValue(list[i], null) + "\"");
                            if (j < pis.Length - 1)
                            {
                                json.Append(",");
                            }
                        }
                        json.Append("}");
                        if (i < list.Count - 1)
                        {
                            json.Append(",");
                        }
                    }
                }
                json.Append("]");
                return json.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     List转成json
        /// </summary>
        /// <returns>成功(JSON) 异常99998|信息</returns>
        public static string ToJson(IList list)
        {
            try
            {
                var json = new StringBuilder();
                json.Append("[");
                if (list.Count > 0)
                {
                    for (var i = 0; i < list.Count; i++)
                    {
                        var type = list[i].GetType();
                        var pis = type.GetProperties();
                        json.Append("{");
                        for (var j = 0; j < pis.Length; j++)
                        {
                            json.Append("\"" + pis[j].Name + "\":\"" + pis
                                [j].GetValue(list[i], null) + "\"");
                            if (j < pis.Length - 1)
                            {
                                json.Append(",");
                            }
                        }
                        json.Append("}");
                        if (i < list.Count - 1)
                        {
                            json.Append(",");
                        }
                    }
                }
                json.Append("]");
                return json.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     对象转换为Json字符串
        /// </summary>
        /// <param name="jsonObject">对象</param>
        /// <returns>Json字符串 成功(JSON) 异常99998|信息</returns>
        public static string ToJson(object jsonObject)
        {
            try
            {
                var jsonString = "{";
                var propertyInfo = jsonObject.GetType().GetProperties();
                for (var i = 0; i < propertyInfo.Length; i++)
                {
                    var objectValue = propertyInfo[i].GetGetMethod().Invoke
                        (jsonObject, null);
                    var value = string.Empty;
                    if (objectValue is DateTime || objectValue is Guid || objectValue is
                        TimeSpan)
                    {
                        value = "'" + objectValue + "'";
                    }
                    else if (objectValue is string)
                    {
                        value = "'" + ToJson(objectValue.ToString()) + "'";
                    }
                    else if (objectValue is IEnumerable)
                    {
                        value = ToJson((IEnumerable) objectValue);
                    }
                    else
                    {
                        value = ToJson(objectValue.ToString());
                    }
                    jsonString += "\"" + ToJson(propertyInfo[i].Name) + "\":" + value +
                                  ",";
                }
                return DeleteLast(jsonString) + "}";
            }
            catch (Exception)
            {
                //Echao.Switch.Switch.CallLogs("ADMIN", "CTXTGL9999", "对象转换为Json字符串

                //", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName, 

                //ex.Message.ToString(), "ADMIN", "ToJson(object jsonObject)");
                //return "99998|" + ex.Message.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        ///     对象集合转换Json
        /// </summary>
        /// <param name="array">集合对象</param>
        /// <returns>Json字符串 成功(JSON) 异常99998|信息</returns>
        public static string ToJson(IEnumerable array)
        {
            try
            {
                var jsonString = "[";
                foreach (var item in array)
                {
                    jsonString += ToJson(item) + ",";
                }
                return DeleteLast(jsonString) + "]";
            }
            catch (Exception)
            {
                //Echao.Switch.Switch.CallLogs("ADMIN", "CTXTGL9999", "对象集合转换Json", 

                //System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName, 

                //ex.Message.ToString(), "ADMIN", "string ToJson(IEnumerable array)");
                //return "99998|" + ex.Message.ToString();
                return string.Empty;
            }
        }

        /// <summary>
        ///     Datatable转换为Json
        /// </summary>
        public static string ToJson2(DataTable table)
        {
            try
            {
                var jsonString = "[";
                var drc = table.Rows;
                for (var i = 0; i < drc.Count; i++)
                {
                    jsonString += "{";
                    foreach (DataColumn column in table.Columns)
                    {
                        jsonString += "\"" + ToJson(column.ColumnName) + "\":";
                        if (column.DataType == typeof (DateTime) || column.DataType ==
                            typeof (string))
                        {
                            jsonString += "\"" + ToJson(drc[i]
                                [column.ColumnName].ToString()) + "\",";
                        }
                        else
                        {
                            jsonString += ToJson(drc[i][column.ColumnName].ToString()) +
                                          ",";
                        }
                    }
                    jsonString = DeleteLast(jsonString) + "},";
                }
                return DeleteLast(jsonString) + "]";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     DataReader转换为Json
        /// </summary>
        public static string ToJson(DbDataReader dataReader)
        {
            try
            {
                var jsonString = "[";
                while (dataReader.Read())
                {
                    jsonString += "{";

                    for (var i = 0; i < dataReader.FieldCount; i++)
                    {
                        jsonString += "\"" + ToJson(dataReader.GetName(i)) + "\":";
                        if (dataReader.GetFieldType(i) == typeof (DateTime) ||
                            dataReader.GetFieldType(i) == typeof (string))
                        {
                            jsonString += "\"" + ToJson(dataReader[i].ToString()) +
                                          "\",";
                        }
                        else
                        {
                            jsonString += ToJson(dataReader[i].ToString()) + ",";
                        }
                    }
                    jsonString = DeleteLast(jsonString) + "}";
                }
                dataReader.Close();
                return DeleteLast(jsonString) + "]";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     DataSet转换为Json
        /// </summary>
        public static string ToJson2(DataSet dataSet)
        {
            try
            {
                var jsonString = "{";
                foreach (DataTable table in dataSet.Tables)
                {
                    jsonString += "\"" + ToJson(table.TableName) + "\":" + ToJson(table)
                                  + ",";
                }
                return jsonString = DeleteLast(jsonString) + "}";
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     String转换为Json
        /// </summary>
        public static string ToJson(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }

                string temstr;
                temstr = value;
                temstr = temstr.Replace("{", "｛").Replace("}", "｝").Replace(":",
                    "：").Replace(",", "，").Replace("[", "【").Replace("]", "】").Replace(";",
                        "；").Replace("\n", "<br/>").Replace("\r", "");

                temstr = temstr.Replace("\t", " ");
                temstr = temstr.Replace("'", "\'");
                temstr = temstr.Replace(@"\", @"\\");
                temstr = temstr.Replace("\"", "\"\"");
                return temstr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        ///     将可序列的类转化Json数据格式；[采用.net3.5自带的json支持类]
        /// </summary>
        public static string ToJson<T>(T obj)
        {
            var ds = new DataContractJsonSerializer(typeof (T));
            using (var ms = new MemoryStream())
            {
                ds.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        ///     将指定的Json字符串转化为指定的实体类；[采用.net3.5自带的json支持类]
        /// </summary>
        public static T ToObject<T>(string json) where T : class
        {
            var ds = new DataContractJsonSerializer(typeof (T));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var obj = (T) ds.ReadObject(ms);
            ms.Close();
            return obj;
        }

        #region private method

        /// <summary>
        ///     删除结尾字符
        /// </summary>
        private static string DeleteLast(string str)
        {
            try
            {
                if (str.Length > 1)
                {
                    return str.Substring(0, str.Length - 1);
                }
                return str;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion
    }
}