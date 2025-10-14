using AutoMapper;
using ENSEK.API.DTOs;
using ENSEK.API.Models;

namespace ENSEK.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>();
        CreateMap<Account, AccountWithReadingsDto>();
        CreateMap<MeterReading, MeterReadingDto>();
    }
}


