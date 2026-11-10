using System.Text.Json.Serialization;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.DTOs.Tracks;

public class UpdateTrackStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrackStatus Status { get; set; }
}
