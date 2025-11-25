using AutoMapper;
using SGE.Application.DTOs;
using SGE.Application.DTOs.Users;
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
        
        CreateMap<EmployeeCreateDto, Employee>()
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => 
                src.HireDate.Kind == DateTimeKind.Utc ? src.HireDate : 
                src.HireDate.Kind == DateTimeKind.Local ? src.HireDate.ToUniversalTime() : 
                DateTime.SpecifyKind(src.HireDate, DateTimeKind.Utc)));
        
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
            .ForMember(dest => dest.BreakDuration, opt => opt.MapFrom(src => TimeSpan.FromHours(src.BreakDurationHours)))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => 
                src.Date.Kind == DateTimeKind.Utc ? src.Date.Date : 
                src.Date.Kind == DateTimeKind.Local ? src.Date.ToUniversalTime().Date : 
                DateTime.SpecifyKind(src.Date, DateTimeKind.Utc).Date));

        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"));

        // LeaveRequest mappings
        CreateMap<LeaveRequestCreateDto, LeaveRequest>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => 
                src.StartDate.Kind == DateTimeKind.Utc ? src.StartDate : 
                src.StartDate.Kind == DateTimeKind.Local ? src.StartDate.ToUniversalTime() : 
                DateTime.SpecifyKind(src.StartDate, DateTimeKind.Utc)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => 
                src.EndDate.Kind == DateTimeKind.Utc ? src.EndDate : 
                src.EndDate.Kind == DateTimeKind.Local ? src.EndDate.ToUniversalTime() : 
                DateTime.SpecifyKind(src.EndDate, DateTimeKind.Utc)));

        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.LeaveTypeName, opt => opt.MapFrom(src => src.LeaveType.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

        // User mappings
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Sera rempli manuellement
    }
}

