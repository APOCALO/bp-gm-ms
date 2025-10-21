using AutoMapper;
using Framework.Domain.ValueObjects;
using Web.Api.Application.Companies.Commands.CreateCompany;
using Web.Api.Application.Companies.Commands.PatchCompany;
using Web.Api.Application.Companies.Commands.UpdateCompany;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Domain.Companies;

namespace Web.Api.Application.Profiles
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile()
        {
            // Mapeo de CreateCompanyCommand a Company
            CreateMap<CreateCompanyCommand, Company>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => PhoneNumber.Create(src.PhoneNumber, src.CountryCode)))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => Address.Create(src.Country, src.Department, src.City, src.StreetType, src.StreetNumber, src.CrossStreetNumber, src.PropertyNumber, src.ZipCode)))
                .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => WorkSchedule.Create(src.WorkingDays, src.OpeningHour, src.ClosingHour, src.LunchStart, src.LunchEnd, src.AllowAppointmentsDuringLunch, src.AppointmentDurationMinutes)))
                .ForMember(d => d.CompanyPhotos, o => o.Ignore()) //Evita que mapee IFormFile -> string
                .ForMember(d => d.CoverPhotoUrls, o => o.Ignore());

            // Mapeo de Company a CustomerResponseDTO
            CreateMap<Company, CompanyResponseDTO>()
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

            // Mapeo de UpdateCompanyCommand a Company ignoring null values
            CreateMap<UpdateCompanyCommand, Company>()
                .ForMember(d => d.CompanyPhotos, o => o.Ignore()) //Evita que mapee IFormFile -> string
                .ForMember(d => d.CoverPhotoUrls, o => o.Ignore());

            // Mapeo de PatchCompanyCommand a Company ignoring null values
            CreateMap<PatchCompanyCommand, Company>()
                .ForMember(d => d.CompanyPhotos, o => o.Ignore()) //Evita que mapee IFormFile -> string
                .ForMember(d => d.CoverPhotoUrls, o => o.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
