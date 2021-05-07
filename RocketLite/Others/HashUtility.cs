using System.Runtime.CompilerServices;

namespace RocketLite
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