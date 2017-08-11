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
using System.Threading;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.Models;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ProviderModel;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Properties;
using Microsoft.Azure.Management.Internal.Resources;
using Microsoft.Azure.Management.Internal.Resources.Models;
using StorageModels = Microsoft.Azure.Management.Storage.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Restores an item using the recovery point provided within the recovery services vault
    /// </summary>
    [Cmdlet(VerbsData.Restore, "AzureRmRecoveryServicesBackupItem"), OutputType(typeof(JobBase))]
    public class RestoreAzureRmRecoveryServicesBackupItem : RecoveryServicesBackupCmdletBase
    {
        /// <summary>
        /// Recovery point of the item to be restored
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0,
            HelpMessage = ParamHelpMsgs.RestoreDisk.RecoveryPoint)]
        [ValidateNotNullOrEmpty]
        public RecoveryPointBase RecoveryPoint { get; set; }

        /// <summary>
        /// Storage account name where the disks need to be recovered
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,
            HelpMessage = ParamHelpMsgs.RestoreDisk.StorageAccountName)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        /// <summary>
        /// Resource group name of Storage account name where the disks need to be recovered
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,
            HelpMessage = ParamHelpMsgs.RestoreDisk.StorageAccountResourceGroupName)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountResourceGroupName { get; set; }

        public override void ExecuteCmdlet()
        {
            ExecutionBlock(() =>
            {
                base.ExecuteCmdlet();

                string storageAccountId, storageAccountlocation, storageAccountType;
                GetStorageAccountDetails(out storageAccountId, out storageAccountlocation, out storageAccountType);

                WriteDebug(string.Format("StorageId = {0}", storageAccountId));

                PsBackupProviderManager providerManager = new PsBackupProviderManager(
                    new Dictionary<Enum, object>()
                {
                    {RestoreBackupItemParams.RecoveryPoint, RecoveryPoint},
                    {RestoreBackupItemParams.StorageAccountId, storageAccountId},
                    {RestoreBackupItemParams.StorageAccountLocation, storageAccountlocation},
                    {RestoreBackupItemParams.StorageAccountType, storageAccountType}
                }, ServiceClientAdapter);

                IPsBackupProvider psBackupProvider = providerManager.GetProviderInstance(
                    RecoveryPoint.WorkloadType, RecoveryPoint.BackupManagementType);
                var jobResponse = psBackupProvider.TriggerRestore();

                WriteDebug(string.Format("Restore submitted"));
                HandleCreatedJob(jobResponse, Resources.RestoreOperation);
            });
        }

        private void GetStorageAccountDetails(out string storageAccountId, out string storageAccountlocation, out string storageAccountType)
        {
            try
            {
                TryGetClassicStorageAccount(out storageAccountId, out storageAccountlocation, out storageAccountType);
            }
            catch (Exception)
            {
                TryGetComputeStorageAccount(out storageAccountId, out storageAccountlocation, out storageAccountType);
            }
        }

        private void TryGetClassicStorageAccount(out string storageAccountId, out string storageAccountlocation, out string storageAccountType)
        {
            var storageAccountName = StorageAccountName.ToLower();
            ResourceIdentity identity = new ResourceIdentity();
            identity.ResourceName = storageAccountName;
            identity.ResourceProviderNamespace = "Microsoft.ClassicStorage/storageAccounts";
            identity.ResourceProviderApiVersion = "2015-12-01";
            identity.ResourceType = string.Empty;

            GenericResource resource = null;
            WriteDebug(string.Format("Query Microsoft.ClassicStorage with name = {0}",
                    storageAccountName));
            resource = RmClient.Resources.GetAsync(
                StorageAccountResourceGroupName,
                identity.ResourceProviderNamespace,
                identity.ParentResourcePath,
                identity.ResourceType,
                identity.ResourceName,
                identity.ResourceProviderApiVersion,
                CancellationToken.None).Result;

            storageAccountId = resource.Id;
            storageAccountlocation = resource.Location;
            storageAccountType = resource.Type;
        }

        private void TryGetComputeStorageAccount(out string storageAccountId, out string storageAccountlocation, out string storageAccountType)
        {
            StorageModels.StorageAccount storageAccountDetails = null;
            storageAccountDetails = this.StorageClient.StorageAccounts.GetPropertiesWithHttpMessagesAsync(
                       StorageAccountResourceGroupName,
                       StorageAccountName).Result.Body;

            if (storageAccountDetails.Kind == StorageModels.Kind.BlobStorage)
            {
                throw new ArgumentException(String.Format(Resources.UnsupportedStorageAccountException,
                storageAccountDetails.Kind.ToString(), StorageAccountName));
            }

            storageAccountId = storageAccountDetails.Id;
            storageAccountlocation = storageAccountDetails.Location;
            storageAccountType = storageAccountDetails.Type;
        }
    }
}
