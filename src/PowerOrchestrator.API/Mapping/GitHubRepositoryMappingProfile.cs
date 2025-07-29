using AutoMapper;
using PowerOrchestrator.API.DTOs;
using PowerOrchestrator.Domain.Entities;

namespace PowerOrchestrator.API.Mapping;

/// <summary>
/// AutoMapper profile for GitHub repository entity mappings
/// </summary>
public class GitHubRepositoryMappingProfile : Profile
{
    /// <summary>
    /// Initializes the GitHub repository mapping profile
    /// </summary>
    public GitHubRepositoryMappingProfile()
    {
        // GitHubRepository to GitHubRepositoryDto
        CreateMap<GitHubRepository, GitHubRepositoryDto>()
            .ForMember(dest => dest.ScriptCount, opt => opt.MapFrom(src => src.Scripts.Count));

        // CreateGitHubRepositoryDto to GitHubRepository
        CreateMap<CreateGitHubRepositoryDto, GitHubRepository>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Owner}/{src.Name}"))
            .ForMember(dest => dest.LastSyncAt, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.ValueObjects.RepositoryStatus.Active))
            .ForMember(dest => dest.Configuration, opt => opt.Ignore())
            .ForMember(dest => dest.Scripts, opt => opt.Ignore())
            .ForMember(dest => dest.SyncHistory, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RowVersion, opt => opt.Ignore());
    }
}