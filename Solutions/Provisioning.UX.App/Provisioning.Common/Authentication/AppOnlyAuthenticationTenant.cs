﻿using Microsoft.SharePoint.Client;
using Provisioning.Common.Configuration;
using Provisioning.Common.Configuration.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Provisioning.Common;

namespace Provisioning.Common.Authentication
{
    /// <summary>
    /// Authenticated Class for using the Tenant Api
    /// </summary>
    public class AppOnlyAuthenticationTenant : IAuthentication
    {
        #region Instance Members
        const string LOGGING_SOURCE = "AppOnlyAuthentication";
        private static readonly IConfigurationFactory _cf = ConfigurationFactory.GetInstance();
        private static readonly IAppSettingsManager _manager = _cf.GetAppSetingsManager();

        private string _appID;
        private string _appSecret;
        private string _tenantAdminUrl;
        private string _realm;
        #endregion

        /// <summary>
        /// SiteUrl
        /// Currently Not Implemented
        /// </summary>
        public string SiteUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The SharePoint Realm
        /// </summary>
        public string Realm
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._realm))
                {
                    var _result = string.Empty;
                    if (!String.IsNullOrWhiteSpace(this.TenantAdminUrl))
                    {
                        this._realm = TokenHelper.GetRealmFromTargetUrl(new Uri(this.TenantAdminUrl));
                    }
                }
                return this._realm;
            }
            set
            {
                this._realm = value;
            }
        }
        /// <summary>
        /// AppID that is registered
        /// By Default this will read from the ClientId in your config file of your solution.
        /// </summary>
        public string AppId
        {
            get
            {
                this._appID = string.IsNullOrEmpty(this._appID) ? _manager.GetAppSettings().ClientID : this._appID;
                return this._appID;
            }
            set
            {
                this._appID = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string AppSecret
        {
            get
            {
                this._appSecret = string.IsNullOrEmpty(this._appSecret) ? _manager.GetAppSettings().ClientSecret : this._appSecret;
                return this._appSecret;
            }
            set { this._appSecret = value; }
        }
        /// <summary>
        /// The Client secret or app secret that has been registered.
        /// By Default this will read from the ClientSecret in your config file of your solution.
        /// </summary>
        private string AccessToken
        {
            get;
            set;
        }
        /// <summary>
        /// The tenant admin Url for the environment.
        /// By Default this will read from the TenantAdminUrl in your config file of your solution.
        /// </summary>
        public string TenantAdminUrl
        {
            get
            {
                this._tenantAdminUrl = string.IsNullOrEmpty(this._tenantAdminUrl) ? _manager.GetAppSettings().TenantAdminUrl : this._tenantAdminUrl;
                return this._tenantAdminUrl;
            }
            set
            {
                this._tenantAdminUrl = value;
            }
        }

        /// <summary>
        /// Returns am Authenticated ClientContext
        /// </summary>
        /// <returns></returns>
        public ClientContext GetAuthenticatedContext()
        {
            EnsureToken();
            var ctx = TokenHelper.GetClientContextWithAccessToken(TenantAdminUrl, AccessToken);
            return ctx;
        }

        /// <summary>
        /// Method to Ensure that an OAuth token is valid
        /// </summary>
        public void EnsureToken()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                var _oAuthResponse = TokenHelper.GetAppOnlyAccessToken(
                    TokenHelper.SharePointPrincipal,
                    new Uri(this.TenantAdminUrl).Authority,
                    this.Realm);

                this.AccessToken = _oAuthResponse.AccessToken;
            }
        }
        /// <summary>
        /// Gets an HttpWebRequest that is Authenticated
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpWebRequest GetAuthenticatedWebRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException(PCResources.Exception_Message_EmptyString_Arg, "url");
            EnsureToken();
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
            return request;
        }


       
    }
}
