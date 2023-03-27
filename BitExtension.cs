using System;
using System.Collections.Generic;
using System.Linq;

public static class BitExtension
{
    public static IEnumerable<int> GetAllIndex(this long bitset)
    {
        for (int i = 0; (bitset >> i) != 0; i++)
        {
            if ((bitset & (1 << i)) != 0)
                yield return i;
        }
    }

    public static int ToIndex(this long bitSet) => GetAllIndex(bitSet).FirstOrDefault();
    
    public static int RandomIndex(this long num, Random rand)
    {
        // randomly select a bit that is set to 1
        int bitIndex;
        do
        {
            bitIndex = rand.Next(0, 32); // assuming a 32-bit integer
        } while (((num >> bitIndex) & 1) == 0); // continue until a 1 bit is found

        return bitIndex;
    }

    public static bool IsOnlyOneBit(this long bitset)
        => (bitset != 0) && ((bitset & (bitset - 1)) == 0);
}