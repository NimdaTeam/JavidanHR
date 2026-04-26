using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using HrSystem.Application.DTO;
using HrSystem.Domain.Entities;

namespace HrSystem.Application.Mappings.Profiles
{
    public class HrSystemMappings:Profile
    {
        public HrSystemMappings()
        {
            CreateMap<Employee, EmployeeDetailsVM>()
                .PreserveReferences();
        }
    }
}
