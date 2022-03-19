﻿namespace Twilight.Engine.Memory
{
    using Twilight.Engine.Common;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// An interface for querying virtual memory.
    /// </summary>
    public interface IMemoryQueryer
    {
        /// <summary>
        /// Gets regions of memory allocated in the remote process based on provided parameters.
        /// </summary>
        /// <param name="requiredProtection">Protection flags required to be present.</param>
        /// <param name="excludedProtection">Protection flags that must not be present.</param>
        /// <param name="allowedTypes">Memory types that can be present.</param>
        /// <param name="startAddress">The start address of the query range.</param>
        /// <param name="endAddress">The end address of the query range.</param>
        /// <returns>A collection of pointers to virtual pages in the target process.</returns>
        IEnumerable<NormalizedRegion> GetVirtualPages(
            Process process,
            MemoryProtectionEnum requiredProtection,
            MemoryProtectionEnum excludedProtection,
            MemoryTypeEnum allowedTypes,
            UInt64 startAddress,
            UInt64 endAddress);

        /// <summary>
        /// Gets all virtual pages in the opened process.
        /// </summary>
        /// <returns>A collection of regions in the process.</returns>
        IEnumerable<NormalizedRegion> GetAllVirtualPages(Process process);

        /// <summary>
        /// Gets a value indicating whether an address is writable.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        bool IsAddressWritable(Process process, UInt64 address);

        /// <summary>
        /// Gets the maximum address possible in the target process.
        /// </summary>
        /// <returns>The maximum address possible in the target process.</returns>
        UInt64 GetMaximumAddress(Process process);

        /// <summary>
        /// Gets the maximum usermode address possible in the target process.
        /// </summary>
        /// <returns>The maximum usermode address possible in the target process.</returns>
        UInt64 GetMinUsermodeAddress(Process process);

        /// <summary>
        /// Gets the maximum usermode address possible in the target process.
        /// </summary>
        /// <returns>The maximum usermode address possible in the target process.</returns>
        UInt64 GetMaxUsermodeAddress(Process process);

        /// <summary>
        /// Gets all modules in the opened process.
        /// </summary>
        /// <returns>A collection of modules in the process.</returns>
        IEnumerable<NormalizedModule> GetModules(Process process);

        /// <summary>
        /// Gets the address of the stacks in the opened process.
        /// </summary>
        /// <returns>A pointer to the stacks of the opened process.</returns>
        IEnumerable<NormalizedRegion> GetStackAddresses(Process process);

        /// <summary>
        /// Gets the addresses of the heaps in the opened process.
        /// </summary>
        /// <returns>A collection of pointers to all heaps in the opened process.</returns>
        IEnumerable<NormalizedRegion> GetHeapAddresses(Process process);

        /// <summary>
        /// Converts an address to a module and an address offset.
        /// </summary>
        /// <param name="address">The original address.</param>
        /// <param name="moduleName">The module name containing this address, if there is one. Otherwise, empty string.</param>
        /// <returns>The module name and address offset. If not contained by a module, the original address is returned.</returns>
        UInt64 AddressToModule(Process process, UInt64 address, out String moduleName);

        /// <summary>
        /// Determines the base address of a module given a module name.
        /// </summary>
        /// <param name="identifier">The module identifier, or name.</param>
        /// <returns>The base address of the module.</returns>
        UInt64 ResolveModule(Process process, String identifier);

        /// <summary>
        /// Dtermines the real address of an emulator address.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="emulatorAddress"></param>
        /// <param name="emulatorType"></param>
        /// <returns></returns>
        UInt64 EmulatorAddressToRealAddress(Process process, UInt64 emulatorAddress, EmulatorType emulatorType);

        /// <summary>
        /// Dtermines the real address of an emulator address.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="realAddress"></param>
        /// <param name="emulatorType"></param>
        /// <returns></returns>
        UInt64 RealAddressToEmulatorAddress(Process process, UInt64 realAddress, EmulatorType emulatorType);

        /// <summary>
        /// Gets all virtual pages for the target emulator in the opened process.
        /// </summary>
        /// <returns>A collection of regions in the process.</returns>
        IEnumerable<NormalizedRegion> GetEmulatorVirtualPages(Process process, EmulatorType emulatorType);
    }
    //// End interface
}
//// End namespace