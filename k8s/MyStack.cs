using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.Apps.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Yaml;
using Deployment = Pulumi.Deployment;

class MyStack : Stack
{
    public MyStack()
    {
        
        var workloadStack = new StackReference($"evgenyb/iac-workload/{Deployment.Instance.StackName}");
        
        // Create a k8s provider pointing to the kubeconfig.
        var k8sProvider = new Pulumi.Kubernetes.Provider("k8s", new Pulumi.Kubernetes.ProviderArgs
        {
            KubeConfig = workloadStack.RequireOutput("KubeConfig").Apply(x => x.ToString()!)
        });

        var options = new ComponentResourceOptions
        {
            Provider = k8sProvider
        };

        DeployNamespaces(options, k8sProvider);
        DeployMsi(options);
    }

    private static void DeployNamespaces(ComponentResourceOptions options, Provider provider)
    {
        var namespaces = new ConfigFile("ns", new ConfigFileArgs
        {
            File = "01-namespaces/namespaces.yaml"
        }, options);

        var ns = new Namespace("lab", new NamespaceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Name = Deployment.Instance.StackName
            }
        }, new CustomResourceOptions
        {
            Provider = provider
        });
    }
    
    private static void DeployMsi(ComponentResourceOptions options)
    {
        var msi = new ConfigFile("msi", new ConfigFileArgs
        {
            File = "02-msi/msi-infrastructure-rbac.yaml"
        }, options);
    }
}
