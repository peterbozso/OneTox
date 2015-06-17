using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SharpTox.Core;
using SharpTox.Dns;

namespace WinTox.Helpers
{
    // Kudos: https://github.com/Reverp/Toxy/blob/master/Toxy/ToxHelpers/DnsTools.cs
    public static class DnsTools
    {
        public static string TryDiscoverToxId(string domain, out bool success)
        {
            if (!domain.Contains("@"))
            {
                success = false;
                return String.Empty;
            }

            for (var tries = 0; tries < 3; tries++)
            {
                try
                {
                    var toxId = DiscoverToxId(domain);

                    if (string.IsNullOrEmpty(toxId))
                    {
                        success = false;
                        return String.Empty;
                    }

                    success = true;
                    return toxId;
                }
                catch (Exception)
                {
                }
            }

            success = false;
            return String.Empty;
        }

        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true,
            ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)] ref string pszName, QueryTypes wType,
            QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);

        [DllImport("dnsapi", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, int freeType);

        private static ToxNameService FindNameServiceFromStore(ToxNameService[] services, string suffix)
        {
            return services.FirstOrDefault(s => s.Domain == suffix);
        }

        private static string DiscoverToxId(string domain)
        {
            var services = new[]
            {
                new ToxNameService
                {
                    Domain = "toxme.se",
                    PublicKey = "5D72C517DF6AEC54F1E977A6B6F25914EA4CF7277A85027CD9F5196DF17E0B13"
                },
                new ToxNameService
                {
                    Domain = "utox.org",
                    PublicKey = "D3154F65D28A5B41A05D4AC7E4B39C6B1C233CC857FB365C56E8392737462A12"
                }
            };

            var service = FindNameService(domain.Split('@')[1]) ??
                          FindNameServiceFromStore(services, domain.Split('@')[1]);

            if (service == null)
            {
                //this name service does not use tox3, how unencrypted of them
                domain = domain.Replace("@", "._tox.");

                var records = GetSpfRecords(domain);
                if (records == null)
                    return null;

                foreach (var record in records)
                {
                    if (record.Contains("v=tox1"))
                    {
                        var entries = record.Split(';');

                        foreach (var entry in entries)
                        {
                            var parts = entry.Split('=');
                            var name = parts[0];
                            var value = parts[1];

                            if (name == "id")
                                return value;
                        }
                    }
                }
            }
            else
            {
                string publicKey;

                if (!string.IsNullOrWhiteSpace(service.PublicKey))
                    publicKey = service.PublicKey;
                else
                    return null;

                var split = domain.Split('@');

                var toxDns = new ToxDns(new ToxKey(ToxKeyType.Public, publicKey));
                uint requestId;
                var dns3String = toxDns.GenerateDns3String(split[0], out requestId);

                var query = string.Format("_{0}._tox.{1}", dns3String, split[1]);

                var records = GetSpfRecords(query);
                if (records == null)
                    return null;

                foreach (var record in records)
                {
                    if (record.Contains("v=tox3"))
                    {
                        var entries = record.Split(';');

                        foreach (var entry in entries)
                        {
                            var parts = entry.Split('=');
                            var name = parts[0];
                            var value = parts[1];

                            if (name == "id")
                            {
                                var result = toxDns.DecryptDns3TXT(value, requestId);

                                toxDns.Dispose();
                                return result;
                            }
                        }
                    }
                }

                toxDns.Dispose();
            }

            return null;
        }

        private static ToxNameService FindNameService(string domain)
        {
            for (var i = 0; i < 3; i++)
            {
                var records = GetSpfRecords("_tox." + domain);
                if (records == null)
                    return null;

                foreach (var record in records)
                {
                    if (!string.IsNullOrEmpty(record))
                        return new ToxNameService {Domain = domain, PublicKey = record};
                }
            }

            return null;
        }

        private static string[] GetSpfRecords(string domain)
        {
            var ptr1 = IntPtr.Zero;

            var list = new List<string>();
            try
            {
                var num1 = DnsQuery(ref domain, QueryTypes.DNS_TYPE_TXT, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0,
                    ref ptr1, 0);
                if (num1 != 0)
                {
                    return null;
                }

                SpfRecord recSpf;
                for (var ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recSpf.pNext)
                {
                    recSpf = Marshal.PtrToStructure<SpfRecord>(ptr2);
                    if (recSpf.wType == (short) QueryTypes.DNS_TYPE_TXT)
                    {
                        for (var i = 0; i < recSpf.dwStringCount; i++)
                        {
                            var pString = recSpf.pStringArray + i;
                            var s = Marshal.PtrToStringUni(pString);

                            list.Add(s);
                        }
                    }
                }
            }
            finally
            {
                DnsRecordListFree(ptr1, 0);
            }

            return list.ToArray();
        }

        private class ToxNameService
        {
            public string Domain { get; set; }
            public string PublicKey { get; set; }
        }

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum QueryTypes
        {
            DNS_TYPE_A = 1,
            DNS_TYPE_NS = 2,
            DNS_TYPE_CNAME = 5,
            DNS_TYPE_SOA = 6,
            DNS_TYPE_PTR = 12,
            DNS_TYPE_HINFO = 13,
            DNS_TYPE_MX = 15,
            DNS_TYPE_TXT = 16,
            DNS_TYPE_AAAA = 28
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SpfRecord
        {
            public readonly IntPtr pNext;
            public readonly string pName;
            public readonly short wType;
            public readonly short wDataLength;
            public readonly int flags;
            public readonly int dwTtl;
            public readonly int dwReserved;
            public readonly Int32 dwStringCount;
            public readonly IntPtr pStringArray;
        }
    }
}