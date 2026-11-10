using System.Text.Json.Serialization;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.DTOs.Tracks;

public class UpdateDistributionStatusRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DistributionStatus Status { get; set; }
}
