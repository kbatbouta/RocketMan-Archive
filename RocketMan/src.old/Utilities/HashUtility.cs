using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RocketMan
{
    public static class HashUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int HashOne(int numberToHash, int previousHash = 17)
        {
            return previousHash * 7919 + numberToHash;
        }
    }
}