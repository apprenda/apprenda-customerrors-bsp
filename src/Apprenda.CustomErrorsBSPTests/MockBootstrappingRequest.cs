using Apprenda.API.Extension.Bootstrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.API.Extension.CustomProperties;

namespace Apprenda.CustomErrorsBSPTests
{
    public class MockBootstrappingRequest : BootstrappingRequest
    {
        private string devTeamAlias;
        private bool authNEnabled;
        private bool authZEnabled;
        private bool multitenancyEnabled;
        private bool billingEnabled;
        private Stage stage;

        public MockBootstrappingRequest(string componentPath, ComponentType componentType, Guid componentId, Guid instanceId, string componentName, string applicationAlias, string versionAlias, IEnumerable<CustomProperty> properties) : base(componentPath, componentType, componentId, instanceId, componentName, applicationAlias, versionAlias, properties)
        {
        }

        public MockBootstrappingRequest(string componentPath, ComponentType componentType, Guid componentId, Guid instanceId, string componentName, string applicationAlias, string versionAlias, IEnumerable<CustomProperty> properties, string devTeamAlias, bool authNEnabled, bool authZEnabled, bool multitenancyEnabled, bool billingEnabled, Stage stage) : base(componentPath, componentType, componentId, instanceId, componentName, applicationAlias, versionAlias, properties)
        {
            this.devTeamAlias = devTeamAlias;
            this.authNEnabled = authNEnabled;
            this.authZEnabled = authZEnabled;
            this.multitenancyEnabled = multitenancyEnabled;
            this.billingEnabled = billingEnabled;
            this.stage = stage;
        }

        public override string DevelopmentTeamAlias
        {
            get { return devTeamAlias; }
        }

        public override bool IsAuthenticationEnabled
        {
            get { return authNEnabled; }
        }

        public override bool IsAuthorizationEnabled
        {
            get { return authZEnabled; }
        }

        public override bool IsBillingEnabled
        {
            get { return billingEnabled; }
        }

        public override bool IsMultitenancyEnabled
        {
            get { return multitenancyEnabled; }
        }

        public override Stage Stage
        {
            get { return stage; }
        }
    }
}
