#define ICALL_TABLE_corlib 1

static int corlib_icall_indexes [] = {
186,
196,
197,
198,
199,
200,
201,
202,
203,
204,
207,
208,
305,
306,
307,
330,
331,
332,
348,
349,
350,
467,
468,
469,
472,
502,
503,
504,
505,
506,
510,
512,
514,
516,
522,
530,
531,
532,
533,
534,
535,
536,
537,
612,
619,
620,
688,
694,
697,
699,
704,
705,
707,
708,
712,
713,
715,
716,
719,
720,
721,
724,
726,
729,
731,
733,
742,
804,
806,
808,
818,
819,
820,
822,
828,
829,
830,
831,
832,
840,
841,
842,
846,
847,
849,
851,
1043,
1201,
1202,
7060,
7061,
7063,
7064,
7065,
7066,
7067,
7069,
7070,
7071,
7086,
7088,
7095,
7097,
7099,
7101,
7104,
7154,
7155,
7157,
7158,
7159,
7160,
7161,
7163,
7165,
8163,
8167,
8169,
8170,
8171,
8172,
8568,
8569,
8570,
8571,
8587,
8588,
8589,
8633,
8698,
8708,
8709,
8710,
8711,
8712,
8983,
8987,
8988,
9015,
9049,
9056,
9063,
9074,
9077,
9098,
9173,
9175,
9184,
9186,
9187,
9194,
9209,
9229,
9230,
9238,
9240,
9247,
9248,
9251,
9256,
9262,
9263,
9270,
9272,
9284,
9287,
9288,
9289,
9300,
9310,
9316,
9317,
9318,
9320,
9321,
9338,
9340,
9355,
9373,
9400,
9428,
9429,
9899,
9978,
9979,
10133,
10134,
10138,
10141,
10188,
10570,
10571,
10767,
10772,
10782,
11551,
11572,
11574,
11576,
};
void ves_icall_System_Array_InternalCreate (int,int,int,int,int);
int ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal (int);
int ves_icall_System_Array_IsValueOfElementTypeInternal (int,int);
int ves_icall_System_Array_CanChangePrimitive (int,int,int);
int ves_icall_System_Array_FastCopy (int,int,int,int,int);
int ves_icall_System_Array_GetLengthInternal_raw (int,int,int);
int ves_icall_System_Array_GetLowerBoundInternal_raw (int,int,int);
void ves_icall_System_Array_GetGenericValue_icall (int,int,int);
void ves_icall_System_Array_GetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_SetGenericValue_icall (int,int,int);
void ves_icall_System_Array_SetValueImpl_raw (int,int,int,int);
void ves_icall_System_Array_SetValueRelaxedImpl_raw (int,int,int,int);
void ves_icall_System_Runtime_RuntimeImports_ZeroMemory (int,int);
void ves_icall_System_Runtime_RuntimeImports_Memmove (int,int,int);
void ves_icall_System_Buffer_BulkMoveWithWriteBarrier (int,int,int,int);
int ves_icall_System_Delegate_AllocDelegateLike_internal_raw (int,int);
int ves_icall_System_Delegate_CreateDelegate_internal_raw (int,int,int,int,int);
int ves_icall_System_Delegate_GetVirtualMethod_internal_raw (int,int);
void ves_icall_System_Enum_GetEnumValuesAndNames_raw (int,int,int,int);
int ves_icall_System_Enum_InternalGetCorElementType (int);
void ves_icall_System_Enum_InternalGetUnderlyingType_raw (int,int,int);
int ves_icall_System_Environment_get_ProcessorCount ();
int ves_icall_System_Environment_get_TickCount ();
int64_t ves_icall_System_Environment_get_TickCount64 ();
void ves_icall_System_Environment_FailFast_raw (int,int,int,int);
int ves_icall_System_GC_GetCollectionCount (int);
int ves_icall_System_GC_GetMaxGeneration ();
void ves_icall_System_GC_register_ephemeron_array_raw (int,int);
int ves_icall_System_GC_get_ephemeron_tombstone_raw (int);
int64_t ves_icall_System_GC_GetTotalAllocatedBytes_raw (int,int);
void ves_icall_System_GC_SuppressFinalize_raw (int,int);
void ves_icall_System_GC_ReRegisterForFinalize_raw (int,int);
void ves_icall_System_GC_GetGCMemoryInfo (int,int,int,int,int,int);
int ves_icall_System_GC_AllocPinnedArray_raw (int,int,int);
int ves_icall_System_Object_MemberwiseClone_raw (int,int);
double ves_icall_System_Math_Ceiling (double);
double ves_icall_System_Math_Cos (double);
double ves_icall_System_Math_Floor (double);
double ves_icall_System_Math_Pow (double,double);
double ves_icall_System_Math_Sin (double);
double ves_icall_System_Math_Sqrt (double);
double ves_icall_System_Math_Tan (double);
double ves_icall_System_Math_ModF (double,int);
int ves_icall_RuntimeMethodHandle_GetFunctionPointer_raw (int,int);
void ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw (int,int,int);
void ves_icall_RuntimeMethodHandle_ReboxToNullable_raw (int,int,int,int);
int ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw (int,int,int);
void ves_icall_RuntimeType_make_array_type_raw (int,int,int,int);
void ves_icall_RuntimeType_make_byref_type_raw (int,int,int);
void ves_icall_RuntimeType_make_pointer_type_raw (int,int,int);
void ves_icall_RuntimeType_MakeGenericType_raw (int,int,int,int);
int ves_icall_RuntimeType_GetMethodsByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetPropertiesByName_native_raw (int,int,int,int,int);
int ves_icall_RuntimeType_GetConstructors_native_raw (int,int,int);
int ves_icall_System_RuntimeType_CreateInstanceInternal_raw (int,int);
void ves_icall_RuntimeType_GetDeclaringMethod_raw (int,int,int);
void ves_icall_System_RuntimeType_getFullName_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetGenericArgumentsInternal_raw (int,int,int,int);
int ves_icall_RuntimeType_GetGenericParameterPosition (int);
int ves_icall_RuntimeType_GetEvents_native_raw (int,int,int,int);
int ves_icall_RuntimeType_GetFields_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetInterfaces_raw (int,int,int);
int ves_icall_RuntimeType_GetNestedTypes_native_raw (int,int,int,int,int);
void ves_icall_RuntimeType_GetDeclaringType_raw (int,int,int);
void ves_icall_RuntimeType_GetName_raw (int,int,int);
void ves_icall_RuntimeType_GetNamespace_raw (int,int,int);
int ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetAttributes (int);
int ves_icall_RuntimeTypeHandle_GetMetadataToken_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_GetCorElementType (int);
int ves_icall_RuntimeTypeHandle_HasInstantiation (int);
int ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_HasReferences_raw (int,int);
int ves_icall_RuntimeTypeHandle_GetArrayRank_raw (int,int);
void ves_icall_RuntimeTypeHandle_GetAssembly_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetElementType_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetModule_raw (int,int,int);
void ves_icall_RuntimeTypeHandle_GetBaseType_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition (int);
int ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw (int,int);
int ves_icall_RuntimeTypeHandle_is_subclass_of_raw (int,int,int);
int ves_icall_RuntimeTypeHandle_IsByRefLike_raw (int,int);
void ves_icall_System_RuntimeTypeHandle_internal_from_name_raw (int,int,int,int,int,int);
int ves_icall_System_String_FastAllocateString_raw (int,int);
int ves_icall_System_Type_internal_from_handle_raw (int,int);
int ves_icall_System_ValueType_InternalGetHashCode_raw (int,int,int);
int ves_icall_System_ValueType_Equals_raw (int,int,int,int);
int ves_icall_System_Threading_Interlocked_CompareExchange_Int (int,int,int);
void ves_icall_System_Threading_Interlocked_CompareExchange_Object (int,int,int,int);
int ves_icall_System_Threading_Interlocked_Decrement_Int (int);
int ves_icall_System_Threading_Interlocked_Increment_Int (int);
int64_t ves_icall_System_Threading_Interlocked_Increment_Long (int);
int ves_icall_System_Threading_Interlocked_Exchange_Int (int,int);
void ves_icall_System_Threading_Interlocked_Exchange_Object (int,int,int);
int64_t ves_icall_System_Threading_Interlocked_CompareExchange_Long (int,int64_t,int64_t);
int64_t ves_icall_System_Threading_Interlocked_Exchange_Long (int,int64_t);
int ves_icall_System_Threading_Interlocked_Add_Int (int,int);
void ves_icall_System_Threading_Monitor_Monitor_Enter_raw (int,int);
void mono_monitor_exit_icall_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_raw (int,int);
void ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw (int,int);
int ves_icall_System_Threading_Monitor_Monitor_wait_raw (int,int,int,int);
void ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw (int,int,int,int,int);
int64_t ves_icall_System_Threading_Monitor_Monitor_get_lock_contention_count ();
void ves_icall_System_Threading_Thread_InitInternal_raw (int,int);
int ves_icall_System_Threading_Thread_GetCurrentThread ();
void ves_icall_System_Threading_InternalThread_Thread_free_internal_raw (int,int);
int ves_icall_System_Threading_Thread_GetState_raw (int,int);
void ves_icall_System_Threading_Thread_SetState_raw (int,int,int);
void ves_icall_System_Threading_Thread_ClrState_raw (int,int,int);
void ves_icall_System_Threading_Thread_SetName_icall_raw (int,int,int,int);
int ves_icall_System_Threading_Thread_YieldInternal ();
void ves_icall_System_Threading_Thread_SetPriority_raw (int,int,int);
void ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw (int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw (int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw (int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw (int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw (int);
int ves_icall_System_GCHandle_InternalAlloc_raw (int,int,int);
void ves_icall_System_GCHandle_InternalFree_raw (int,int);
int ves_icall_System_GCHandle_InternalGet_raw (int,int);
void ves_icall_System_GCHandle_InternalSet_raw (int,int,int);
int ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError ();
void ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError (int);
void ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw (int,int,int,int);
int ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw (int,int,int,int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw (int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw (int,int);
void ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw (int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw (int,int,int,int);
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack ();
int ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalBox_raw (int,int,int);
int ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw (int);
int ves_icall_System_Reflection_Assembly_InternalLoad_raw (int,int,int,int);
int ves_icall_System_Reflection_Assembly_InternalGetType_raw (int,int,int,int,int,int);
int ves_icall_System_Reflection_AssemblyName_GetNativeName (int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw (int,int,int,int);
int ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw (int,int);
int ves_icall_MonoCustomAttrs_IsDefinedInternal_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw (int,int);
int ves_icall_System_Reflection_LoaderAllocatorScout_Destroy (int);
void ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw (int,int,int,int);
int ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw (int,int,int,int,int);
void ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw (int,int,int);
void ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw (int,int,int,int,int,int,int);
void ves_icall_RuntimeEventInfo_get_event_info_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_ResolveType_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetParentType_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_GetFieldOffset_raw (int,int);
int ves_icall_RuntimeFieldInfo_GetValueInternal_raw (int,int,int);
int ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw (int,int);
int ves_icall_reflection_get_token_raw (int,int);
void ves_icall_get_method_info_raw (int,int,int);
int ves_icall_get_method_attributes (int);
int ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw (int,int,int);
int ves_icall_System_MonoMethodInfo_get_retval_marshal_raw (int,int);
int ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw (int,int,int,int);
int ves_icall_RuntimeMethodInfo_get_name_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_base_method_raw (int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
void ves_icall_RuntimeMethodInfo_GetPInvoke_raw (int,int,int,int,int);
int ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw (int,int,int);
int ves_icall_RuntimeMethodInfo_GetGenericArguments_raw (int,int);
int ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw (int,int);
int ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw (int,int);
void ves_icall_InvokeClassConstructor_raw (int,int);
int ves_icall_InternalInvoke_raw (int,int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw (int,int,int,int,int,int);
void ves_icall_RuntimePropertyInfo_get_property_info_raw (int,int,int,int);
int ves_icall_reflection_get_token_raw (int,int);
int ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw (int,int,int);
void ves_icall_DynamicMethod_create_dynamic_method_raw (int,int,int,int,int);
void ves_icall_AssemblyBuilder_basic_init_raw (int,int);
void ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw (int,int);
void ves_icall_ModuleBuilder_basic_init_raw (int,int);
void ves_icall_ModuleBuilder_set_wrappers_type_raw (int,int,int);
int ves_icall_ModuleBuilder_getToken_raw (int,int,int,int);
void ves_icall_ModuleBuilder_RegisterToken_raw (int,int,int,int);
int ves_icall_TypeBuilder_create_runtime_class_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw (int,int);
int ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw (int,int);
int ves_icall_System_Diagnostics_Debugger_IsAttached_internal ();
int ves_icall_System_Diagnostics_StackFrame_GetFrameInfo (int,int,int,int,int,int,int,int);
void ves_icall_System_Diagnostics_StackTrace_GetTrace (int,int,int,int);
int ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass (int);
void ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree (int);
int ves_icall_Mono_SafeStringMarshal_StringToUtf8 (int);
void ves_icall_Mono_SafeStringMarshal_GFree (int);
static void *corlib_icall_funcs [] = {
// token 186,
ves_icall_System_Array_InternalCreate,
// token 196,
ves_icall_System_Array_GetCorElementTypeOfElementTypeInternal,
// token 197,
ves_icall_System_Array_IsValueOfElementTypeInternal,
// token 198,
ves_icall_System_Array_CanChangePrimitive,
// token 199,
ves_icall_System_Array_FastCopy,
// token 200,
ves_icall_System_Array_GetLengthInternal_raw,
// token 201,
ves_icall_System_Array_GetLowerBoundInternal_raw,
// token 202,
ves_icall_System_Array_GetGenericValue_icall,
// token 203,
ves_icall_System_Array_GetValueImpl_raw,
// token 204,
ves_icall_System_Array_SetGenericValue_icall,
// token 207,
ves_icall_System_Array_SetValueImpl_raw,
// token 208,
ves_icall_System_Array_SetValueRelaxedImpl_raw,
// token 305,
ves_icall_System_Runtime_RuntimeImports_ZeroMemory,
// token 306,
ves_icall_System_Runtime_RuntimeImports_Memmove,
// token 307,
ves_icall_System_Buffer_BulkMoveWithWriteBarrier,
// token 330,
ves_icall_System_Delegate_AllocDelegateLike_internal_raw,
// token 331,
ves_icall_System_Delegate_CreateDelegate_internal_raw,
// token 332,
ves_icall_System_Delegate_GetVirtualMethod_internal_raw,
// token 348,
ves_icall_System_Enum_GetEnumValuesAndNames_raw,
// token 349,
ves_icall_System_Enum_InternalGetCorElementType,
// token 350,
ves_icall_System_Enum_InternalGetUnderlyingType_raw,
// token 467,
ves_icall_System_Environment_get_ProcessorCount,
// token 468,
ves_icall_System_Environment_get_TickCount,
// token 469,
ves_icall_System_Environment_get_TickCount64,
// token 472,
ves_icall_System_Environment_FailFast_raw,
// token 502,
ves_icall_System_GC_GetCollectionCount,
// token 503,
ves_icall_System_GC_GetMaxGeneration,
// token 504,
ves_icall_System_GC_register_ephemeron_array_raw,
// token 505,
ves_icall_System_GC_get_ephemeron_tombstone_raw,
// token 506,
ves_icall_System_GC_GetTotalAllocatedBytes_raw,
// token 510,
ves_icall_System_GC_SuppressFinalize_raw,
// token 512,
ves_icall_System_GC_ReRegisterForFinalize_raw,
// token 514,
ves_icall_System_GC_GetGCMemoryInfo,
// token 516,
ves_icall_System_GC_AllocPinnedArray_raw,
// token 522,
ves_icall_System_Object_MemberwiseClone_raw,
// token 530,
ves_icall_System_Math_Ceiling,
// token 531,
ves_icall_System_Math_Cos,
// token 532,
ves_icall_System_Math_Floor,
// token 533,
ves_icall_System_Math_Pow,
// token 534,
ves_icall_System_Math_Sin,
// token 535,
ves_icall_System_Math_Sqrt,
// token 536,
ves_icall_System_Math_Tan,
// token 537,
ves_icall_System_Math_ModF,
// token 612,
ves_icall_RuntimeMethodHandle_GetFunctionPointer_raw,
// token 619,
ves_icall_RuntimeMethodHandle_ReboxFromNullable_raw,
// token 620,
ves_icall_RuntimeMethodHandle_ReboxToNullable_raw,
// token 688,
ves_icall_RuntimeType_GetCorrespondingInflatedMethod_raw,
// token 694,
ves_icall_RuntimeType_make_array_type_raw,
// token 697,
ves_icall_RuntimeType_make_byref_type_raw,
// token 699,
ves_icall_RuntimeType_make_pointer_type_raw,
// token 704,
ves_icall_RuntimeType_MakeGenericType_raw,
// token 705,
ves_icall_RuntimeType_GetMethodsByName_native_raw,
// token 707,
ves_icall_RuntimeType_GetPropertiesByName_native_raw,
// token 708,
ves_icall_RuntimeType_GetConstructors_native_raw,
// token 712,
ves_icall_System_RuntimeType_CreateInstanceInternal_raw,
// token 713,
ves_icall_RuntimeType_GetDeclaringMethod_raw,
// token 715,
ves_icall_System_RuntimeType_getFullName_raw,
// token 716,
ves_icall_RuntimeType_GetGenericArgumentsInternal_raw,
// token 719,
ves_icall_RuntimeType_GetGenericParameterPosition,
// token 720,
ves_icall_RuntimeType_GetEvents_native_raw,
// token 721,
ves_icall_RuntimeType_GetFields_native_raw,
// token 724,
ves_icall_RuntimeType_GetInterfaces_raw,
// token 726,
ves_icall_RuntimeType_GetNestedTypes_native_raw,
// token 729,
ves_icall_RuntimeType_GetDeclaringType_raw,
// token 731,
ves_icall_RuntimeType_GetName_raw,
// token 733,
ves_icall_RuntimeType_GetNamespace_raw,
// token 742,
ves_icall_RuntimeType_FunctionPointerReturnAndParameterTypes_raw,
// token 804,
ves_icall_RuntimeTypeHandle_GetAttributes,
// token 806,
ves_icall_RuntimeTypeHandle_GetMetadataToken_raw,
// token 808,
ves_icall_RuntimeTypeHandle_GetGenericTypeDefinition_impl_raw,
// token 818,
ves_icall_RuntimeTypeHandle_GetCorElementType,
// token 819,
ves_icall_RuntimeTypeHandle_HasInstantiation,
// token 820,
ves_icall_RuntimeTypeHandle_IsInstanceOfType_raw,
// token 822,
ves_icall_RuntimeTypeHandle_HasReferences_raw,
// token 828,
ves_icall_RuntimeTypeHandle_GetArrayRank_raw,
// token 829,
ves_icall_RuntimeTypeHandle_GetAssembly_raw,
// token 830,
ves_icall_RuntimeTypeHandle_GetElementType_raw,
// token 831,
ves_icall_RuntimeTypeHandle_GetModule_raw,
// token 832,
ves_icall_RuntimeTypeHandle_GetBaseType_raw,
// token 840,
ves_icall_RuntimeTypeHandle_type_is_assignable_from_raw,
// token 841,
ves_icall_RuntimeTypeHandle_IsGenericTypeDefinition,
// token 842,
ves_icall_RuntimeTypeHandle_GetGenericParameterInfo_raw,
// token 846,
ves_icall_RuntimeTypeHandle_is_subclass_of_raw,
// token 847,
ves_icall_RuntimeTypeHandle_IsByRefLike_raw,
// token 849,
ves_icall_System_RuntimeTypeHandle_internal_from_name_raw,
// token 851,
ves_icall_System_String_FastAllocateString_raw,
// token 1043,
ves_icall_System_Type_internal_from_handle_raw,
// token 1201,
ves_icall_System_ValueType_InternalGetHashCode_raw,
// token 1202,
ves_icall_System_ValueType_Equals_raw,
// token 7060,
ves_icall_System_Threading_Interlocked_CompareExchange_Int,
// token 7061,
ves_icall_System_Threading_Interlocked_CompareExchange_Object,
// token 7063,
ves_icall_System_Threading_Interlocked_Decrement_Int,
// token 7064,
ves_icall_System_Threading_Interlocked_Increment_Int,
// token 7065,
ves_icall_System_Threading_Interlocked_Increment_Long,
// token 7066,
ves_icall_System_Threading_Interlocked_Exchange_Int,
// token 7067,
ves_icall_System_Threading_Interlocked_Exchange_Object,
// token 7069,
ves_icall_System_Threading_Interlocked_CompareExchange_Long,
// token 7070,
ves_icall_System_Threading_Interlocked_Exchange_Long,
// token 7071,
ves_icall_System_Threading_Interlocked_Add_Int,
// token 7086,
ves_icall_System_Threading_Monitor_Monitor_Enter_raw,
// token 7088,
mono_monitor_exit_icall_raw,
// token 7095,
ves_icall_System_Threading_Monitor_Monitor_pulse_raw,
// token 7097,
ves_icall_System_Threading_Monitor_Monitor_pulse_all_raw,
// token 7099,
ves_icall_System_Threading_Monitor_Monitor_wait_raw,
// token 7101,
ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var_raw,
// token 7104,
ves_icall_System_Threading_Monitor_Monitor_get_lock_contention_count,
// token 7154,
ves_icall_System_Threading_Thread_InitInternal_raw,
// token 7155,
ves_icall_System_Threading_Thread_GetCurrentThread,
// token 7157,
ves_icall_System_Threading_InternalThread_Thread_free_internal_raw,
// token 7158,
ves_icall_System_Threading_Thread_GetState_raw,
// token 7159,
ves_icall_System_Threading_Thread_SetState_raw,
// token 7160,
ves_icall_System_Threading_Thread_ClrState_raw,
// token 7161,
ves_icall_System_Threading_Thread_SetName_icall_raw,
// token 7163,
ves_icall_System_Threading_Thread_YieldInternal,
// token 7165,
ves_icall_System_Threading_Thread_SetPriority_raw,
// token 8163,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_PrepareForAssemblyLoadContextRelease_raw,
// token 8167,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_GetLoadContextForAssembly_raw,
// token 8169,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFile_raw,
// token 8170,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalInitializeNativeALC_raw,
// token 8171,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalLoadFromStream_raw,
// token 8172,
ves_icall_System_Runtime_Loader_AssemblyLoadContext_InternalGetLoadedAssemblies_raw,
// token 8568,
ves_icall_System_GCHandle_InternalAlloc_raw,
// token 8569,
ves_icall_System_GCHandle_InternalFree_raw,
// token 8570,
ves_icall_System_GCHandle_InternalGet_raw,
// token 8571,
ves_icall_System_GCHandle_InternalSet_raw,
// token 8587,
ves_icall_System_Runtime_InteropServices_Marshal_GetLastPInvokeError,
// token 8588,
ves_icall_System_Runtime_InteropServices_Marshal_SetLastPInvokeError,
// token 8589,
ves_icall_System_Runtime_InteropServices_Marshal_StructureToPtr_raw,
// token 8633,
ves_icall_System_Runtime_InteropServices_NativeLibrary_LoadByName_raw,
// token 8698,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalGetHashCode_raw,
// token 8708,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetUninitializedObjectInternal_raw,
// token 8709,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray_raw,
// token 8710,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_GetSpanDataFrom_raw,
// token 8711,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_SufficientExecutionStack,
// token 8712,
ves_icall_System_Runtime_CompilerServices_RuntimeHelpers_InternalBox_raw,
// token 8983,
ves_icall_System_Reflection_Assembly_GetEntryAssembly_raw,
// token 8987,
ves_icall_System_Reflection_Assembly_InternalLoad_raw,
// token 8988,
ves_icall_System_Reflection_Assembly_InternalGetType_raw,
// token 9015,
ves_icall_System_Reflection_AssemblyName_GetNativeName,
// token 9049,
ves_icall_MonoCustomAttrs_GetCustomAttributesInternal_raw,
// token 9056,
ves_icall_MonoCustomAttrs_GetCustomAttributesDataInternal_raw,
// token 9063,
ves_icall_MonoCustomAttrs_IsDefinedInternal_raw,
// token 9074,
ves_icall_System_Reflection_FieldInfo_internal_from_handle_type_raw,
// token 9077,
ves_icall_System_Reflection_FieldInfo_get_marshal_info_raw,
// token 9098,
ves_icall_System_Reflection_LoaderAllocatorScout_Destroy,
// token 9173,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceNames_raw,
// token 9175,
ves_icall_System_Reflection_RuntimeAssembly_GetExportedTypes_raw,
// token 9184,
ves_icall_System_Reflection_RuntimeAssembly_GetInfo_raw,
// token 9186,
ves_icall_System_Reflection_RuntimeAssembly_GetManifestResourceInternal_raw,
// token 9187,
ves_icall_System_Reflection_Assembly_GetManifestModuleInternal_raw,
// token 9194,
ves_icall_System_Reflection_RuntimeCustomAttributeData_ResolveArgumentsInternal_raw,
// token 9209,
ves_icall_RuntimeEventInfo_get_event_info_raw,
// token 9229,
ves_icall_reflection_get_token_raw,
// token 9230,
ves_icall_System_Reflection_EventInfo_internal_from_handle_type_raw,
// token 9238,
ves_icall_RuntimeFieldInfo_ResolveType_raw,
// token 9240,
ves_icall_RuntimeFieldInfo_GetParentType_raw,
// token 9247,
ves_icall_RuntimeFieldInfo_GetFieldOffset_raw,
// token 9248,
ves_icall_RuntimeFieldInfo_GetValueInternal_raw,
// token 9251,
ves_icall_RuntimeFieldInfo_GetRawConstantValue_raw,
// token 9256,
ves_icall_reflection_get_token_raw,
// token 9262,
ves_icall_get_method_info_raw,
// token 9263,
ves_icall_get_method_attributes,
// token 9270,
ves_icall_System_Reflection_MonoMethodInfo_get_parameter_info_raw,
// token 9272,
ves_icall_System_MonoMethodInfo_get_retval_marshal_raw,
// token 9284,
ves_icall_System_Reflection_RuntimeMethodInfo_GetMethodFromHandleInternalType_native_raw,
// token 9287,
ves_icall_RuntimeMethodInfo_get_name_raw,
// token 9288,
ves_icall_RuntimeMethodInfo_get_base_method_raw,
// token 9289,
ves_icall_reflection_get_token_raw,
// token 9300,
ves_icall_InternalInvoke_raw,
// token 9310,
ves_icall_RuntimeMethodInfo_GetPInvoke_raw,
// token 9316,
ves_icall_RuntimeMethodInfo_MakeGenericMethod_impl_raw,
// token 9317,
ves_icall_RuntimeMethodInfo_GetGenericArguments_raw,
// token 9318,
ves_icall_RuntimeMethodInfo_GetGenericMethodDefinition_raw,
// token 9320,
ves_icall_RuntimeMethodInfo_get_IsGenericMethodDefinition_raw,
// token 9321,
ves_icall_RuntimeMethodInfo_get_IsGenericMethod_raw,
// token 9338,
ves_icall_InvokeClassConstructor_raw,
// token 9340,
ves_icall_InternalInvoke_raw,
// token 9355,
ves_icall_reflection_get_token_raw,
// token 9373,
ves_icall_System_Reflection_RuntimeModule_ResolveMethodToken_raw,
// token 9400,
ves_icall_RuntimePropertyInfo_get_property_info_raw,
// token 9428,
ves_icall_reflection_get_token_raw,
// token 9429,
ves_icall_System_Reflection_RuntimePropertyInfo_internal_from_handle_type_raw,
// token 9899,
ves_icall_DynamicMethod_create_dynamic_method_raw,
// token 9978,
ves_icall_AssemblyBuilder_basic_init_raw,
// token 9979,
ves_icall_AssemblyBuilder_UpdateNativeCustomAttributes_raw,
// token 10133,
ves_icall_ModuleBuilder_basic_init_raw,
// token 10134,
ves_icall_ModuleBuilder_set_wrappers_type_raw,
// token 10138,
ves_icall_ModuleBuilder_getToken_raw,
// token 10141,
ves_icall_ModuleBuilder_RegisterToken_raw,
// token 10188,
ves_icall_TypeBuilder_create_runtime_class_raw,
// token 10570,
ves_icall_System_IO_Stream_HasOverriddenBeginEndRead_raw,
// token 10571,
ves_icall_System_IO_Stream_HasOverriddenBeginEndWrite_raw,
// token 10767,
ves_icall_System_Diagnostics_Debugger_IsAttached_internal,
// token 10772,
ves_icall_System_Diagnostics_StackFrame_GetFrameInfo,
// token 10782,
ves_icall_System_Diagnostics_StackTrace_GetTrace,
// token 11551,
ves_icall_Mono_RuntimeClassHandle_GetTypeFromClass,
// token 11572,
ves_icall_Mono_RuntimeGPtrArrayHandle_GPtrArrayFree,
// token 11574,
ves_icall_Mono_SafeStringMarshal_StringToUtf8,
// token 11576,
ves_icall_Mono_SafeStringMarshal_GFree,
};
static uint8_t corlib_icall_flags [] = {
0,
0,
0,
0,
0,
4,
4,
0,
4,
0,
4,
4,
0,
0,
0,
4,
4,
4,
4,
0,
4,
0,
0,
0,
4,
0,
0,
4,
4,
4,
4,
4,
0,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
0,
0,
0,
4,
4,
4,
4,
4,
4,
0,
4,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
4,
0,
0,
0,
0,
0,
0,
0,
};
