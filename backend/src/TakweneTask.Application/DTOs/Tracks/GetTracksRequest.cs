using TakweneTask.Application.Common.Models;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.DTOs.Tracks;

public class GetTracksRequest
{
    public Guid? ArtistId { get; set; }
    public string? Genre { get; set; }
    public TrackStatus? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public PaginationParams? ToPaginationParams() =>
        (PageNumber.HasValue || PageSize.HasValue)
            ? new PaginationParams(PageNumber ?? 1, PageSize ?? 10)
            : null;
}
