﻿namespace Twilight.Engine.Memory.Windows
{
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.Extensions;
    using Twilight.Engine.Memory.Windows.Native;
    using Twilight.Engine.Processes;
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class for memory editing a remote process.
    /// </summary>
    internal class WindowsMemoryReader : IMemoryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsMemoryReader"/> class.
        /// </summary>
        /// <param name="process">The target process.</param>
        public WindowsMemoryReader()
        {
        }

        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <param name="dataType">Type of value being read.</param>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public Object Read(Process process, ScannableType dataType, UInt64 address, out Boolean success)
        {
            Object value;

            switch (dataType)
            {
                case ScannableType type when type == ScannableType.Byte:
                    value = this.Read<Byte>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.SByte:
                    value = this.Read<SByte>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.Int16:
                    value = this.Read<Int16>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.Int32:
                    value = this.Read<Int32>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.Int64:
                    value = this.Read<Int64>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.UInt16:
                    value = this.Read<UInt16>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.UInt32:
                    value = this.Read<UInt32>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.UInt64:
                    value = this.Read<UInt64>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.Single:
                    value = this.Read<Single>(process, address, out success);
                    break;
                case ScannableType type when type == ScannableType.Double:
                    value = this.Read<Double>(process, address, out success);
                    break;
                case ScannableType typeBE when typeBE == ScannableType.Int16BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<Int16>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.Int32BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<Int32>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.Int64BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<Int64>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.UInt16BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<UInt16>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.UInt32BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<UInt32>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.UInt64BE:
                    value = BinaryPrimitives.ReverseEndianness(this.Read<UInt64>(process, address, out success));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.SingleBE:
                    value = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(this.Read<Int32>(process, address, out success)));
                    break;
                case ScannableType typeBE when typeBE == ScannableType.DoubleBE:
                    value = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReverseEndianness(this.Read<Int64>(process, address, out success)));
                    break;
                case ByteArrayType type:
                    value = this.ReadBytes(process, address, type.Length, out success);
                    break;
                default:
                    value = "?";
                    success = false;
                    break;
            }

            if (!success)
            {
                value = "?";
            }

            return value;
        }

        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public T Read<T>(Process process, UInt64 address, out Boolean success)
        {
            Byte[] byteArray = this.ReadBytes(process, address, Conversions.SizeOf(typeof(T)), out success);

            if (!success)
            {
                return default(T);
            }

            return Conversions.BytesToObject<T>(byteArray);
        }

        /// <summary>
        /// Reads an array of bytes in the remote process.
        /// </summary>
        /// <param name="address">The address where the array is read.</param>
        /// <param name="count">The number of cells.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The array of bytes.</returns>
        public Byte[] ReadBytes(Process process, UInt64 address, Int32 count, out Boolean success)
        {
            // Allocate the buffer
            Byte[] buffer = new Byte[count];
            Int32 bytesRead;

            // Read the data from the target process
            success = NativeMethods.ReadProcessMemory(process == null ? IntPtr.Zero : process.Handle, address.ToIntPtr(), buffer, count, out bytesRead) && count == bytesRead;

            return buffer;
        }

        /// <summary>
        /// Reads a string with a specified encoding in the remote process.
        /// </summary>
        /// <param name="address">The address where the string is read.</param>
        /// <param name="encoding">The encoding used.</param>
        /// <param name="success">Whether or not the read succeeded</param>
        /// <param name="maxLength">[Optional] The number of maximum bytes to read. The string is automatically cropped at this end ('\0' char).</param>
        /// <returns>The string.</returns>
        public String ReadString(Process process, UInt64 address, Encoding encoding, out Boolean success, Int32 maxLength = 512)
        {
            // Read the string
            String data = encoding.GetString(this.ReadBytes(process, address, maxLength, out success));

            // Search the end of the string
            Int32 end = data.IndexOf('\0');

            // Crop the string with this end
            return data.Substring(0, end);
        }

        public UInt64 EvaluatePointer(Process process, UInt64 address, IEnumerable<int> offsets)
        {
            UInt64 finalAddress = address;

            if (!offsets.IsNullOrEmpty())
            {
                // Add and trace offsets
                foreach (Int32 offset in offsets.Take(offsets.Count() - 1))
                {
                    if (process.Is32Bit())
                    {
                        finalAddress = this.Read<UInt32>(process, finalAddress.Add(offset), out _);
                    }
                    else
                    {
                        finalAddress = this.Read<UInt64>(process, finalAddress, out _).Add(offset);
                    }
                }

                // The last offset is added, but not traced
                finalAddress = finalAddress.Add(offsets.Last());
            }

            return finalAddress;
        }
    }
    //// End class
}
//// End namespace