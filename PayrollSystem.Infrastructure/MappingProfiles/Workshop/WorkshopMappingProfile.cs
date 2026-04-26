using AutoMapper;
using PayrollSystem.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayrollSystem.Infrastructure.MappingProfiles.Workshop
{
    public class WorkshopMappingProfile : Profile
    {
        public WorkshopMappingProfile()
        {
            CreateMap<Domain.Entities.Workshop.Workshop, UpdateWorkshopDto>()
                .ReverseMap();
        }
    }
}
