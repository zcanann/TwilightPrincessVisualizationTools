﻿namespace Twilight.Engine.Memory.Windows
{
    using Native;
    using Twilight.Engine.Common;
    using Twilight.Engine.Common.DataStructures;
    using Twilight.Engine.Common.Extensions;
    using Twilight.Engine.Common.Logging;
    using Twilight.Engine.Memory.Windows.PEB;
    using Twilight.Engine.Processes;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using static Native.Enumerations;
    using static Native.Structures;

    /// <summary>
    /// Class for memory editing a remote process.
    /// </summary>
    internal unsafe class WindowsMemoryQuery : IMemoryQueryer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsMemoryQuery"/> class.
        /// </summary>
        public WindowsMemoryQuery()
        {
            this.ModuleCache = new TtlCache<Int32, IList<NormalizedModule>>(TimeSpan.FromSeconds(10.0));
            this.DolphinRegionCache = new SingleItemTtlCache<IList<NormalizedRegion>>(TimeSpan.FromSeconds(10.0));
        }

        /// <summary>
        /// Gets or sets the module cache of process modules.
        /// </summary>
        private TtlCache<Int32, IList<NormalizedModule>> ModuleCache { get; set; }

        private SingleItemTtlCache<IList<NormalizedRegion>> DolphinRegionCache { get; set; }

        /// <summary>
        /// Gets the address of the stacks in the opened process.
        /// </summary>
        /// <returns>A pointer to the stacks of the opened process.</returns>
        public IEnumerable<NormalizedRegion> GetStackAddresses(Process process)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the address(es) of the heap in the target process.
        /// </summary>
        /// <returns>The heap addresses in the target process.</returns>
        public IEnumerable<NormalizedRegion> GetHeapAddresses(Process process)
        {
            ManagedPeb peb = new ManagedPeb(process == null ? IntPtr.Zero : process.Handle);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets regions of memory allocated in the remote process based on provided parameters.
        /// </summary>
        /// <param name="requiredProtection">Protection flags required to be present.</param>
        /// <param name="excludedProtection">Protection flags that must not be present.</param>
        /// <param name="allowedTypes">Memory types that can be present.</param>
        /// <param name="startAddress">The start address of the query range.</param>
        /// <param name="endAddress">The end address of the query range.</param>
        /// <returns>A collection of pointers to virtual pages in the target process.</returns>
        public IEnumerable<NormalizedRegion> GetVirtualPages(
            Process process,
            MemoryProtectionEnum requiredProtection,
            MemoryProtectionEnum excludedProtection,
            MemoryTypeEnum allowedTypes,
            UInt64 startAddress,
            UInt64 endAddress)
        {
            MemoryProtectionFlags requiredFlags = 0;
            MemoryProtectionFlags excludedFlags = 0;

            if ((requiredProtection & MemoryProtectionEnum.Write) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                requiredFlags |= MemoryProtectionFlags.ReadWrite;
            }

            if ((requiredProtection & MemoryProtectionEnum.Execute) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.Execute;
                requiredFlags |= MemoryProtectionFlags.ExecuteRead;
                requiredFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                requiredFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((requiredProtection & MemoryProtectionEnum.CopyOnWrite) != 0)
            {
                requiredFlags |= MemoryProtectionFlags.WriteCopy;
                requiredFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((excludedProtection & MemoryProtectionEnum.Write) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                excludedFlags |= MemoryProtectionFlags.ReadWrite;
            }

            if ((excludedProtection & MemoryProtectionEnum.Execute) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.Execute;
                excludedFlags |= MemoryProtectionFlags.ExecuteRead;
                excludedFlags |= MemoryProtectionFlags.ExecuteReadWrite;
                excludedFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            if ((excludedProtection & MemoryProtectionEnum.CopyOnWrite) != 0)
            {
                excludedFlags |= MemoryProtectionFlags.WriteCopy;
                excludedFlags |= MemoryProtectionFlags.ExecuteWriteCopy;
            }

            IEnumerable<MemoryBasicInformation64> memoryInfo = WindowsMemoryQuery.VirtualPages(process == null ? IntPtr.Zero : process.Handle, startAddress, endAddress, requiredFlags, excludedFlags, allowedTypes);

            IList<NormalizedRegion> regions = new List<NormalizedRegion>();

            foreach (MemoryBasicInformation64 next in memoryInfo)
            {
                regions.Add(new NormalizedRegion(next.BaseAddress.ToUInt64(), next.RegionSize.ToInt32()));
            }

            return regions;
        }

        /// <summary>
        /// Gets all virtual pages in the opened process.
        /// </summary>
        /// <returns>A collection of regions in the process.</returns>
        public IEnumerable<NormalizedRegion> GetAllVirtualPages(Process process)
        {
            MemoryTypeEnum flags = MemoryTypeEnum.None | MemoryTypeEnum.Private | MemoryTypeEnum.Image | MemoryTypeEnum.Mapped;

            return this.GetVirtualPages(process, 0, 0, flags, 0, this.GetMaximumAddress(process));
        }

        public bool IsAddressWritable(Process process, UInt64 address)
        {
            MemoryTypeEnum flags = MemoryTypeEnum.None | MemoryTypeEnum.Private | MemoryTypeEnum.Image | MemoryTypeEnum.Mapped;

            IEnumerable<MemoryBasicInformation64> memoryInfo = WindowsMemoryQuery.VirtualPages(process == null ? IntPtr.Zero : process.Handle, address, address + 1, 0, 0, flags);

            if (memoryInfo.Count() > 0)
            {
                return (memoryInfo.First().Protect & (MemoryProtectionFlags.ExecuteReadWrite | MemoryProtectionFlags.ReadWrite)) != 0;
            }

            return false;
        }


        /// <summary>
        /// Gets the maximum address possible in the target process.
        /// </summary>
        /// <returns>The maximum address possible in the target process.</returns>
        public UInt64 GetMaximumAddress(Process process)
        {
            if (IntPtr.Size == Conversions.SizeOf(ScannableType.Int32))
            {
                return unchecked(UInt32.MaxValue);
            }
            else if (IntPtr.Size == Conversions.SizeOf(ScannableType.Int64))
            {
                return unchecked(UInt64.MaxValue);
            }

            throw new Exception("Unable to determine maximum address");
        }

        /// <summary>
        /// Gets the minimum usermode address possible in the target process.
        /// </summary>
        /// <returns>The minimum usermode address possible in the target process.</returns>
        public UInt64 GetMinUsermodeAddress(Process process)
        {
            return UInt16.MaxValue;
        }

        /// <summary>
        /// Gets the maximum usermode address possible in the target process.
        /// </summary>
        /// <returns>The maximum usermode address possible in the target process.</returns>
        public UInt64 GetMaxUsermodeAddress(Process process)
        {
            if (process.Is32Bit())
            {
                return Int32.MaxValue;
            }
            else
            {
                return 0x7FFFFFFFFFF;
            }
        }

        /// <summary>
        /// Gets all modules in the opened process.
        /// </summary>
        /// <returns>A collection of modules in the process.</returns>
        public IEnumerable<NormalizedModule> GetModules(Process process)
        {
            // Query all modules in the target process
            IntPtr[] modulePointers = new IntPtr[0];
            Int32 bytesNeeded = 0;
            IList<NormalizedModule> modules = new List<NormalizedModule>();

            if (process == null)
            {
                return modules;
            }

            if (this.ModuleCache.Contains(process.Id) && this.ModuleCache.TryGetValue(process.Id, out modules))
            {
                return modules;
            }

            try
            {
                // Determine number of modules
                if (!NativeMethods.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    // Failure, return our current empty list
                    return modules;
                }

                Int32 totalNumberofModules = bytesNeeded / IntPtr.Size;
                modulePointers = new IntPtr[totalNumberofModules];

                if (NativeMethods.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (UInt32)Enumerations.ModuleFilter.ListModulesAll))
                {
                    for (Int32 index = 0; index < totalNumberofModules; index++)
                    {
                        StringBuilder moduleFilePath = new StringBuilder(1024);
                        NativeMethods.GetModuleFileNameEx(process.Handle, modulePointers[index], moduleFilePath, (UInt32)moduleFilePath.Capacity);

                        ModuleInformation moduleInformation = new ModuleInformation();
                        NativeMethods.GetModuleInformation(process.Handle, modulePointers[index], out moduleInformation, (UInt32)(IntPtr.Size * modulePointers.Length));

                        // Ignore modules in 64-bit address space for WoW64 processes
                        if (process.Is32Bit() && moduleInformation.ModuleBase.ToUInt64() > Int32.MaxValue)
                        {
                            continue;
                        }

                        // Convert to a normalized module and add it to our list
                        NormalizedModule module = new NormalizedModule(moduleFilePath.ToString(), moduleInformation.ModuleBase.ToUInt64(), (Int32)moduleInformation.SizeOfImage);
                        modules.Add(module);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to fetch modules from selected process", ex);
            }

            this.ModuleCache.Add(process.Id, modules);

            return modules;
        }

        /// <summary>
        /// Converts an address to a module and an address offset.
        /// </summary>
        /// <param name="address">The original address.</param>
        /// <param name="moduleName">The module name containing this address, if there is one. Otherwise, empty string.</param>
        /// <returns>The module name and address offset. If not contained by a module, the original address is returned.</returns>
        public UInt64 AddressToModule(Process process, UInt64 address, out String moduleName)
        {
            NormalizedModule containingModule = this.GetModules(process)
                .Select(module => module)
                .Where(module => module.ContainsAddress(address))
                .FirstOrDefault();

            moduleName = containingModule?.Name ?? String.Empty;

            return containingModule == null ? address : address - containingModule.BaseAddress;
        }

        /// <summary>
        /// Determines the base address of a module given a module name.
        /// </summary>
        /// <param name="identifier">The module identifier, or name.</param>
        /// <returns>The base address of the module.</returns>
        public UInt64 ResolveModule(Process process, String identifier)
        {
            UInt64 result = 0;

            identifier = identifier?.RemoveSuffixes(true, ".exe", ".dll");
            IEnumerable<NormalizedModule> modules = this.GetModules(process)
                ?.ToList()
                ?.Select(module => module)
                ?.Where(module => module.Name.RemoveSuffixes(true, ".exe", ".dll").Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (modules.Count() > 0)
            {
                result = modules.First().BaseAddress;
            }

            return result;
        }

        /// <summary>
        /// Retrieves information about a range of pages within the virtual address space of a specified process.
        /// </summary>
        /// <param name="processHandle">A handle to the process whose memory information is queried.</param>
        /// <param name="startAddress">A pointer to the starting address of the region of pages to be queried.</param>
        /// <param name="endAddress">A pointer to the ending address of the region of pages to be queried.</param>
        /// <returns>
        /// A collection of <see cref="MemoryBasicInformation64"/> structures containing info about all virtual pages in the target process.
        /// </returns>
        public static IEnumerable<MemoryBasicInformation64> QueryUnallocatedMemory(
            IntPtr processHandle,
            UInt64 startAddress,
            UInt64 endAddress)
        {
            if (startAddress >= endAddress)
            {
                yield return new MemoryBasicInformation64();
            }

            Boolean wrappedAround = false;
            Int32 queryResult;

            // Enumerate the memory pages
            do
            {
                // Allocate the structure to store information of memory
                MemoryBasicInformation64 memoryInfo = new MemoryBasicInformation64();

                if (!Environment.Is64BitProcess)
                {
                    // 32 Bit struct is not the same
                    MemoryBasicInformation32 memoryInfo32 = new MemoryBasicInformation32();

                    // Query the memory region (32 bit native method)
                    queryResult = NativeMethods.VirtualQueryEx(processHandle, startAddress.ToIntPtr(), out memoryInfo32, Marshal.SizeOf(memoryInfo32));

                    // Copy from the 32 bit struct to the 64 bit struct
                    memoryInfo.AllocationBase = memoryInfo32.AllocationBase;
                    memoryInfo.AllocationProtect = memoryInfo32.AllocationProtect;
                    memoryInfo.BaseAddress = memoryInfo32.BaseAddress;
                    memoryInfo.Protect = memoryInfo32.Protect;
                    memoryInfo.RegionSize = memoryInfo32.RegionSize;
                    memoryInfo.State = memoryInfo32.State;
                    memoryInfo.Type = memoryInfo32.Type;
                }
                else
                {
                    // Query the memory region (64 bit native method)
                    queryResult = NativeMethods.VirtualQueryEx(processHandle, startAddress.ToIntPtr(), out memoryInfo, Marshal.SizeOf(memoryInfo));
                }

                // Increment the starting address with the size of the page
                UInt64 previousFrom = startAddress;
                startAddress = startAddress.Add(memoryInfo.RegionSize);

                if (previousFrom > startAddress)
                {
                    wrappedAround = true;
                }

                if ((memoryInfo.State & MemoryStateFlags.Free) != 0)
                {
                    // Return the unallocated memory page
                    yield return memoryInfo;
                }
                else
                {
                    // Ignore actual memory
                    continue;
                }
            }
            while (startAddress < endAddress && queryResult != 0 && !wrappedAround);
        }

        /// <summary>
        /// Retrieves information about a range of pages within the virtual address space of a specified process.
        /// </summary>
        /// <param name="processHandle">A handle to the process whose memory information is queried.</param>
        /// <param name="startAddress">A pointer to the starting address of the region of pages to be queried.</param>
        /// <param name="endAddress">A pointer to the ending address of the region of pages to be queried.</param>
        /// <param name="requiredProtection">Protection flags required to be present.</param>
        /// <param name="excludedProtection">Protection flags that must not be present.</param>
        /// <param name="allowedTypes">Memory types that can be present.</param>
        /// <returns>
        /// A collection of <see cref="MemoryBasicInformation64"/> structures containing info about all virtual pages in the target process.
        /// </returns>
        public static IEnumerable<MemoryBasicInformation64> VirtualPages(
            IntPtr processHandle,
            UInt64 startAddress,
            UInt64 endAddress,
            MemoryProtectionFlags requiredProtection,
            MemoryProtectionFlags excludedProtection,
            MemoryTypeEnum allowedTypes)
        {
            if (startAddress >= endAddress)
            {
                yield return new MemoryBasicInformation64();
            }

            Boolean wrappedAround = false;
            Int32 queryResult;

            // Enumerate the memory pages
            do
            {
                // Allocate the structure to store information of memory
                MemoryBasicInformation64 memoryInfo = new MemoryBasicInformation64();

                if (!Environment.Is64BitProcess)
                {
                    // 32 Bit struct is not the same
                    MemoryBasicInformation32 memoryInfo32 = new MemoryBasicInformation32();

                    // Query the memory region (32 bit native method)
                    queryResult = NativeMethods.VirtualQueryEx(processHandle, startAddress.ToIntPtr(), out memoryInfo32, Marshal.SizeOf(memoryInfo32));

                    // Copy from the 32 bit struct to the 64 bit struct
                    memoryInfo.AllocationBase = memoryInfo32.AllocationBase;
                    memoryInfo.AllocationProtect = memoryInfo32.AllocationProtect;
                    memoryInfo.BaseAddress = memoryInfo32.BaseAddress;
                    memoryInfo.Protect = memoryInfo32.Protect;
                    memoryInfo.RegionSize = memoryInfo32.RegionSize;
                    memoryInfo.State = memoryInfo32.State;
                    memoryInfo.Type = memoryInfo32.Type;
                }
                else
                {
                    // Query the memory region (64 bit native method)
                    queryResult = NativeMethods.VirtualQueryEx(processHandle, startAddress.ToIntPtr(), out memoryInfo, Marshal.SizeOf(memoryInfo));
                }

                // Increment the starting address with the size of the page
                UInt64 previousFrom = startAddress;
                startAddress = startAddress.Add(memoryInfo.RegionSize);

                if (previousFrom > startAddress)
                {
                    wrappedAround = true;
                }

                // Ignore free memory. These are unallocated memory regions.
                if ((memoryInfo.State & MemoryStateFlags.Free) != 0)
                {
                    continue;
                }

                // At least one readable memory flag is required
                if ((memoryInfo.Protect & MemoryProtectionFlags.ReadOnly) == 0 && (memoryInfo.Protect & MemoryProtectionFlags.ExecuteRead) == 0 &&
                    (memoryInfo.Protect & MemoryProtectionFlags.ExecuteReadWrite) == 0 && (memoryInfo.Protect & MemoryProtectionFlags.ReadWrite) == 0)
                {
                    continue;
                }

                // Do not bother with this shit, this memory is not worth scanning
                if ((memoryInfo.Protect & MemoryProtectionFlags.ZeroAccess) != 0 || (memoryInfo.Protect & MemoryProtectionFlags.NoAccess) != 0 || (memoryInfo.Protect & MemoryProtectionFlags.Guard) != 0)
                {
                    continue;
                }

                // Enforce allowed types
                switch (memoryInfo.Type)
                {
                    case MemoryTypeFlags.None:
                        if ((allowedTypes & MemoryTypeEnum.None) == 0)
                        {
                            continue;
                        }

                        break;
                    case MemoryTypeFlags.Private:
                        if ((allowedTypes & MemoryTypeEnum.Private) == 0)
                        {
                            continue;
                        }

                        break;
                    case MemoryTypeFlags.Image:
                        if ((allowedTypes & MemoryTypeEnum.Image) == 0)
                        {
                            continue;
                        }

                        break;
                    case MemoryTypeFlags.Mapped:
                        if ((allowedTypes & MemoryTypeEnum.Mapped) == 0)
                        {
                            continue;
                        }

                        break;
                }

                // Ensure at least one required protection flag is set
                if (requiredProtection != 0 && (memoryInfo.Protect & requiredProtection) == 0)
                {
                    continue;
                }

                // Ensure no ignored protection flags are set
                if (excludedProtection != 0 && (memoryInfo.Protect & excludedProtection) != 0)
                {
                    continue;
                }

                // Return the memory page
                yield return memoryInfo;
            }
            while (startAddress < endAddress && queryResult != 0 && !wrappedAround);
        }

        /// <summary>
        /// Dtermines the real address of an emulator address.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="emulatorAddress"></param>
        /// <param name="emulatorType"></param>
        /// <returns></returns>
        public UInt64 EmulatorAddressToRealAddress(Process process, UInt64 emulatorAddress, EmulatorType emulatorType)
        {
            switch (emulatorType)
            {
                case EmulatorType.Dolphin:
                    const UInt64 MemoryBase = 0x80000000;
                    const UInt64 WiiMemoryBase = 0x90000000;

                    if (emulatorAddress < MemoryBase)
                    {
                        return 0;
                    }

                    bool isWiiExtendedMemory = emulatorAddress >= WiiMemoryBase;
                    UInt64 baseRelativeAddress = emulatorAddress - (isWiiExtendedMemory ? WiiMemoryBase : MemoryBase);
                    IEnumerable<NormalizedRegion> dolphinRegions = this.GetEmulatorVirtualPages(process, emulatorType).OrderByDescending(region => region.BaseAddress);

                    if (isWiiExtendedMemory && dolphinRegions.Count() >= 2)
                    {
                        return dolphinRegions.First().BaseAddress + baseRelativeAddress;
                    }

                    if (dolphinRegions.Count() >= 1)
                    {
                        return dolphinRegions.Last().BaseAddress + baseRelativeAddress;
                    }

                    break;
            }

            return 0;
        }

        /// <summary>
        /// Dtermines the real address of an emulator address.
        /// </summary>
        /// <param name="process"></param>
        /// <param name="realAddress"></param>
        /// <param name="emulatorType"></param>
        /// <returns></returns>
        public UInt64 RealAddressToEmulatorAddress(Process process, UInt64 realAddress, EmulatorType emulatorType)
        {
            switch (emulatorType)
            {
                case EmulatorType.Dolphin:
                    const UInt64 MemoryBase = 0x80000000;
                    const UInt64 WiiMemoryBase = 0x90000000;
                    IEnumerable<NormalizedRegion> dolphinRegions = this.GetEmulatorVirtualPages(process, emulatorType).OrderByDescending(region => region.BaseAddress);

                    if (dolphinRegions.Count() >= 2)
                    {
                        NormalizedRegion region = dolphinRegions.First();
                        if (realAddress >= region.BaseAddress)
                        {
                            return realAddress - region.BaseAddress + WiiMemoryBase;
                        }
                    }

                    if (dolphinRegions.Count() >= 1)
                    {
                        NormalizedRegion region = dolphinRegions.Last();
                        if (realAddress >= region.BaseAddress)
                        {
                            return realAddress - region.BaseAddress + MemoryBase;
                        }
                    }
                    break;
            }

            return 0;
        }

        /// <summary>
        /// Gets all virtual pages for the target emulator in the opened process.
        /// </summary>
        /// <returns>A collection of regions in the process.</returns>
        public IEnumerable<NormalizedRegion> GetEmulatorVirtualPages(Process process, EmulatorType emulatorType)
        {
            if (this.DolphinRegionCache.HasValue())
            {
                return this.DolphinRegionCache.GetValue();
            }

            IntPtr processHandle = process?.Handle ?? IntPtr.Zero;
            IList<NormalizedRegion> regions = new List<NormalizedRegion>();

            switch (emulatorType)
            {
                case EmulatorType.Dolphin:
                    IEnumerable<NormalizedRegion> mappedRegions = this.GetVirtualPages(process, 0, 0, MemoryTypeEnum.Mapped, 0, this.GetMaximumAddress(process));

                    foreach (NormalizedRegion region in mappedRegions)
                    {
                        // Dolphin stores main memory in a memory mapped region of this exact size.
                        if (region.RegionSize == 0x2000000 && this.IsRegionBackedByPhysicalMemory(processHandle, region))
                        {
                            // Check to see if there is a game id. This should weed out any false positives.
                            bool readSuccess = false;
                            Byte[] gameId = new WindowsMemoryReader().ReadBytes(process, region.BaseAddress, 6, out readSuccess);

                            if (readSuccess)
                            {
                                String gameIdStr = Encoding.ASCII.GetString(gameId);

                                if ((gameIdStr.StartsWith('G') || gameIdStr.StartsWith('R')) && gameIdStr.All(character => Char.IsLetterOrDigit(character)))
                                {
                                    // Oddly Dolphin seems to map multiple main memory regions into RAM. These are identical.
                                    // Changing values in one will change the other. This means that we can just take the first one we find.
                                    regions.Add(region);
                                    break;
                                }
                            }
                        }
                    }

                    bool mem2Found = false;
                    foreach (NormalizedRegion region in mappedRegions)
                    {
                        // Dolphin stores wii memory in a memory mapped region of this exact size.
                        if (region.RegionSize == 0x4000000 && IsRegionBackedByPhysicalMemory(processHandle, region))
                        {
                            regions.Add(region);
                            mem2Found = true;
                            break;
                        }
                    }

                    // Try private regions if mapped didn't contain mem2
                    if (!mem2Found)
                    {
                        IEnumerable<NormalizedRegion> privateRegions = this.GetVirtualPages(process, 0, 0, MemoryTypeEnum.Private, 0, this.GetMaximumAddress(process));

                        foreach (NormalizedRegion region in privateRegions)
                        {
                            // Dolphin stores wii memory in a memory mapped region of this exact size.
                            if (region.RegionSize == 0x4000000 && IsRegionBackedByPhysicalMemory(processHandle, region))
                            {
                                regions.Add(region);
                                mem2Found = true;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            if (regions.Count > 0)
            {
                this.DolphinRegionCache.Add(regions);
            }

            return regions;
        }

        private Boolean IsRegionBackedByPhysicalMemory(IntPtr processHandle, NormalizedRegion region)
        {
            // Taken from Dolphin Memory Engine, this checks that the region is backed by physical memory, which apparently is required.
            MemoryWorkingSetExInformation[] workingSetExInformation = new MemoryWorkingSetExInformation[1];

            workingSetExInformation[0].VirtualAddress = region.BaseAddress.ToIntPtr();

            if (NativeMethods.QueryWorkingSetEx(processHandle, workingSetExInformation, Marshal.SizeOf<MemoryWorkingSetExInformation>()))
            {
                if (workingSetExInformation[0].VirtualAttributes.Valid)
                {
                    return true;
                }
            }

            return false;
        }
    }
    //// End class
}
//// End namespace