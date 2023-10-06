using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Urmf
{
    public static class UnityNative
    {
        public enum UnityType
        {
            VOID = 0,
            BOOL = 1,
            CHAR = 2,
            UCHAR = 3,
            INT16 = 4,
            UINT16 = 5,
            INT32 = 6,
            UINT32 = 7,
            INT64 = 8,
            UINT64 = 9,
            FLOAT = 10,
            DOUBLE = 11,
            VOID_P = 12,
            CHAR_P = 13,
            CS_STRING = 14,
        }

        public class Funcs
        {
            public static List<UnityNativeFuncAttribute> Definitions =
                new List<UnityNativeFuncAttribute>();

            [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
            public class UnityNativeFuncAttribute : Attribute
            {
                public string MonoName { get; }
                public string Il2cppName { get; }
                public UnityType ReturnType { get; }
                public bool IsUserDataRequired { get; }
                public int Index { get; }

                public UnityNativeFuncAttribute(
                    string monoName = null,
                    string il2cppName = null,
                    UnityType returnType = UnityType.VOID,
                    bool isUserDataRequired = true
                )
                {
                    MonoName = monoName;
                    Il2cppName = il2cppName;
                    ReturnType = returnType;
                    IsUserDataRequired = isUserDataRequired;
                    Definitions.Add(this);
                }
            }

            public class Func
            {
                public UnityNativeFuncAttribute Definition;
                public bool IsIl2cpp;
                public string Name;
                public IntPtr Address;
            }

            public class UserData
            {
                public IntPtr Domain;
                public IntPtr Attach;
                public IntPtr Detach;
            }

            public bool IsIl2cpp;
            public UserData Data;

            private readonly Func[] All = Definitions
                .Select(def => new Func() { Definition = def })
                .ToArray();

            public object CallNative(MethodBase method, params int[] args)
            {
                var definition = method.GetCustomAttribute<UnityNativeFuncAttribute>();
                var func = All[definition.Index];

                // Allocate 0x20 on stack (required for call) + space for args after all 4
                // registers are used and align to 16
                var frameSize = 0x20 + Math.Max((args.Length - 4 + 1 >> 1) * 16, 0);

                var asm = new Asm.X86_64().Sub(Asm.X86_64.Register.RSP, frameSize);

                if (definition.IsUserDataRequired)
                {
                    if (Data == null)
                        throw new Exception("User data is required but has not been set");
                    var threadAddress = 0; // TODO
                    asm.Mov(Asm.X86_64.Register.RCX, Data.Domain.ToInt64())
                        .Mov(Asm.X86_64.Register.RAX, Data.Attach.ToInt64())
                        .Call(Asm.X86_64.Register.RAX)
                        .MovRaxToPtrR12(threadAddress);
                }

                asm.Mov(Asm.X86_64.Register.RCX, moduleHandle.ToInt64())
                    .Mov(Asm.X86_64.Register.RDX, nameAddress.ToInt64())
                    .Mov(Asm.X86_64.Register.RAX, addressGetter.ToInt64())
                    .Call(Asm.X86_64.Register.RAX)
                    .Mov(Asm.X86_64.Register.R12, returnAddress.ToInt64())
                    .MovRaxToPtrR12();

                asm.Add(Asm.X86_64.Register.RSP, frameSize).Ret();

                var bytes = asm.GetBytes();

                return null;
            }

            [UnityNativeFunc(monoName: "g_free", il2cppName: "g_free")]
            public void GFree(int a) => CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(monoName: "mono_free", il2cppName: "il2cpp_free")]
            public void Free(int a) => CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_get_root_domain",
                il2cppName: "il2cpp_get_root_domain",
                returnType: UnityType.VOID_P
            )]
            public IntPtr GetRootDomain() => (IntPtr)CallNative(MethodBase.GetCurrentMethod());

            [UnityNativeFunc(
                monoName: "mono_thread_attach",
                il2cppName: "il2cpp_thread_attach",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ThreadAttach(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(monoName: "mono_thread_detach", il2cppName: "il2cpp_thread_detach")]
            public void ThreadDetach(int a) => CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(monoName: "mono_thread_cleanup")]
            public void ThreadCleanup() => CallNative(MethodBase.GetCurrentMethod());

            [UnityNativeFunc(
                monoName: "mono_object_get_class",
                il2cppName: "il2cpp_object_get_class",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ObjectGetClass(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(monoName: "mono_domain_foreach", il2cppName: "il2cpp_domain_foreach")]
            public void DomainForeach(int a, int b) =>
                CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_domain_set",
                il2cppName: "il2cpp_domain_set",
                returnType: UnityType.INT32
            )]
            public int DomainSet(int a, int b) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_domain_get",
                il2cppName: "il2cpp_domain_get",
                returnType: UnityType.VOID_P
            )]
            public IntPtr DomainGet() => (IntPtr)CallNative(MethodBase.GetCurrentMethod());

            [UnityNativeFunc(
                monoName: "mono_assembly_foreach",
                il2cppName: "il2cpp_assembly_foreach",
                returnType: UnityType.INT32
            )]
            public int AssemblyForeach(int a, int b) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_assembly_get_image",
                il2cppName: "il2cpp_assembly_get_image",
                returnType: UnityType.VOID_P
            )]
            public IntPtr AssemblyGetImage(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_image_get_assembly",
                il2cppName: "il2cpp_image_get_assembly",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ImageGetAssembly(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_image_get_name",
                il2cppName: "il2cpp_image_get_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr ImageGetName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_image_get_filename",
                il2cppName: "il2cpp_image_get_filename",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr ImageGetFilename(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_image_get_table_info",
                il2cppName: "il2cpp_image_get_table_info",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ImageGetTableInfo(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_image_rva_map",
                il2cppName: "il2cpp_image_rva_map",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ImageRvaMap(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_table_info_get_rows",
                il2cppName: "il2cpp_table_info_get_rows",
                returnType: UnityType.INT32
            )]
            public int TableInfoGetRows(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_metadata_decode_row_col",
                il2cppName: "il2cpp_metadata_decode_row_col",
                returnType: UnityType.INT32
            )]
            public int MetadataDecodeRowCol(int a, int b, int c) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_metadata_string_heap",
                il2cppName: "il2cpp_metadata_string_heap",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr MetadataStringHeap(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_get",
                il2cppName: "il2cpp_class_get",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGet(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_from_typeref",
                il2cppName: "il2cpp_class_from_typeref",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassFromTyperef(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_name_from_token",
                il2cppName: "il2cpp_class_name_from_token",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr ClassNameFromToken(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_from_name_case",
                il2cppName: "il2cpp_class_from_name_case",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassFromNameCase(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_class_from_name",
                il2cppName: "il2cpp_class_from_name",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassFromName(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_class_get_name",
                il2cppName: "il2cpp_class_get_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr ClassGetName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_get_namespace",
                il2cppName: "il2cpp_class_get_namespace",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr ClassGetNamespace(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_get_methods",
                il2cppName: "il2cpp_class_get_methods",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetMethods(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_get_method_from_name",
                il2cppName: "il2cpp_class_get_method_from_name",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetMethodFromName(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_class_get_fields",
                il2cppName: "il2cpp_class_get_fields",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetFields(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_get_parent",
                il2cppName: "il2cpp_class_get_parent",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetParent(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_get_image",
                il2cppName: "il2cpp_class_get_image",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetImage(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_is_generic",
                il2cppName: "il2cpp_class_is_generic",
                returnType: UnityType.INT32
            )]
            public int ClassIsGeneric(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_vtable",
                il2cppName: "il2cpp_class_vtable",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassVtable(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_from_mono_type",
                il2cppName: "il2cpp_class_from_mono_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassFromMonoType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_get_element_class",
                il2cppName: "il2cpp_class_get_element_class",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetElementClass(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_instance_size",
                il2cppName: "il2cpp_class_instance_size",
                returnType: UnityType.INT32
            )]
            public int ClassInstanceSize(int a) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_num_fields",
                il2cppName: "il2cpp_class_num_fields",
                returnType: UnityType.INT32
            )]
            public int ClassNumFields(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_num_methods",
                il2cppName: "il2cpp_class_num_methods",
                returnType: UnityType.INT32
            )]
            public int ClassNumMethods(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_field_get_name",
                il2cppName: "il2cpp_field_get_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr FieldGetName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_field_get_type",
                il2cppName: "il2cpp_field_get_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr FieldGetType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_field_get_parent",
                il2cppName: "il2cpp_field_get_parent",
                returnType: UnityType.VOID_P
            )]
            public IntPtr FieldGetParent(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_field_get_offset",
                il2cppName: "il2cpp_field_get_offset",
                returnType: UnityType.INT32
            )]
            public int FieldGetOffset(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_field_get_flags",
                il2cppName: "il2cpp_field_get_flags",
                returnType: UnityType.INT32
            )]
            public int FieldGetFlags(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_type_get_name",
                il2cppName: "il2cpp_type_get_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr TypeGetName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_type_get_type",
                il2cppName: "il2cpp_type_get_type",
                returnType: UnityType.INT32
            )]
            public int TypeGetType(int a) => (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_type_get_name_full",
                il2cppName: "il2cpp_type_get_name_full",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr TypeGetNameFull(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_method_get_name",
                il2cppName: "il2cpp_method_get_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr MethodGetName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_get_class",
                il2cppName: "il2cpp_method_get_class",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodGetClass(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_get_header",
                il2cppName: "il2cpp_method_get_header",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodGetHeader(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_signature",
                il2cppName: "il2cpp_method_signature",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodSignature(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_get_param_names",
                il2cppName: "il2cpp_method_get_param_names",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodGetParamNames(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_signature_get_desc",
                il2cppName: "il2cpp_signature_get_desc",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr SignatureGetDesc(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_signature_get_params",
                il2cppName: "il2cpp_signature_get_params",
                returnType: UnityType.VOID_P
            )]
            public IntPtr SignatureGetParams(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_signature_get_param_count",
                il2cppName: "il2cpp_signature_get_param_count",
                returnType: UnityType.INT32
            )]
            public int SignatureGetParamCount(int a) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_signature_get_return_type",
                il2cppName: "il2cpp_signature_get_return_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr SignatureGetReturnType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_compile_method",
                il2cppName: "il2cpp_compile_method",
                returnType: UnityType.VOID_P
            )]
            public IntPtr CompileMethod(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(monoName: "mono_free_method", il2cppName: "il2cpp_free_method")]
            public void FreeMethod(int a) => CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_jit_info_table_find",
                il2cppName: "il2cpp_jit_info_table_find",
                returnType: UnityType.VOID_P
            )]
            public IntPtr JitInfoTableFind(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_jit_info_get_method",
                il2cppName: "il2cpp_jit_info_get_method",
                returnType: UnityType.VOID_P
            )]
            public IntPtr JitInfoGetMethod(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_jit_info_get_code_start",
                il2cppName: "il2cpp_jit_info_get_code_start",
                returnType: UnityType.VOID_P
            )]
            public IntPtr JitInfoGetCodeStart(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_jit_info_get_code_size",
                il2cppName: "il2cpp_jit_info_get_code_size",
                returnType: UnityType.INT32
            )]
            public int JitInfoGetCodeSize(int a) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_jit_exec",
                il2cppName: "il2cpp_jit_exec",
                returnType: UnityType.INT32
            )]
            public int JitExec(int a, int b, int c, int d) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a, b, c, d);

            [UnityNativeFunc(
                monoName: "mono_method_header_get_code",
                il2cppName: "il2cpp_method_header_get_code",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodHeaderGetCode(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_disasm_code",
                il2cppName: "il2cpp_disasm_code",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr DisasmCode(int a, int b, int c, int d) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c, d);

            [UnityNativeFunc(
                monoName: "mono_vtable_get_static_field_data",
                il2cppName: "il2cpp_vtable_get_static_field_data",
                returnType: UnityType.VOID_P
            )]
            public IntPtr VtableGetStaticFieldData(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_desc_new",
                il2cppName: "il2cpp_method_desc_new",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodDescNew(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_method_desc_from_method",
                il2cppName: "il2cpp_method_desc_from_method",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodDescFromMethod(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_desc_free",
                il2cppName: "il2cpp_method_desc_free"
            )]
            public void MethodDescFree(int a) => CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_string_new",
                il2cppName: "il2cpp_string_new",
                returnType: UnityType.VOID_P
            )]
            public IntPtr StringNew(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_string_to_utf8",
                il2cppName: "il2cpp_string_to_utf8",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr StringToUtf8(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_array_new",
                il2cppName: "il2cpp_array_new",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ArrayNew(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_value_box",
                il2cppName: "il2cpp_value_box",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ValueBox(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_object_unbox",
                il2cppName: "il2cpp_object_unbox",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ObjectUnbox(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_object_new",
                il2cppName: "il2cpp_object_new",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ObjectNew(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_class_get_type",
                il2cppName: "il2cpp_class_get_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_class_get_nesting_type",
                il2cppName: "il2cpp_class_get_nesting_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetNestingType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_method_desc_search_in_image",
                il2cppName: "il2cpp_method_desc_search_in_image",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodDescSearchInImage(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_runtime_invoke",
                il2cppName: "il2cpp_runtime_invoke",
                returnType: UnityType.VOID_P
            )]
            public IntPtr RuntimeInvoke(int a, int b, int c, int d) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c, d);

            [UnityNativeFunc(
                monoName: "mono_runtime_object_init",
                il2cppName: "il2cpp_runtime_object_init",
                returnType: UnityType.VOID_P
            )]
            public IntPtr RuntimeObjectInit(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_assembly_name_new",
                il2cppName: "il2cpp_assembly_name_new",
                returnType: UnityType.VOID_P
            )]
            public IntPtr AssemblyNameNew(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_assembly_loaded",
                il2cppName: "il2cpp_assembly_loaded",
                returnType: UnityType.VOID_P
            )]
            public IntPtr AssemblyLoaded(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                monoName: "mono_assembly_open",
                il2cppName: "il2cpp_assembly_open",
                returnType: UnityType.VOID_P
            )]
            public IntPtr AssemblyOpen(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_image_open",
                il2cppName: "il2cpp_image_open",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ImageOpen(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(monoName: "mono_field_static_get_value", returnType: UnityType.VOID_P)]
            public IntPtr FieldStaticGetValue(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(monoName: "mono_field_static_set_value", returnType: UnityType.VOID_P)]
            public IntPtr FieldStaticSetValue(int a, int b, int c) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b, c);

            [UnityNativeFunc(
                monoName: "mono_class_get_field_from_name",
                il2cppName: "il2cpp_class_get_field_from_name",
                returnType: UnityType.VOID_P
            )]
            public IntPtr ClassGetFieldFromName(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_method_get_flags",
                il2cppName: "il2cpp_method_get_flags",
                returnType: UnityType.UINT32
            )]
            public uint MethodGetFlags(int a, int b) =>
                (uint)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                monoName: "mono_type_get_class",
                il2cppName: "il2cpp_type_get_class",
                returnType: UnityType.VOID_P
            )]
            public IntPtr TypeGetClass(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                il2cppName: "il2cpp_field_static_get_value",
                returnType: UnityType.VOID_P
            )]
            public IntPtr FieldStaticGetValue(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                il2cppName: "il2cpp_field_static_set_value",
                returnType: UnityType.VOID_P
            )]
            public IntPtr FieldStaticSetValue(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                il2cppName: "il2cpp_domain_get_assemblies",
                returnType: UnityType.VOID_P
            )]
            public IntPtr DomainGetAssemblies(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                il2cppName: "il2cpp_image_get_class_count",
                returnType: UnityType.INT32
            )]
            public int ImageGetClassCount(int a) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(il2cppName: "il2cpp_image_get_class", returnType: UnityType.VOID_P)]
            public IntPtr ImageGetClass(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                il2cppName: "il2cpp_type_get_assembly_qualified_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr TypeGetAssemblyQualifiedName(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                il2cppName: "il2cpp_method_get_param_count",
                returnType: UnityType.INT32
            )]
            public int MethodGetParamCount(int a) =>
                (int)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(
                il2cppName: "il2cpp_method_get_param_name",
                returnType: UnityType.CHAR_P
            )]
            public IntPtr MethodGetParamName(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(il2cppName: "il2cpp_method_get_param", returnType: UnityType.VOID_P)]
            public IntPtr MethodGetParam(int a, int b) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a, b);

            [UnityNativeFunc(
                il2cppName: "il2cpp_method_get_return_type",
                returnType: UnityType.VOID_P
            )]
            public IntPtr MethodGetReturnType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(il2cppName: "il2cpp_class_from_type", returnType: UnityType.VOID_P)]
            public IntPtr ClassFromType(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);

            [UnityNativeFunc(il2cppName: "il2cpp_string_chars", returnType: UnityType.VOID_P)]
            public IntPtr StringChars(int a) =>
                (IntPtr)CallNative(MethodBase.GetCurrentMethod(), a);
        }
    }
}
