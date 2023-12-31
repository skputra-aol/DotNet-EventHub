using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace EventHub.Organizations.Memberships
{
    public class OrganizationMembershipAppService : EventHubAppService, IOrganizationMembershipAppService 
    {
        private readonly OrganizationMembershipManager _organizationMembershipManager;
        private readonly IRepository<AppUser, Guid> _userRepository;
        private readonly IRepository<Organization, Guid> _organizationRepository;
        private readonly IRepository<OrganizationMembership, Guid>  _organizationMembershipsRepository;

        public OrganizationMembershipAppService(
            OrganizationMembershipManager organizationMembershipManager, 
            IRepository<AppUser, Guid> userRepository, 
            IRepository<Organization, Guid> organizationRepository, 
            IRepository<OrganizationMembership, Guid> organizationMembershipsRepository)
        {
            _organizationMembershipManager = organizationMembershipManager;
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
            _organizationMembershipsRepository = organizationMembershipsRepository;
        }
        
        [Authorize]
        public async Task JoinAsync(Guid organizationId)
        {
            await _organizationMembershipManager.JoinAsync(
                await _organizationRepository.GetAsync(organizationId),
                await _userRepository.GetAsync(CurrentUser.GetId())
            );
        }
        
        [Authorize]
        public async Task LeaveAsync(Guid organizationId)
        {
            await _organizationMembershipsRepository.DeleteAsync(
                x => x.OrganizationId == organizationId && x.UserId == CurrentUser.GetId()
            );
        }

        [Authorize]
        public async Task<bool> IsJoinedAsync(Guid organizationId)
        {
            return await _organizationMembershipManager.IsJoinedAsync(
                await _organizationRepository.GetAsync(organizationId),
                await _userRepository.GetAsync(CurrentUser.GetId())
            );
        }

        public async Task<PagedResultDto<OrganizationMemberDto>> GetMembersAsync(Guid organizationId)
        {
            var organizationMembershipsQueryable = await _organizationMembershipsRepository.GetQueryableAsync();
            var userQueryable = await _userRepository.GetQueryableAsync();

            var query = from organizationMembership in organizationMembershipsQueryable
                join user in userQueryable on organizationMembership.UserId equals user.Id
                where organizationMembership.OrganizationId == organizationId
                orderby organizationMembership.CreationTime descending
                select user;

            var totalCount = await AsyncExecuter.CountAsync(query);
            var users = await AsyncExecuter.ToListAsync(query.Take(10));

            return new PagedResultDto<OrganizationMemberDto>(
                totalCount,
                ObjectMapper.Map<List<AppUser>, List<OrganizationMemberDto>>(users)
            );
        }
    }
}