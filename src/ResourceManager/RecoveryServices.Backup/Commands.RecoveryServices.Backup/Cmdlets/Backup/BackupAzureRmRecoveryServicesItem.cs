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

using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.Models;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ProviderModel;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Helpers;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Backup Item
    /// </summary>
    [Cmdlet(VerbsData.Backup, "AzureRmRecoveryServicesBackupItem"), OutputType(typeof(AzureRmRecoveryServicesJobBase))]
    public class BackupAzureRmRecoveryServicesBackupItem : RecoveryServicesBackupCmdletBase
    {
        [Parameter(Mandatory = true, HelpMessage = ParamHelpMsg.Item.ProtectedItem, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public AzureRmRecoveryServicesBackupItemBase Item { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            PsBackupProviderManager providerManager = new PsBackupProviderManager(new Dictionary<System.Enum, object>()
                {
                    {ItemParams.Item, Item},
                }, HydraAdapter);

            IPsBackupProvider psBackupProvider = providerManager.GetProviderInstance(Item.WorkloadType, Item.BackupManagementType);
            var jobResponse = psBackupProvider.TriggerBackup();

            HandleCreatedJob(jobResponse, Resources.TriggerBackupOperation);
        }
    }
}