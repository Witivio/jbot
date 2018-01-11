using System;
using System.Collections.Generic;
using System.Text;

namespace Witivio.JBot.Core.Configuration
{
    public class ConfigurationKeys
    {
        public static class Runtime
        {
            public static string Mode => "Runtime:Mode";
        }


        public static class Azure
        {
            public static string SubscriptionId => "AppSettings:Azure:SubscriptionId";
            public static string ResourcesGroupName => "AppSettings:Azure:ResourcesGroupName";
            public static string TenantId => "AppSettings:Azure:TenantId";
            public static string ClientId => "AppSettings:Azure:ClientId";
            public static string ClientSecret => "AppSettings:Azure:ClientSecret";

            public static string ActiveDirectoryAuthority => "AppSettings:Azure:ActiveDirectoryAuthority";

            public static string ServiceManagementUrl => "AppSettings:Azure:ServiceManagementUrl";

        }

        public static class Credentials
        {
            public static string UseKeyVault => "Credentials:UseKeyVault";

            public static string DirectLine => "Credentials:DirectLine:Key";

            public static string Email => "Credentials:S4b:Email";

            public static string Password => "Credentials:S4b:Password";

            public static string BotId => "Credentials:BotId";
            public static string AADApplicationId => "Credentials:S4b:AADApplicationId";
            public static string DomainLogin => "Credentials:S4b:DomainLogin";
        }

        public static class ConnectorLine
        {
            public static string Url => "AppSettings:ConnectorLine:Url";
        }

        public static class KeyVault
        {
            public static string Url => "AppSettings:KeyVault:Url";
        }

        public static class Storage
        {
            public static string ConnectionString => "AppSettings:Storage:ConnectionString";
        }
    }
}
