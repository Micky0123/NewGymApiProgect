using AutoMapper;
using DBEntities.Models;
using DTO;

namespace API.Profiles
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TraineeDTO, Trainee>()
                .ForMember(dest => dest.TraineeId, opt => opt.Ignore()); // מתעלם מהמיפוי של TraineeId
        }
    }
}