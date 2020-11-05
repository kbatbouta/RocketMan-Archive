using System;
using HugsLib;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using System.CodeDom;
using System.Threading;
using System.Diagnostics;
using UnityEngine.Assertions.Must;

namespace RocketMan
{

    public struct CachedUnit<T>
    {
        public readonly int tick;

        public readonly T value;

        public CachedUnit(T value)
        {
            this.tick = GenTicks.TicksGame;
            this.value = value;
        }

        public bool IsValid(int expiry = 0)
        {
            if (GenTicks.TicksGame - tick <= expiry)
                return true;
            return false;
        }
    }

    public class CachedDict<A, B>
    {
        private Dictionary<A, CachedUnit<B>> cache = new Dictionary<A, CachedUnit<B>>();

        public bool TryGetValue(A key, out B value, int expiry = 0)
        {
            if (cache.TryGetValue(key, out var store) && store.IsValid(expiry))
            {
                value = store.value;
                return true;
            }
            value = default(B);
            return false;
        }

        public bool TryGetValue(A key, out B value, out bool failed, int expiry = 0)
        {
            if (cache.TryGetValue(key, out var store) && store.IsValid(expiry))
            {
                failed = false;
                value = store.value;
                return true;
            }
            failed = true;
            value = default(B);
            return false;
        }

        public void AddPair(A key, B value) => cache[key] = new CachedUnit<B>(value);

        public B this[A key]
        {
            get => this.cache[key].value;
            set => this.AddPair(key, value);
        }

        public void Remove(A key)
        {
            cache.Remove(key);
        }
    }
}
