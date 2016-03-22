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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Get list of containers
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmRecoveryServicesContainer"), OutputType(typeof(List<AzureRmRecoveryServicesContainerBase>), typeof(AzureRmRecoveryServicesContainerBase))]
    public class GetAzureRmRecoveryServicesContainer : RecoveryServicesBackupCmdletBase
    {
        [Parameter(Mandatory = true, HelpMessage = ParamHelpMsg.Container.ContainerType)]
        [ValidateNotNullOrEmpty]
        public ContainerType ContainerType { get; set; }

        [Parameter(Mandatory = false, HelpMessage = ParamHelpMsg.Container.Name)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(Mandatory = false, HelpMessage = ParamHelpMsg.Container.ResourceGroupName)]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = true, HelpMessage = ParamHelpMsg.Container.Status)]
        [ValidateNotNullOrEmpty]
        public ContainerRegistrationStatus Status { get; set; }

        public override void ExecuteCmdlet()
        {
            ExecutionBlock(() =>
            {
                base.ExecuteCmdlet();

                PsBackupProviderManager providerManager = new PsBackupProviderManager(new Dictionary<System.Enum, object>()
                {  
                    {ContainerParams.ContainerType, ContainerType},
                    {ContainerParams.Name, Name},
                    {ContainerParams.ResourceGroupName, ResourceGroupName},
                    {ContainerParams.Status, Status},
                }, HydraAdapter);

                IPsBackupProvider psBackupProvider = providerManager.GetProviderInstance(ContainerType);
                var containerModels = psBackupProvider.ListProtectionContainers();

                if (containerModels.Count == 1)
                {
                    WriteObject(containerModels.First());
                }
                else
                {
                    WriteObject(containerModels);
                }
            });
        }
    }
}