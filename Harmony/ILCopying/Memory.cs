﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarmonyErdelf.ILCopying
{
    [Flags]
    public enum Protection
    {
#pragma warning disable IDE1006 // Benennungsstile
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
#pragma warning restore IDE1006 // Benennungsstile
    }

    public static class Memory
    {
        private static readonly HashSet<PlatformID> windowsPlatformIDSet = new HashSet<PlatformID>
        {
            PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE
        };

        [DllImport("kernel32.dll")]
 	    public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, Protection flNewProtect, out Protection lpflOldProtect);

        public static bool IsWindows => windowsPlatformIDSet.Contains(Environment.OSVersion.Platform);

        public static void UnprotectMemoryPage(long memory)
        {
            if (IsWindows)
                if (!VirtualProtect(new IntPtr(memory), new UIntPtr(1), Protection.PAGE_EXECUTE_READWRITE, out Protection _ignored))
                    throw new System.ComponentModel.Win32Exception();
        }

        public static long WriteJump(long memory, long destination)
		{
            UnprotectMemoryPage(memory);

			if (IntPtr.Size == sizeof(long))
			{
				memory = WriteBytes(memory, new byte[] { 0x48, 0xB8 });
				memory = WriteLong(memory, destination);
				memory = WriteBytes(memory, new byte[] { 0xFF, 0xE0 });
			}
			else
			{
				memory = WriteByte(memory, 0x68);
				memory = WriteInt(memory, (int)destination);
				memory = WriteByte(memory, 0xc3);
			}
			return memory;
		}

		private static RuntimeMethodHandle GetRuntimeMethodHandle(MethodBase method)
		{
			if (method is DynamicMethod)
			{
                BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

                // DynamicMethod actually generates its m_methodHandle on-the-fly and therefore
                // we should call GetMethodDescriptor to force it to be created.
                //
                MethodInfo m_GetMethodDescriptor = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", nonPublicInstance);
				if (m_GetMethodDescriptor != null)
					return (RuntimeMethodHandle)m_GetMethodDescriptor.Invoke(method, new object[0]);

                // .Net Core
                FieldInfo f_m_method = typeof(DynamicMethod).GetField("m_method", nonPublicInstance);
				if (f_m_method != null)
					return (RuntimeMethodHandle)f_m_method.GetValue(method);

                // Mono
                FieldInfo f_mhandle = typeof(DynamicMethod).GetField("mhandle", nonPublicInstance);
				return (RuntimeMethodHandle)f_mhandle.GetValue(method);
			}

			return method.MethodHandle;
		}

		public static long GetMethodStart(MethodBase method)
		{
            // Required in .NET Core so that the method is JITed and the method start does not change
            //
            RuntimeMethodHandle handle = GetRuntimeMethodHandle(method);
			RuntimeHelpers.PrepareMethod(handle);

			return handle.GetFunctionPointer().ToInt64();
		}

		public static unsafe long WriteByte(long memory, byte value)
		{
            byte* p = (byte*)memory;
			*p = value;
			return memory + sizeof(byte);
		}

		public static unsafe long WriteBytes(long memory, byte[] values)
		{
			foreach (byte value in values)
				memory = WriteByte(memory, value);
			return memory;
		}

		public static unsafe long WriteInt(long memory, int value)
		{
            int* p = (int*)memory;
			*p = value;
			return memory + sizeof(int);
		}

		public static unsafe long WriteLong(long memory, long value)
		{
            long* p = (long*)memory;
			*p = value;
			return memory + sizeof(long);
		}
	}
}