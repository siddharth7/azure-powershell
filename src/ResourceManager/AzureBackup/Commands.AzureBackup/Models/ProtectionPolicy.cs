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

using Microsoft.Azure.Management.BackupServices.Models;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.AzureBackup.Models
{
    /// <summary>
    /// Represents ProtectionPolicy object
    /// </summary>
    public class AzureBackupProtectionPolicy : AzureBackupVaultContextObject
    {
        /// <summary>
        /// Name of the azurebackup object
        /// </summary>

        public string PolicyId { get; set; }

        public string Name { get; set; }

        public string WorkloadType { get; set; }

        public string ScheduleType { get; set; }

        public List<string> ScheduleRunDays { get; set; }

        public DateTime ScheduleRunTimes { get; set; }

        public IList<AzureBackupRetentionPolicy> RetentionPolicyList { get; set; }

        public AzureBackupProtectionPolicy()
        {
        }

        public AzureBackupProtectionPolicy(AzurePSBackupVault vault, CSMProtectionPolicyProperties sourcePolicy, string policyId)
            : base(vault)
        {
            PolicyId = policyId;
            Name = sourcePolicy.PolicyName;
            WorkloadType = sourcePolicy.WorkloadType;
            ScheduleType = sourcePolicy.BackupSchedule.ScheduleRun;
            ScheduleRunTimes = ConvertScheduleRunTimes(sourcePolicy.BackupSchedule.ScheduleRunTimes);
            ScheduleRunDays = ConvertScheduleRunDays(sourcePolicy.BackupSchedule.ScheduleRunDays);
            RetentionPolicyList = ConvertCSMRetentionPolicyListToPowershell(sourcePolicy.LtrRetentionPolicy);            
        }

        private IList<AzureBackupRetentionPolicy> ConvertCSMRetentionPolicyListToPowershell(CSMLongTermRetentionPolicy LTRRetentionPolicy)
        {
            IList<AzureBackupRetentionPolicy> retentionPolicyList = new List<AzureBackupRetentionPolicy>();
            AzureBackupDailyRetentionPolicy dailyRetentionPolicy = ConvertToPowershellDailyRetentionObject(LTRRetentionPolicy.DailySchedule);
            AzureBackupWeeklyRetentionPolicy weeklyRetentionPolicy = ConvertToPowershellWeeklyRetentionObject(LTRRetentionPolicy.WeeklySchedule);
            AzureBackupMonthlyRetentionPolicy monthlyRetentionPolicy = ConvertToPowershellMonthlyRetentionObject(LTRRetentionPolicy.MonthlySchedule);
            AzureBackupYearlyRetentionPolicy yearlyRetentionPolicy = ConvertToPowershellYearlyRetentionObject(LTRRetentionPolicy.YearlySchedule);

            if(dailyRetentionPolicy != null)
            {
                retentionPolicyList.Add(dailyRetentionPolicy);
            }
            if (weeklyRetentionPolicy != null)
            {
                retentionPolicyList.Add(weeklyRetentionPolicy);
            }
            if (monthlyRetentionPolicy != null)
            {
                retentionPolicyList.Add(monthlyRetentionPolicy);
            }
            if (yearlyRetentionPolicy != null)
            {
                retentionPolicyList.Add(yearlyRetentionPolicy);
            }

            return retentionPolicyList;
        }

        AzureBackupDailyRetentionPolicy ConvertToPowershellDailyRetentionObject(CSMDailyRetentionSchedule DailySchedule)
        {
            if (DailySchedule == null)
                return null;
            AzureBackupDailyRetentionPolicy dailyRetention = new AzureBackupDailyRetentionPolicy("Daily", DailySchedule.CSMRetentionDuration.Count);

            return dailyRetention;
        }

        AzureBackupWeeklyRetentionPolicy ConvertToPowershellWeeklyRetentionObject(CSMWeeklyRetentionSchedule WeeklySchedule)
        {
            if (WeeklySchedule == null)
                return null;
            AzureBackupWeeklyRetentionPolicy weeklyRetention = new AzureBackupWeeklyRetentionPolicy("Weekly", WeeklySchedule.CSMRetentionDuration.Count,
                WeeklySchedule.DaysOfTheWeek);

            return weeklyRetention;
        }

        AzureBackupMonthlyRetentionPolicy ConvertToPowershellMonthlyRetentionObject(CSMMonthlyRetentionSchedule MonthlySchedule)
        {
            if (MonthlySchedule == null)
                return null;
            AzureBackupMonthlyRetentionPolicy monthlyRetention = null;

            RetentionFormat retentionFormat = (RetentionFormat)Enum.Parse(typeof(RetentionFormat), MonthlySchedule.RetentionScheduleType.ToString(), true);
            if(retentionFormat == RetentionFormat.Daily)
            {
                List<int> dayList = GetDayList(MonthlySchedule.RetentionScheduleDaily.DaysOfTheMonth);
                monthlyRetention = new AzureBackupMonthlyRetentionPolicy("Monthly", MonthlySchedule.CSMRetentionDuration.Count,
                retentionFormat, dayList, null, null);
            }
            else if(retentionFormat == RetentionFormat.Weekly)
            {
                List<WeekNumber> weekNumberList = GetWeekNumberList(MonthlySchedule.RetentionScheduleWeekly);
                List<DayOfWeek> dayOfWeekList = GetWeekDaysList(MonthlySchedule.RetentionScheduleWeekly);
                monthlyRetention = new AzureBackupMonthlyRetentionPolicy("Monthly", MonthlySchedule.CSMRetentionDuration.Count,
                retentionFormat, null, weekNumberList, dayOfWeekList);
            }
            
            return monthlyRetention;
        }

        AzureBackupYearlyRetentionPolicy ConvertToPowershellYearlyRetentionObject(CSMYearlyRetentionSchedule YearlySchedule)
        {
            if (YearlySchedule == null)
                return null;
            AzureBackupYearlyRetentionPolicy yearlyRetention = null;

            List<Month> monthOfTheYearList = GetMonthsOfYearList(YearlySchedule.MonthsOfYear);
            
            RetentionFormat retentionFormat = (RetentionFormat)Enum.Parse(typeof(RetentionFormat), YearlySchedule.RetentionScheduleType.ToString(), true);
            if(retentionFormat == RetentionFormat.Daily)
            {
                List<int> dayList = GetDayList(YearlySchedule.RetentionScheduleDaily.DaysOfTheMonth);
                yearlyRetention = new AzureBackupYearlyRetentionPolicy("Yearly", YearlySchedule.CSMRetentionDuration.Count,
                monthOfTheYearList, retentionFormat, dayList, null, null);
            }
            else if (retentionFormat == RetentionFormat.Weekly)
            {
                List<WeekNumber> weekNumberList = GetWeekNumberList(YearlySchedule.RetentionScheduleWeekly);
                List<DayOfWeek> dayOfWeekList = GetWeekDaysList(YearlySchedule.RetentionScheduleWeekly);
                yearlyRetention = new AzureBackupYearlyRetentionPolicy("Yearly", YearlySchedule.CSMRetentionDuration.Count,
                 monthOfTheYearList, retentionFormat, null, weekNumberList, dayOfWeekList);
            }

            return yearlyRetention;
        }
            
        private List<string> ConvertScheduleRunDays(IList<DayOfWeek> weekDaysList)
        {
            List<string> scheduelRunDays = new List<string>();

            foreach (object item in weekDaysList)
            {
                scheduelRunDays.Add(item.ToString());
            }

            return scheduelRunDays;
        }

        private DateTime ConvertScheduleRunTimes(IList<DateTime> scheduleRunTimeList)
        {
            IEnumerator<DateTime> scheduleEnumerator = scheduleRunTimeList.GetEnumerator();
            scheduleEnumerator.MoveNext();
            return scheduleEnumerator.Current;
        }

        public List<int> GetDayList(IList<Day> daysOfTheMonthList)
        {
            int lastDayOfTheMonth = 29;
            List<int> dayList = new List<int>();
            foreach (Day day in daysOfTheMonthList)
            {
                dayList.Add(day.Date);
                if(day.IsLast)
                {
                    dayList.Add(lastDayOfTheMonth);
                }
            }

            return dayList;
        }

        public List<WeekNumber> GetWeekNumberList(CSMWeeklyRetentionFormat csmWeekNumberList)
        {
            List<WeekNumber> weekNumberList = new List<WeekNumber>();
            foreach (WeekNumber weekNumber in csmWeekNumberList.WeeksOfTheMonth)
            {
                weekNumberList.Add(weekNumber);
            }
            return weekNumberList;
        }

        public List<DayOfWeek> GetWeekDaysList(CSMWeeklyRetentionFormat csmWeekNumberList)
        {
            List<DayOfWeek> dayOfWeekList = new List<DayOfWeek>();
            foreach (DayOfWeek dayOfWeek in csmWeekNumberList.DaysOfTheWeek)
            {
                dayOfWeekList.Add(dayOfWeek);
            }
            return dayOfWeekList;
        }

        public List<Month> GetMonthsOfYearList(IList<Month> MonthsOfYear)
        {
            List<Month> monthOfTheYearList = new List<Month>();
            foreach (Month monthOfTheYear in MonthsOfYear)
            {
                monthOfTheYearList.Add(monthOfTheYear);
            }
            return monthOfTheYearList;             
        }

        public CSMLongTermRetentionPolicy ConvertToCSMRetentionPolicyObject(IList<AzureBackupRetentionPolicy> retentionPolicyList, CSMBackupSchedule backupSchedule)
        {
            CSMLongTermRetentionPolicy csmLongTermRetentionPolicy = new CSMLongTermRetentionPolicy();
            foreach(AzureBackupRetentionPolicy retentionPolicy in retentionPolicyList)
            {
                if(retentionPolicy.RetentionType == "Daily")
                {
                    csmLongTermRetentionPolicy.DailySchedule = ConvertToCSMDailyRetentionObject((AzureBackupDailyRetentionPolicy)retentionPolicy,
                        backupSchedule.ScheduleRunTimes);
                }
                if(retentionPolicy.RetentionType == "Weekly")
                {
                    csmLongTermRetentionPolicy.WeeklySchedule = ConvertToCSMWeeklyRetentionObject((AzureBackupWeeklyRetentionPolicy)retentionPolicy,
                        backupSchedule.ScheduleRunTimes);
                }
                if(retentionPolicy.RetentionType == "Monthly")
                {
                    csmLongTermRetentionPolicy.MonthlySchedule = ConvertToGetCSMMonthlyRetentionObject((AzureBackupMonthlyRetentionPolicy)retentionPolicy,
                        backupSchedule.ScheduleRunTimes);
                }
                if(retentionPolicy.RetentionType == "Yearly")
                {
                    csmLongTermRetentionPolicy.YearlySchedule = ConvertToCSMYearlyRetentionObject((AzureBackupYearlyRetentionPolicy)retentionPolicy,
                        backupSchedule.ScheduleRunTimes);
                }
            }

            return csmLongTermRetentionPolicy;
        }

        public CSMDailyRetentionSchedule ConvertToCSMDailyRetentionObject(AzureBackupDailyRetentionPolicy retentionPolicy, IList<DateTime> RetentionTimes)
        {
            CSMDailyRetentionSchedule csmDailyRetention = new CSMDailyRetentionSchedule();
            csmDailyRetention.CSMRetentionDuration = new CSMRetentionDuration();
            csmDailyRetention.CSMRetentionDuration.Count = retentionPolicy.Retention;
            csmDailyRetention.CSMRetentionDuration.DurationType = RetentionDurationType.Days;
            csmDailyRetention.RetentionTimes = RetentionTimes;

            return csmDailyRetention;
        }
        public CSMWeeklyRetentionSchedule ConvertToCSMWeeklyRetentionObject(AzureBackupWeeklyRetentionPolicy retentionPolicy, IList<DateTime> RetentionTimes)
        {
            CSMWeeklyRetentionSchedule csmWeeklyRetention = new CSMWeeklyRetentionSchedule();
            csmWeeklyRetention.DaysOfTheWeek = retentionPolicy.DaysOfWeek;
            csmWeeklyRetention.CSMRetentionDuration = new CSMRetentionDuration();
            csmWeeklyRetention.CSMRetentionDuration.Count = retentionPolicy.Retention;
            csmWeeklyRetention.CSMRetentionDuration.DurationType = RetentionDurationType.Weeks;
            csmWeeklyRetention.RetentionTimes = RetentionTimes;
            return csmWeeklyRetention;
        }

        public CSMMonthlyRetentionSchedule ConvertToGetCSMMonthlyRetentionObject(AzureBackupMonthlyRetentionPolicy retentionPolicy, IList<DateTime> RetentionTimes)
        {
            CSMMonthlyRetentionSchedule csmMonthlyRetention = new CSMMonthlyRetentionSchedule();

            if(retentionPolicy.RetentionFormat == RetentionFormat.Daily)
            {
                csmMonthlyRetention.RetentionScheduleType = RetentionScheduleFormat.Daily;
                csmMonthlyRetention.RetentionScheduleDaily = new CSMDailyRetentionFormat();
                csmMonthlyRetention.RetentionScheduleDaily.DaysOfTheMonth = ConvertToCSMDayList(retentionPolicy.DaysOfMonth);
            }

            else if (retentionPolicy.RetentionFormat == RetentionFormat.Weekly)
            {
                csmMonthlyRetention.RetentionScheduleWeekly = new CSMWeeklyRetentionFormat();
                csmMonthlyRetention.RetentionScheduleType = RetentionScheduleFormat.Weekly;
                csmMonthlyRetention.RetentionScheduleWeekly.DaysOfTheWeek = retentionPolicy.DaysOfWeek;
                csmMonthlyRetention.RetentionScheduleWeekly.WeeksOfTheMonth = retentionPolicy.WeekNumber;
            }

            csmMonthlyRetention.CSMRetentionDuration = new CSMRetentionDuration();
            csmMonthlyRetention.CSMRetentionDuration.Count = retentionPolicy.Retention;
            csmMonthlyRetention.CSMRetentionDuration.DurationType = RetentionDurationType.Months;
            csmMonthlyRetention.RetentionTimes = RetentionTimes;

            return csmMonthlyRetention;
        }

        public CSMYearlyRetentionSchedule ConvertToCSMYearlyRetentionObject(AzureBackupYearlyRetentionPolicy retentionPolicy, IList<DateTime> RetentionTimes)
        {
            CSMYearlyRetentionSchedule csmYearlyRetention = new CSMYearlyRetentionSchedule();

            if (retentionPolicy.RetentionFormat == RetentionFormat.Daily)
            {
                csmYearlyRetention.RetentionScheduleType = RetentionScheduleFormat.Daily;
                csmYearlyRetention.RetentionScheduleDaily = new CSMDailyRetentionFormat();
                csmYearlyRetention.RetentionScheduleDaily.DaysOfTheMonth = ConvertToCSMDayList(retentionPolicy.DaysOfMonth);
            }

            else if (retentionPolicy.RetentionFormat == RetentionFormat.Weekly)
            {
                csmYearlyRetention.RetentionScheduleWeekly = new CSMWeeklyRetentionFormat();
                csmYearlyRetention.RetentionScheduleType = RetentionScheduleFormat.Weekly;
                csmYearlyRetention.RetentionScheduleWeekly.DaysOfTheWeek = retentionPolicy.DaysOfWeek;
                csmYearlyRetention.RetentionScheduleWeekly.WeeksOfTheMonth = retentionPolicy.WeekNumber;
            }

            csmYearlyRetention.CSMRetentionDuration = new CSMRetentionDuration();
            csmYearlyRetention.CSMRetentionDuration.Count = retentionPolicy.Retention;
            csmYearlyRetention.CSMRetentionDuration.DurationType = RetentionDurationType.Years;
            csmYearlyRetention.RetentionTimes = RetentionTimes;
            csmYearlyRetention.MonthsOfYear = retentionPolicy.MonthsOfYear;

            return csmYearlyRetention;
        }

        public IList<Day> ConvertToCSMDayList(List<int> DaysOfMonth)
        {
            IList<Day> dayList = new List<Day>();

            foreach(int DayOfMonth in DaysOfMonth)
            {
                Day day = new Day();
                if (DayOfMonth == 29)
                {
                    day.IsLast = true;                    
                }
                else
                {
                    day.Date = DayOfMonth;
                    day.IsLast = false;
                }
                dayList.Add(day);
            }

            return dayList;
        }
    }    

    public class AzureBackupRetentionPolicy
    {
        public string RetentionType { get; set; }

        public int Retention { get; set; }

        public IList<DateTime> RetentionTimes { get; set; }

        public AzureBackupRetentionPolicy(string retentionType, int retention)
        {
            this.RetentionType = retentionType;
            this.Retention = retention;
        }
    }

    public class AzureBackupDailyRetentionPolicy : AzureBackupRetentionPolicy
    {
        public AzureBackupDailyRetentionPolicy(string retentionType, int retention)
            : base(retentionType, retention)
        { }
    }

    public class AzureBackupWeeklyRetentionPolicy : AzureBackupRetentionPolicy
    {
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public AzureBackupWeeklyRetentionPolicy(string retentionType, int retention, IList<DayOfWeek> daysOfWeek)
            : base(retentionType, retention)
        {
            this.DaysOfWeek = new List<DayOfWeek>(daysOfWeek);
        }
    }

    public class AzureBackupMonthlyRetentionPolicy : AzureBackupRetentionPolicy
    {
        public RetentionFormat RetentionFormat { get; set; }

        public List<int> DaysOfMonth { get; set; }

        public List<WeekNumber> WeekNumber { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public AzureBackupMonthlyRetentionPolicy(string retentionType, int retention, RetentionFormat retentionFormat, List<int> daysOfMonth,
            List<WeekNumber> weekNumber, List<DayOfWeek> daysOfWeek)
            : base(retentionType, retention)
        {
            this.RetentionFormat = retentionFormat;
            this.DaysOfMonth = daysOfMonth;
            this.WeekNumber = weekNumber;
            this.DaysOfWeek = daysOfWeek;
        }
    }

    public class AzureBackupYearlyRetentionPolicy : AzureBackupRetentionPolicy
    {
        public List<Month> MonthsOfYear { get; set; }

        public RetentionFormat RetentionFormat { get; set; }

        public List<int> DaysOfMonth { get; set; }

        public List<WeekNumber> WeekNumber { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; }

        public AzureBackupYearlyRetentionPolicy(string retentionType, int retention, List<Month> monthsOfYear, RetentionFormat retentionFormat, List<int> daysOfMonth,
            List<WeekNumber> weekNumber, List<DayOfWeek> daysOfWeek)
            : base(retentionType, retention)
        {
            this.MonthsOfYear = monthsOfYear;
            this.RetentionFormat = retentionFormat;
            this.DaysOfMonth = daysOfMonth;
            this.WeekNumber = weekNumber;
            this.DaysOfWeek = daysOfWeek;
        }
    }

    //public enum WeekNumber
    //{
    //    First,
    //    Second,
    //    Third,
    //    Fourth,
    //    Last
    //}
}
