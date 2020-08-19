using iac.core;
using Pulumi;
using Pulumi.Azure.Core;

class BaseStack : Stack
{
    public BaseStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("rg", new ResourceGroupArgs()
        {
            Name = NamingConvention.GetResourceGroupName(Deployment.Instance.StackName)
        });

    }
}
