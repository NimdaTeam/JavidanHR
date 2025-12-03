using _0_Framework.GenericRepositoy.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Domain.Entities;

namespace HrSystem.Domain.Interfaces
{
    public interface IEmployeeRepository: IRepository<long, Employee>
    {
        Task<List<string?>> GetAllDepartments();
        Task<List<string?>> GetAllPositions();

    }
}
