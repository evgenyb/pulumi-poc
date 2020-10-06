using System.Collections.Generic;
using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Helm;
using Pulumi.Kubernetes.Helm.V2;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Yaml;
using Deployment = Pulumi.Deployment;

class k8sStack : Stack
{
    private readonly Config _config;

    public k8sStack()
    {
        _config = new Config();
        
        DeployNamespaces();
        DeployMsi();
        DeployIngressController();
    }

    private void DeployIngressController()
    {
        /*
         * controller:
  config:
    disable-access-log: 'true'
    generate-request-id: 'false'
    load-balancer: ewma
    server-tokens: 'false'
    use-forwarded-headers: 'true'
  image:
    tag: v0.34.1
  ingressClass: nginx-internal
  metrics:
    enabled: true
    service:
      clusterIP: 10.0.13.40
  replicaCount: 2
  service:
    annotations:
      service.beta.kubernetes.io/azure-load-balancer-internal: 'true'
    clusterIP: 10.0.2.187
    loadBalancerIP: 10.2.15.10
  stats:
    enabled: true
    service:
      clusterIP: 10.0.3.74
defaultBackend:
  service:
    clusterIP: 10.0.2.115
         * 
         */
        var values = new Dictionary<string, object>
        {
            ["controller"] = new Dictionary<string, object>
            {
                ["ingressClass"] = "nginx-internal",
                ["replicaCount"] = 2,
                ["config"] = new Dictionary<string, object>
                {
                    ["disable-access-log"] = "true",
                    ["generate-request-id"] = "false",
                    ["load-balancer"] = "ewma",
                    ["server-tokens"] = "false",
                    ["use-forwarded-headers"] = "true"
                },
                ["image"] = new Dictionary<string, object>
                {
                    ["tag"] = _config.Require("nginx.version")
                },
                ["metrics"] = new Dictionary<string, object>
                {
                    ["enabled"] = "true",
                    ["service"] = new Dictionary<string, object>
                    {
                        ["clusterIP"] = "10.2.2.40"                        
                    }
                },
                ["service"] = new Dictionary<string, object>
                {
                    ["annotations"] = new Dictionary<string, object>
                    {
                        ["service.beta.kubernetes.io/azure-load-balancer-internal"] = "true"                        
                    },
                    ["clusterIP"] = "10.2.2.187",
                    ["loadBalancerIP"] = _config.Require("nginx.loadBalancerIP") 
                },
                ["stats"] = new Dictionary<string, object>
                {
                    ["enabled"] = "true",
                    ["service"] = new Dictionary<string, object>
                    {
                        ["clusterIP"] = "10.2.2.74"                        
                    }                
                },
            },
            ["defaultBackend"] = new Dictionary<string, object>
            {
                ["service"] = new Dictionary<string, object>
                {
                    ["clusterIP"] = "10.2.2.115"
                }
            }
        };
        
        var chart = new Chart("nginx-ingress", new ChartArgs
        {
            Chart = "nginx-ingress",
            Namespace = "kube-system",
            FetchOptions = new ChartFetchArgs
            {
                Repo = "https://kubernetes-charts.storage.googleapis.com/"
            },
            Values = values
        });
    }

    private static void DeployNamespaces()
    {
        var namespaces = new ConfigFile("ns", new ConfigFileArgs
        {
            File = "01-namespaces/namespaces.yaml"
        });

        var ns = new Namespace("lab", new NamespaceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Name = Deployment.Instance.StackName
            }
        });
    }
    
    private static void DeployMsi()
    {
        var msi = new ConfigFile("msi", new ConfigFileArgs
        {
            File = "02-msi/msi-infrastructure-rbac.yaml"
        });
    }
}
