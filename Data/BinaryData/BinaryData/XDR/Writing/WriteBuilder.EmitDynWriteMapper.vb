﻿Imports System
Imports System.Reflection
Imports System.Reflection.Emit

Namespace Xdr
    Public NotInheritable Partial Class WriteBuilder
        Private Function EmitDynWriteMapper() As Type
            Dim typeBuilder = _modBuilder.DefineType("DynWriteMapper", TypeAttributes.NotPublic Or TypeAttributes.Class Or TypeAttributes.Sealed, GetType(WriteMapper))
            Dim fb_oneCacheType = DefineCacheField(typeBuilder, "_oneCacheType")
            Dim fb_fixCacheType = DefineCacheField(typeBuilder, "_fixCacheType")
            Dim fb_varCacheType = DefineCacheField(typeBuilder, "_varCacheType")
            Dim ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, New Type(-1) {})
            Dim ilCtor As ILGenerator = ctor.GetILGenerator()
            ilCtor.Emit(OpCodes.Ldarg_0)
            ilCtor.Emit(OpCodes.Call, GetType(WriteMapper).GetConstructor(BindingFlags.Instance Or BindingFlags.NonPublic, Nothing, New Type(-1) {}, Nothing))
            ilCtor.Emit(OpCodes.Ldarg_0)
            ilCtor.Emit(OpCodes.Ldftn, GetType(WriteMapper).GetMethod("AppendBuildRequest", BindingFlags.NonPublic Or BindingFlags.Instance))
            ilCtor.Emit(OpCodes.Newobj, GetType(Action(Of Type, OpaqueType)).GetConstructor(New Type() {GetType(Object), GetType(IntPtr)}))
            ilCtor.Emit(OpCodes.Stsfld, _buildBinderDescription.BuildRequest)
            EmitInitField(ilCtor, fb_oneCacheType, _oneCacheDescription.Result)
            EmitInitField(ilCtor, fb_fixCacheType, _fixCacheDescription.Result)
            EmitInitField(ilCtor, fb_varCacheType, _varCacheDescription.Result)

            ' run init
            ilCtor.Emit(OpCodes.Ldarg_0)
            ilCtor.Emit(OpCodes.Call, GetType(WriteMapper).GetMethod("Init", BindingFlags.Instance Or BindingFlags.NonPublic))
            ilCtor.Emit(OpCodes.Ret)
            EmitOverride_GetCacheType(typeBuilder, "GetOneCacheType", fb_oneCacheType)
            EmitOverride_GetCacheType(typeBuilder, "GetFixCacheType", fb_fixCacheType)
            EmitOverride_GetCacheType(typeBuilder, "GetVarCacheType", fb_varCacheType)
            Return typeBuilder.CreateType()
        End Function

        Private Shared Function DefineCacheField(typeBuilder As TypeBuilder, name As String) As FieldBuilder
            Return typeBuilder.DefineField(name, GetType(Type), FieldAttributes.Private Or FieldAttributes.InitOnly)
        End Function

        Private Shared Sub EmitInitField(il As ILGenerator, fb As FieldBuilder, type As Type)
            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Ldtoken, type)
            il.Emit(OpCodes.Call, GetType(Type).GetMethod("GetTypeFromHandle"))
            il.Emit(OpCodes.Stfld, fb)
        End Sub

        Private Sub EmitOverride_GetCacheType(typeBuilder As TypeBuilder, overrideName As String, fb_cacheType As FieldBuilder)
            Dim mb = typeBuilder.DefineMethod(overrideName, MethodAttributes.Family Or MethodAttributes.Virtual)
            mb.SetReturnType(GetType(Type))
            typeBuilder.DefineMethodOverride(mb, GetType(WriteMapper).GetMethod(overrideName, BindingFlags.NonPublic Or BindingFlags.Instance))
            Dim il As ILGenerator = mb.GetILGenerator()
            il.Emit(OpCodes.Ldarg_0)
            il.Emit(OpCodes.Ldfld, fb_cacheType)
            il.Emit(OpCodes.Ret)
        End Sub
    End Class
End Namespace
