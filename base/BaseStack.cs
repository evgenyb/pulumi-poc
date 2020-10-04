using iac.core;
using iac.nsgs;
using Pulumi;
using Pulumi.Azure.ApiManagement;
using Pulumi.Azure.ApiManagement.Inputs;
using Pulumi.Azure.Authorization;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using Pulumi.Azure.Network.Inputs;
using Pulumi.Azure.OperationalInsights;

class BaseStack : Stack
{
    
    [Output] public Output<string> LogAnalyticsWorkspaceId { get; set; }
    public BaseStack()
    {
        var config = new Config();
        var environment = Deployment.Instance.StackName;

        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs()
        {
            Name = NamingConvention.GetResourceGroupName(Deployment.Instance.StackName)
        });

        var la = new AnalyticsWorkspace("la", new AnalyticsWorkspaceArgs
        {
            Name = NamingConvention.GetLogAnalyticsName(environment),
            ResourceGroupName = resourceGroup.Name,
            Location = resourceGroup.Location,
            Sku = "PerGB2018"
        });

        this.LogAnalyticsWorkspaceId = la.Id;
        
        // var vnet = new VirtualNetwork("vnet", new VirtualNetworkArgs
        // {
        //     Name = NamingConvention.GetVNetName(environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     AddressSpaces =
        //     {
        //         config.Require("vnet.addressSpaces")
        //     }
        // });
        //
        // var apimNSG = APIMNSG.Get(environment, resourceGroup, config);
        //
        // // Create a Subnet for the cluster
        // var apimSubnet = new Subnet("apim-net", new SubnetArgs
        // {
        //     Name = "apim-net",
        //     ResourceGroupName = resourceGroup.Name,
        //     VirtualNetworkName = vnet.Name,
        //     AddressPrefixes =
        //     {
        //         config.Require("vnet.subnets.apim.addressPrefixes")
        //     },
        // });
        //
        // var subnetNetworkSecurityGroupAssociation = new SubnetNetworkSecurityGroupAssociation("apimSubnetAssociation", new SubnetNetworkSecurityGroupAssociationArgs
        // {
        //     SubnetId = apimSubnet.Id,
        //     NetworkSecurityGroupId = apimNSG.Id
        // });
        //
        // // Create a Subnet for the cluster
        // var firewallSubnet = new Subnet("afw-net", new SubnetArgs
        // {
        //     Name = "AzureFirewallSubnet",
        //     ResourceGroupName = resourceGroup.Name,
        //     VirtualNetworkName = vnet.Name,
        //     AddressPrefixes =
        //     {
        //         config.Require("vnet.subnets.afw.addressPrefixes")
        //     },
        // });

        // var apimMI = new UserAssignedIdentity("apim-mi", new UserAssignedIdentityArgs
        // {
        //     Name = NamingConvention.GetManagedIdentityName("apim", environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Location = resourceGroup.Location
        // });

        // var apim = new Service("apim", new ServiceArgs
        // {
        //     Name = NamingConvention.GetApimName(environment),
        //     ResourceGroupName = resourceGroup.Name,
        //     Location = resourceGroup.Location,
        //     PublisherName = "IaC",
        //     PublisherEmail = "evgeny.borzenin@gmail.com",
        //     SkuName = "Developer_1",
        //     Identity = new ServiceIdentityArgs
        //     {
        //         Type = "SystemAssigned, UserAssigned",
        //         IdentityIds = apimMI.Id
        //     },
        //     
        //     VirtualNetworkType = "Internal",
        //     VirtualNetworkConfiguration = new ServiceVirtualNetworkConfigurationArgs
        //     {
        //         SubnetId = apimSubnet.Id
        //     },
        //     HostnameConfiguration = new ServiceHostnameConfigurationArgs
        //     {
        //         Proxies = new []
        //         {
        //             new ServiceHostnameConfigurationProxyArgs
        //             {
        //                 HostName = "iac-lab-api.iac-labs.com",
        //                 KeyVaultId = config.RequireSecret("apim.certificate.proxy.keyvaultid"),
        //                 DefaultSslBinding = true
        //             }
        //         },
        //         Managements = new []
        //         {
        //             new ServiceHostnameConfigurationManagementArgs
        //             {
        //                 HostName = "iac-lab-management.iac-labs.com",
        //                 KeyVaultId = config.RequireSecret("apim.certificate.proxy.keyvaultid")
        //             }
        //         },
        //         DeveloperPortals = new []
        //         {
        //             new ServiceHostnameConfigurationDeveloperPortalArgs
        //             {
        //                 HostName = "iac-lab-portal.iac-labs.com",
        //                 KeyVaultId = config.RequireSecret("apim.certificate.proxy.keyvaultid")
        //             }
        //         }
        //     }
        // });
    }
}
