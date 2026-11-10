using AutoMapper;
using TakweneTask.Application.Mappings;

namespace TakweneTask.Tests.Application;

public class MappingProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        config.AssertConfigurationIsValid();
    }
}
