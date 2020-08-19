using iac.core;
using Pulumi;
using Pulumi.Azure.Authorization;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using Pulumi.Azure.Network.Inputs;

class WorkloadStack : Stack
{
    public WorkloadStack()
    {
        var config = new Config();
        var environment = Deployment.Instance.StackName;
        
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs
        {
            Name = NamingConvention.GetResourceGroupName(environment),
            Tags =
            {
                { "owner", Constants.TeamPlatform },
                { "environment", environment }
            }
        });
        
        var vnet = new VirtualNetwork("vnet", new VirtualNetworkArgs
        {
            Name = NamingConvention.GetVNetName(environment),
            ResourceGroupName = resourceGroup.Name,
            AddressSpaces =
            {
                config.Require("vnetAddressSpaces")
            },
            Tags = 
            {
                { "owner", Constants.TeamPlatform },
                { "environment", environment }
            }
        });

        // Create a Subnet for the cluster
        var aksSubnet = new Subnet("aks-net", new SubnetArgs
        {
            Name = "aks-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("aksSubnetAddressPrefixes")
            },
        });
        
        var agwSubnet = new Subnet("agw-net", new SubnetArgs
        {
            Name = "agw-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("agwSubnetAddressPrefixes")
            },
        });

        var agwName = NamingConvention.GetAGWName("api", environment);
        var publicId = new PublicIp("agw-api-pip", new PublicIpArgs
        {
            Name = NamingConvention.GetPublicIpName("agw-api", environment),
            ResourceGroupName = resourceGroup.Name,
            Sku = "Standard",
            AllocationMethod = "Static",
            DomainNameLabel = agwName
        });
        
        var agwMI = new UserAssignedIdentity("agw-mi", new UserAssignedIdentityArgs
        {
            Name = NamingConvention.GetManagedIdentityName("agw", environment),
            ResourceGroupName = resourceGroup.Name
        });
        
        var agw = new ApplicationGateway("agw-api", new ApplicationGatewayArgs
        {
            Name = agwName,
            ResourceGroupName = resourceGroup.Name,
            Identity = new ApplicationGatewayIdentityArgs
            {
                Type = "UserAssigned",
                IdentityIds = agwMI.Id
            },
            Sku = new ApplicationGatewaySkuArgs
            {
                Name = "WAF_v2",
                Tier = "WAF_v2",
                Capacity = 1
            },
            SslCertificates = new []
            {
                new ApplicationGatewaySslCertificateArgs
                {
                    Name = "gateway-listener",
                    KeyVaultSecretId = config.Require("keyVaultSecretId")
                }
            },
            FrontendPorts = new []
            {
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = "port443",
                    Port = 443
                },
                new ApplicationGatewayFrontendPortArgs
                {
                    Name = "port80",
                    Port = 80
                }
            },
            GatewayIpConfigurations = new []
            {
                new ApplicationGatewayGatewayIpConfigurationArgs
                {
                    Name = "appGatewayIpConfig",
                    SubnetId = agwSubnet.Id
                }
            },
            FrontendIpConfigurations = new []
            {
                new ApplicationGatewayFrontendIpConfigurationArgs
                {
                    Name = "appGatewayFrontendIP",
                    PublicIpAddressId = publicId.Id
                }
            },
            HttpListeners = new []
            {
                new ApplicationGatewayHttpListenerArgs
                {
                    Name = "gateway-listener",    
                    FrontendIpConfigurationName = "appGatewayFrontendIP",
                    FrontendPortName = "port443",
                    Protocol = "Https",
                    HostName = $"{agwName}.iac-labs.com",
                    RequireSni = true,
                    SslCertificateName = "gateway-listener"
                }
            },
            BackendAddressPools = new[]
            {
                new ApplicationGatewayBackendAddressPoolArgs
                {
                    Name = "api-a-pool",
                    Fqdns = new []
                    {
                        "api-a.iac-lab-green-aks.iac-labs.com"
                    }
                }
            },
            Probes = new[]
            {
                new ApplicationGatewayProbeArgs
                {
                    Name = "api-a-probe",
                    Protocol = "Http",
                    Path = "/health",
                    Interval = 30,
                    Timeout = 30,
                    UnhealthyThreshold = 3,
                    PickHostNameFromBackendHttpSettings = true,
                    MinimumServers = 0
                }
            },
            BackendHttpSettings = new []
            {
                new ApplicationGatewayBackendHttpSettingArgs
                {
                    Name = "api-a-settings",
                    Port = 80,
                    Protocol = "Http",
                    CookieBasedAffinity = "Disabled",
                    PickHostNameFromBackendAddress = true,
                    AffinityCookieName = "ApplicationGatewayAffinity",
                    RequestTimeout = 30,
                    ProbeName = "api-a-probe"
                    
                }
            },
            RequestRoutingRules = new[]
            {
                new ApplicationGatewayRequestRoutingRuleArgs
                {
                    Name = "api-a",
                    RuleType = "Basic",
                    HttpListenerName = "gateway-listener",
                    BackendAddressPoolName = "api-a-pool",
                    BackendHttpSettingsName = "api-a-settings"
                }
            }
        });
    }
}
