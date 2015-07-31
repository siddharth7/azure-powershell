// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Management.Automation;
using System.Collections.Generic;
using System.Xml;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using System.Threading;
using Hyak.Common;
using Microsoft.Azure.Commands.AzureBackup.Properties;
using System.Net;
using Microsoft.Azure.Management.BackupServices.Models;
using Microsoft.Azure.Commands.AzureBackup.Cmdlets;
using System.Linq;
using Microsoft.Azure.Commands.AzureBackup.Models;
using CmdletModel = Microsoft.Azure.Commands.AzureBackup.Models;
using System.Collections.Specialized;
using System.Web;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Commands.AzureBackup.Helpers
{
    public static class ProtectionPolicyHelpers
    {
        public const int MinRetentionInDays = 7;
        public const int MaxRetentionInDays = 90;
        public const int MinRetention = 1;
        public const int MaxRetentionInWeeks = 30;
        public const int MaxRetentionInMonths = 24;
        public const int MaxRetentionInYears = 99;
        public const int MinPolicyNameLength = 3;
        public const int MaxPolicyNameLength = 150;
        public static Regex rgx = new Regex(@"^[A-Za-z][-A-Za-z0-9]*[A-Za-z0-9]$");

        public static AzureBackupProtectionPolicy GetCmdletPolicy(CmdletModel.AzurePSBackupVault vault, CSMProtectionPolicyResponse sourcePolicy)
        {
            if (sourcePolicy == null)
            {
                return null;
            }

            return new AzureBackupProtectionPolicy(vault, sourcePolicy.Properties, sourcePolicy.Id);
        }

        public static IEnumerable<AzureBackupProtectionPolicy> GetCmdletPolicies(CmdletModel.AzurePSBackupVault vault, IEnumerable<CSMProtectionPolicyResponse> sourcePolicyList)
        {
            if (sourcePolicyList == null)
            {
                return null;
            }

            List<AzureBackupProtectionPolicy> targetList = new List<AzureBackupProtectionPolicy>();

            foreach (var sourcePolicy in sourcePolicyList)
            {
                targetList.Add(GetCmdletPolicy(vault, sourcePolicy));
            }

            return targetList;
        }

        public static CSMBackupSchedule FillCSMBackupSchedule(string scheduleType, DateTime scheduleStartTime,
            string[] scheduleRunDays)
        {
            var backupSchedule = new CSMBackupSchedule();

            backupSchedule.BackupType = BackupType.Full.ToString();
            
            scheduleType = FillScheduleType(scheduleType, scheduleRunDays);
            backupSchedule.ScheduleRun = scheduleType;

            if (string.Compare(scheduleType, ScheduleType.Weekly.ToString(), true) == 0)
            {
                backupSchedule.ScheduleRunDays = ParseScheduleRunDays(scheduleRunDays);
            }

            DateTime scheduleRunTime = ParseScheduleRunTime(scheduleStartTime);

            backupSchedule.ScheduleRunTimes = new List<DateTime> { scheduleRunTime };

            return backupSchedule;
        }

        public static void ValidateProtectionPolicyName(string policyName)
        {
            if(policyName.Length < MinPolicyNameLength || policyName.Length > MaxPolicyNameLength)
            {
                var exception = new ArgumentException("The protection policy name must contain between 3 and 150 characters.");
                throw exception;
            }
           
            if(!rgx.IsMatch(policyName))
            {
                var exception = new ArgumentException("The protection policy name should contain alphanumeric characters and cannot start with a number.");
                throw exception;
            }
        }

        public static string GetScheduleType(string[] ScheduleRunDays, string parameterSetName,
            string dailyParameterSet, string weeklyParameterSet)
        {
            if (ScheduleRunDays != null && ScheduleRunDays.Length > 0)
            {
                if (parameterSetName == dailyParameterSet)
                {
                    throw new ArgumentException("For daily schedule, protection policy cannot have scheduleRundays");
                }
                else
                {
                    return ScheduleType.Weekly.ToString();
                }
            }
            else
            {
                if (parameterSetName == weeklyParameterSet)
                {
                    throw new ArgumentException("For weekly schedule, ScheduleRundays param cannot be empty");
                }
                else
                {
                    return ScheduleType.Daily.ToString();
                }                
            }  
        }

        private static string FillScheduleType(string scheduleType, string[] scheduleRunDays)
        {
            if (scheduleType == ScheduleType.Daily.ToString() && scheduleRunDays != null && scheduleRunDays.Length > 0)
            {
                return ScheduleType.Weekly.ToString();
            }

            else
            {
                return Enum.Parse(typeof(ScheduleType), scheduleType, true).ToString();
            }
        }

        private static IList<DayOfWeek> ParseScheduleRunDays(string[] scheduleRunDays)
        {
            if (scheduleRunDays == null || scheduleRunDays.Length <= 0)
            {
                var exception = new ArgumentException("For weekly scheduletype , ScheduleRunDays should not be empty.");
                throw exception;
            }

            IList<DayOfWeek> ListofWeekDays = new List<DayOfWeek>();

            foreach (var dayOfWeek in scheduleRunDays)
            {
                DayOfWeek item = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayOfWeek, true);
                if (!ListofWeekDays.Contains(item))
                {
                    ListofWeekDays.Add(item);
                }
            }

            return ListofWeekDays;
        }

        private static DateTime ParseScheduleRunTime(DateTime scheduleStartTime)
        {
            if (scheduleStartTime.Kind == DateTimeKind.Local)
            {
                scheduleStartTime = scheduleStartTime.ToUniversalTime();
            }
            DateTime scheduleRunTime = new DateTime(scheduleStartTime.Year, scheduleStartTime.Month,
                scheduleStartTime.Day, scheduleStartTime.Hour, scheduleStartTime.Minute - (scheduleStartTime.Minute % 30), 0);
            return scheduleRunTime;
        }

        public static void ValidateRetentionPolicy(IList<AzureBackupRetentionPolicy> retentionPolicyList, string scheduleType = "")
        {
            bool validateDailyRetention = false;
            bool validateWeeklyRetention = false;
            if(retentionPolicyList.Count == 0 )
            {
                var exception = new ArgumentException("Please pass atlease one retention policy");
                throw exception;
            }

            foreach (AzureBackupRetentionPolicy retentionPolicy in retentionPolicyList)
            {
                if(retentionPolicy.RetentionType == "Daily")
                {
                    ValidateDailyRetention((AzureBackupDailyRetentionPolicy)retentionPolicy);
                    validateDailyRetention = true;
                }
                else if (retentionPolicy.RetentionType == "Weekly")
                {
                    ValidateWeeklyRetention((AzureBackupWeeklyRetentionPolicy)retentionPolicy);
                    validateWeeklyRetention = true;
                }
                else if (retentionPolicy.RetentionType == "Monthly")
                {
                    ValidateMonthlyRetention((AzureBackupMonthlyRetentionPolicy)retentionPolicy);
                }
                else if (retentionPolicy.RetentionType == "Yearly")
                {
                    ValidateYearlyRetention((AzureBackupYearlyRetentionPolicy)retentionPolicy);
                }
            }

            if (!string.IsNullOrEmpty(scheduleType))
                {
                    if (scheduleType == ScheduleType.Daily.ToString() && validateDailyRetention == false)
                    {
                        var exception = new ArgumentException("For Daily Schedule, please pass AzureBackupDailyRetentionPolicy in RetentionPolicies param.");
                        throw exception;
                    }

                    if (scheduleType == ScheduleType.Weekly.ToString() && validateWeeklyRetention == false)
                    {
                        var exception = new ArgumentException("For Weekly Schedule, please pass AzureBackupWeeklyRetentionPolicy in RetentionPolicies param.");
                        throw exception;
                    }

                    if (scheduleType == ScheduleType.Weekly.ToString() && validateDailyRetention == true)
                    {
                        var exception = new ArgumentException("For Weekly Schedule, you cannot pass AzureBackupDailyRetentionPolicy in RetentionPolicies param.");
                        throw exception;
                    }
               }
        }

        private static void ValidateDailyRetention(AzureBackupDailyRetentionPolicy dailyRetention)
        {
            if (dailyRetention.Retention < MinRetentionInDays || dailyRetention.Retention > MaxRetentionInDays)
            {
                var exception = new ArgumentException(string.Format("For DailyRetentionPolicy , valid values of retention are {0} to {1}.", MinRetentionInDays, MaxRetentionInDays));
                throw exception;
            }            
        }

        private static void ValidateWeeklyRetention(AzureBackupWeeklyRetentionPolicy weeklyRetention)
        {
            if (weeklyRetention.Retention < MinRetention || weeklyRetention.Retention > MaxRetentionInWeeks)
            {
                var exception = new ArgumentException(string.Format("For WeeklyRetentionPolicy , valid values of retention are {0} to {1}.", MinRetention, MaxRetentionInWeeks));
                throw exception;
            }

            if(weeklyRetention.DaysOfWeek == null || weeklyRetention.DaysOfWeek.Count == 0)
            {
                var exception = new ArgumentException("For WeeklyRetentionPolicy , pass atleast one value for DaysOfWeek param.");
                throw exception;
            }
        }

        private static void ValidateMonthlyRetention(AzureBackupMonthlyRetentionPolicy monthlyRetention)
        {
            if (monthlyRetention.Retention < MinRetention || monthlyRetention.Retention > MaxRetentionInMonths)
            {
                var exception = new ArgumentException(string.Format("For MonthlyRetentionPolicy , valid values of retention are {0} to {1}.", MinRetention, MaxRetentionInMonths));
                throw exception;
            }

            if(monthlyRetention.RetentionFormat == RetentionFormat.Daily)
            {
                if(monthlyRetention.DaysOfMonth == null || monthlyRetention.DaysOfMonth.Count == 0)
                {
                    var exception = new ArgumentException("For MonthlyRetentionPolicy and RetentionFormat in Days, pass atleast one value for DaysOfMonth param.");
                    throw exception;
                }
            }

            if (monthlyRetention.RetentionFormat == RetentionFormat.Weekly)
            {
                if (monthlyRetention.DaysOfWeek == null || monthlyRetention.DaysOfWeek.Count == 0)
                {
                    var exception = new ArgumentException("For MonthlyRetentionPolicy and RetentionFormat in Weeks, pass atleast one value for DaysOfWeek param.");
                    throw exception;
                }

                if (monthlyRetention.WeekNumber == null || monthlyRetention.WeekNumber.Count == 0)
                {
                    var exception = new ArgumentException("For MonthlyRetentionPolicy and RetentionFormat in Weeks, pass atleast one value for WeekNumber param.");
                    throw exception;
                }
            }
        }

        private static void ValidateYearlyRetention(AzureBackupYearlyRetentionPolicy yearlyRetention)
        {
            if (yearlyRetention.Retention < MinRetention || yearlyRetention.Retention > MaxRetentionInYears)
            {
                var exception = new ArgumentException(string.Format("For YearlyRetentionPolicy , valid values of retention are {0} to {1}.", MinRetention, MaxRetentionInYears));
                throw exception;
            }

            if(yearlyRetention.MonthsOfYear == null || yearlyRetention.MonthsOfYear.Count == 0)
            {
                var exception = new ArgumentException("For YearlyRetentionPolicy and RetentionFormat in days,pass atleast one value for MonthsOfYear param.");
                throw exception;
            }

            if (yearlyRetention.RetentionFormat == RetentionFormat.Daily)
            {
                if (yearlyRetention.DaysOfMonth == null || yearlyRetention.DaysOfMonth.Count == 0)
                {
                    var exception = new ArgumentException("For YearlyRetentionPolicy and RetentionFormat in Days, pass atleast one value for DaysOfMonth param.");
                    throw exception;
                }
            }

            if (yearlyRetention.RetentionFormat == RetentionFormat.Weekly)
            {
                if (yearlyRetention.DaysOfWeek == null || yearlyRetention.DaysOfWeek.Count == 0)
                {
                    var exception = new ArgumentException("For YearlyRetentionPolicy and RetentionFormat in Weeks,pass atleast one value for DaysOfWeek param.");
                    throw exception;
                }

                if (yearlyRetention.WeekNumber == null || yearlyRetention.WeekNumber.Count == 0)
                {
                    var exception = new ArgumentException("For YearlyRetentionPolicy and RetentionFormat in Weeks, pass atleast one value for WeekNumber param.");
                    throw exception;
                }
            }
        }

    }
}
