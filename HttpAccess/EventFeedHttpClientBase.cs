using System;

using SKBKontur.Catalogue.ClientLib.Domains;
using SKBKontur.Catalogue.ClientLib.HttpClientBases;
using SKBKontur.Catalogue.ClientLib.HttpClientBases.Configuration;
using SKBKontur.Catalogue.ClientLib.Topology;

namespace SKBKontur.Catalogue.Core.EventFeeds.HttpAccess
{
    public abstract class EventFeedHttpClientBase : HttpClientBase
    {
        protected EventFeedHttpClientBase(IDomainTopologyFactory domainTopologyFactory, IMethodDomainFactory methodDomainFactory, IHttpServiceClientConfiguration initialConfiguration)
            : base(domainTopologyFactory, methodDomainFactory, initialConfiguration)
        {
        }

        public void UpdateAndFlush(string key)
        {
            Method("UpdateAndFlush").InvokeOnRandomReplica(key);
        }
        
        public void UpdateAndFlushAll(TimeSpan delayUpperBound)
        {
            Method("UpdateAndFlushAll").InvokeOnRandomReplica(delayUpperBound);
        }

        protected override IHttpServiceClientConfiguration DoGetConfiguration(IHttpServiceClientConfiguration defaultConfiguration)
        {
            return defaultConfiguration.WithTimeout(TimeSpan.FromMinutes(1));
        }
    }
}