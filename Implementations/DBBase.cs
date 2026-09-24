//using System;

//namespace Avae.DAL;

//public class DBBase
//{
//    private static readonly object _lock = new();
//    private static IDBLayer? _instance;
//    public static IDBLayer Instance
//    {
//        get
//        {
//            return _instance ?? throw new InvalidOperationException("DBBase not initialized");
//        }
//    }

//    public static void Initialize(IDBLayer layer)
//    {
//        lock (_lock)
//        {
//            _instance ??= layer;
//        }
//    }
//}
