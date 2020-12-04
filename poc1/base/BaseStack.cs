using iac.core;
using Pulumi;
using Pulumi.Azure.Core;
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
    }
}
