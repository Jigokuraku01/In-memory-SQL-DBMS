using MknImmiSql.Inner_Logic.V1;
using System;
using System.Collections.Generic;

namespace MknImmiSql.Api.V1
{
    public class GlocalServeiseContest
    {
        public sealed class ContestSingleton
        {
            public readonly String token;
            private static volatile ContestSingleton _instance;
            private static readonly object _lock = new object();

            public Dictionary<string, TableInfo> Tables;
            private ContestSingleton()
            {
                token = Guid.NewGuid().ToString("N");
                Tables = new Dictionary<string, TableInfo>();
            }
            public static ContestSingleton Instance
            {
                get
                {
                    if (_instance is not null)
                        return _instance;
                    lock (_lock)
                    {
                        if (_instance is not null)
                            return _instance;
                        _instance = new ContestSingleton();
                        return _instance;
                    }
                }
            }
        }
    }
}
