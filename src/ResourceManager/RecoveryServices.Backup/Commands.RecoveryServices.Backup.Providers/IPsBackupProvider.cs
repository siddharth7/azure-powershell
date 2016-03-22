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
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ProviderModel
{
    public interface IPsBackupProvider
    {
        void Initialize(ProviderData providerData, HydraAdapter.HydraAdapter hydraAdapter);

        BaseRecoveryServicesJobResponse EnableProtection();

        BaseRecoveryServicesJobResponse DisableProtection();

        BaseRecoveryServicesJobResponse TriggerBackup();

        BaseRecoveryServicesJobResponse TriggerRestore();

        ProtectedItemResponse GetProtectedItem();
        
        AzureRmRecoveryServicesRecoveryPointBase GetRecoveryPointDetails();

        List<AzureRmRecoveryServicesRecoveryPointBase> ListRecoveryPoints();

        ProtectionPolicyResponse CreatePolicy();

        List<AzureRmRecoveryServicesJobBase> ModifyPolicy();

        ProtectionPolicyResponse GetPolicy();

        AzureRmRecoveryServicesSchedulePolicyBase GetDefaultSchedulePolicyObject();

        AzureRmRecoveryServicesRetentionPolicyBase GetDefaultRetentionPolicyObject();
        void DeletePolicy();

        List<AzureRmRecoveryServicesContainerBase> ListProtectionContainers();
    }
}