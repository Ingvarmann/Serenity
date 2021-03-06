﻿using Serenity.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Serenity.Web
{
    public static class HtmlScriptExtensions
    {
        public static IHtmlString Stylesheet(this HtmlHelper helper, string cssUrl)
        {
            return new HtmlString(string.Format("    <link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\"/>\n",
                ContentHashCache.ResolveWithHash(cssUrl)));
        }

        public static IHtmlString Script(this HtmlHelper helper, string includeJS)
        {
            if (helper == null)
                throw new ArgumentNullException("helper");
            if (String.IsNullOrEmpty(includeJS))
                throw new ArgumentNullException("includeJS");

            var context = helper.ViewContext.HttpContext;

            var script = ScriptBundleManager.GetScriptBundle(includeJS);
            var scripts = GetIncludedScripts(context);

            if (!scripts.Contains(script))
            {
                scripts.Add(script);

                return new HtmlString(String.Format("    <script src=\"{0}\" type=\"text/javascript\"></script>\n",
                    HttpUtility.HtmlAttributeEncode(ContentHashCache.ResolveWithHash(script))));
            }
            else
                return new HtmlString("");
        }

        const string IncludedScriptsKey = "IncludedScripts";

        private static HashSet<string> GetIncludedScripts(HttpContextBase context)
        {
            HashSet<string> scripts = (HashSet<string>)context.Items[IncludedScriptsKey];
            if (scripts == null)
            {
                scripts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                context.Items[IncludedScriptsKey] = scripts;
            }

            return scripts;
        }

        public static string GetLocalTextInclude(this HtmlHelper page, string package)
        {
            string languageId = CultureInfo.CurrentUICulture.Name.TrimToNull() ?? "invariant";
            var context = Dependency.TryResolve<ILocalTextContext>();
            var isPending = context != null && context.IsApprovalMode;
            string scriptName = LocalTextScript.GetScriptName(package, languageId, isPending);
            DynamicScriptManager.IfNotRegistered(scriptName, () =>
            {
                var script = new LocalTextScript(package, (string)languageId, isPending);
                DynamicScriptManager.Register(script);
            });

            return DynamicScriptManager.GetScriptInclude(scriptName);
        }
    }
}