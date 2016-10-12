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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ServiceClientAdapterNS
{
    public partial class ServiceClientAdapter
    {
        /// <summary>
        /// Gets result of the refresh container operation using the operation tracking URL
        /// </summary>
        /// <param name="operationResultLink">Operation tracking URL</param>
        /// <returns>Job response returned by the service</returns>
        public BaseRecoveryServicesJobResponse GetRefreshContainerOperationResultByURL(
                string operationResultLink)
        {
            string resourceName = BmsAdapter.GetResourceName();
            string resourceGroupName = BmsAdapter.GetResourceGroupName();

            return BmsAdapter.Client.Containers.GetRefreshOperationResultByURLAsync(
                                     operationResultLink,
                                     cancellationToken: BmsAdapter.CmdletCancellationToken).Result;
        }

        /// <summary>
        /// Gets result of a generic operation on the protected item using the operation tracking URL
        /// </summary>
        /// <param name="operationResultLink">Operation tracking URL</param>
        /// <returns>Operation status response returned by the service</returns>
        public BackUpOperationStatusResponse GetProtectedItemOperationStatusByURL(
                string operationResultLink)
        {
            string resourceName = BmsAdapter.GetResourceName();
            string resourceGroupName = BmsAdapter.GetResourceGroupName();

            return BmsAdapter.Client.GetOperationStatusByURLAsync(
                                     operationResultLink,
                                     cancellationToken: BmsAdapter.CmdletCancellationToken).Result;
        }
    }
}
