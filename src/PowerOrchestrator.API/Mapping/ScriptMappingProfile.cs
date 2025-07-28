using AutoMapper;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.API.Mapping;

/// <summary>
/// AutoMapper profile for Script entity mappings
/// </summary>
public class ScriptMappingProfile : Profile
{
    /// <summary>
    /// Initializes the Script mapping profile
    /// </summary>
    public ScriptMappingProfile()
    {
        // Script to ScriptDto
        CreateMap<Script, ScriptDto>();

        // CreateScriptDto to Script
        CreateMap<CreateScriptDto, Script>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Executions, opt => opt.Ignore());

        // UpdateScriptDto to Script - only map non-null values
        CreateMap<UpdateScriptDto, Script>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore())
            .ForMember(dest => dest.Executions, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}