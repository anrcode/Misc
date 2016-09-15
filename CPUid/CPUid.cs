﻿using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;


namespace Misc.CPUId
{
    public class CPUid
    {
        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public unsafe delegate void CPUID0Delegate(byte* buffer);

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public unsafe delegate Int32 CPUID1Delegate();

        [Flags()]
        public enum AllocationType : uint
        {
            COMMIT = 0x1000,
            RESERVE = 0x2000,
            RESET = 0x80000,
            LARGE_PAGES = 0x20000000,
            PHYSICAL = 0x400000,
            TOP_DOWN = 0x100000,
            WRITE_WATCH = 0x200000
        }

        [Flags()]
        public enum MemoryProtection : uint
        {
            EXECUTE = 0x10,
            EXECUTE_READ = 0x20,
            EXECUTE_READWRITE = 0x40,
            EXECUTE_WRITECOPY = 0x80,
            NOACCESS = 0x01,
            READONLY = 0x02,
            READWRITE = 0x04,
            WRITECOPY = 0x08,
            GUARD_Modifierflag = 0x100,
            NOCACHE_Modifierflag = 0x200,
            WRITECOMBINE_Modifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
             AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32")]
        private static extern bool VirtualFree(
                IntPtr lpAddress,
                UInt32 dwSize,
                UInt32 dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, UInt32 VolumeNameSize, ref UInt32 VolumeSerialNumber, ref UInt32 MaximumComponentLength, ref UInt32 FileSystemFlags, StringBuilder FileSystemNameBuffer, UInt32 FileSystemNameSize);

        public static uint GetVolumeSerial(string strDriveLetter)
        {
            uint serNum = 0;
            uint maxCompLen = 0;
            StringBuilder VolLabel = new StringBuilder(256); // Label
            UInt32 VolFlags = new UInt32();
            StringBuilder FSName = new StringBuilder(256); // File System Name
            strDriveLetter += ":\\"; // fix up the passed-in drive letter for the API call
            long Ret = GetVolumeInformation(strDriveLetter, VolLabel, (UInt32)VolLabel.Capacity, ref serNum, ref maxCompLen, ref VolFlags, FSName, (UInt32)FSName.Capacity);

            return serNum;
        }

