/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// File:	StringBuilderExtNumeric.cs
// Date:	9th March 2010
// Author:	Gavin Pugh
// Details:	Extension methods for the 'StringBuilder' standard .NET class, to allow garbage-free concatenation of
//			a selection of simple numeric types.
//
// Copyright (c) Gavin Pugh 2010 - Released under the zlib license: http://www.opensource.org/licenses/zlib-license.php
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Modifications by Adam Reed, to simplify api and add support for larger numeric types
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace Beamable.Extensions
{
    public static class StringBuilderExtensions
    {
        // Since A-Z don't sit next to 0-9 in the ascii table.
        private static readonly char[]	ms_digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

        //! Convert a given unsigned integer value to a string and concatenate onto the stringbuilder. Any base value allowed.
        public static StringBuilder Concat( this StringBuilder string_builder, ulong ulong_val)
        {
            // Calculate length of integer when written out
            int length = 0;
            ulong length_calc = ulong_val;

            do
            {
                length_calc /= 10;
                ++length;
            }
            while ( length_calc > 0 );

            // Pad out space for writing.
            string_builder.Length += length;
            int strpos = string_builder.Length;

            // We're writing backwards, one character at a time.
            while ( length > 0 )
            {
                strpos--;
                // Lookup from static char array, to cover hex values too
                string_builder[strpos] = ms_digits[ulong_val % 10];
                ulong_val /= 10;
                length--;
            }

            return string_builder;
        }

        // Unsigned Integer Types
        public static StringBuilder Concat( this StringBuilder string_builder, uint uint_val )
        {
            return Concat(string_builder, (ulong) uint_val);
        }

        public static StringBuilder Concat( this StringBuilder string_builder, ushort ushort_val )
        {
            return Concat(string_builder, (ulong) ushort_val);
        }

        public static StringBuilder Concat(this StringBuilder string_builder, byte byte_val)
        {
            return Concat(string_builder, (ulong) byte_val);
        }

        // Signed Integer Types
        public static StringBuilder Concat(this StringBuilder string_builder, long long_val)
        {
            if (long_val >= 0)
                return Concat(string_builder, (ulong) long_val);
            string_builder.Append("-");
            ulong ulong_val = ulong.MaxValue - ((ulong) long_val ) + 1; // This is to deal with Int64.MinValue
            return Concat(string_builder, ulong_val);
        }

        public static StringBuilder Concat( this StringBuilder string_builder, int int_val )
        {
            return Concat(string_builder, (long) int_val);
        }

        public static StringBuilder Concat( this StringBuilder string_builder, short short_val )
        {
            return Concat(string_builder, (long) short_val);
        }




        //! Convert a given float value to a string and concatenate onto the stringbuilder
        public static StringBuilder Concat(this StringBuilder string_builder, double double_val, uint decimal_places)
        {
            long long_part = (long) double_val;

            // First part is easy, just cast to an integer
            Concat(string_builder, long_part);

            if (decimal_places > 0)
            {
                // Decimal point
                string_builder.Append('.');

                // Work out remainder we need to print after the d.p.
                double remainder = System.Math.Abs(double_val - long_part);

                // Multiply up to become an int that we can print
                do
                {
                    remainder *= 10;
                    decimal_places--;
                } while (decimal_places > 0);

                // Round up. It's guaranteed to be a positive number, so no extra work required here.
                remainder += 0.5f;

                // All done, print that as an int!
                Concat(string_builder, (ulong) remainder);
            }
            return string_builder;
        }

        public static StringBuilder Concat(this StringBuilder string_builder, float float_value, uint decimal_places)
        {
            return Concat(string_builder, (double) float_value, decimal_places);
        }
    }
}
