using System;
using System.Collections.Generic;
using ProtoBuf;
using System.Reflection;
using Harmony;

public class ProtobufSerializerPatcher
{
	public static bool Prefix_Serialize(ProtobufSerializerPrecompiled __instance, int num, object obj, ProtoWriter writer)
	{
		AlternativeSerializer.Serialize(num, obj, writer, __instance, out bool shouldnotcontinue);
		return !shouldnotcontinue;
	}

	public static bool Prefix_Deserialize(ProtobufSerializerPrecompiled __instance, int num, object obj, ProtoReader reader, ref object __result)
	{
		__result = AlternativeSerializer.Deserialize(num, obj, reader, __instance, out bool shouldnotcontinue);
		return !shouldnotcontinue;
	}

	public static bool Prefix_GetKeyImpl(Type key, ref int __result)
	{
		if (AlternativeSerializer.types.TryGetValue(key, out int num))
		{
			__result = num;
			return false;
		}
		return true;
	}

	public static void Patch(HarmonyInstance instance)
	{
		Type type = typeof(ProtobufSerializerPrecompiled);
		instance.Patch(type.GetMethod("Serialize", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(ProtobufSerializerPatcher).GetMethod("Prefix_Serialize")), null);
		instance.Patch(type.GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(ProtobufSerializerPatcher).GetMethod("Prefix_Deserialize")), null);
		instance.Patch(type.GetMethod("GetKeyImpl", BindingFlags.NonPublic | BindingFlags.Instance), new HarmonyMethod(typeof(ProtobufSerializerPatcher).GetMethod("Prefix_GetKeyImpl")), null);
	}
}

public static class AlternativeSerializer
{
	public static readonly Dictionary<int, ICustomSerializer> customSerializers = new Dictionary<int, ICustomSerializer>();
	public static readonly Dictionary<Type, int> types = new Dictionary<Type, int>();
	static readonly Dictionary<Type, int> existingTypes;
	static MethodInfo SerializeFunc;
	static MethodInfo DeserializeFunc;
	static MethodInfo GetKeyFunc;

	static AlternativeSerializer()
	{
		GetKeyFunc = typeof(ProtobufSerializerPrecompiled).GetMethod("GetKeyImpl", BindingFlags.NonPublic | BindingFlags.Instance);
		SerializeFunc = typeof(ProtobufSerializerPrecompiled).GetMethod("Serialize", BindingFlags.NonPublic | BindingFlags.Instance);
		DeserializeFunc = typeof(ProtobufSerializerPrecompiled).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance);
		existingTypes = typeof(ProtobufSerializerPrecompiled).GetField("knownTypes", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as Dictionary<Type, int>;
	}

	public static void PrecompSerialize(ProtobufSerializerPrecompiled model, int num, object obj, ProtoWriter writer)
	{
		SerializeFunc.Invoke(model, new object[] { num, obj, writer });
	}

	public static int PrecompGetKey(ProtobufSerializerPrecompiled model, Type type)
	{
		return (int)GetKeyFunc.Invoke(model, new object[] { type });
	}

	public static object PrecompDeserialize(ProtobufSerializerPrecompiled model, int num, object obj, ProtoReader reader)
	{
		return DeserializeFunc.Invoke(model, new object[] { num, obj, reader });
	}

	public static void RegisterCustomSerializer(int num, Type type, ICustomSerializer serializer)
	{
		if (!types.ContainsKey(type))
		{
			types.Add(type, num);
			customSerializers.Add(num, serializer);
		}
	}

	public static void RegisterCustomSerializer<T>(int num, ICustomSerializer serializer)
	{
		RegisterCustomSerializer(num, typeof(T), serializer);
	}

	public static void Serialize(int num, object obj, ProtoWriter writer, ProtobufSerializerPrecompiled model, out bool exists)
	{
		if (!customSerializers.TryGetValue(num, out ICustomSerializer s))
		{
			exists = false;
			return;
		}
		try
		{
			s.Serialize(obj, writer, model);
			exists = true;
			return;
		}
		catch { }
		exists = false;
	}

	public static object Deserialize(int num, object obj, ProtoReader reader, ProtobufSerializerPrecompiled model, out bool exists)
	{
		if (!customSerializers.TryGetValue(num, out ICustomSerializer s))
		{
			exists = false;
			return null;
		}
		try
		{
			exists = true;
			return s.Deserialize(obj, reader, model);
		}
		catch { }
		exists = true;
		return null;
	}
}

public interface ICustomSerializer
{
	object Deserialize(object obj, ProtoReader reader, ProtobufSerializerPrecompiled model);
	void Serialize(object obj, ProtoWriter writer, ProtobufSerializerPrecompiled model);
}
