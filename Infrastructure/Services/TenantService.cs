﻿using Core.Interfaces;
using Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantSettings _tenantSettings;
        private HttpContext _httpContext;
        private Tenant _currentTenant;

        public TenantService()
        {
            SetTenant("dbo");
        }

        public TenantService(IOptions<TenantSettings> tenantSettings, IHttpContextAccessor contextAccessor)
        {
            _tenantSettings = tenantSettings.Value;
            _httpContext = contextAccessor.HttpContext;
            if (_httpContext != null)
            {
                if (_httpContext.Request.Headers.TryGetValue("tenant", out var tenantId))
                {

                    SetTenant(tenantId);
                }
                else
                {
                    throw new Exception("Invalid Tenant!");
                }
            }
        }

        private void SetTenant(string tenantId)
        {
            _currentTenant = _tenantSettings?.Tenants?.Where(a => a.TID == tenantId).FirstOrDefault();

            if (_currentTenant == null)
            {
                //throw new Exception("Invalid Tenant!");
                // HACK JS: não conseguimos apanhar as settings injectando no construtor
                _currentTenant = new Tenant();
                _currentTenant.ConnectionString = "Data Source=(localdb)\\mssqllocaldb;Initial Catalog=mig_sharedTenantDb;Integrated Security=True;MultipleActiveResultSets=True";
                _currentTenant.Name = "dbo";
                _currentTenant.TID = "dbo";
            };


            if (string.IsNullOrEmpty(_currentTenant.ConnectionString))
            {
                SetDefaultConnectionStringToCurrentTenant();
            }
        }

        private void SetDefaultConnectionStringToCurrentTenant()
        {
            _currentTenant.ConnectionString = _tenantSettings.Defaults.ConnectionString;
        }

        public string GetConnectionString()
        {
            return _currentTenant?.ConnectionString;
        }

        public string GetDatabaseProvider()
        {
            return "mssql";

            // return _tenantSettings.Defaults?.DBProvider;
        }

        public Tenant GetTenant()
        {
            return _currentTenant;
        }
    }
}