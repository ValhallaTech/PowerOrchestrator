using AutoMapper;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Domain.Entities;
using PowerOrchestrator.Domain.ValueObjects;

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
            .ForMember(dest => dest.ScriptName, opt => opt.MapFrom(src => src.Script != null ? src.Script.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt ?? DateTime.MinValue))
            .ForMember(dest => dest.Environment, opt => opt.MapFrom(src => src.ExecutedOn ?? string.Empty))
            .ForMember(dest => dest.TriggeredBy, opt => opt.MapFrom(src => src.CreatedBy ?? string.Empty));

        // ExecutionMetrics to ExecutionMetricsDto
        CreateMap<ExecutionMetrics, ExecutionMetricsDto>();
    }
}