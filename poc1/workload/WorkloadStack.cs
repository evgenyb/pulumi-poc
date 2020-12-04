using iac.core;
using Pulumi;
using Pulumi.Azure.ContainerService;
using Pulumi.Azure.ContainerService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;

class WorkloadStack : Stack
{
    public WorkloadStack()
    {
        
        var baseStack = new StackReference("evgenyb/iac-base/lab");
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
                config.Require("vnet.addressSpaces")
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
                config.Require("vnet.subnets.aks.addressPrefixes")
            },
        });
        
        var agwSubnet = new Subnet("agw-net", new SubnetArgs
        {
            Name = "agw-net",
            ResourceGroupName = resourceGroup.Name,
            VirtualNetworkName = vnet.Name,
            AddressPrefixes =
            {
                config.Require("vnet.subnets.agw.addressPrefixes")
            },
        });
        
        // var agwName = NamingConvention.GetAGWName("api", environment);
        // var agwPublicIp = new PublicIp("agw-api-pip", new PublicIpArgs
        // {
        //     Name = NamingConvention.GetPublicIpName("agw-api", environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Sku = "Standard",
        //     AllocationMethod = "Static",
        //     DomainNameLabel = agwName
        // });
        //
        // var agwMI = new UserAssignedIdentity("agw-mi", new UserAssignedIdentityArgs
        // {
        //     Name = NamingConvention.GetManagedIdentityName("agw", environment),
        //     ResourceGroupName = resourceGroup.Name
        // });
        //
        // var apiAgw = new ApplicationGateway("agw-api", new ApplicationGatewayArgs
        // {
        //     Name = agwName,
        //     ResourceGroupName = resourceGroup.Name,
        //     Identity = new ApplicationGatewayIdentityArgs
        //     {
        //         Type = "UserAssigned",
        //         IdentityIds = agwMI.Id
        //     },
        //     Sku = new ApplicationGatewaySkuArgs
        //     {
        //         Name = "WAF_v2",
        //         Tier = "WAF_v2",
        //         Capacity = 1
        //     },
        //     SslCertificates = new []
        //     {
        //         new ApplicationGatewaySslCertificateArgs
        //         {
        //             Name = "gateway-listener",
        //             KeyVaultSecretId = config.Require("keyVaultSecretId")
        //         }
        //     },
        //     FrontendPorts = new []
        //     {
        //         new ApplicationGatewayFrontendPortArgs
        //         {
        //             Name = "port443",
        //             Port = 443
        //         },
        //         new ApplicationGatewayFrontendPortArgs
        //         {
        //             Name = "port80",
        //             Port = 80
        //         }
        //     },
        //     GatewayIpConfigurations = new []
        //     {
        //         new ApplicationGatewayGatewayIpConfigurationArgs
        //         {
        //             Name = "appGatewayIpConfig",
        //             SubnetId = agwSubnet.Id
        //         }
        //     },
        //     FrontendIpConfigurations = new []
        //     {
        //         new ApplicationGatewayFrontendIpConfigurationArgs
        //         {
        //             Name = "appGatewayFrontendIP",
        //             PublicIpAddressId = agwPublicIp.Id
        //         }
        //     },
        //     HttpListeners = new []
        //     {
        //         new ApplicationGatewayHttpListenerArgs
        //         {
        //             Name = "gateway-listener",    
        //             FrontendIpConfigurationName = "appGatewayFrontendIP",
        //             FrontendPortName = "port443",
        //             Protocol = "Https",
        //             HostName = "iac-lab-api.iac-labs.com",
        //             RequireSni = true,
        //             SslCertificateName = "gateway-listener"
        //         },
        //         new ApplicationGatewayHttpListenerArgs
        //         {
        //             Name = "management-listener",    
        //             FrontendIpConfigurationName = "appGatewayFrontendIP",
        //             FrontendPortName = "port443",
        //             Protocol = "Https",
        //             HostName = "iac-lab-management.iac-labs.com",
        //             RequireSni = true,
        //             SslCertificateName = "gateway-listener"
        //         },
        //         new ApplicationGatewayHttpListenerArgs
        //         {
        //             Name = "portal-listener",    
        //             FrontendIpConfigurationName = "appGatewayFrontendIP",
        //             FrontendPortName = "port443",
        //             Protocol = "Https",
        //             HostName = "iac-lab-portal.iac-labs.com",
        //             RequireSni = true,
        //             SslCertificateName = "gateway-listener"
        //         }
        //     },
        //     BackendAddressPools = new[]
        //     {
        //         new ApplicationGatewayBackendAddressPoolArgs
        //         {
        //             Name = "apim-backend-pool",
        //             IpAddresses = config.RequireSecret("apim.backend.ip")
        //         }
        //     },
        //     Probes = new[]
        //     {
        //         new ApplicationGatewayProbeArgs
        //         {
        //             Name = "apim-probe",
        //             Protocol = "Https",
        //             Path = "/status-0123456789abcdef",
        //             Host = "iac-lab-api.iac-labs.com",
        //             Interval = 30,
        //             Timeout = 120,
        //             UnhealthyThreshold = 8,
        //             PickHostNameFromBackendHttpSettings = false,
        //             MinimumServers = 0
        //         }
        //     },
        //     BackendHttpSettings = new []
        //     {
        //         new ApplicationGatewayBackendHttpSettingArgs
        //         {
        //             Name = "apim-settings",
        //             Port = 443,
        //             Protocol = "Https",
        //             CookieBasedAffinity = "Disabled",
        //             PickHostNameFromBackendAddress = false,
        //             RequestTimeout = 30,
        //             ProbeName = "apim-probe"
        //         }
        //     },
        //     RequestRoutingRules = new[]
        //     {
        //         new ApplicationGatewayRequestRoutingRuleArgs
        //         {
        //             Name = "gateway",
        //             RuleType = "Basic",
        //             HttpListenerName = "gateway-listener",
        //             BackendAddressPoolName = "apim-backend-pool",
        //             BackendHttpSettingsName = "apim-settings"
        //         },
        //         new ApplicationGatewayRequestRoutingRuleArgs
        //         {
        //             Name = "management",
        //             RuleType = "Basic",
        //             HttpListenerName = "management-listener",
        //             BackendAddressPoolName = "apim-backend-pool",
        //             BackendHttpSettingsName = "apim-settings"
        //         },
        //         new ApplicationGatewayRequestRoutingRuleArgs
        //         {
        //             Name = "portal",
        //             RuleType = "Basic",
        //             HttpListenerName = "portal-listener",
        //             BackendAddressPoolName = "apim-backend-pool",
        //             BackendHttpSettingsName = "apim-settings"
        //         },
        //     }
        // });
        //
        // var appInsight = new Insights("ai", new InsightsArgs
        // {
        //     Name = NamingConvention.GetAppInsightName(environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Location = resourceGroup.Location,
        //     ApplicationType = "web",
        //     
        // });

        // var la = new AnalyticsWorkspace("la", new AnalyticsWorkspaceArgs
        // {
        //     Name = NamingConvention.GetLogAnalyticsName(environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Location = resourceGroup.Location,
        //     Sku = "PerGB2018"
        // });
        
        var aksEgressPublicIp = new PublicIp("aks-egress-pip", new PublicIpArgs
        {
            Name = NamingConvention.GetPublicIpName("aks-egress", environment),
            ResourceGroupName = resourceGroup.Name,
            Sku = "Standard",
            AllocationMethod = "Static"
        });

        var logAnalyticsWorkspaceId = baseStack.RequireOutput("LogAnalyticsWorkspaceId").Apply(x => x.ToString());
        var aks = new KubernetesCluster("aks", new KubernetesClusterArgs
        {
            Name = NamingConvention.GetAksName(environment),
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Identity = new KubernetesClusterIdentityArgs
            {
                Type = "SystemAssigned"
            },
            DefaultNodePool = new KubernetesClusterDefaultNodePoolArgs
            {
                Name = "aksagentpool",
                NodeCount = 1,
                VmSize = "Standard_B2s",
                OsDiskSizeGb = 30,
                VnetSubnetId = aksSubnet.Id
            },
            DnsPrefix = "iacpulumiaks",
            RoleBasedAccessControl = new KubernetesClusterRoleBasedAccessControlArgs
            {
                Enabled = true,
                AzureActiveDirectory = new KubernetesClusterRoleBasedAccessControlAzureActiveDirectoryArgs
                {
                    AdminGroupObjectIds = config.RequireSecret("teamPlatformAADId"),
                    TenantId = config.RequireSecret("tenantId"),
                    Managed = true
                }
            },
            NetworkProfile = new KubernetesClusterNetworkProfileArgs
            {
                NetworkPlugin = "azure",
                NetworkPolicy = "calico",
                DnsServiceIp = "10.2.2.254",
                ServiceCidr = "10.2.2.0/24",
                DockerBridgeCidr = "172.17.0.1/16",
                LoadBalancerProfile = new KubernetesClusterNetworkProfileLoadBalancerProfileArgs
                {
                    OutboundIpAddressIds = new []
                    {
                        aksEgressPublicIp.Id             
                    }
                }
            },
            AddonProfile = new KubernetesClusterAddonProfileArgs
            {
                OmsAgent = new KubernetesClusterAddonProfileOmsAgentArgs
                {
                    Enabled = true,
                    LogAnalyticsWorkspaceId = logAnalyticsWorkspaceId
                },
                KubeDashboard = new KubernetesClusterAddonProfileKubeDashboardArgs
                {
                    Enabled = false
                }
            }
        });
        
        var pool = new KubernetesClusterNodePool("workload-pool", new KubernetesClusterNodePoolArgs
        {
            Name = "workload",
            KubernetesClusterId = aks.Id,
            Mode = "User",
            NodeCount = 1,
            VmSize = "Standard_B2s",
            OsDiskSizeGb = 30,
            VnetSubnetId = aksSubnet.Id,
            NodeLabels = 
            {
                {"disk", "ssd"},
                {"type", "workload"}
            }
        });

        this.KubeConfig = aks.KubeConfigRaw;
    }
    [Output] public Output<string> KubeConfig { get; set; }
}
