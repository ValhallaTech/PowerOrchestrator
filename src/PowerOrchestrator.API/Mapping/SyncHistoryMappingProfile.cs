using AutoMapper;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.API.Mapping;

/// <summary>
/// AutoMapper profile for sync history entity mappings
/// </summary>
public class SyncHistoryMappingProfile : Profile
{
    /// <summary>
    /// Initializes the sync history mapping profile
    /// </summary>
    public SyncHistoryMappingProfile()
    {
        // SyncHistory to SyncHistoryDto
        CreateMap<SyncHistory, SyncHistoryDto>()
            .ForMember(dest => dest.RepositoryName, opt => opt.MapFrom(src => src.Repository != null ? src.Repository.FullName : null))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => TimeSpan.FromMilliseconds(src.DurationMs)));
    }
}