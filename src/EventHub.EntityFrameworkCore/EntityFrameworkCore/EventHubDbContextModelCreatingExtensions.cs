﻿using EventHub.Countries;
using EventHub.Events;
using EventHub.Events.Registrations;
using EventHub.Organizations;
using EventHub.Organizations.Memberships;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.Identity;

namespace EventHub.EntityFrameworkCore
{
    public static class EventHubDbContextModelCreatingExtensions
    {
        public static void ConfigureEventHub(
            this ModelBuilder builder,
            bool isMigrationDbContext)
        {
            Check.NotNull(builder, nameof(builder));

            /* Configure your own tables/entities inside here */

            builder.Entity<Organization>(b =>
            {
                b.ToTable(EventHubConsts.DbTablePrefix + "Organizations", EventHubConsts.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.Name).IsRequired().HasMaxLength(OrganizationConsts.MaxNameLength);
                b.Property(x => x.DisplayName).IsRequired().HasMaxLength(OrganizationConsts.MaxDisplayNameLength);
                b.Property(x => x.Description).IsRequired().HasMaxLength(OrganizationConsts.MaxDescriptionNameLength);

                if (isMigrationDbContext)
                {
                    b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.OwnerUserId).IsRequired().OnDelete(DeleteBehavior.NoAction);
                }

                b.HasIndex(x => x.Name);
                b.HasIndex(x => x.DisplayName);
            });
            
            builder.Entity<OrganizationMembership>(b =>
            {
                b.ToTable(EventHubConsts.DbTablePrefix + "OrganizationMemberships", EventHubConsts.DbSchema);

                b.ConfigureByConvention();

                b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).IsRequired().OnDelete(DeleteBehavior.NoAction);

                if (isMigrationDbContext)
                {
                    b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.NoAction);
                }

                b.HasIndex(x => new {x.OrganizationId, x.UserId});
            });

            builder.Entity<Event>(b =>
            {
                b.ToTable(EventHubConsts.DbTablePrefix + "Events", EventHubConsts.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.Title).IsRequired().HasMaxLength(EventConsts.MaxTitleLength);
                b.Property(x => x.Description).IsRequired().HasMaxLength(EventConsts.MaxDescriptionLength);
                b.Property(x => x.UrlCode).IsRequired().HasMaxLength(EventConsts.UrlCodeLength);
                b.Property(x => x.Url).IsRequired().HasMaxLength(EventConsts.MaxUrlLength);
                b.Property(x => x.OnlineLink).HasMaxLength(EventConsts.MaxOnlineLinkLength);
                b.Property(x => x.City).HasMaxLength(EventConsts.MaxCityLength);
                b.Property(x => x.Language).HasMaxLength(EventConsts.MaxLanguageLength);

                b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).IsRequired().OnDelete(DeleteBehavior.NoAction);
               
                b.HasOne<Country>().WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.NoAction);

                b.HasIndex(x => new {x.OrganizationId, x.StartTime});
                b.HasIndex(x => x.StartTime);
                b.HasIndex(x => x.UrlCode);
                b.HasIndex(x => new {x.IsRemindingEmailSent, x.StartTime});
                b.HasIndex(x => x.IsEmailSentToMembers);
            });

            builder.Entity<EventRegistration>(b =>
            {
                b.ToTable(EventHubConsts.DbTablePrefix + "EventRegistrations", EventHubConsts.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.IsTimingChangeEmailSent).HasDefaultValue(true)
                    .HasColumnName(nameof(EventRegistration.IsTimingChangeEmailSent));

                b.HasOne<Event>().WithMany().HasForeignKey(x => x.EventId).IsRequired().OnDelete(DeleteBehavior.NoAction);

                if (isMigrationDbContext)
                {
                    b.HasOne<IdentityUser>().WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.NoAction);
                }

                b.HasIndex(x => new {x.EventId, x.UserId});
            });
            
            builder.Entity<Country>(b =>
            {
                b.ToTable(EventHubConsts.DbTablePrefix + "Countries", EventHubConsts.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.Name).IsRequired().HasMaxLength(CountryConsts.MaxNameLength);

                b.HasIndex(x => new {x.Name});
            });
        }
    }
}
