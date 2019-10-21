﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Exercises.Dtos;
using Application.User.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.User
{
    public class List
    {
        public class Query : IRequest<List<UserDto>>
        {
        }

        public class Handler : IRequestHandler<Query, List<UserDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<List<UserDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                var users = await _context.Users.ToListAsync();
                return _mapper.Map<List<UserDto>>(users);
            }
        }
    }
}