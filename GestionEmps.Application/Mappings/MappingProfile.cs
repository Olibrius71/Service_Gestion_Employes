﻿using AutoMapper;
using GestionEmps.Application.DTOs;
using GestionEmps.Core.Entities;
using GestionEmps.Application.DTOs.Employees;

namespace GestionEmps.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentCreateDto, Department>();
        CreateMap<DepartmentUpdateDto, Department>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        /*
        CreateMap<Employee, EmployeeDto>().ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
        $"{src.FirstName} {src.LastName}")).ForMember(dest => dest.DepartmentName, opt =>
        opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));
        CreateMap<EmployeeCreateDto, Employee>();
        CreateMap<EmployeeUpdateDto, Employee>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // ignore nulls
        */
        
        CreateMap<Employee, EmployeeDto>().ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>$"{src.FirstName} {src.LastName}")).ForMember(dest => dest.DepartmentName, opt =>opt.MapFrom(src => src.Department.Name));
        CreateMap<EmployeeCreateDto, Employee>();
        CreateMap<EmployeeUpdateDto, Employee>().ForAllMembers(opts => opts.Condition((src, dest, srcMember)=> srcMember != null)); // ignore nulls

    }
}