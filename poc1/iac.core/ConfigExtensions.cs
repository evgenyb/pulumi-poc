using Pulumi;

namespace iac.core
{
    public static class ConfigExtensions
    {
        public static string GetSubnetAddressRange(this Config self, string subnetName)
        {
            return self.Require($"vnet.subnets.{subnetName}.addressPrefixes");
        }
    }
}