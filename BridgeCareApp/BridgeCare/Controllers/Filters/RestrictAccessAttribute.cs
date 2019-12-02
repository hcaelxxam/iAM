﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BridgeCare.Controllers.Filters
{
    /// <summary>
    /// Restricts access to an API endpoint, only allowing requests with a valid access token to be processed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RestrictAccessAttribute : AuthorizeAttribute
    {
        private string[] PermittedRoles { get; set; }

        public RestrictAccessAttribute (params string[] roles) : base()
        {
            this.PermittedRoles = roles;
        }

        protected override bool IsAuthorized(HttpActionContext httpContext)
        {
            if (!httpContext.Request.Headers.Contains("Authorization"))
            {
                return false;
            }
            if (httpContext.Request.Headers.Authorization.Parameter == null)
            {
                return false;
            }
            string accessToken = httpContext.Request.Headers.Authorization.Parameter.ToString();
            string userInfoString = AuthenticationController.GetUserInfoString(accessToken);
            Dictionary<string, string> userInfo = (new JavaScriptSerializer()).Deserialize<Dictionary<string, string>>(userInfoString);
            if (!userInfo.ContainsKey("roles")) {
                return false;
            }
            if (PermittedRoles.Length == 0)
            {
                return true;
            }
            string role = ParseRoleResponse(userInfo["roles"]);
            return PermittedRoles.Contains(role);
        }

        private static string ParseRoleResponse(string roleResponse)
        {
            string firstSegment = roleResponse.Split(',')[0];
            string role = firstSegment.Substring(3);
            return role;
        }
    }
}