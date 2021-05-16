using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using HarmonyLib;
using UnityEngine;

namespace RocketMan
{
    static class Platform
    {
        public static Func<IntPtr, IntPtr, IntPtr> mono_jit_info_table_find
        {
            get;
            private set;
        }

        public static Func<IntPtr, IntPtr> mono_jit_info_get_method
        {
            get;
            private set;
        }

        public static Func<IntPtr, IntPtr> mono_jit_info_get_code_start
        {
            get;
            private set;
        }

        public static Func<IntPtr, int> mono_jit_info_get_code_size
        {
            get;
            private set;
        }

        public static Func<IntPtr, int, IntPtr, string> mono_debug_print_stack_frame
        {
            get;
            private set;
        }

        public static Func<string, int> mini_parse_debug_option
        {
            get;
            private set;
        }

        public static long LmfPtr
        {
            get;
            private set;
        }

        public static IntPtr DomainPtr
        {
            get;
            private set;
        }

        public static bool Linux => Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer;

        public static bool Windows => Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer;

        public static bool OSX => Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;

        public static void EarlyInit()
        {
            if (Linux)
            {
                mono_jit_info_table_find = LinuxNative.mono_jit_info_table_find;
                mono_jit_info_get_method = LinuxNative.mono_jit_info_get_method;
                mono_jit_info_get_code_start = LinuxNative.mono_jit_info_get_code_start;
                mono_jit_info_get_code_size = LinuxNative.mono_jit_info_get_code_size;
                mono_debug_print_stack_frame = LinuxNative.mono_debug_print_stack_frame;
                mini_parse_debug_option = LinuxNative.mini_parse_debug_option;
                DomainPtr = LinuxNative.mono_domain_get();
            }
            else if (Windows)
            {
                mono_jit_info_table_find = WindowsNative.mono_jit_info_table_find;
                mono_jit_info_get_method = WindowsNative.mono_jit_info_get_method;
                mono_jit_info_get_code_start = WindowsNative.mono_jit_info_get_code_start;
                mono_jit_info_get_code_size = WindowsNative.mono_jit_info_get_code_size;
                mono_debug_print_stack_frame = WindowsNative.mono_debug_print_stack_frame;
                mini_parse_debug_option = WindowsNative.mini_parse_debug_option;
                DomainPtr = WindowsNative.mono_domain_get();
            }
            else if (OSX)
            {
                mono_jit_info_table_find = MacOSNative.mono_jit_info_table_find;
                mono_jit_info_get_method = MacOSNative.mono_jit_info_get_method;
                mono_jit_info_get_code_start = MacOSNative.mono_jit_info_get_code_start;
                mono_jit_info_get_code_size = MacOSNative.mono_jit_info_get_code_size;
                mono_debug_print_stack_frame = MacOSNative.mono_debug_print_stack_frame;
                mini_parse_debug_option = MacOSNative.mini_parse_debug_option;
                mini_parse_debug_option = MacOSNative.mini_parse_debug_option;
                DomainPtr = MacOSNative.mono_domain_get();
            }
        }

        public static void Init()
        {
            FieldInfo fieldInfo = AccessTools.Field(typeof(Thread), "internal_thread");
            FieldInfo fieldInfo2 = AccessTools.Field(fieldInfo.FieldType, "runtime_thread_info");
            long num = (long)(IntPtr)fieldInfo2.GetValue(fieldInfo.GetValue(Thread.CurrentThread));
            if (Linux)
            {
                LmfPtr = num + 1152 - 32;
            }
            else if (Windows)
            {
                LmfPtr = num + 1096 - 32;
            }
            else if (OSX)
            {
                LmfPtr = num + 1096 - 32;
            }
        }

        public static string MethodNameFromAddr(long addr)
        {
            if (OSX) // :(
            {
                return null;
            }

            IntPtr domainPtr = DomainPtr;
            IntPtr intPtr = mono_jit_info_table_find(domainPtr, (IntPtr)addr);
            if (intPtr == IntPtr.Zero)
            {
                return null;
            }
            IntPtr arg = mono_jit_info_get_method(intPtr);
            IntPtr value = mono_jit_info_get_code_start(intPtr);
            int num = mono_jit_info_get_code_size(intPtr);
            string text = mono_debug_print_stack_frame(arg, (int)(addr - (long)value), domainPtr);
            if (text == null || text.Length == 0)
            {
                return null;
            }
            return text;
        }

        static class WindowsNative
        {
            private const string WindowsMonoLib = "mono-2.0-bdwgc.dll";

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern int mini_parse_debug_option(string option);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_domain_get();

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_jit_info_table_find(IntPtr domain, IntPtr addr);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_jit_info_get_method(IntPtr ji);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_jit_info_get_code_start(IntPtr ji);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern int mono_jit_info_get_code_size(IntPtr ji);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern string mono_debug_print_stack_frame(IntPtr method, int nativeOffset, IntPtr domain);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_compile_method(IntPtr method);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_class_vtable(IntPtr domain, IntPtr klass);

            [DllImport("mono-2.0-bdwgc.dll")]
            public static extern IntPtr mono_class_from_mono_type(IntPtr type);

            public unsafe static bool CctorRan(Type t)
            {
                return ((byte*)(void*)mono_class_vtable(mono_domain_get(), mono_class_from_mono_type(t.TypeHandle.Value)))[45] != 0;
            }
        }

        static class LinuxNative
        {
            private const string LinuxMonoLib = "libmonobdwgc-2.0.so";

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern int mini_parse_debug_option(string option);

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern IntPtr mono_domain_get();

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern IntPtr mono_jit_info_table_find(IntPtr domain, IntPtr addr);

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern IntPtr mono_jit_info_get_method(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern IntPtr mono_jit_info_get_code_start(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern int mono_jit_info_get_code_size(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.so")]
            public static extern string mono_debug_print_stack_frame(IntPtr method, int nativeOffset, IntPtr domain);
        }

        static class MacOSNative
        {
            private const string MacOSMonoLib = "libmonobdwgc-2.0.dylib";

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern int mini_parse_debug_option(string option);

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern IntPtr mono_domain_get();

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern IntPtr mono_jit_info_table_find(IntPtr domain, IntPtr addr);

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern IntPtr mono_jit_info_get_method(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern IntPtr mono_jit_info_get_code_start(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern int mono_jit_info_get_code_size(IntPtr ji);

            [DllImport("libmonobdwgc-2.0.dylib")]
            public static extern string mono_debug_print_stack_frame(IntPtr method, int nativeOffset, IntPtr domain);
        }
    }
}
