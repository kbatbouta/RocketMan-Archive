using System;
using System.Runtime.CompilerServices;
using Verse;

namespace RocketMan
{
    internal static class DeferredStackTracingImpl
    {
        private struct AddrInfo
        {
            public long addr;

            public long stackUsage;

            public long nameHash;

            public long unused;
        }

        private const int StartingN = 7;

        private const int StartingShift = 57;

        private const int StartingSize = 128;

        private const float LoadFactor = 0.5f;

        private static AddrInfo[] hashtable = new AddrInfo[StartingSize];

        public static int hashtableSize = StartingSize;

        public static int hashtableEntries;

        public static int hashtableShift = StartingShift;

        public static int collisions;

        const long NotJIT = long.MaxValue;

        const long RBPBased = 9223372036854775806L;

        const long UsesRBPAsGPR = 262144L;

        const long UsesRBX = 524288L;

        const long RBPInfoClearMask = -786433L;

        public const int MaxDepth = 32;

        public const int HashInfluence = 6;

        public unsafe static int TraceImpl(long[] traceIn, ref int hash)
        {
            long rbp = GetRbp();
            long num = rbp;
            rbp = *(long*)rbp;
            int num2 = hashtableSize - 1;
            int num3 = hashtableShift;
            long num4 = *(long*)Platform.LmfPtr;
            int depth = 0;
            while (true)
            {
                long num6 = *(long*)(num + 8);
                int num7 = (int)(HashAddr((ulong)num6) >> num3);
                ref AddrInfo reference = ref hashtable[num7];
                int num8 = 0;
                while (reference.addr != 0L && reference.addr != num6)
                {
                    num7 = ((num7 + 1) & num2);
                    reference = ref hashtable[num7];
                    num8++;
                }
                if (num8 > collisions)
                {
                    collisions = num8;
                }
                long num9 = 0L;
                num9 = ((reference.addr == 0) ? UpdateNewElement(ref reference, num6) : reference.stackUsage);
                if (num9 == NotJIT)
                {
                    num4 = *(long*)num4;
                    long num10 = *(long*)(num4 + 8);
                    if (num4 == 0L || num10 == 0)
                    {
                        break;
                    }
                    rbp = num10;
                    num = *(long*)(num4 + 16) - 16;
                    continue;
                }
                traceIn[depth] = num6;
                if (depth < 6)
                {
                    hash = Gen.HashCombineInt(hash, (int)reference.nameHash);
                }
                if (++depth == MaxDepth)
                {
                    break;
                }
                if (num9 == RBPBased)
                {
                    num = rbp;
                    rbp = *(long*)rbp;
                    continue;
                }
                num += 8;
                if ((num9 & 0x40000) != 0)
                {
                    rbp = (((num9 & 0x80000) == 0) ? (*(long*)(num + 8)) : (*(long*)(num + 16)));
                    num9 &= -786433;
                }
                num += num9;
            }
            return depth;
        }

        static long UpdateNewElement(ref AddrInfo info, long ret)
        {
            long stackUsage = GetStackUsage(ret);
            info.addr = ret;
            info.stackUsage = stackUsage;
            string text = Platform.MethodNameFromAddr(ret);
            info.nameHash = ((text == null) ? 1 : GenText.StableStringHash(SyncCoordinator.MethodNameWithoutIL(text)));
            hashtableEntries++;
            if ((float)hashtableEntries > (float)hashtableSize * LoadFactor)
            {
                ResizeHashtable();
            }
            return stackUsage;
        }

        static ulong HashAddr(ulong addr)
        {
            return (ulong)((long)((addr >> 4) | (addr << 60)) * -7046029254386353131L);
        }

        static int ResizeHashtable()
        {
            AddrInfo[] array = hashtable;
            hashtableSize *= 2;
            hashtableShift--;
            hashtable = new AddrInfo[hashtableSize];
            collisions = 0;
            int num = hashtableSize - 1;
            int num2 = hashtableShift;
            for (int i = 0; i < array.Length; i++)
            {
                ref AddrInfo reference = ref array[i];
                if (reference.addr != 0)
                {
                    int num3 = (int)(HashAddr((ulong)reference.addr) >> num2);
                    while (hashtable[num3].addr != 0)
                    {
                        num3 = ((num3 + 1) & num);
                    }
                    ref AddrInfo reference2 = ref hashtable[num3];
                    reference2.addr = reference.addr;
                    reference2.stackUsage = reference.stackUsage;
                    reference2.nameHash = reference.nameHash;
                }
            }
            return num;
        }

        unsafe static long GetStackUsage(long addr)
        {
            IntPtr intPtr = Platform.mono_jit_info_table_find(Platform.DomainPtr, (IntPtr)addr);
            if (intPtr == IntPtr.Zero)
            {
                return NotJIT;
            }
            uint* ptr = (uint*)(void*)Platform.mono_jit_info_get_code_start(intPtr);
            long stackUsage = 0L;
            if ((*ptr & 0xFFFFFF) == 15500104)
            {
                stackUsage = *ptr >> 24;
                ptr++;
            }
            else if ((*ptr & 0xFFFFFF) == 15499592)
            {
                stackUsage = *(uint*)((long)ptr + 3);
                ptr = (uint*)((long)ptr + StartingN);
            }
            if (stackUsage != 0)
            {
                CheckRbpUsage(ptr, ref stackUsage);
                return stackUsage;
            }
            if (*(byte*)ptr == 85)
            {
                return RBPBased;
            }
            throw new Exception($"Deferred stack tracing: Unknown function header {*ptr} {Platform.MethodNameFromAddr(addr)}");
        }

        unsafe static void CheckRbpUsage(uint* at, ref long stackUsage)
        {
            if (*at == 606898504)
            {
                stackUsage |= UsesRBPAsGPR;
            }
            else if (*at == 605849928 && at[1] == 611092808)
            {
                stackUsage |= UsesRBPAsGPR;
                stackUsage |= UsesRBX;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        unsafe static long GetRbp()
        {
            long num = 0L;
            return (&num)[1];
        }
    }
}
