﻿using System;
using System.Collections.Generic;
using UserRightsUtil.Interop;

namespace UserRightsUtil.Library
{
    class Modules
    {
        public static bool EnumerateUserRights(
            string domain,
            string username,
            string strSid)
        {
            List<Win32Const.Rights> userRights;
            string accountName;
            Win32Const.SID_NAME_USE peUse;

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(strSid))
            {
                Console.WriteLine("\n[!] Username and SID should not be specified at a time.\n");

                return false;
            }
            else if (!string.IsNullOrEmpty(username))
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    accountName = string.Format("{0}\\{1}", domain, username);
                }
                else
                {
                    accountName = username;
                }

                strSid = Helpers.ConvertAccountNameToSidString(
                    ref accountName,
                    out peUse);

                if (string.IsNullOrEmpty(strSid))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeGroup &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeWellKnownGroup &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeAlias)
                {
                    Console.WriteLine("\n[-] Specified account is not user or group account.\n");
                
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(strSid))
            {
                accountName = Helpers.ConvertSidStringToAccountName(
                    ref strSid,
                    out peUse);

                if (string.IsNullOrEmpty(accountName))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeGroup &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeWellKnownGroup &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeAlias)
                {
                    Console.WriteLine("\n[-] Specified account is not user or group account.\n");
                
                    return false;
                }
            }
            else
            {
                Console.WriteLine("\n[!] Username or SID should be not specified.\n");

                return false;
            }

            Console.WriteLine();
            Console.WriteLine("[>] Trying to enumerate user rights.");
            Console.WriteLine("    |-> Account Name : {0}", accountName);
            Console.WriteLine("    |-> SID          : {0}", strSid);
            Console.WriteLine("    |-> Account Type : {0}", peUse.ToString());

            Win32Api.ConvertStringSidToSid(strSid, out IntPtr pSid);

            userRights = Utilities.GetUserRights(pSid);

            if (userRights.Count > 0)
            {
                Console.WriteLine("[+] Got {0} user right(s).", userRights.Count);

                foreach (var right in userRights)
                {
                    Console.WriteLine("    |-> {0}", right.ToString());
                }
            }
            else
            {
                Console.WriteLine("[-] No available user rights.");
            }

            Console.WriteLine("[*] Done.\n");

            return true;
        }


        public static bool EnumerateUsersWithRights(
            Win32Const.Rights right)
        {
            Console.WriteLine();
            Console.WriteLine("[>] Trying to find users with {0}.", right.ToString());
            List<string> users = Utilities.GetUsersWithRight(right);

            if (users.Count > 0)
            {
                Console.WriteLine("[+] Found {0} user(s).", users.Count);

                foreach (var user in users)
                {
                    Console.WriteLine("    |-> {0}", user);
                }
            }
            else
            {
                Console.WriteLine("[-] No users.");
            }

            Console.WriteLine("[*] Done.\n");

            return true;
        }


        public static bool GrantUserRight(
            string domain,
            string username,
            string strSid,
            Win32Const.Rights userRight)
        {
            IntPtr hLsa;
            string accountName;
            Win32Const.SID_NAME_USE peUse;
            var policy = Win32Const.PolicyAccessRights.POLICY_LOOKUP_NAMES |
                Win32Const.PolicyAccessRights.POLICY_CREATE_ACCOUNT;

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(strSid))
            {
                Console.WriteLine("\n[!] Username and SID should not be specified at a time.\n");

                return false;
            }
            else if (!string.IsNullOrEmpty(username))
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    accountName = string.Format("{0}\\{1}", domain, username);
                }
                else
                {
                    accountName = username;
                }

                strSid = Helpers.ConvertAccountNameToSidString(
                    ref accountName,
                    out peUse);

                if (string.IsNullOrEmpty(strSid))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeGroup)
                {
                    Console.WriteLine("\n[-] Specified account is not user or non well-known group account.\n");

                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(strSid))
            {
                accountName = Helpers.ConvertSidStringToAccountName(
                    ref strSid,
                    out peUse);

                if (string.IsNullOrEmpty(accountName))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser &&
                    peUse != Win32Const.SID_NAME_USE.SidTypeGroup)
                {
                    Console.WriteLine("\n[-] Specified account is not user or non well-known group account.\n");

                    return false;
                }
            }
            else
            {
                Console.WriteLine("\n[!] Username or SID should be not specified.\n");

                return false;
            }

            Console.WriteLine();
            Console.WriteLine("[>] Target account information:");
            Console.WriteLine("    |-> Account Name : {0}", accountName);
            Console.WriteLine("    |-> SID          : {0}", strSid);
            Console.WriteLine("    |-> Account Type : {0}", peUse.ToString());

            hLsa = Utilities.GetSystemLsaHandle(policy);

            if (hLsa == IntPtr.Zero)
                return false;

            bool status = Utilities.GrantSingleUserRight(
                hLsa,
                strSid,
                userRight);
            Win32Api.LsaClose(hLsa);

            return status;
        }


        public static bool LookupSid(
            string domain,
            string username,
            string sid)
        {
            string result;
            string accountName;

            if ((!string.IsNullOrEmpty(domain) || !string.IsNullOrEmpty(username)) &&
                !string.IsNullOrEmpty(sid))
            {
                Console.WriteLine("\n[!] Username or domain name should not be specified with SID at a time.\n");

                return false;
            }
            else if (!string.IsNullOrEmpty(domain) || !string.IsNullOrEmpty(username))
            {
                if (!string.IsNullOrEmpty(domain) && domain.Trim() == ".")
                    domain = Environment.MachineName;

                if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(username))
                    accountName = string.Format("{0}\\{1}", domain, username);
                else if (!string.IsNullOrEmpty(domain))
                    accountName = domain;
                else if (!string.IsNullOrEmpty(username))
                    accountName = username;
                else
                    return false;

                result = Helpers.ConvertAccountNameToSidString(
                    ref accountName,
                    out Win32Const.SID_NAME_USE peUse);

                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine();
                    Console.WriteLine("[*] Result:");
                    Console.WriteLine("    |-> Account Name : {0}", accountName);
                    Console.WriteLine("    |-> SID          : {0}", result);
                    Console.WriteLine("    |-> Account Type : {0}", peUse.ToString());
                    Console.WriteLine();

                    return true;
                }
                else
                {
                    Console.WriteLine("\n[*] No result.\n");

                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(sid))
            {
                result = Helpers.ConvertSidStringToAccountName(
                    ref sid,
                    out Win32Const.SID_NAME_USE peUse);

                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine();
                    Console.WriteLine("[*] Result:");
                    Console.WriteLine("    |-> Account Name : {0}", result);
                    Console.WriteLine("    |-> SID          : {0}", sid);
                    Console.WriteLine("    |-> Account Type : {0}", peUse.ToString());
                    Console.WriteLine();

                    return true;
                }
                else
                {
                    Console.WriteLine("\n[*] No result.\n");

                    return false;
                }
            }
            else
            {
                Console.WriteLine("\n[!] SID, domain name or username to lookup is required.\n");

                return false;
            }
        }


        public static bool RevokeUserRight(
            string domain,
            string username,
            string strSid,
            Win32Const.Rights userRight)
        {
            IntPtr hLsa;
            string accountName;
            Win32Const.SID_NAME_USE peUse;
            var policy = Win32Const.PolicyAccessRights.POLICY_LOOKUP_NAMES;

            if (!string.IsNullOrEmpty(username) &&
                !string.IsNullOrEmpty(strSid))
            {
                Console.WriteLine("\n[!] Username and SID should not be specified at a time.\n");

                return false;
            }
            else if (!string.IsNullOrEmpty(username))
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    accountName = string.Format("{0}\\{1}", domain, username);
                }
                else
                {
                    accountName = username;
                }

                strSid = Helpers.ConvertAccountNameToSidString(
                    ref accountName,
                    out peUse);

                if (string.IsNullOrEmpty(strSid))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser)
                {
                    Console.WriteLine("\n[-] Specified account is not user account.\n");

                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(strSid))
            {
                accountName = Helpers.ConvertSidStringToAccountName(
                    ref strSid,
                    out peUse);

                if (string.IsNullOrEmpty(accountName))
                {
                    Console.WriteLine("\n[-] Failed to resolve SID.\n");

                    return false;
                }
                else if (peUse != Win32Const.SID_NAME_USE.SidTypeUser)
                {
                    Console.WriteLine("\n[-] Specified account is not user account.\n");

                    return false;
                }
            }
            else
            {
                Console.WriteLine("\n[!] Username or SID should be not specified.\n");

                return false;
            }

            Console.WriteLine();
            Console.WriteLine("[>] Target account information:");
            Console.WriteLine("    |-> Account Name : {0}", accountName);
            Console.WriteLine("    |-> SID          : {0}", strSid);
            Console.WriteLine("    |-> Account Type : {0}", peUse.ToString());

            hLsa = Utilities.GetSystemLsaHandle(policy);

            if (hLsa == IntPtr.Zero)
                return false;

            bool status = Utilities.RevokeSingleUserRight(
                hLsa,
                strSid,
                userRight);
            Win32Api.LsaClose(hLsa);

            return status;
        }
    }
}
