using AutoMapper;
using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Entities;

namespace TakweneTask.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Artist, ArtistResponse>();

        CreateMap<Track, TrackSummaryResponse>()
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()));

        CreateMap<Track, TrackDetailResponse>()
            .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => src.Artist != null ? src.Artist.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()))
            .ForMember(dest => dest.Distributions, opt => opt.MapFrom(src => src.Distributions));

        CreateMap<TrackDistribution, TrackDistributionResponse>()
            .ForMember(dest => dest.DspName, opt => opt.MapFrom(src => src.Dsp != null ? src.Dsp.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLowerInvariant()));
    }
}
