﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Xunit;

namespace EventHub.Events.Registrations
{
    public class EventRegistrationAppServiceTests : EventHubApplicationTestBase
    {
        private readonly IEventRegistrationAppService _eventRegistrationAppService;
        private readonly IRepository<EventRegistration, Guid> _eventRegistrationRepository;
        private readonly EventHubTestData _testData;
        private ICurrentUser _currentUser;

        public EventRegistrationAppServiceTests()
        {
            _eventRegistrationAppService = GetRequiredService<IEventRegistrationAppService>();
            _eventRegistrationRepository = GetRequiredService<IRepository<EventRegistration, Guid>>();
            _testData = GetRequiredService<EventHubTestData>();
        }
        
        protected override void AfterAddApplication(IServiceCollection services)
        {
            _currentUser = Substitute.For<ICurrentUser>();
            services.AddSingleton(_currentUser);
        }

        [Fact]
        public async Task Should_Register_To_An_Event()
        {
            Login(_testData.UserAdminId);

            await _eventRegistrationAppService.RegisterAsync(
                _testData.AbpMicroservicesFutureEventId
            );

            (await GetRegistrationOrNull(
                _testData.AbpMicroservicesFutureEventId,
                _currentUser.GetId()
            )).ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Unregister_From_An_Event()
        {
            Login(_testData.UserAdminId);

            await WithUnitOfWorkAsync(async () =>
            {
                await _eventRegistrationRepository.InsertAsync(
                    new EventRegistration(
                        Guid.NewGuid(),
                        _testData.AbpMicroservicesFutureEventId,
                        _currentUser.GetId()
                    )
                );
            });

            await _eventRegistrationAppService.UnregisterAsync(
                _testData.AbpMicroservicesFutureEventId
            );

            (await GetRegistrationOrNull(
                _testData.AbpMicroservicesFutureEventId,
                _currentUser.GetId())
            ).ShouldBeNull();
        }

        [Fact]
        public async Task Should_Not_Be_Registered_For_Capacity_Is_Full()
        {
            Login(_testData.UserAdminId);

            await _eventRegistrationAppService.RegisterAsync(
                _testData.AbpMicroservicesFutureEventId
            );
            
            Login(_testData.UserJohnId);
            
            var exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _eventRegistrationAppService.RegisterAsync(
                    _testData.AbpMicroservicesFutureEventId
                );
            });
            
            exception.Code.ShouldBe(EventHubErrorCodes.CapacityOfEventFull);
        }

        [Fact]
        public async Task Should_Get_List_Of_Attendees()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _eventRegistrationRepository.InsertAsync(
                    new EventRegistration(
                        Guid.NewGuid(),
                        _testData.AbpMicroservicesFutureEventId,
                        _testData.UserAdminId
                    )
                );

                await _eventRegistrationRepository.InsertAsync(
                    new EventRegistration(
                        Guid.NewGuid(),
                        _testData.AbpMicroservicesFutureEventId,
                        _testData.UserJohnId
                    )
                );
            });

            var result = await _eventRegistrationAppService.GetAttendeesAsync(
                _testData.AbpMicroservicesFutureEventId
            );

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
            result.Items.ShouldContain(x => x.Id == _testData.UserAdminId);
            result.Items.ShouldContain(x => x.Id == _testData.UserJohnId);
        }

        private async Task<EventRegistration> GetRegistrationOrNull(Guid eventId, Guid userId)
        {
            return await WithUnitOfWorkAsync(async () =>
            {
                return await _eventRegistrationRepository.FirstOrDefaultAsync(
                    x => x.EventId == eventId && x.UserId == userId
                );
            });
        }
        
        private void Login(Guid userId)
        {
            _currentUser.Id.Returns(userId);
            _currentUser.IsAuthenticated.Returns(true);
        }
    }
}
