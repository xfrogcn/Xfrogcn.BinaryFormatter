using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xfrogcn.BinaryFormatter.Serialization;

namespace Xfrogcn.BinaryFormatter
{
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    internal sealed partial class BinaryClassInfo
    {
        // The number of parameters the deserialization constructor has. If this is not equal to ParameterCache.Count, this means
        // that not all parameters are bound to object properties, and an exception will be thrown if deserialization is attempted.
        public int ParameterCount { get; internal set; }

        private const int PropertyNameKeyLength = 7;

        // The limit to how many constructor parameter names from the Binary Doc are cached in _parameterRefsSorted before using _parameterCache.
        private const int ParameterNameCountCacheThreshold = 32;

        // The limit to how many property names from the Binary Doc are cached in _propertyRefsSorted before using PropertyCache.
        private const int PropertyNameCountCacheThreshold = 64;

        // All of the serializable parameters on a POCO constructor keyed on parameter name.
        // Only paramaters which bind to properties are cached.
        public Dictionary<string, BinaryParameterInfo> ParameterCache;

        // All of the serializable properties on a POCO (except the optional extension property) keyed on property name.
        public Dictionary<string, BinaryPropertyInfo> PropertyCache;

        // All of the serializable properties on a POCO including the optional extension property.
        // Used for performance during serialization instead of 'PropertyCache' above.
        public BinaryPropertyInfo[] PropertyCacheArray;

        private volatile ParameterRef[] _parameterRefsSorted;

        private volatile PropertyRef[] _propertyRefsSorted;


        public BinaryMemberInfo[] GetMemberInfos()
        {
            if (PropertyCacheArray == null || PropertyCacheArray.Length == 0)
            {
                return new BinaryMemberInfo[0];
            }

            return PropertyCacheArray.Select(p => p.GetBinaryMemberInfo()).ToArray();
        }

        public static BinaryPropertyInfo AddProperty(
            TypeMap typeMap,
            MemberInfo memberInfo,
            Type memberType,
            Type parentClassType,
            BinarySerializerOptions options)
        {
            BinaryIgnoreCondition? ignoreCondition = BinaryPropertyInfo.GetAttribute<BinaryIgnoreAttribute>(memberInfo)?.Condition;
            if (ignoreCondition == BinaryIgnoreCondition.Always)
            {
                return BinaryPropertyInfo.CreateIgnoredPropertyPlaceholder(typeMap, memberInfo, options);
            }

            BinaryConverter converter = GetConverter(
                memberType,
                parentClassType,
                memberInfo,
                out Type runtimeType,
                options);

            return CreateProperty(
                typeMap,
                declaredPropertyType: memberType,
                runtimePropertyType: runtimeType,
                memberInfo,
                parentClassType,
                converter,
                options,
                ignoreCondition);
        }
        internal static BinaryPropertyInfo CreateProperty(
            TypeMap typeMap,
            Type declaredPropertyType,
            Type runtimePropertyType,
            MemberInfo memberInfo,
            Type parentClassType,
            BinaryConverter converter,
            BinarySerializerOptions options,
            BinaryIgnoreCondition? ignoreCondition = null)
        {
            // Create the BinaryPropertyInfo instance.
            BinaryPropertyInfo binaryPropertyInfo = converter.CreateBinaryPropertyInfo();

            binaryPropertyInfo.Initialize(
                typeMap,
                parentClassType,
                declaredPropertyType,
                runtimePropertyType,
                runtimeClassType: converter.ClassType,
                memberInfo,
                converter,
                ignoreCondition,
                options);

            return binaryPropertyInfo;
        }

        /// <summary>
        /// Get a key from the property name.
        /// The key consists of the first 7 bytes of the property name and then the length.
        /// </summary>
        // AggressiveInlining used since this method is only called from two locations and is on a hot path.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetKey(ReadOnlySpan<byte> name)
        {
            ulong key;

            ref byte reference = ref MemoryMarshal.GetReference(name);
            int length = name.Length;

            if (length > 7)
            {
                key = Unsafe.ReadUnaligned<ulong>(ref reference) & 0x00ffffffffffffffL;
                key |= (ulong)Math.Min(length, 0xff) << 56;
            }
            else
            {
                key =
                    length > 5 ? Unsafe.ReadUnaligned<uint>(ref reference) | (ulong)Unsafe.ReadUnaligned<ushort>(ref Unsafe.Add(ref reference, 4)) << 32 :
                    length > 3 ? Unsafe.ReadUnaligned<uint>(ref reference) :
                    length > 1 ? Unsafe.ReadUnaligned<ushort>(ref reference) : 0UL;
                key |= (ulong)length << 56;

                if ((length & 1) != 0)
                {
                    var offset = length - 1;
                    key |= (ulong)Unsafe.Add(ref reference, offset) << (offset * 8);
                }
            }

            // Verify key contains the embedded bytes as expected.
            const int BitsInByte = 8;
            Debug.Assert(
                // Verify embedded property name.
                (name.Length < 1 || name[0] == ((key & ((ulong)0xFF << BitsInByte * 0)) >> BitsInByte * 0)) &&
                (name.Length < 2 || name[1] == ((key & ((ulong)0xFF << BitsInByte * 1)) >> BitsInByte * 1)) &&
                (name.Length < 3 || name[2] == ((key & ((ulong)0xFF << BitsInByte * 2)) >> BitsInByte * 2)) &&
                (name.Length < 4 || name[3] == ((key & ((ulong)0xFF << BitsInByte * 3)) >> BitsInByte * 3)) &&
                (name.Length < 5 || name[4] == ((key & ((ulong)0xFF << BitsInByte * 4)) >> BitsInByte * 4)) &&
                (name.Length < 6 || name[5] == ((key & ((ulong)0xFF << BitsInByte * 5)) >> BitsInByte * 5)) &&
                (name.Length < 7 || name[6] == ((key & ((ulong)0xFF << BitsInByte * 6)) >> BitsInByte * 6)) &&
                // Verify embedded length.
                (name.Length >= 0xFF || (key & ((ulong)0xFF << BitsInByte * 7)) >> BitsInByte * 7 == (ulong)name.Length) &&
                (name.Length < 0xFF || (key & ((ulong)0xFF << BitsInByte * 7)) >> BitsInByte * 7 == 0xFF));

            return key;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryParameterInfo GetParameter(
            ReadOnlySpan<byte> propertyName,
            ref ReadStackFrame frame,
            out byte[] utf8PropertyName)
        {
            ParameterRef parameterRef;

            ulong key = GetKey(propertyName);

            // Keep a local copy of the cache in case it changes by another thread.
            ParameterRef[] localParameterRefsSorted = _parameterRefsSorted;

            // If there is an existing cache, then use it.
            if (localParameterRefsSorted != null)
            {
                // Start with the current parameter index, and then go forwards\backwards.
                int parameterIndex = frame.CtorArgumentState!.ParameterIndex;

                int count = localParameterRefsSorted.Length;
                int iForward = Math.Min(parameterIndex, count);
                int iBackward = iForward - 1;

                while (true)
                {
                    if (iForward < count)
                    {
                        // 正向搜索
                        parameterRef = localParameterRefsSorted[iForward];
                        if (IsParameterRefEqual(parameterRef, propertyName, key))
                        {
                            utf8PropertyName = parameterRef.NameFromBinary;
                            return parameterRef.Info;
                        }

                        ++iForward;

                        // 反向搜索
                        if (iBackward >= 0)
                        {
                            parameterRef = localParameterRefsSorted[iBackward];
                            if (IsParameterRefEqual(parameterRef, propertyName, key))
                            {
                                utf8PropertyName = parameterRef.NameFromBinary;
                                return parameterRef.Info;
                            }

                            --iBackward;
                        }
                    }
                    else if (iBackward >= 0)
                    {
                        parameterRef = localParameterRefsSorted[iBackward];
                        if (IsParameterRefEqual(parameterRef, propertyName, key))
                        {
                            utf8PropertyName = parameterRef.NameFromBinary;
                            return parameterRef.Info;
                        }

                        --iBackward;
                    }
                    else
                    {
                        // Property was not found.
                        break;
                    }
                }
            }

            // 如果在快速缓存中未找到，从ParameterCache中获取

            // No cached item was found. Try the main dictionary which has all of the parameters.
            Debug.Assert(ParameterCache != null);

            if (ParameterCache.TryGetValue(BinaryReaderHelper.TranscodeHelper(propertyName), out BinaryParameterInfo info))
            {
                if (Options.PropertyNameCaseInsensitive)
                {
                    // 如果名称完全相等，直接使用ParameterInfo.NameAsUft8Bytes
                    if (propertyName.SequenceEqual(info.NameAsUtf8Bytes))
                    {
                        Debug.Assert(key == GetKey(info.NameAsUtf8Bytes.AsSpan()));

                        // Use the existing byte[] reference instead of creating another one.
                        utf8PropertyName = info.NameAsUtf8Bytes!;
                    }
                    else
                    {
                        // Make a copy of the original Span.
                        utf8PropertyName = propertyName.ToArray();
                    }
                }
                else
                {
                    Debug.Assert(key == GetKey(info.NameAsUtf8Bytes!.AsSpan()));
                    utf8PropertyName = info.NameAsUtf8Bytes!;
                }
            }
            else
            {
                Debug.Assert(info == null);

                // Make a copy of the original Span.
                utf8PropertyName = propertyName.ToArray();
            }

            // Check if we should add this to the cache.
            // Only cache up to a threshold length and then just use the dictionary when an item is not found in the cache.
            int cacheCount = 0;
            if (localParameterRefsSorted != null)
            {
                cacheCount = localParameterRefsSorted.Length;
            }

            // 如果缓存未超过水位线
            // Do a quick check for the stable (after warm-up) case.
            if (cacheCount < ParameterNameCountCacheThreshold)
            {
                // Do a slower check for the warm-up case.
                if (frame.CtorArgumentState!.ParameterRefCache != null)
                {
                    cacheCount += frame.CtorArgumentState.ParameterRefCache.Count;
                }

                // 把缓存放入读取帧的构造器参数缓存，后面通过UpdateSortedParameterCache统一更新缓存
                // Check again to append the cache up to the threshold.
                if (cacheCount < ParameterNameCountCacheThreshold)
                {
                    if (frame.CtorArgumentState.ParameterRefCache == null)
                    {
                        frame.CtorArgumentState.ParameterRefCache = new List<ParameterRef>();
                    }

                    parameterRef = new ParameterRef(key, info!, utf8PropertyName);
                    frame.CtorArgumentState.ParameterRefCache.Add(parameterRef);
                }
            }

            return info;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsParameterRefEqual(in ParameterRef parameterRef, ReadOnlySpan<byte> parameterName, ulong key)
        {
            // 先筛选了前7个字节相同的参数，然后再进行全字比较
            if (key == parameterRef.Key)
            {
                // We compare the whole name, although we could skip the first 7 bytes (but it's not any faster)
                if (parameterName.Length <= PropertyNameKeyLength ||
                    parameterName.SequenceEqual(parameterRef.NameFromBinary))
                {
                    return true;
                }
            }

            return false;
        }


        public void UpdateSortedParameterCache(ref ReadStackFrame frame)
        {
            Debug.Assert(frame.CtorArgumentState!.ParameterRefCache != null);

            // frame.PropertyRefCache is only read\written by a single thread -- the thread performing
            // the deserialization for a given object instance.

            List<ParameterRef> listToAppend = frame.CtorArgumentState.ParameterRefCache;

            // _parameterRefsSorted can be accessed by multiple threads, so replace the reference when
            // appending to it. No lock() is necessary.

            if (_parameterRefsSorted != null)
            {
                List<ParameterRef> replacementList = new List<ParameterRef>(_parameterRefsSorted);
                Debug.Assert(replacementList.Count <= ParameterNameCountCacheThreshold);

                // Verify replacementList will not become too large.
                while (replacementList.Count + listToAppend.Count > ParameterNameCountCacheThreshold)
                {
                    // This code path is rare; keep it simple by using RemoveAt() instead of RemoveRange() which requires calculating index\count.
                    listToAppend.RemoveAt(listToAppend.Count - 1);
                }

                // Add the new items; duplicates are possible but that is tolerated during property lookup.
                replacementList.AddRange(listToAppend);
                _parameterRefsSorted = replacementList.ToArray();
            }
            else
            {
                _parameterRefsSorted = listToAppend.ToArray();
            }

            frame.CtorArgumentState.ParameterRefCache = null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BinaryPropertyInfo GetProperty(
            ReadOnlySpan<byte> propertyName,
            ref ReadStackFrame frame,
            out byte[] utf8PropertyName)
        {
            PropertyRef propertyRef;

            ulong key = GetKey(propertyName);

            // Keep a local copy of the cache in case it changes by another thread.
            PropertyRef[] localPropertyRefsSorted = _propertyRefsSorted;

            // If there is an existing cache, then use it.
            if (localPropertyRefsSorted != null)
            {
                // Start with the current property index, and then go forwards\backwards.
                int propertyIndex = frame.PropertyIndex;

                int count = localPropertyRefsSorted.Length;
                int iForward = Math.Min(propertyIndex, count);
                int iBackward = iForward - 1;

                while (true)
                {
                    if (iForward < count)
                    {
                        propertyRef = localPropertyRefsSorted[iForward];
                        if (IsPropertyRefEqual(propertyRef, propertyName, key))
                        {
                            utf8PropertyName = propertyRef.NameFromBinary;
                            return propertyRef.Info;
                        }

                        ++iForward;

                        if (iBackward >= 0)
                        {
                            propertyRef = localPropertyRefsSorted[iBackward];
                            if (IsPropertyRefEqual(propertyRef, propertyName, key))
                            {
                                utf8PropertyName = propertyRef.NameFromBinary;
                                return propertyRef.Info;
                            }

                            --iBackward;
                        }
                    }
                    else if (iBackward >= 0)
                    {
                        propertyRef = localPropertyRefsSorted[iBackward];
                        if (IsPropertyRefEqual(propertyRef, propertyName, key))
                        {
                            utf8PropertyName = propertyRef.NameFromBinary;
                            return propertyRef.Info;
                        }

                        --iBackward;
                    }
                    else
                    {
                        // Property was not found.
                        break;
                    }
                }
            }

            // No cached item was found. Try the main dictionary which has all of the properties.
            Debug.Assert(PropertyCache != null);

            if (PropertyCache.TryGetValue(BinaryReaderHelper.TranscodeHelper(propertyName), out BinaryPropertyInfo info))
            {
                if (Options.PropertyNameCaseInsensitive)
                {
                    if (propertyName.SequenceEqual(info.NameAsUtf8Bytes))
                    {
                        Debug.Assert(key == GetKey(info.NameAsUtf8Bytes.AsSpan()));

                        // Use the existing byte[] reference instead of creating another one.
                        utf8PropertyName = info.NameAsUtf8Bytes!;
                    }
                    else
                    {
                        // Make a copy of the original Span.
                        utf8PropertyName = propertyName.ToArray();
                    }
                }
                else
                {
                    Debug.Assert(key == GetKey(info.NameAsUtf8Bytes!.AsSpan()));
                    utf8PropertyName = info.NameAsUtf8Bytes!;
                }
            }
            else
            {
                info = BinaryPropertyInfo.s_missingProperty;

                // Make a copy of the original Span.
                utf8PropertyName = propertyName.ToArray();
            }

            // Check if we should add this to the cache.
            // Only cache up to a threshold length and then just use the dictionary when an item is not found in the cache.
            int cacheCount = 0;
            if (localPropertyRefsSorted != null)
            {
                cacheCount = localPropertyRefsSorted.Length;
            }

            // Do a quick check for the stable (after warm-up) case.
            if (cacheCount < PropertyNameCountCacheThreshold)
            {
                // Do a slower check for the warm-up case.
                if (frame.PropertyRefCache != null)
                {
                    cacheCount += frame.PropertyRefCache.Count;
                }

                // Check again to append the cache up to the threshold.
                if (cacheCount < PropertyNameCountCacheThreshold)
                {
                    if (frame.PropertyRefCache == null)
                    {
                        frame.PropertyRefCache = new List<PropertyRef>();
                    }

                    Debug.Assert(info != null);

                    propertyRef = new PropertyRef(key, info, utf8PropertyName);
                    frame.PropertyRefCache.Add(propertyRef);
                }
            }

            return info;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPropertyRefEqual(in PropertyRef propertyRef, ReadOnlySpan<byte> propertyName, ulong key)
        {
            if (key == propertyRef.Key)
            {
                // We compare the whole name, although we could skip the first 7 bytes (but it's not any faster)
                if (propertyName.Length <= PropertyNameKeyLength ||
                    propertyName.SequenceEqual(propertyRef.NameFromBinary))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateSortedPropertyCache(ref ReadStackFrame frame)
        {
            Debug.Assert(frame.PropertyRefCache != null);

            // frame.PropertyRefCache is only read\written by a single thread -- the thread performing
            // the deserialization for a given object instance.

            List<PropertyRef> listToAppend = frame.PropertyRefCache;

            // _propertyRefsSorted can be accessed by multiple threads, so replace the reference when
            // appending to it. No lock() is necessary.

            if (_propertyRefsSorted != null)
            {
                List<PropertyRef> replacementList = new List<PropertyRef>(_propertyRefsSorted);
                Debug.Assert(replacementList.Count <= PropertyNameCountCacheThreshold);

                // Verify replacementList will not become too large.
                while (replacementList.Count + listToAppend.Count > PropertyNameCountCacheThreshold)
                {
                    // This code path is rare; keep it simple by using RemoveAt() instead of RemoveRange() which requires calculating index\count.
                    listToAppend.RemoveAt(listToAppend.Count - 1);
                }

                // Add the new items; duplicates are possible but that is tolerated during property lookup.
                replacementList.AddRange(listToAppend);
                _propertyRefsSorted = replacementList.ToArray();
            }
            else
            {
                _propertyRefsSorted = listToAppend.ToArray();
            }

            frame.PropertyRefCache = null;
        }


    }
}
