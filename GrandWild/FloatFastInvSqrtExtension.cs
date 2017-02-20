///<summary>fast 1/sqrt(x) function</summary>
///
///Fast (but not accurate) way to calculate 1/sqrt(x).
///
///From: https://en.wikipedia.org/wiki/Fast_inverse_square_root
///
///Licensed under public domain.
using System;

namespace org.flamerat.GrandWild {
    static class FloatFastInvSqrtExtension {
        public static float FastInvSqrt(this float number) {
            unsafe
            {
                Int32 i;
                float x2, y;
                const float threehalfs = 1.5F;
                x2 = number * 0.5F;
                y = number;
                i = *(Int32*)&y;
                i = 0x5f3759df - (i >> 1);
                y = *(float*)&i;
                y = y * (threehalfs - (x2 * y * y)); //This line can be repeated to get more accuracy
                return y;
            }
        }
    }
}
