using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _0_Framework.Mappings.Interfaces;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _0_Framework.Mappings.Profiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
           
            // هر مدل → به ViewModel با نام یکسان + پسوند VM
            ApplyMappingsFromAssembly(typeof(AppMappingProfile).Assembly);
        }

        private void ApplyMappingsFromAssembly(System.Reflection.Assembly assembly)
        {
            var types = assembly.GetExportedTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                // اگر کلاسی وجود دارد که IMapFrom<T> را پیاده‌سازی کرده باشد
                foreach (var i in interfaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>))
                    {
                        var sourceType = i.GetGenericArguments()[0];

                        // map کامل
                        CreateMap(sourceType, type);

                        // اگر خواستی map معکوس هم داشته باشی:
                        CreateMap(type, sourceType);
                    }
                }
            }
        }
    }
}
