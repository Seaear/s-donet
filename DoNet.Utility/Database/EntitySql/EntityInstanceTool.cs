using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using DoNet.Utility.Database.EntitySql.Entity;

namespace DoNet.Utility.Database.EntitySql
{
    /// <summary>
    ///     实体实例的工具
    /// </summary>
    public static class EntityInstanceTool
    {
        /// <summary>
        ///     从DataReader实例化一个Entity
        /// </summary>
        /// <typeparam name="T">实体类的类型</typeparam>
        /// <param name="reader">DataReader</param>
        /// <param name="entityPropertyInfos">已经排好序的实体类的属性集合</param>
        /// <returns></returns>
        public static T FillOneEntity<T>(IDataReader reader) where T : class, new()
        {
            var entityPropertyInfo = EntityMappingTool.GetEntityPropertyInfos(typeof (T));
            var entity = new T();

            for (var i = 0; i < entityPropertyInfo.Count; i++)
            {
                if (reader.IsDBNull(i))
                    continue;
                entityPropertyInfo[i].SetValue(entity, reader.GetValue(i), null);
            }

            return entity;
        }

        /// <summary>
        ///     从DataReader实例化一个Entity对
        /// </summary>
        /// <typeparam name="TA">实体类的类型TA</typeparam>
        /// <typeparam name="TB">实体类的类型TB</typeparam>
        /// <param name="reader">DataReader</param>
        /// <param name="entityPropertysA">已经排好序的实体类的属性集合TA</param>
        /// <param name="entityPropertysB">已经排好序的实体类的属性集合TB</param>
        /// <returns></returns>
        public static GenericPairEntity<TA, TB> FillOnePairEntity<TA, TB>(IDataReader reader)
            where TA : class, new()
            where TB : class, new()
        {
            var entityPropertyInfoA = EntityMappingTool.GetEntityPropertyInfos(typeof (TA));
            var entityPropertyInfoB = EntityMappingTool.GetEntityPropertyInfos(typeof (TB));
            var pair = new GenericPairEntity<TA, TB>();
            pair.EntityA = new TA();
            pair.EntityB = new TB();

            var offset = 0;
            for (var i = 0; i < entityPropertyInfoA.Count; i++)
            {
                if (reader.IsDBNull(offset + i))
                    continue;
                entityPropertyInfoA[i].SetValue(pair.EntityA, reader.GetValue(offset + i), null);
            }

            offset = entityPropertyInfoA.Count;
            for (var i = 0; i < entityPropertyInfoB.Count; i++)
            {
                if (reader.IsDBNull(offset + i))
                    continue;
                entityPropertyInfoB[i].SetValue(pair.EntityB, reader.GetValue(offset + i), null);
            }

            return pair;
        }

        /// <summary>
        ///     检索出Entity实例中有设置值的字段
        /// </summary>
        /// <typeparam name="T">实体类的类型</typeparam>
        /// <param name="entity">实体类的实例</param>
        /// <param name="effectiveEntityMemberNames">有效的Entity字段名称</param>
        /// <param name="entityPropertyInfos">实体类对应的属性</param>
        /// <returns>Entity实例中有设置值的字段</returns>
        public static List<string> GetNotNullFields<T>(T entity)
        {
            var entityFieldNames = EntityMappingTool.GetEntityFieldNames((typeof (T)));
            var entityPropertyInfos = EntityMappingTool.GetEntityPropertyInfos((typeof (T)));
            var notNullEntityFields = new List<string>(entityFieldNames.Count);

            for (var i = 0; i < entityFieldNames.Count; i++)
            {
                if (entityPropertyInfos[i].GetValue(entity, null) != null)
                    notNullEntityFields.Add(entityFieldNames[i]);
            }

            return notNullEntityFields;
        }

        /// <summary>
        ///     检索出Entity实例中有设置值的属性
        /// </summary>
        /// <typeparam name="T">实体类的类型</typeparam>
        /// <param name="entity">实体类的实例</param>
        /// <param name="entityPropertyInfos">实体类对应的属性</param>
        /// <returns>Entity实例中有设置值的属性</returns>
        public static List<PropertyInfo> GetNotNullEntityPropertys<T>(T entity)
        {
            var entityPropertyInfos = EntityMappingTool.GetEntityPropertyInfos((typeof (T)));
            var notNullEntityPropertys = new List<PropertyInfo>(entityPropertyInfos.Count);

            for (var i = 0; i < entityPropertyInfos.Count; i++)
            {
                if (entityPropertyInfos[i].GetValue(entity, null) != null)
                    notNullEntityPropertys.Add(entityPropertyInfos[i]);
            }

            return notNullEntityPropertys;
        }

        /// <summary>
        ///     判断Entity实例中有没有设置主键值
        /// </summary>
        public static bool HasPrimaryKeyValue<T>(T entity)
        {
            if (!EntityMappingTool.HasPrimaryKey(typeof (T)))
                return false;

            var notNullEntityFields = GetNotNullFields(entity);
            if (notNullEntityFields == null)
                return false;

            var primaryKeyEntityFieldNames = EntityMappingTool.GetPrimaryKeyOfEntityField(typeof (T));
            for (var i = 0; i < primaryKeyEntityFieldNames.Count; i++)
            {
                if (!notNullEntityFields.Any(n => n == primaryKeyEntityFieldNames[i]))
                    return false;
            }

            return true;
        }
    }
}