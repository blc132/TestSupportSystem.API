﻿using Application.Errors;
using Application.Groups.Dtos;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Groups
{
    public class List
    {
        public class Query : IRequest<List<GroupDto>>
        {
        }

        public class Handler : IRequestHandler<Query, List<GroupDto>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _mapper = mapper;
                _userAccessor = userAccessor;
                _userManager = userManager;
            }

            public async Task<List<GroupDto>> Handle(List.Query request, CancellationToken cancellationToken)
            {
                var currentUserName = _userAccessor.GetCurrentUsername();
                if (currentUserName == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { Role = "Brak uprawnień" });

                var currentUser = await _userManager.FindByNameAsync(currentUserName);
                if (currentUser == null)
                    throw new RestException(HttpStatusCode.Unauthorized, new { Role = "Brak uprawnień" });

                var userGroups = await _context.UserGroups.Where(x => x.UserId == currentUser.Id).ToListAsync();
                currentUser.UserGroups = userGroups;

                var groups = new System.Collections.Generic.List<Group>();

                switch (currentUser.Role)
                {
                    case Role.Administrator:
                        groups = await _context.Groups
                            .Include(x => x.Course)
                            .Include(x => x.UserGroups)
                            .ThenInclude(y => y.User)
                            .ToListAsync();
                        break;

                    case Role.MainLecturer:
                        if (currentUser.CourseMainLecturers == null || !currentUser.CourseMainLecturers.Any())
                            throw new RestException(HttpStatusCode.BadRequest, new { Kursy = "Główny prowadzący nie jest przypisany do żadnego kursu" });

                        var courseMainLecturers = _context.CourseMainLecturers.Where(x => x.MainLecturerId == currentUser.Id);

                        groups = await _context.Groups.Where(x => courseMainLecturers.Any(y => y.CourseId == x.CourseId))
                            .Include(x => x.Course)
                            .Include(x => x.UserGroups)
                            .ThenInclude(y => y.User)
                            .ToListAsync();
                        break;

                    case Role.Lecturer:
                        if (currentUser.UserGroups == null || currentUser.UserGroups.Count == 0)
                            groups = new List<Group>();
                        else
                        {
                            groups = await _context.Groups
                                .Include(x => x.Course)
                                .Include(x => x.UserGroups)
                                .ThenInclude(y => y.User)
                                .ToListAsync();

                            groups = groups.Where(x => currentUser.UserGroups
                                .Any(y => y.GroupId == x.Id)).ToList();
                        }

                        break;

                    case Role.Student:
                        groups = await _context.Groups
                            .Include(x => x.Course)
                            .ToListAsync();

                        groups = groups
                            .Where(x => currentUser.UserGroups
                                .Any(y => y.GroupId == x.Id))
                            .ToList();
                        break;
                }
                return _mapper.Map<List<GroupDto>>(groups);
            }
        }
    }
}
