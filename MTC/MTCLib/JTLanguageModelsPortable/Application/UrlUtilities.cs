using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Application
{
    public static class UrlUtilities
    {
        public static string Action(string actionName, string controllerName, object routeValues)
        {
            return Action(
                actionName,
                controllerName,
                routeValues,
                "",
                "");
        }

        public static string Action(
            string actionName,
            string controllerName = "",
            object routeValues = null,
            string scheme = "",
            string hostName = "")
        {

            if (String.IsNullOrEmpty(scheme))
                scheme = ApplicationData.ActionUrlPrefix;

            var qs = GenerateQueryString(routeValues);
            if (qs.Length > 0)
                qs = "?" + qs;

            return string.Format("{0}{1}{2}{3}{4}",
                scheme,
                String.IsNullOrEmpty(hostName) ? String.Empty : hostName + ".",
                String.IsNullOrEmpty(controllerName) ? String.Empty : controllerName + "/",
                actionName,
                qs);
        }

        public static string Encode(string url)
        {
            return ApplicationData.Global.UrlEncode(url);
        }

        public static string Content(string contentPath)
        {
            if (String.IsNullOrEmpty(contentPath))
                return String.Empty;

            if (contentPath.StartsWith("~"))
                contentPath = contentPath.Substring(1);

            if (contentPath.StartsWith("/"))
                contentPath = contentPath.Substring(1);

            return contentPath;
        }

        public static string ActionUrl(string url)
        {
            if (String.IsNullOrEmpty(url))
                return url;

            string scheme = ApplicationData.ActionUrlPrefix;

            if (url.StartsWith("/"))
                return scheme + url.Substring(1);

            return url;
        }

        public static string GenerateQueryString(object routeValues = null)
        {
            if (routeValues == null)
                return String.Empty;

            var qs = new StringBuilder();
            foreach (var property in routeValues.GetType().GetProperties())
            {
                string name = property.Name;
                object value = property.GetGetMethod().Invoke(routeValues, null);
                string encodedValue = EncodeQueryValue(value != null ? value.ToString() : String.Empty);
                qs.AppendFormat("&{0}={1}", name, encodedValue);
            }

            if (qs.Length == 0)
                return String.Empty;

            return qs.ToString(1, qs.Length - 1);
        }

        public static string EncodeQueryValue(string str)
        {
            return ApplicationData.Global.UrlEncode(str);
        }

        public static bool HasQueryAttribute(string url, string attributeName)
        {
            if (String.IsNullOrEmpty(url))
                return false;

            int ofs = url.IndexOf("?" + attributeName);

            if (ofs == -1)
                ofs = url.IndexOf("&" + attributeName);

            if (ofs == -1)
                return false;

            return true;
        }

        public static string GetQueryValue(string url, string attributeName)
        {
            if (String.IsNullOrEmpty(url))
                return null;

            int ofs = url.IndexOf("?" + attributeName);

            if (ofs == -1)
                ofs = url.IndexOf("&" + attributeName);

            if (ofs == -1)
                return null;

            ofs++;

            int endOfs = ofs + attributeName.Length;

            if (endOfs >= url.Length)
                return null;

            if (url[endOfs] != '=')
                return null;

            ofs = endOfs + 1;

            endOfs = url.IndexOf('&', ofs);

            if (endOfs == -1)
                endOfs = url.IndexOf('#', ofs);

            if (endOfs == -1)
                endOfs = url.Length;

            int length = endOfs - ofs;

            string value = url.Substring(ofs, length);

            return value;
        }

        public static string SetQueryValue(string url, string attributeName, string attributeValue)
        {
            if (String.IsNullOrEmpty(url))
                return null;

            attributeValue = EncodeQueryValue(attributeValue);

            string attribute = attributeName + "=" + attributeValue;

            int ofs = url.IndexOf("?" + attributeName);

            if (ofs == -1)
                ofs = url.IndexOf("&" + attributeName);

            if (ofs == -1)
            {
                ofs = url.IndexOf("#");

                string sep = ((url.IndexOf('?') == -1) ? "?" : "&");

                if (ofs == -1)
                    return url + sep + attribute;
                else
                    return url.Insert(ofs, sep + attribute);
            }

            ofs++;

            int endOfs = ofs + attributeName.Length;

            if (endOfs >= url.Length)
                return null;

            if (url[endOfs] != '=')
                return null;

            ofs = endOfs + 1;

            endOfs = url.IndexOf('&', ofs);

            if (endOfs == -1)
                endOfs = url.IndexOf('#', ofs);

            if (endOfs == -1)
                endOfs = url.Length;

            url = url.Substring(0, ofs) + attributeValue + url.Substring(endOfs);

            return url;
        }
    }
}