        public unsafe static byte[] CPUID0()
        {
            byte[] buffer = new byte[12];

            // Get the FieldInfo for "_methodPtr" 
            Type delType = typeof(Delegate);
            FieldInfo _methodPtr = delType.GetField("_methodPtr", BindingFlags.NonPublic | BindingFlags.Instance);

            if (IntPtr.Size == 4)
            {
                IntPtr p = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)x86_CPUID0_INSNS.Length),
                    AllocationType.COMMIT | AllocationType.RESERVE,
                    MemoryProtection.EXECUTE_READWRITE);
                try
                {
                    Marshal.Copy(x86_CPUID0_INSNS, 0, p, x86_CPUID0_INSNS.Length);

                    CPUID0Delegate del = (CPUID0Delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(CPUID0Delegate));

                    fixed (byte* newBuffer = &buffer[0])
                    {
                        del(newBuffer);
                    }
                }
                finally
                {
                    VirtualFree(p, 0, 0x8000);
                }
            }
            else if (IntPtr.Size == 8)
            {
                IntPtr p = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)x64_CPUID0_INSNS.Length),
                    AllocationType.COMMIT | AllocationType.RESERVE,
                    MemoryProtection.EXECUTE_READWRITE);
                try
                {
                    Marshal.Copy(x64_CPUID0_INSNS, 0, p, x64_CPUID0_INSNS.Length);

                    CPUID0Delegate del = (CPUID0Delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(CPUID0Delegate));

                    fixed (byte* newBuffer = &buffer[0])
                    {
                        del(newBuffer);
                    }
                }
                finally
                {
                    VirtualFree(p, 0, 0x8000);
                }
            }
            else
            {
                throw new NotSupportedException("32 bit and 64 bit only");
            }

            return buffer;
        }

        public unsafe static byte[] CPUID1()
        {
            byte[] buffer = new byte[3];
            Int32 result;

            // Get the FieldInfo for "_methodPtr" 
            Type delType = typeof(Delegate);
            FieldInfo _methodPtr = delType.GetField("_methodPtr", BindingFlags.NonPublic | BindingFlags.Instance);

            if (IntPtr.Size == 4)
            {
                IntPtr p = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)x86_CPUID1_INSNS.Length),
                    AllocationType.COMMIT | AllocationType.RESERVE,
                    MemoryProtection.EXECUTE_READWRITE);
                try
                {
                    Marshal.Copy(x86_CPUID1_INSNS, 0, p, x86_CPUID1_INSNS.Length);

                    CPUID1Delegate del = (CPUID1Delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(CPUID1Delegate));

                    result = del();
                }
                finally
                {
                    VirtualFree(p, 0, 0x8000);
                }
            }
            else if (IntPtr.Size == 8)
            {
                IntPtr p = VirtualAlloc(IntPtr.Zero, new UIntPtr((uint)x64_CPUID1_INSNS.Length),
                    AllocationType.COMMIT | AllocationType.RESERVE,
                    MemoryProtection.EXECUTE_READWRITE);
                try
                {
                    Marshal.Copy(x64_CPUID1_INSNS, 0, p, x64_CPUID1_INSNS.Length);

                    CPUID1Delegate del = (CPUID1Delegate)Marshal.GetDelegateForFunctionPointer(p, typeof(CPUID1Delegate));

                    result = del();
                }
                finally
                {
                    VirtualFree(p, 0, 0x8000);
                }
            }
            else
            {
                throw new NotSupportedException("32 bit and 64 bit only");
            }

            return BitConverter.GetBytes(result).Take(3).Reverse().ToArray();
        }

        #region ASM
        public static byte[] x86_CPUID0_INSNS = new byte[] 
        { 
            0x53,                   // push   %ebx 
            0x31,0xc0,              // xor    %eax,%eax 
            0x0f,0xa2,              // cpuid 
            0x8b,0x44,0x24,0x08,    // mov    0x8(%esp),%eax 
            0x89,0x18,              // mov    %ebx,0x0(%eax) 
            0x89,0x50,0x04,         // mov    %edx,0x4(%eax) 
            0x89,0x48,0x08,         // mov    %ecx,0x8(%eax) 
            0x5b,                   // pop    %ebx 
            0xc3                    // ret 
        };

        public static byte[] x86_CPUID1_INSNS = new byte[] 
        { 
            0x53,                   // push   %ebx 
            0x31, 0xc0,             // xor    %eax,%eax 
            0x40,                   // inc    %eax 
            0x0f, 0xa2,             // cpuid 
            0x5b,                   // pop    %ebx 
            0xc3                    // ret 
        };

        public static byte[] x64_CPUID0_INSNS = new byte[] 
        { 
            0x49, 0x89, 0xd8,       // mov    %rbx,%r8 
            0x49, 0x89, 0xc9,       // mov    %rcx,%r9 
            0x48, 0x31, 0xc0,       // xor    %rax,%rax 
            0x0f, 0xa2,             // cpuid 
            0x4c, 0x89, 0xc8,       // mov    %r9,%rax 
            0x89, 0x18,             // mov    %ebx,0x0(%rax) 
            0x89, 0x50, 0x04,       // mov    %edx,0x4(%rax) 
            0x89, 0x48, 0x08,       // mov    %ecx,0x8(%rax) 
            0x4c, 0x89, 0xc3,       // mov    %r8,%rbx 
            0xc3                    // retq 
        };

        public static byte[] x64_CPUID1_INSNS = new byte[] 
        { 
            0x53,                   // push   %rbx 
            0x48,0x31,0xc0,         // xor    %rax,%rax 
            0x48,0xff,0xc0,         // inc    %rax 
            0x0f,0xa2,              // cpuid 
            0x5b,                   // pop    %rbx 
            0xc3                    // retq 
        };
        #endregion
    }
}
