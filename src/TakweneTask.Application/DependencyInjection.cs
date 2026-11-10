using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TakweneTask.Application.Mappings;
using TakweneTask.Application.Services.Artists;
using TakweneTask.Application.Services.Auth;
using TakweneTask.Application.Services.Tracks;

namespace TakweneTask.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ITrackService, TrackService>();

        return services;
    }
}
