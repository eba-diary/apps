using Sentry.Common.Logging;
using Sentry.data.Core;
using StructureMap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Infrastructure
{
    /// <summary>
    /// Provides common code between projects
    /// </summary>
    /// 
    public static class Utilities
    {

        /// <summary>
        /// Generate storage location path.  <i>Specify all parameters</i>
        /// </summary>
        /// <param name="levels"></param>
        /// <returns></returns>
        public static string GenerateCustomStorageLocation(string[] levels)
        {
            StringBuilder result = new StringBuilder();
            result.Append(Configuration.Config.GetHostSetting("S3DataPrefix"));
            foreach (string level in levels)
            {
                result.Append(level);
                result.Append('/');
            }
            return result.ToString();
        }
      

      
        public static string GenerateDatasetFrequencyLocationName(string frequency)
        {
            string freq = null;
            switch (frequency.ToLower())
            {
                case "yearly":
                    freq = "yrly";
                    break;
                case "quarterly":
                    freq = "qrtly";
                    break;
                case "monthly":
                    freq = "mntly";
                    break;
                case "weekly":
                    freq = "wkly";
                    break;
                case "daily":
                    freq = "dly";
                    break;
                case "nonschedule":
                    freq = "nskd";
                    break;
                case "transaction":
                    freq = "trn";
                    break;
                default:
                    freq = "dflt";
                    break;
            };
            return freq;
        }
      
        /// <summary>
        /// Get file extension of file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).TrimStart('.').ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS1998")]
        public static async Task CreateEventAsync(Event e)
        {
            IContainer _container;
            using (_container = Bootstrapper.Container.GetNestedContainer())
            {
                var _datasetContext = _container.GetInstance<IDatasetContext>();

                try
                {
                    _datasetContext.Merge<Event>(e);
                    _datasetContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to save event", ex);
                }

            }
        }
    }

    public static class DirectoryUtilities
    {
        /// <summary>
        /// Utilizes the Windows API to return a list of effective permissions for given user on a directory path
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> EffectivePermissions(string user, string path)
        {
            //String UserName = "NT Authority\\Authenticated Users";

            List<string> results = new List<string>();

            IntPtr pSidOwner, pSidGroup, pDacl, pSacl, pSecurityDescriptor;
            ACCESS_MASK mask = new ACCESS_MASK();
            uint ret = GetNamedSecurityInfo(path,
                SE_OBJECT_TYPE.SE_FILE_OBJECT,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION | SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION | SECURITY_INFORMATION.GROUP_SECURITY_INFORMATION,
                out pSidOwner, out pSidGroup, out pDacl, out pSacl, out pSecurityDescriptor);

            IntPtr hManager = IntPtr.Zero;


            bool f = AuthzInitializeResourceManager(1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, null, out hManager);

            NTAccount ac = new NTAccount(user);
            SecurityIdentifier sid = (SecurityIdentifier)ac.Translate(typeof(SecurityIdentifier));
            byte[] bytes = new byte[sid.BinaryLength];
            sid.GetBinaryForm(bytes, 0);
            String _psUserSid = "";
            foreach (byte si in bytes)
            {
                _psUserSid += si;
            }

            LUID unusedSid = new LUID();
            IntPtr UserSid = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, UserSid, bytes.Length);
            IntPtr pClientContext = IntPtr.Zero;

            if (f)
            {
                f = AuthzInitializeContextFromSid(0, UserSid, hManager, IntPtr.Zero, unusedSid, IntPtr.Zero, out pClientContext);

                AUTHZ_ACCESS_REQUEST request = new AUTHZ_ACCESS_REQUEST();
                request.DesiredAccess = 0x02000000;
                request.PrincipalSelfSid = null;
                request.ObjectTypeList = null;
                request.ObjectTypeListLength = 0;
                request.OptionalArguments = IntPtr.Zero;

                AUTHZ_ACCESS_REPLY reply = new AUTHZ_ACCESS_REPLY();
                reply.GrantedAccessMask = IntPtr.Zero;
                reply.ResultListLength = 0;
                reply.SaclEvaluationResults = IntPtr.Zero;
                IntPtr AccessReply = IntPtr.Zero;
                reply.Error = Marshal.AllocHGlobal(1020);
                reply.GrantedAccessMask = Marshal.AllocHGlobal(sizeof(uint));
                reply.ResultListLength = 1;
                int i = 0;
                Dictionary<String, String> rightsmap = new Dictionary<String, String>();
                List<string> effectivePermissionList = new List<string>();
                                
                rightsmap.Add("FILE_TRAVERSE", "Traverse_Folder_and_Execute_File");
                rightsmap.Add("FILE_LIST_DIRECTORY", "List_Folder_and_Read_Data");
                rightsmap.Add("FILE_READ_DATA", "List_Folder_and_Read_Data");
                rightsmap.Add("FILE_READ_ATTRIBUTES", "Read_Attributes");
                rightsmap.Add("FILE_READ_EA", "Read_Extended_Attributes");
                rightsmap.Add("FILE_ADD_FILE", "Create_Files_and_Write_Files");
                rightsmap.Add("FILE_WRITE_DATA", "Create_Files_and_Write_Files");
                rightsmap.Add("FILE_ADD_SUBDIRECTORY", "Create_Folders_and_Append_Data");
                rightsmap.Add("FILE_APPEND_DATA", "Create_Folders_and_Append_Data");
                rightsmap.Add("FILE_WRITE_ATTRIBUTES", "Write_Attributes");
                rightsmap.Add("FILE_WRITE_EA", "Write_Extended_Attributes");
                rightsmap.Add("FILE_DELETE_CHILD", "Delete_Subfolders_and_Files");
                rightsmap.Add("DELETE", "Delete");
                rightsmap.Add("READ_CONTROL", "Read_Permission");
                rightsmap.Add("WRITE_DAC", "Change_Permission");
                rightsmap.Add("WRITE_OWNER", "Take_Ownership");


                f = AuthzAccessCheck(0, pClientContext, ref request, IntPtr.Zero, pSecurityDescriptor, null, 0, ref reply, out AccessReply);
                if (f)
                {
                    int granted_access = Marshal.ReadInt32(reply.GrantedAccessMask);

                    mask = (ACCESS_MASK)granted_access;

                    foreach (ACCESS_MASK item in Enum.GetValues(typeof(ACCESS_MASK)))
                    {
                        if ((mask & item) == item)
                        {
                            effectivePermissionList.Add(rightsmap[item.ToString()]);
                            i++;
                        }

                    }
                }
                Marshal.FreeHGlobal(reply.GrantedAccessMask);


                if (i == 16)
                {
                    effectivePermissionList.Insert(0, "Full_Control");
                }

                foreach (AccessRights r in Enum.GetValues(typeof(AccessRights)))
                {
                    if (effectivePermissionList.Contains(r.ToString()))
                    {
                        results.Add(r.ToString());
                    }
                }
            }

            return results;

        }

        /// <summary>
        /// Determines if user has all AccessRights on specified path.  Will return false if
        /// only paritial AccessRights are found.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <param name="rights"></param>
        /// <returns></returns>
        public static Boolean HasPermission(string user, string path, List<AccessRights> rights)
        {
            List<string> folderRights = EffectivePermissions(user, path);
            //List<string> expectedRights = ;

            //rights.Except(folderRights);

            //foreach (string right in )
            //{
            //    if (Enum.IsDefined(typeof(AccessRights), right))
            //    {
            //        foundRights.Add(right);
            //    }
            //}

            return (rights.Select(s => s.ToString()).Except(folderRights).Any()) ? false : true;

        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern uint GetEffectiveRightsFromAcl(IntPtr pDacl, ref TRUSTEE pTrustee, ref ACCESS_MASK pAccessRights);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private struct TRUSTEE
        {
            IntPtr pMultipleTrustee; // must be null
            public int MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            [MarshalAs(UnmanagedType.LPStr)]
            public string ptstrName;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AUTHZ_ACCESS_REQUEST
        {
            public int DesiredAccess;
            public byte[] PrincipalSelfSid;
            public OBJECT_TYPE_LIST[] ObjectTypeList;
            public int ObjectTypeListLength;
            public IntPtr OptionalArguments;
        };
        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_TYPE_LIST
        {
            OBJECT_TYPE_LEVEL Level;
            int Sbz;
            IntPtr ObjectType;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct AUTHZ_ACCESS_REPLY
        {
            public int ResultListLength;
            public IntPtr GrantedAccessMask;
            public IntPtr SaclEvaluationResults;
            public IntPtr Error;
        };

        private enum OBJECT_TYPE_LEVEL : int
        {
            ACCESS_OBJECT_GUID = 0,
            ACCESS_PROPERTY_SET_GUID = 1,
            ACCESS_PROPERTY_GUID = 2,
            ACCESS_MAX_LEVEL = 4
        };
        private enum TRUSTEE_FORM
        {
            TRUSTEE_IS_SID,
            TRUSTEE_IS_NAME,
            TRUSTEE_BAD_FORM,
            TRUSTEE_IS_OBJECTS_AND_SID,
            TRUSTEE_IS_OBJECTS_AND_NAME
        }

        private enum AUTHZ_RM_FLAG : uint
        {
            AUTHZ_RM_FLAG_NO_AUDIT = 1,
            AUTHZ_RM_FLAG_INITIALIZE_UNDER_IMPERSONATION = 2,
            AUTHZ_RM_FLAG_NO_CENTRAL_ACCESS_POLICIES = 4,
        }

        private enum TRUSTEE_TYPE
        {
            TRUSTEE_IS_UNKNOWN,
            TRUSTEE_IS_USER,
            TRUSTEE_IS_GROUP,
            TRUSTEE_IS_DOMAIN,
            TRUSTEE_IS_ALIAS,
            TRUSTEE_IS_WELL_KNOWN_GROUP,
            TRUSTEE_IS_DELETED,
            TRUSTEE_IS_INVALID,
            TRUSTEE_IS_COMPUTER
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern uint GetNamedSecurityInfo(
            string pObjectName,
            SE_OBJECT_TYPE ObjectType,
            SECURITY_INFORMATION SecurityInfo,
            out IntPtr pSidOwner,
            out IntPtr pSidGroup,
            out IntPtr pDacl,
            out IntPtr pSacl,
            out IntPtr pSecurityDescriptor);
        [DllImport("authz.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "AuthzInitializeContextFromSid", CharSet = CharSet.Unicode)]
        static extern private bool AuthzInitializeContextFromSid(
                                               int Flags,
                                               IntPtr UserSid,
                                               IntPtr AuthzResourceManager,
                                               IntPtr pExpirationTime,
                                               LUID Identitifier,
                                               IntPtr DynamicGroupArgs,
                                               out IntPtr pAuthzClientContext
                                               );



        [DllImport("authz.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall, EntryPoint = "AuthzInitializeResourceManager", CharSet = CharSet.Unicode)]
        static extern private bool AuthzInitializeResourceManager(
                                        int flags,
                                        IntPtr pfnAccessCheck,
                                        IntPtr pfnComputeDynamicGroups,
                                        IntPtr pfnFreeDynamicGroups,
                                        string name,
                                        out IntPtr rm
                                        );
        [DllImport("authz.dll", EntryPoint = "AuthzAccessCheck", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern bool AuthzAccessCheck(int flags,
                                                    IntPtr hAuthzClientContext,
                                                     ref AUTHZ_ACCESS_REQUEST pRequest,
                                                     IntPtr AuditEvent,
                                                     IntPtr pSecurityDescriptor,
                                                    byte[] OptionalSecurityDescriptorArray,
                                                    int OptionalSecurityDescriptorCount,
                                                    ref AUTHZ_ACCESS_REPLY pReply,
                                                    out IntPtr phAccessCheckResults);

        private enum ACCESS_MASK : uint
        {
            FILE_TRAVERSE = 0x20,
            FILE_LIST_DIRECTORY = 0x1,
            FILE_READ_DATA = 0x1,
            FILE_READ_ATTRIBUTES = 0x80,
            FILE_READ_EA = 0x8,
            FILE_ADD_FILE = 0x2,
            FILE_WRITE_DATA = 0x2,
            FILE_ADD_SUBDIRECTORY = 0x4,
            FILE_APPEND_DATA = 0x4,
            FILE_WRITE_ATTRIBUTES = 0x100,
            FILE_WRITE_EA = 0x10,
            FILE_DELETE_CHILD = 0x40,
            DELETE = 0x10000,
            READ_CONTROL = 0x20000,
            WRITE_DAC = 0x40000,
            WRITE_OWNER = 0x80000,


            ////////FILE_EXECUTE =0x20,   
        }

        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            GROUP_SECURITY_INFORMATION = 0x00000002,
            DACL_SECURITY_INFORMATION = 0x00000004,
            SACL_SECURITY_INFORMATION = 0x00000008,
            UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000,
            UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000,
            PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000,
            PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000
        }

        private enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }


        public enum AccessRights
        {
            Full_Control = 0,
            Traverse_Folder_and_Execute_File = 1,
            Read_Attributes = 2,
            Read_Extended_Attributes = 3,
            Create_Files_and_Write_Files = 4,
            Create_Folders_and_Append_Data = 5,
            Write_Attributes = 6,
            Write_Extended_Attributes = 7,
            Delete_Subfolders_and_Files = 8,
            Delete = 9,
            Read_Permission = 10,
            Change_Permission = 11,
            Take_Ownership = 12,
            List_Folder_and_Read_Data = 13
        }
    }
}
