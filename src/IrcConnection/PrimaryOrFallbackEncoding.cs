/*
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2013 Ondřej Hošek <ondra.hosek@gmail.com>
 *
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Text;

namespace Meebey.SmartIrc4net
{
    internal class PrimaryOrFallbackEncoding : Encoding
    {
        public Encoding PrimaryEncoding { get; private set; }
        public Encoding FallbackEncoding { get; private set; }

        public PrimaryOrFallbackEncoding(Encoding primary, Encoding fallback)
        {
            try {
                PrimaryEncoding = Encoding.GetEncoding(primary.WebName, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            } catch (ArgumentException) {
                // probably not a standard encoding; check if it's throw-exception
                if (!(primary.EncoderFallback is EncoderExceptionFallback)) {
                    throw new System.ArgumentException("a custom primary encoding's encoder fallback must be an EncoderExceptionFallback");
                }
                if (!(primary.DecoderFallback is DecoderExceptionFallback)) {
                    throw new System.ArgumentException("a custom primary encoding's decoder fallback must be a DecoderExceptionFallback");
                }
            }

            FallbackEncoding = fallback;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            try {
                return PrimaryEncoding.GetByteCount(chars, index, count);
            } catch (EncoderFallbackException) {
                return FallbackEncoding.GetByteCount(chars, index, count);
            }
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            try {
                return PrimaryEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            } catch (EncoderFallbackException) {
                return FallbackEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            try {
                return PrimaryEncoding.GetCharCount(bytes, index, count);
            } catch (DecoderFallbackException) {
                return FallbackEncoding.GetCharCount(bytes, index, count);
            }
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            try {
                return PrimaryEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            } catch (DecoderFallbackException) {
                return FallbackEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            }
        }

        public override int GetMaxByteCount(int charCount)
        {
            try {
                int pri = PrimaryEncoding.GetMaxByteCount(charCount);
                int fab = FallbackEncoding.GetMaxByteCount(charCount);
                return Math.Max(pri, fab);
            } catch (EncoderFallbackException) {
                return FallbackEncoding.GetMaxByteCount(charCount);
            }
        }

        public override int GetMaxCharCount(int byteCount)
        {
            try {
                int pri = PrimaryEncoding.GetMaxCharCount(byteCount);
                int fab = FallbackEncoding.GetMaxCharCount(byteCount);
                return Math.Max(pri, fab);
            } catch (DecoderFallbackException) {
                return FallbackEncoding.GetMaxCharCount(byteCount);
            }
        }

        public override byte[] GetPreamble()
        {
            return PrimaryEncoding.GetPreamble();
        }
    }
}

