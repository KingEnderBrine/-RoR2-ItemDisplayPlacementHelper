using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ItemDisplayPlacementHelper.Native.Structs;
using UnityEngine;

namespace ItemDisplayPlacementHelper.Native
{
    public static unsafe class NativeHelpers
    {
        private static readonly MethodInfo GetCachedPtr = typeof(UnityEngine.Object).GetMethod(nameof(GetCachedPtr), (BindingFlags)(-1));
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr PersistentManagerGetPathNameHandler(PersistentManager* persistentManager, StringStorageDefaultV2* returnString, int instanceID);
        private const int PersistentManagerGetPathNameOffset = 0x67e6f0;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FreeAllocInternalHandler(IntPtr ptr, int label, IntPtr file, int line);
        private const int FreeAllocInternalOffset = 0x279340;

        private const int PersistentManagerOffset = 0x1b3c168;
        private static IntPtr PersistentManagerPtr;
        private static PersistentManager* PersistentManager => *(PersistentManager**)PersistentManagerPtr.ToPointer();
        private static PersistentManagerGetPathNameHandler PersistentManagerGetPathName;
        private static FreeAllocInternalHandler FreeAllocInternal;

        public static void Init()
        {
            static bool IsUnityPlayer(ProcessModule p)
            {
                return p.ModuleName.ToLowerInvariant().Contains("unityplayer");
            }

            var proc = Process.GetCurrentProcess().Modules
                .Cast<ProcessModule>()
                .FirstOrDefault(IsUnityPlayer) ?? Process.GetCurrentProcess().MainModule;
            var baseAddress = proc.BaseAddress;

            PersistentManagerPtr = baseAddress + PersistentManagerOffset;
            PersistentManagerGetPathName = Marshal.GetDelegateForFunctionPointer<PersistentManagerGetPathNameHandler>(baseAddress + PersistentManagerGetPathNameOffset);
            FreeAllocInternal = Marshal.GetDelegateForFunctionPointer<FreeAllocInternalHandler>(baseAddress + FreeAllocInternalOffset);
        }

        /// <summary>
        /// Get internal name for an AssetBundle that the object is loaded from
        /// </summary>
        /// <param name="instanceID"></param>
        /// <returns></returns>
        public static string GetPathName(int instanceID)
        {
            var stringStorage = new StringStorageDefaultV2();
            PersistentManagerGetPathName(PersistentManager, &stringStorage, instanceID);

            return StringStorageToString(stringStorage, true);
        }

        public static List<string> GetBundleDirectories(AssetBundle assetBundle)
        {
            var bundleRef = (NativeAssetBundle*)(IntPtr)GetCachedPtr.Invoke(assetBundle, Array.Empty<object>());
            var storage = bundleRef->archiveStorage;
            var dirs = storage->directoryInfo.vector;
            var result = new List<string>();
            for (PathNode* s = dirs.first; s != dirs.last; s++)
            {
                result.Add(StringStorageToString(s->path, false));
            }

            return result;
        }

        public static Dictionary<int, string> GetBundleContainerPaths(AssetBundle assetBundle)
        {
            var bundleRef = (NativeAssetBundle*)(IntPtr)GetCachedPtr.Invoke(assetBundle, Array.Empty<object>());
            var result = new Dictionary<int, string>();

            foreach (var node in bundleRef->pathContainer)
            {
                result[node.value.second.instanceId] = StringStorageToString(node.value.first, false);
            }

            return result;
        }

        private static string StringStorageToString(StringStorageDefaultV2 stringStorage, bool freeHeap)
        {
            if (stringStorage.data_repr != StringRepresentation.Embedded && (stringStorage.union.heap.data == 0 || stringStorage.union.heap.size == 0))
            {
                return "";
            }

            switch (stringStorage.data_repr)
            {
                case StringRepresentation.Embedded:
                {
                    return Marshal.PtrToStringAnsi((IntPtr)stringStorage.union.embedded.data);
                }
                default:
                {
                    var str = Marshal.PtrToStringAnsi(stringStorage.union.heap.data, (int)stringStorage.union.heap.size);
                    if (str != null)
                    {
                        if (freeHeap)
                        {
                            FreeAllocInternal(stringStorage.union.heap.data, stringStorage.label, IntPtr.Zero, 0);
                        }
                        return str;
                    }

                    return "";
                }
            }
        }
    }
}
