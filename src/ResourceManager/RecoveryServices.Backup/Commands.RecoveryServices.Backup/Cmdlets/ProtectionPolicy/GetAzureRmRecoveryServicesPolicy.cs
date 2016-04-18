﻿// ----------------------------------------------------------------------------------
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.Models;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ProviderModel;
using HydraModel = Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Helpers;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Properties;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Get list of protection policies
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmRecoveryServicesBackupProtectionPolicy", DefaultParameterSetName = NoParamSet), 
            OutputType(typeof(AzureRmRecoveryServicesBackupPolicyBase))]
    public class GetAzureRmRecoveryServicesProtectionPolicy : RecoveryServicesBackupCmdletBase
    {
        protected const string PolicyNameParamSet = "PolicyNameParamSet";
        protected const string WorkloadParamSet = "WorkloadParamSet";
        protected const string NoParamSet = "NoParamSet";
        protected const string WorkloadBackupMangementTypeParamSet = "WorkloadBackupManagementTypeParamSet";

        [Parameter(ParameterSetName = PolicyNameParamSet, Position = 1, Mandatory = true, HelpMessage = ParamHelpMsg.Policy.Name)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ParameterSetName = WorkloadParamSet, Position = 2, Mandatory = true, HelpMessage = ParamHelpMsg.Common.WorkloadType)]
        [Parameter(ParameterSetName = WorkloadBackupMangementTypeParamSet, Position = 2, Mandatory = true, HelpMessage = ParamHelpMsg.Common.WorkloadType)]
        [ValidateNotNullOrEmpty]
        public WorkloadType? WorkloadType { get; set; }

        [Parameter(ParameterSetName = WorkloadBackupMangementTypeParamSet, Position = 3, Mandatory = true, HelpMessage = ParamHelpMsg.Common.BackupManagementType)]
        [ValidateNotNullOrEmpty]
        public BackupManagementType? BackupManagementType { get; set; }

        public override void ExecuteCmdlet()
        {
           ExecutionBlock(() =>
           {
               base.ExecuteCmdlet();

               WriteDebug(string.Format("Input params - Name:{0}, " +
                                     "WorkloadType: {1}, BackupManagementType:{2}, " +
                                     "ParameterSetName: {3}",
                                     Name == null ? "NULL" : Name,
                                     WorkloadType.HasValue ? WorkloadType.ToString() : "NULL",
                                     BackupManagementType.HasValue ? BackupManagementType.ToString() : "NULL",
                                     this.ParameterSetName));
               
               if (this.ParameterSetName == PolicyNameParamSet)
               {                   
                   // validate policyName
                   PolicyCmdletHelpers.ValidateProtectionPolicyName(Name);

                   // query service
                   HydraModel.ProtectionPolicyResponse policy = PolicyCmdletHelpers.GetProtectionPolicyByName(
                                                     Name,
                                                     HydraAdapter);
                   if (policy == null)
                   {
                       throw new ArgumentException(string.Format(Resources.PolicyNotFoundException, Name));
                   }

                   WriteObject(ConversionHelpers.GetPolicyModel(policy.Item));
               }
               else
               {
                   List<AzureRmRecoveryServicesBackupPolicyBase> policyList = new List<AzureRmRecoveryServicesBackupPolicyBase>();
                   string hydraProviderType = null;                   

                   switch (this.ParameterSetName)
                   {
                       case WorkloadParamSet:
                           if (WorkloadType == Models.WorkloadType.AzureVM)
                           {
                               hydraProviderType = HydraHelpers.GetHydraProviderType(Models.WorkloadType.AzureVM);
                           }
                           break;

                       case WorkloadBackupMangementTypeParamSet:
                           if (WorkloadType == Models.WorkloadType.AzureVM)
                           {
                               if (BackupManagementType != Models.BackupManagementType.AzureVM)
                               {
                                   throw new ArgumentException(Resources.AzureVMUnsupportedBackupManagementTypeException);
                               }
                               hydraProviderType = HydraHelpers.GetHydraProviderType(Models.WorkloadType.AzureVM);
                           }
                           else
                           {
                               throw new ArgumentException(string.Format(
                                           Resources.UnsupportedWorkloadBackupManagementTypeException,       
                                           WorkloadType.ToString(),
                                           BackupManagementType.ToString()));
                           }
                           break;

                       case NoParamSet:
                           // query params should be null by default
                           break;

                       default:
                           break;
                   }

                   HydraModel.ProtectionPolicyQueryParameters queryParams = new HydraModel.ProtectionPolicyQueryParameters()
                   {
                       BackupManagementType = hydraProviderType
                   };

                   WriteDebug("going to query service to get list of policies");
                   HydraModel.ProtectionPolicyListResponse respList = HydraAdapter.ListProtectionPolicy(queryParams);
                   WriteDebug("Successfully got response from service");

                   policyList = ConversionHelpers.GetPolicyModelList(respList);
                   WriteObject(policyList, enumerateCollection: true);
               }
           });
        }
    }
}