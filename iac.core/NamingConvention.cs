namespace iac.core
{
    public static class NamingConvention
    {
        private const string BasePrefix = "iac";
        public static string GetResourceGroupName(string environment)
        {
            return $"{BasePrefix}-{environment}-rg";
        }

        public static string GetVNetName(string environment)
        {
            return $"{BasePrefix}-{environment}-vnet";
        }

        public static string GetAGWName(string agwName, string environment)
        {
            return $"{BasePrefix}-{environment}-{agwName}-agw";
        }

        public static string GetPublicIpName(string pipName, string environment)
        {
            return $"{BasePrefix}-{environment}-{pipName}-pip";
        }

        public static string GetManagedIdentityName(string miName, string environment)
        {
            return $"{BasePrefix}-{environment}-{miName}-mi";
        }
    }
}