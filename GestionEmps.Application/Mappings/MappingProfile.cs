using AutoMapper;
using SGE.Application.DTOs;
using SGE.Core.Entities;

namespace SGE.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Department mappings
        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentCreateDto, Department>();
        CreateMap<DepartmentUpdateDto, Department>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Employee mappings
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));
        
        CreateMap<EmployeeCreateDto, Employee>();
        
        CreateMap<EmployeeUpdateDto, Employee>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember, destMember, context) =>
            {                
                if (srcMember == null) return false;
                                
                if (srcMember is string str)
                    return !string.IsNullOrEmpty(str);
                                
                return true;
            }));
        
        
        // Attendance mappings
        CreateMap<AttendanceCreateDto, Attendance>()
            .ForMember(dest => dest.BreakDuration, opt => opt.MapFrom(src =>
                TimeSpan.FromHours(src.BreakDurationHours)));
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                $"{src.Employee.FirstName} {src.Employee.LastName}"));
        
        
        // Leave Request mappings
        CreateMap<LeaveRequestCreateDto, LeaveRequest>();
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src
                => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.LeaveTypeName, opt =>
                opt.MapFrom(src => src.LeaveType.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src
                => src.Status.ToString()));
    }
}

