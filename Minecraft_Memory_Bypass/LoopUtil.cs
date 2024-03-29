﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Memory_Bypass
{
    public class LoopUtil
    {
        internal List<LoopUtil.INET_FIREWALL_APP_CONTAINER> _AppList;
        internal IntPtr _pACs;

        public LoopUtil()
        {
            LoadApps();
        }

        private List<INET_FIREWALL_APP_CONTAINER> PI_NetworkIsolationEnumAppContainers()
        {
            IntPtr arrayValue = IntPtr.Zero;
            uint size = 0;
            var list = new List<INET_FIREWALL_APP_CONTAINER>();
            GCHandle handle_pdwCntPublicACs = GCHandle.Alloc(size, GCHandleType.Pinned);
            GCHandle handle_ppACs = GCHandle.Alloc(arrayValue, GCHandleType.Pinned);
            uint retval = NetworkIsolationEnumAppContainers((Int32)NETISO_FLAG.NETISO_FLAG_MAX, out size, out arrayValue);
            _pACs = arrayValue;
            var structSize = Marshal.SizeOf(typeof(INET_FIREWALL_APP_CONTAINER));
            for (var i = 0; i < size; i++)
            {
                var cur = (INET_FIREWALL_APP_CONTAINER)Marshal.PtrToStructure(arrayValue, typeof(INET_FIREWALL_APP_CONTAINER));
                list.Add(cur);
                arrayValue = new IntPtr((long)(arrayValue) + (long)(structSize));
            }
            handle_pdwCntPublicACs.Free();
            handle_ppACs.Free();
            return list;
        }

        public void LoadApps()
        {
            _AppList = PI_NetworkIsolationEnumAppContainers();
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_APP_CONTAINER
        {
            internal IntPtr appContainerSid;
            internal IntPtr userSid;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string appContainerName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string displayName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string description;
            internal INET_FIREWALL_AC_CAPABILITIES capabilities;
            internal INET_FIREWALL_AC_BINARIES binaries;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string workingDirectory;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string packageFullName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_CAPABILITIES
        {
            public uint count;
            public IntPtr capabilities;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        internal struct INET_FIREWALL_AC_BINARIES
        {
            public uint count;
            public IntPtr binaries;
        }

        [DllImport("FirewallAPI.dll")]
        internal static extern uint NetworkIsolationEnumAppContainers(uint Flags, out uint pdwCntPublicACs, out IntPtr ppACs);
        enum NETISO_FLAG
        {
            NETISO_FLAG_FORCE_COMPUTE_BINARIES = 0x1,
            NETISO_FLAG_MAX = 0x2
        }

    }
}
