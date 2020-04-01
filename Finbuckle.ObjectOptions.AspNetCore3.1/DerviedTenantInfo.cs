using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finbuckle.ObjectOptions
{
    public class DerivedTenantInfo : TenantInfo
    {
        public MySubOptions SubOptions { get; set; }
    }
}
