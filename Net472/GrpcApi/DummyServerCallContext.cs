using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace GrpcApi
{
    public class DummyServerCallContext : ServerCallContext
    {
        protected override string MethodCore { get; }

        protected override string HostCore { get; }

        protected override string PeerCore { get; }

        protected override DateTime DeadlineCore { get; }

        protected override Metadata RequestHeadersCore { get; }

        protected override CancellationToken CancellationTokenCore { get; }

        protected override Metadata ResponseTrailersCore { get; }

        protected override Status StatusCore { get; set; }

        protected override WriteOptions WriteOptionsCore { get; set; }

        protected override AuthContext AuthContextCore { get; }

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
        {
            return null;
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            return Task.CompletedTask;
        }
    }
}
