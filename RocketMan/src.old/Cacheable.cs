using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse.Noise;

namespace RocketMan
{
    public abstract class CacheableBase<TType, TTime, TInterval>
        where TInterval : new()
    {
        protected TType _backingFiled;

        public CacheableBase(TType t, Func<TTime> now, TInterval interval, Func<TType> update,
            TTime lastUpdateTime = default)
        {
            _backingFiled = t;
            UpdateInterval = interval;
            Update = update;
            Now = now;
            LastUpdateTime = lastUpdateTime;
        }

        public TInterval UpdateInterval { get; protected set; }

        public TTime LastUpdateTime { get; protected set; }

        public Func<TType> Update { get; protected set; }

        public Func<TTime> Now { get; set; }

        public abstract bool ShouldUpdate(out TTime now);

        public virtual TType Value
        {
            get => _backingFiled;

            set
            {
                LastUpdateTime = Now();
                _backingFiled = value;
            }
        }
    }

    public class CacheableTime<TType> : CacheableBase<TType, DateTime, TimeSpan>
    {
        public CacheableTime(TType t, Func<DateTime> now, TimeSpan updateInterval, Func<TType> update,
            DateTime lastUpdateTime = default)
            : base(t, now, updateInterval, update, DateTime.UtcNow)
        {
        }

        public CacheableTime(TType t, TimeSpan updateInterval, Func<TType> update)
            : this(t, () => DateTime.UtcNow, updateInterval, update, DateTime.UtcNow)
        {
        }

        public static implicit operator TType(CacheableTime<TType> cache)
        {
            if (!cache.ShouldUpdate(out var now))
            {
                return cache._backingFiled;
            }
            else
            {
                cache.LastUpdateTime = now;
                return cache._backingFiled = cache.Update == null ? cache._backingFiled : cache.Update();
            }
        }

        public override bool ShouldUpdate(out DateTime now)
        {
            now = Now();
            return !(LastUpdateTime + UpdateInterval > now);
        }
    }

    public class CacheableTick<TType> : CacheableBase<TType, int, int>
    {
        public CacheableTick(TType t, Func<int> now, int updateInterval, Func<TType> update, int lastUpdateTime = 0)
            : base(t, now, updateInterval, update, lastUpdateTime)
        {
        }

        public static implicit operator TType(CacheableTick<TType> cache)
        {
            if (!cache.ShouldUpdate(out var now))
            {
                return cache._backingFiled;
            }
            else
            {
                cache.LastUpdateTime = now;
                return cache._backingFiled = cache.Update == null ? cache._backingFiled : cache.Update();
            }
        }

        public override bool ShouldUpdate(out int now)
        {
            now = Now();
            return !(LastUpdateTime + UpdateInterval > now);
        }
    }
}