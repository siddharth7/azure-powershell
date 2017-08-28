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
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.Models;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ProviderModel;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Grant access to recovery point of an item for item level recovery.
    /// </summary>
    [Cmdlet(VerbsSecurity.Grant, "AzureRmRecoveryServicesBackupRecoveryPointAccess"), OutputType(typeof(ClientScriptInfo))]
    public class GrantAzureRmRecoveryServicesBackupRecoveryPointAccess : RecoveryServicesBackupCmdletBase
    {
        /// <summary>
        /// Recovery point of the item to be explored
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0,
            HelpMessage = ParamHelpMsgs.RestoreDisk.RecoveryPoint)]
        [ValidateNotNullOrEmpty]
        public RecoveryPointBase RecoveryPoint { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipeline = false,
            Position = 2,
            HelpMessage = ParamHelpMsgs.RecoveryPoint.KeyFileDownloadLocation)]
        [ValidateNotNullOrEmpty]
        public string FileDownloadLocation { get; set; }

        public override void ExecuteCmdlet()
        {
            ExecutionBlock(() =>
            {
                base.ExecuteCmdlet();                

                PsBackupProviderManager providerManager = new PsBackupProviderManager(
                    new Dictionary<Enum, object>()
                {
                    {RestoreBackupItemParams.RecoveryPoint, RecoveryPoint},
                        { RecoveryPointParams.KeyFileDownloadLocation, FileDownloadLocation }
                }, ServiceClientAdapter);

                IPsBackupProvider psBackupProvider = providerManager.GetProviderInstance(
                    RecoveryPoint.WorkloadType, RecoveryPoint.BackupManagementType);
                var response = psBackupProvider.ProvisionItemLevelRecoveryAccess();

                WriteDebug(string.Format("ILR Script download completed"));
                WriteObject(response);
            });
        }
    }

    /// <summary>
    /// Revoke access to recovery point of an item for item level recovery.
    /// </summary>
    [Cmdlet(VerbsSecurity.Revoke, "AzureRmRecoveryServicesBackupRecoveryPointAccess")]
    public class RevokeAzureRmRecoveryServicesBackupRecoveryPointAccess : RecoveryServicesBackupCmdletBase
    {
        /// <summary>
        /// Recovery point of the item fo
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0,
            HelpMessage = ParamHelpMsgs.RestoreDisk.RecoveryPoint)]
        [ValidateNotNullOrEmpty]
        public RecoveryPointBase RecoveryPoint { get; set; }

        public override void ExecuteCmdlet()
        {
            ExecutionBlock(() =>
            {
                base.ExecuteCmdlet();

                PsBackupProviderManager providerManager = new PsBackupProviderManager(
                    new Dictionary<Enum, object>()
                {
                    {RestoreBackupItemParams.RecoveryPoint, RecoveryPoint}
                }, ServiceClientAdapter);

                IPsBackupProvider psBackupProvider = providerManager.GetProviderInstance(
                    RecoveryPoint.WorkloadType, RecoveryPoint.BackupManagementType);
                string content = string.Empty;
                psBackupProvider.RevokeItemLevelRecoveryAccess();

                WriteDebug(string.Format("Revoking of recovery point access is completed"));
            });
        }
    }
}
