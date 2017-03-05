//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Threading;

//namespace Kontur.GameStats.Server
//{
//    public class SuperCache<TKey, TValue>
//    {
//        private readonly int _cleanInterval;
//        private DateTime _lastCacheClean = DateTime.MinValue;
//        private Dictionary<TKey, TValue> _data;
//        private Func<TKey, TValue> _getter;

//        public bool Enabled { set; get; }

//        public SuperCache(int cleanInt, Func<TKey, TValue> getter)
//        {
//            _cleanInterval = cleanInt;
//        }

//        public TValue this[TKey key]
//        {
//            get { return ; }
//        }

//        public TValue Update()
//        {
            
//        }

//        public void Clean()
//        {
            
//        }
//    }
//}
