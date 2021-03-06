﻿using AutoMapper;
using Hated.Core.Domain;
using Hated.Infrastructure.DTO;

namespace Hated.Infrastructure.Mappers
{
    public static class AutoMapperConfig
    {
        public static IMapper Initialize()
            => new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<User, UserDto>();
                    cfg.CreateMap<Post, PostDto>();
                    cfg.CreateMap<Post, DetailPostDto>();
                    cfg.CreateMap<Comment, CommentDto>();
                    cfg.CreateMap<Like, LikeDto>();
                })
                .CreateMapper();
    }
}
