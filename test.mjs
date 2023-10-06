import fs from "fs";
import pascalcase from "pascalcase";

const funcs = [
  {
    MonoName: "g_free",
    Il2cppName: "g_free",
    ArgCount: 1,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_free",
    Il2cppName: "il2cpp_free",
    ArgCount: 1,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_get_root_domain",
    Il2cppName: "il2cpp_get_root_domain",
    ArgCount: 0,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_thread_attach",
    Il2cppName: "il2cpp_thread_attach",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_thread_detach",
    Il2cppName: "il2cpp_thread_detach",
    ArgCount: 1,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_thread_cleanup",
    ArgCount: 0,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_object_get_class",
    Il2cppName: "il2cpp_object_get_class",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_domain_foreach",
    Il2cppName: "il2cpp_domain_foreach",
    ArgCount: 2,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_domain_set",
    Il2cppName: "il2cpp_domain_set",
    ArgCount: 2,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_domain_get",
    Il2cppName: "il2cpp_domain_get",
    ArgCount: 0,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_assembly_foreach",
    Il2cppName: "il2cpp_assembly_foreach",
    ArgCount: 2,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_assembly_get_image",
    Il2cppName: "il2cpp_assembly_get_image",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_image_get_assembly",
    Il2cppName: "il2cpp_image_get_assembly",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_image_get_name",
    Il2cppName: "il2cpp_image_get_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_image_get_filename",
    Il2cppName: "il2cpp_image_get_filename",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_image_get_table_info",
    Il2cppName: "il2cpp_image_get_table_info",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_image_rva_map",
    Il2cppName: "il2cpp_image_rva_map",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_table_info_get_rows",
    Il2cppName: "il2cpp_table_info_get_rows",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_metadata_decode_row_col",
    Il2cppName: "il2cpp_metadata_decode_row_col",
    ArgCount: 3,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_metadata_string_heap",
    Il2cppName: "il2cpp_metadata_string_heap",
    ArgCount: 2,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_class_get",
    Il2cppName: "il2cpp_class_get",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_from_typeref",
    Il2cppName: "il2cpp_class_from_typeref",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_name_from_token",
    Il2cppName: "il2cpp_class_name_from_token",
    ArgCount: 2,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_class_from_name_case",
    Il2cppName: "il2cpp_class_from_name_case",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_from_name",
    Il2cppName: "il2cpp_class_from_name",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_name",
    Il2cppName: "il2cpp_class_get_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_class_get_namespace",
    Il2cppName: "il2cpp_class_get_namespace",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_class_get_methods",
    Il2cppName: "il2cpp_class_get_methods",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_method_from_name",
    Il2cppName: "il2cpp_class_get_method_from_name",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_fields",
    Il2cppName: "il2cpp_class_get_fields",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_parent",
    Il2cppName: "il2cpp_class_get_parent",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_image",
    Il2cppName: "il2cpp_class_get_image",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_is_generic",
    Il2cppName: "il2cpp_class_is_generic",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_class_vtable",
    Il2cppName: "il2cpp_class_vtable",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_from_mono_type",
    Il2cppName: "il2cpp_class_from_mono_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_element_class",
    Il2cppName: "il2cpp_class_get_element_class",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_instance_size",
    Il2cppName: "il2cpp_class_instance_size",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_class_num_fields",
    Il2cppName: "il2cpp_class_num_fields",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_class_num_methods",
    Il2cppName: "il2cpp_class_num_methods",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_field_get_name",
    Il2cppName: "il2cpp_field_get_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_field_get_type",
    Il2cppName: "il2cpp_field_get_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_field_get_parent",
    Il2cppName: "il2cpp_field_get_parent",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_field_get_offset",
    Il2cppName: "il2cpp_field_get_offset",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_field_get_flags",
    Il2cppName: "il2cpp_field_get_flags",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_type_get_name",
    Il2cppName: "il2cpp_type_get_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_type_get_type",
    Il2cppName: "il2cpp_type_get_type",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_type_get_name_full",
    Il2cppName: "il2cpp_type_get_name_full",
    ArgCount: 2,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_method_get_name",
    Il2cppName: "il2cpp_method_get_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_method_get_class",
    Il2cppName: "il2cpp_method_get_class",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_get_header",
    Il2cppName: "il2cpp_method_get_header",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_signature",
    Il2cppName: "il2cpp_method_signature",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_get_param_names",
    Il2cppName: "il2cpp_method_get_param_names",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_signature_get_desc",
    Il2cppName: "il2cpp_signature_get_desc",
    ArgCount: 2,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_signature_get_params",
    Il2cppName: "il2cpp_signature_get_params",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_signature_get_param_count",
    Il2cppName: "il2cpp_signature_get_param_count",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_signature_get_return_type",
    Il2cppName: "il2cpp_signature_get_return_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_compile_method",
    Il2cppName: "il2cpp_compile_method",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_free_method",
    Il2cppName: "il2cpp_free_method",
    ArgCount: 1,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_jit_info_table_find",
    Il2cppName: "il2cpp_jit_info_table_find",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_jit_info_get_method",
    Il2cppName: "il2cpp_jit_info_get_method",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_jit_info_get_code_start",
    Il2cppName: "il2cpp_jit_info_get_code_start",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_jit_info_get_code_size",
    Il2cppName: "il2cpp_jit_info_get_code_size",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_jit_exec",
    Il2cppName: "il2cpp_jit_exec",
    ArgCount: 4,
    ReturnType: "INT32",
  },
  {
    MonoName: "mono_method_header_get_code",
    Il2cppName: "il2cpp_method_header_get_code",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_disasm_code",
    Il2cppName: "il2cpp_disasm_code",
    ArgCount: 4,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_vtable_get_static_field_data",
    Il2cppName: "il2cpp_vtable_get_static_field_data",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_desc_new",
    Il2cppName: "il2cpp_method_desc_new",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_desc_from_method",
    Il2cppName: "il2cpp_method_desc_from_method",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_desc_free",
    Il2cppName: "il2cpp_method_desc_free",
    ArgCount: 1,
    ReturnType: "VOID",
  },
  {
    MonoName: "mono_string_new",
    Il2cppName: "il2cpp_string_new",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_string_to_utf8",
    Il2cppName: "il2cpp_string_to_utf8",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    MonoName: "mono_array_new",
    Il2cppName: "il2cpp_array_new",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_value_box",
    Il2cppName: "il2cpp_value_box",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_object_unbox",
    Il2cppName: "il2cpp_object_unbox",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_object_new",
    Il2cppName: "il2cpp_object_new",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_type",
    Il2cppName: "il2cpp_class_get_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_nesting_type",
    Il2cppName: "il2cpp_class_get_nesting_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_desc_search_in_image",
    Il2cppName: "il2cpp_method_desc_search_in_image",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_runtime_invoke",
    Il2cppName: "il2cpp_runtime_invoke",
    ArgCount: 4,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_runtime_object_init",
    Il2cppName: "il2cpp_runtime_object_init",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_assembly_name_new",
    Il2cppName: "il2cpp_assembly_name_new",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_assembly_loaded",
    Il2cppName: "il2cpp_assembly_loaded",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_assembly_open",
    Il2cppName: "il2cpp_assembly_open",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_image_open",
    Il2cppName: "il2cpp_image_open",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_field_static_get_value",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_field_static_set_value",
    ArgCount: 3,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_class_get_field_from_name",
    Il2cppName: "il2cpp_class_get_field_from_name",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    MonoName: "mono_method_get_flags",
    Il2cppName: "il2cpp_method_get_flags",
    ArgCount: 2,
    ReturnType: "UINT32",
  },
  {
    MonoName: "mono_type_get_class",
    Il2cppName: "il2cpp_type_get_class",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_field_static_get_value",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_field_static_set_value",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_domain_get_assemblies",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_image_get_class_count",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    Il2cppName: "il2cpp_image_get_class",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_type_get_assembly_qualified_name",
    ArgCount: 1,
    ReturnType: "CHAR_P",
  },
  {
    Il2cppName: "il2cpp_method_get_param_count",
    ArgCount: 1,
    ReturnType: "INT32",
  },
  {
    Il2cppName: "il2cpp_method_get_param_name",
    ArgCount: 2,
    ReturnType: "CHAR_P",
  },
  {
    Il2cppName: "il2cpp_method_get_param",
    ArgCount: 2,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_method_get_return_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_class_from_type",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
  {
    Il2cppName: "il2cpp_string_chars",
    ArgCount: 1,
    ReturnType: "VOID_P",
  },
];

const types = {
  VOID: "void",
  BOOL: "bool",
  CHAR: "char",
  UCHAR: "byte",
  INT16: "short",
  UINT16: "ushort",
  INT32: "int",
  UINT32: "uint",
  INT64: "long",
  UINT64: "ulong",
  FLOAT: "float",
  DOUBLE: "double",
  VOID_P: "IntPtr",
  CHAR_P: "IntPtr",
  CS_STRING: "string",
};

fs.writeFileSync(
  "out.cs",
  funcs
    .map(
      (func) => `
[UnityNativeFunc(${[
        func.MonoName ? `monoName: "${func.MonoName}"` : undefined,
        func.Il2cppName ? `il2cppName: "${func.Il2cppName}"` : undefined,
        func.ReturnType !== "VOID"
          ? `returnType: UnityType.${func.ReturnType}`
          : undefined,
      ]
        .filter((x) => x)
        .join(", ")})]
public ${types[func.ReturnType]} ${pascalcase(func.MonoName ?? func.Il2cppName)
        .replace(/^Mono/, "")
        .replace(/^Il2Cpp/, "")}(${[...Array(func.ArgCount)]
        .map((_, i) => `int ${String.fromCharCode(97 + i)}`)
        .join(", ")}) => ${
        func.ReturnType !== "VOID" ? `(${types[func.ReturnType]})` : ""
      }CallNative(MethodBase.GetCurrentMethod()${[...Array(func.ArgCount)]
        .map((_, i) => `, ${String.fromCharCode(97 + i)}`)
        .join("")});
`
    )
    .join("")
);
