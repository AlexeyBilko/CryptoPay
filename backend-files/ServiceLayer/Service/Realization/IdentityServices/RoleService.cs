using AutoMapper;
using Microsoft.AspNetCore.Identity;
using ServiceLayer.DTOs;

namespace ServiceLayer.Services.IdentityServices
{
    public class RoleService
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;

        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
            MapperConfiguration configuration = new MapperConfiguration(opt =>
            {
                opt.CreateMap<IdentityRole, RoleDTO>();
                opt.CreateMap<RoleDTO, IdentityRole>();
            });
            mapper = new Mapper(configuration);
        }

        public string[] GetAllRolesAsString()
        {
            return roleManager.Roles.Select(x => x.Name)
                .ToArray();
        }

        public IEnumerable<RoleDTO> GetAllRoles()
        {
            return roleManager.Roles.Select(role => mapper.Map<RoleDTO>(role))
                .ToList();
        }

        public bool RoleExists(string role)
        {
            return GetAllRoles().Select(r => r.Name == role).Any();
        }

        public async Task<bool> AddRole(string name)
        {
            var role = new IdentityRole(name);
            await roleManager.CreateAsync(role);
            return true;
        }

    }
}
