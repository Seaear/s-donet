﻿using System;

namespace DoNet.Utility.Database.EntitySql.Entity
{
    /// <summary>
    ///     实体对,用于部分连接操作的Entity化
    /// </summary>
    /// <typeparam name="TA"></typeparam>
    /// <typeparam name="TB"></typeparam>
    [Serializable]
    public class GenericPairEntity<TA, TB>
        where TA : class, new()
        where TB : class, new()
    {
        public TA EntityA { get; set; }
        public TB EntityB { get; set; }
    }
}