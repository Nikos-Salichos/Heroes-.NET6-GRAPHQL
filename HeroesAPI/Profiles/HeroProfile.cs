using AutoMapper;
using HeroesAPI.DTOs;
using HeroesAPI.Models;

namespace HeroesAPI.Profiles
{
    public class HeroProfile : Profile
    {
        public HeroProfile()
        {
            CreateMap<Hero, HeroDto>();
        }
    }
}
