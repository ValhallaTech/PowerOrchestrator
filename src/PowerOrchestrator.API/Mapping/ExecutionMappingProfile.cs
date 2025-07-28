using AutoMapper;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.API.Mapping;

/// <summary>
/// AutoMapper profile for Execution entity mappings
/// </summary>
public class ExecutionMappingProfile : Profile
{
    /// <summary>
    /// Initializes the Execution mapping profile
    /// </summary>
    public ExecutionMappingProfile()
    {
        // Execution to ExecutionDto
        CreateMap<Execution, ExecutionDto>()
            .ForMember(dest => dest.ScriptName, opt => opt.MapFrom(src => src.Script != null ? src.Script.Name : string.Empty));
    }
}