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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Commands.RecoveryServices.Backup.Properties;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets
{
    /// <summary>
    /// Provides a capability to explore a selected recovery point for the items that can be recovered.
    /// In the IaaS VM case, a connection has to be made to the iSCSI server in Azure to be able to browse the items.
    /// On successful connection, a script will be downloaded at the specified target location.
    /// When the user runs the script, it will pop up a file explorer window which can be used to explore and restore the items
    /// backed up as part of this recovery point.
    /// </summary>
    [Cmdlet("Explore", "AzureRmRecoveryServicesBackupRecoveryPoint", DefaultParameterSetName = ConnectParamSet),
        OutputType(typeof(RecoveryPointBase), typeof(IList<RecoveryPointBase>))]
    public class ExploreAzureRmRecoveryServicesBackupRecoveryPoint : RecoveryServicesBackupCmdletBase
    {
        const string ConnectParamSet = "Connect";
        const string ExtendParamSet = "Extend";
        const string TerminateParamSet = "Terminate";

        /// <summary>
        /// Recovery point whose items are to be explored.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = false,
            Position = 0, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRRecoveryPoint)]
        [ValidateNotNullOrEmpty]
        public RecoveryPointBase RecoveryPoint { get; set; }

        /// <summary>
        /// Switch parameter to specify if we want to initiate a new iSCSI connection.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ConnectParamSet, ValueFromPipeline = false,
            Position = 1, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRConnect)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Connect { get; set; }

        /// <summary>
        /// Switch parameter to specify if we want to extend an existing iSCSI connection.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ExtendParamSet, ValueFromPipeline = false,
            Position = 1, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRExtend)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Extend { get; set; }

        /// <summary>
        /// Switch parameter to specify if we want to terminate an existing iSCSI connection.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = TerminateParamSet, ValueFromPipeline = false,
            Position = 1, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRTerminate)]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Terminate { get; set; }

        /// <summary>
        /// Location where to download the script.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ConnectParamSet, ValueFromPipeline = false,
            Position = 2, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRConnect)]
        [Parameter(Mandatory = true, ParameterSetName = ExtendParamSet, ValueFromPipeline = false,
            Position = 2, HelpMessage = ParamHelpMsgs.RecoveryPoint.ILRConnect)]
        [ValidateNotNullOrEmpty]
        public string TargetLocation { get; set; }

        public override void ExecuteCmdlet()
        {
            ExecutionBlock(() =>
            {
                PsBackupProviderManager providerManager =
                    new PsBackupProviderManager(new Dictionary<System.Enum, object>()
                {
                    {RecoveryPointParams.RecoveryPoint, RecoveryPoint},
                    {RecoveryPointParams.ILRAction, EnumUtils.GetEnum<ILRAction>(this.ParameterSetName)},
                    {RecoveryPointParams.TargetLocation, TargetLocation},
                }, ServiceClientAdapter);

                IPsBackupProvider psBackupProvider = null;
                psBackupProvider = providerManager.GetProviderInstance(RecoveryPoint.ContainerType, RecoveryPoint.BackupManagementType);
                psBackupProvider.ExploreRecoveryPoint();
            });
        }
    }
}