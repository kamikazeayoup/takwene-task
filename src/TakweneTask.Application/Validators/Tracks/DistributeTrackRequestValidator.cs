using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.DTOs.Tracks;

namespace TakweneTask.Application.Validators.Tracks;

public class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.DspIds)
            .NotEmpty()
            .WithMessage("At least one DSP ID is required.")
            .Must(ids => ids != null && ids.Distinct().Count() == ids.Count)
            .WithMessage("Duplicate DSP IDs are not allowed in distribution request.")
            .MustAsync(async (ids, ct) =>
            {
                if (ids == null || ids.Count == 0) return true;
                var distinctCount = ids.Distinct().Count();
                var countInDb = await dbContext.Dsps.CountAsync(d => ids.Contains(d.Id), ct);
                return countInDb == distinctCount;
            })
            .WithMessage("One or more specified DSPs do not exist.");
    }
}
