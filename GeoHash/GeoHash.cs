using System;
using System.Runtime.CompilerServices;


namespace Misc.GeoHash
{
    public struct GeoHashNeighbors
    {
        public UInt64 n0;
        public UInt64 n1;
        public UInt64 n2;
        public UInt64 n3;
        public UInt64 n4;
        public UInt64 n5;
        public UInt64 n6;
        public UInt64 n7;
        public UInt64 center;
    }

    public struct GeoHashInterval
    {
        public UInt64 lo;
        public UInt64 hi;
    }

    public static class GeoHash
    {
        private static double latIntervalMin = -90.0;
        private static double latIntervalMax = 90.0;
        private static double lonIntervalMin = -180.0;
        private static double lonIntervalMax = 180.0;

        // https://raw.githubusercontent.com/yinqiwen/geohash-int/master/geohash.c
        // https://github.com/arjunmehta/node-geo-proximity/blob/master/main.js

        public static UInt64 Encode(double latitude, double longitude, byte step)
        {
            if (latitude < latIntervalMin || latitude > latIntervalMax ||
                longitude < lonIntervalMin || longitude > lonIntervalMax)
            {
                throw new ArgumentException();
            }

            // compute the coordinate in the range 0-1
            double lat_offset = (latitude - latIntervalMin) / (latIntervalMax - latIntervalMin);
            double lon_offset = (longitude - lonIntervalMin) / (lonIntervalMax - lonIntervalMin);

            // convert it to fixed point based on the step size
            lat_offset *= (1ul << step);
            lon_offset *= (1ul << step);

            uint ilato = (uint)lat_offset;
            uint ilono = (uint)lon_offset;

            // interleave the bits to create the morton code.  No branching and no bounding
            return Interleave(ilato, ilono);
        }

        public static void Decode(UInt64 hash, byte step)
        {
            UInt64 xyhilo = Deinterleave(hash); // decode morton code

            double lat_scale = latIntervalMax - latIntervalMin;
            double lon_scale = lonIntervalMax - lonIntervalMin;

            uint ilato = (uint)xyhilo;        // get back the original integer coordinates
            uint ilono = (uint)(xyhilo >> 32);

            var latitudemin = latIntervalMin + (ilato * 1.0 / (1ul << step)) * lat_scale;
            var latitudemax = latIntervalMin + ((ilato + 1) * 1.0 / (1ul << step)) * lat_scale;
            var longitudemin = lonIntervalMin + (ilono * 1.0 / (1ul << step)) * lon_scale;
            var longitudemax = lonIntervalMin + ((ilono + 1) * 1.0 / (1ul << step)) * lon_scale;
        }

        private static UInt64 Interleave(uint xlo, uint ylo)
        {
            UInt64 x = xlo; // Interleave lower  bits of x and y, so the bits of x
            UInt64 y = ylo; // are in the even positions and bits from y in the odd;
            // https://graphics.stanford.edu/~seander/bithacks.html#InterleaveBMN
            // x and y must initially be less than 2**32.

            x = (x | (x << 16)) & 0x0000FFFF0000FFFFul;
            y = (y | (y << 16)) & 0x0000FFFF0000FFFFul;

            x = (x | (x << 8)) & 0x00FF00FF00FF00FFul;
            y = (y | (y << 8)) & 0x00FF00FF00FF00FFul;

            x = (x | (x << 4)) & 0x0F0F0F0F0F0F0F0Ful;
            y = (y | (y << 4)) & 0x0F0F0F0F0F0F0F0Ful;

            x = (x | (x << 2)) & 0x3333333333333333ul;
            y = (y | (y << 2)) & 0x3333333333333333ul;

            x = (x | (x << 1)) & 0x5555555555555555ul;
            y = (y | (y << 1)) & 0x5555555555555555ul;

            return x | (y << 1);
        }

