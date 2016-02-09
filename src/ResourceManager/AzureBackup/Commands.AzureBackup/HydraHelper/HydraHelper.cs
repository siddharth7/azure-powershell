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

using BackupServicesNS = Microsoft.Azure.Management.BackupServices;
using BackupServicesModelsNS = Microsoft.Azure.Management.BackupServices.Models;
using RecoveryServicesModelsNS = Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using RecoveryServicesNS = Microsoft.Azure.Management.RecoveryServices.Backup;
using System;

namespace Microsoft.Azure.Commands.AzureBackup.Client
{
    public partial class HydraHelper
    {
        const string AzureFabricName = "AzureIaasVM";
        const string RecoveryServicesResourceNamespace = "Microsoft.RecoveryServices";

        public ClientAdapter<BackupServicesNS.BackupServicesManagementClient, BackupServicesModelsNS.CustomRequestHeaders> BackupBmsAdapter;

        public ClientAdapter<BackupServicesNS.BackupVaultServicesManagementClient, BackupServicesModelsNS.CustomRequestHeaders> BackupIdmAdapter;

        public ClientAdapter<RecoveryServicesNS.RecoveryServicesBackupManagementClient, RecoveryServicesModelsNS.CustomRequestHeaders> RecoveryServicesBmsAdapter;

        public HydraHelper(SubscriptionCloudCredentials creds, Uri baseUri)
        {
            BackupBmsAdapter = new ClientAdapter<BackupServicesNS.BackupServicesManagementClient, BackupServicesModelsNS.CustomRequestHeaders>(
                clientRequestId => new BackupServicesModelsNS.CustomRequestHeaders() { ClientRequestId = clientRequestId },
                creds, baseUri);

            BackupIdmAdapter = new ClientAdapter<BackupServicesNS.BackupVaultServicesManagementClient, BackupServicesModelsNS.CustomRequestHeaders>(
                clientRequestId => new BackupServicesModelsNS.CustomRequestHeaders() { ClientRequestId = clientRequestId },
                creds, baseUri);

            // TODO: See if we can take RecoveryServicesResourceNamespace from a config file
            RecoveryServicesBmsAdapter = new ClientAdapter<RecoveryServicesNS.RecoveryServicesBackupManagementClient, RecoveryServicesModelsNS.CustomRequestHeaders>(
                clientRequestId => new RecoveryServicesModelsNS.CustomRequestHeaders() { ClientRequestId = clientRequestId },
                RecoveryServicesResourceNamespace, creds, baseUri);
        }

        public string GetBackupBmsClientRequestId()
        {
            return BackupBmsAdapter.GetClientRequestId();
        }

        public string GetBackupIdmClientRequestId()
        {
            return BackupIdmAdapter.GetClientRequestId();
        }

        public string GetRecoveryServicesBmsClientRequestId()
        {
            return RecoveryServicesBmsAdapter.GetClientRequestId();
        }
    }
}