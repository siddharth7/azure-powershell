using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ServiceClientAdapterNS
{
    public class ClientRequestIdHandler : DelegatingHandler, ICloneable
    {
        const string RequestIdHeaderName = "x-ms-client-request-id";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains(RequestIdHeaderName))
            {
                request.Headers.Remove(RequestIdHeaderName);
            }

            string headerValue = Guid.NewGuid().ToString() + "-PS";
            request.Headers.TryAddWithoutValidation(RequestIdHeaderName, headerValue);

            return base.SendAsync(request, cancellationToken);
        }

        public object Clone()
        {
            return new ClientRequestIdHandler();
        }
    }
}