        private static UInt64 Deinterleave(UInt64 interleaved)
        {
            UInt64 x = interleaved; // reverse the interleave process (http://stackoverflow.com/questions/4909263/how-to-efficiently-de-interleave-bits-inverse-morton)
            UInt64 y = interleaved >> 1;

            x = (x | (x >> 0)) & 0x5555555555555555ul;
            y = (y | (y >> 0)) & 0x5555555555555555ul;

            x = (x | (x >> 1)) & 0x3333333333333333ul;
            y = (y | (y >> 1)) & 0x3333333333333333ul;

            x = (x | (x >> 2)) & 0x0F0F0F0F0F0F0F0Ful;
            y = (y | (y >> 2)) & 0x0F0F0F0F0F0F0F0Ful;

            x = (x | (x >> 4)) & 0x00FF00FF00FF00FFul;
            y = (y | (y >> 4)) & 0x00FF00FF00FF00FFul;

            x = (x | (x >> 8)) & 0x0000FFFF0000FFFFul;
            y = (y | (y >> 8)) & 0x0000FFFF0000FFFFul;

            x = (x | (x >> 16)) & 0x00000000FFFFFFFFul;
            y = (y | (y >> 16)) & 0x00000000FFFFFFFFul;

            return x | (y << 32);
        }

        public static GeoHashNeighbors GetNeighbors(UInt64 hash, byte step)
        {
            GeoHashNeighbors neighbors;
            neighbors.n0 = GeohashMoveY(hash, 1, step);
            neighbors.n1 = GeohashMoveX(neighbors.n0, 1, step);
            neighbors.n2 = GeohashMoveX(hash, 1, step);
            neighbors.n3 = GeohashMoveY(neighbors.n2, -1, step);
            neighbors.n4 = GeohashMoveY(hash, -1, step);
            neighbors.n5 = GeohashMoveX(neighbors.n4, -1, step);
            neighbors.n6 = GeohashMoveX(hash, -1, step);
            neighbors.n7 = GeohashMoveY(neighbors.n6, 1, step);
            neighbors.center = hash;
            return neighbors;
        }

        public static GeoHashInterval[] GetIntervals(GeoHashNeighbors neighbors)
        {
            GeoHashInterval[] intervals = new GeoHashInterval[9];
            UInt64[] ns = new UInt64[9];
            ns[0] = neighbors.n0; ns[1] = neighbors.n1; ns[2] = neighbors.n2; ns[3] = neighbors.n3;
            ns[4] = neighbors.n4; ns[5] = neighbors.n5; ns[6] = neighbors.n6; ns[7] = neighbors.n7;
            ns[8] = neighbors.center;
            Array.Sort(ns);

            UInt64 lo = ns[0];
            UInt64 hi = lo + 1;
            for (int z = 1; z < ns.Length; z++)
            {
                if (ns[z] == hi)
                {
                    // next interval
                    hi++;
                }
                else
                {
                    // add interval [lo, hi]
                    intervals[z].lo = lo;
                    intervals[z].hi = hi;

                    lo = ns[z];
                    hi = lo + 1;
                }
            }
            // add interval [lo, hi]
            intervals[0].lo = lo;
            intervals[0].hi = hi;

            return intervals;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 GeohashMoveX(UInt64 hash, short d, byte step)
        {
            if (d == 0) return hash;

            UInt64 x = hash & 0xaaaaaaaaaaaaaaaaul;
            UInt64 y = hash & 0x5555555555555555ul;

            UInt64 zz = 0x5555555555555555ul >> (64 - step * 2);
            if (d > 0)
            {
                x = x + (zz + 1);
            }
            else
            {
                x = x | zz;
                x = x - (zz + 1);
            }
            x &= (0xaaaaaaaaaaaaaaaaul >> (64 - step * 2));
            return x | y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UInt64 GeohashMoveY(UInt64 hash, short d, byte step)
        {
            if (d == 0) return hash;

            UInt64 x = hash & 0xaaaaaaaaaaaaaaaaul;
            UInt64 y = hash & 0x5555555555555555ul;

            UInt64 zz = 0xaaaaaaaaaaaaaaaaul >> (64 - step * 2);
            if (d > 0)
            {
                y = y + (zz + 1);
            }
            else
            {
                y = y | zz;
                y = y - (zz + 1);
            }
            y &= (0x5555555555555555ul >> (64 - step * 2));
            return x | y;
        }
    }
}
