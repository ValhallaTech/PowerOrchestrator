using AutoMapper;
using PowerOrchestrator.MAUI.Models;

namespace PowerOrchestrator.MAUI.Mappings;

/// <summary>
/// AutoMapper profile for UI model mappings
/// </summary>
public class UIModelProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIModelProfile"/> class
    /// </summary>
    public UIModelProfile()
    {
        CreateMap<Domain.Entities.User, UserUIModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Will be populated separately
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

        CreateMap<Domain.Entities.Script, ScriptUIModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

        CreateMap<Domain.Entities.GitHubRepository, RepositoryUIModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => $"https://github.com/{src.FullName}"))
            .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.DefaultBranch))
            .ForMember(dest => dest.SyncStatus, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status == Domain.ValueObjects.RepositoryStatus.Active))
            .ForMember(dest => dest.LastSyncAt, opt => opt.MapFrom(src => src.LastSyncAt))
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.DefaultBranch, opt => opt.MapFrom(src => src.Branch));

        CreateMap<Domain.Entities.Execution, ExecutionUIModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ScriptId, opt => opt.MapFrom(src => src.ScriptId.ToString()))
            .ForMember(dest => dest.ScriptName, opt => opt.Ignore()) // Will be populated separately
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.ScriptId, opt => opt.MapFrom(src => Guid.Parse(src.ScriptId)));

        CreateMap<Domain.Entities.Role, RoleUIModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()) // Will be populated from JSON
            .ForMember(dest => dest.UserCount, opt => opt.Ignore()) // Will be populated separately
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)));

        // Dashboard stats mapping - for now using mock data
        CreateMap<object, DashboardStatsUIModel>()
            .ForAllMembers(opt => opt.Ignore());
    }
}