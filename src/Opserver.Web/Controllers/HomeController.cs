﻿using System;
using StackExchange.Opserver.Views.Shared;
using StackExchange.Opserver.Data;
using StackExchange.Opserver.Helpers;
using StackExchange.Opserver.Models;
using StackExchange.Opserver.Views.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using StackExchange.Opserver.Data.Dashboard;
using StackExchange.Opserver.Data.SQL;
using StackExchange.Opserver.Data.Redis;
using StackExchange.Opserver.Data.Exceptions;
using StackExchange.Opserver.Data.Elastic;
using StackExchange.Opserver.Data.HAProxy;
using System.Collections.Generic;

namespace StackExchange.Opserver.Controllers
{
    public class HomeController : StatusController
    {
        private IEnumerable<StatusModule> Modules { get; }
        private DashboardModule Dashboard { get; }
        private SQLModule Sql { get; }
        private RedisModule Redis { get; }
        private ElasticModule Elastic { get; }
        private ExceptionsModule Exceptions { get; }
        private HAProxyModule HAProxy { get; }

        public HomeController(
            IOptions<OpserverSettings> _settings,
            IEnumerable<StatusModule> modules,
            DashboardModule dashboard,
            SQLModule sql,
            RedisModule redis,
            ElasticModule elastic,
            ExceptionsModule exceptions,
            HAProxyModule haproxy
            ) : base(_settings)
        {
            Modules = modules;
            Dashboard = dashboard;
            Sql = sql;
            Redis = redis;
            Elastic = elastic;
            Exceptions = exceptions;
            HAProxy = haproxy;
        }

        [DefaultRoute("")]
        public ActionResult Home()
        {
            // TODO: Order
            foreach (var m in Modules)
            {
                //if (m.Enabled && m.SecuritySettings)
                //    return RedirectToAction()...
            }

            if (Current.User.HasAccess(Dashboard))
                return RedirectToAction(nameof(DashboardController.Dashboard), "Dashboard");
            if (Current.User.HasAccess(Sql))
                return RedirectToAction(nameof(SQLController.Dashboard), "SQL");
            if (Current.User.HasAccess(Redis))
                return RedirectToAction(nameof(RedisController.Dashboard), "Redis");
            if (Current.User.HasAccess(Elastic))
                return RedirectToAction(nameof(ElasticController.Dashboard), "Elastic");
            if (Current.User.HasAccess(Exceptions))
                return RedirectToAction(nameof(ExceptionsController.Exceptions), "Exceptions");
            if (Current.User.HasAccess(HAProxy))
                return RedirectToAction(nameof(HAProxyController.Dashboard), "HAProxy");

            return View("NoConfiguration");
        }

        [Route("top-refresh")]
        public ActionResult TopRefresh(string tab)
        {
            Current.NavTab = NavTab.GetByName(tab);

            var vd = new TopRefreshModel
                {
                    Tab = tab
                };
            return View(vd);
        }

        [Route("issues")]
        public ActionResult Issues() => View();

        [Route("about"), AlsoAllow(Roles.InternalRequest)]
        public ActionResult About() => View();

        [Route("about/caches"), AlsoAllow(Roles.InternalRequest)]
        public ActionResult AboutCaches(string filter, bool refresh = true)
        {
            var vd = new AboutModel
                {
                    AutoRefresh = refresh,
                    Filter = filter
                };
            return View("About.Caches", vd);
        }

        [Route("set-theme"), HttpPost]
        public ActionResult SetTheme(string theme)
        {
            Theme.Set(theme, Response);
            return RedirectToAction(nameof(About));
        }

        [Route("debug"), AllowAnonymous]
        public ActionResult Debug()
        {
            var sb = StringBuilderCache.Get()
                .AppendLine("Request Info")
                .Append("  IP: ").AppendLine(Current.RequestIP)
                .Append("  User: ").AppendLine(Current.User.AccountName)
                .Append("  Roles: ").AppendLine(Current.User.Roles.ToString())
                .AppendLine()
                .AppendLine("Headers");
            foreach (string k in Request.Headers.Keys)
            {
                sb.AppendFormat("  {0}: {1}\n", k, Request.Headers[k]);
            }

            var ps = PollingEngine.GetPollingStatus();
            sb.AppendLine()
              .AppendLine("Polling Info")
              .AppendLine(ps.GetPropertyNamesAndValues(prefix: "  "));
            return TextPlain(sb.ToStringRecycle());
        }

        [Route("error-test")]
        public ActionResult ErrorTestPage()
        {
            Current.LogException(new Exception("Test Exception via GlobalApplication.LogException()"));

#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException("I AM IMPLEMENTED, I WAS BORN TO THROW ERRORS!");
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }
    }
}
