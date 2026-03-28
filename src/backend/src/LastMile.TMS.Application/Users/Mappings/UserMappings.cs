using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Application.Users.Mappings;

[Mapper]
public static partial class UserMappings
{
    [MapperIgnoreTarget(nameof(ApplicationUser.Id))]
    [MapperIgnoreTarget(nameof(ApplicationUser.UserName))]
    [MapperIgnoreTarget(nameof(ApplicationUser.NormalizedUserName))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Email))]
    [MapperIgnoreTarget(nameof(ApplicationUser.NormalizedEmail))]
    [MapperIgnoreTarget(nameof(ApplicationUser.EmailConfirmed))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PasswordHash))]
    [MapperIgnoreTarget(nameof(ApplicationUser.SecurityStamp))]
    [MapperIgnoreTarget(nameof(ApplicationUser.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PhoneNumber))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PhoneNumberConfirmed))]
    [MapperIgnoreTarget(nameof(ApplicationUser.TwoFactorEnabled))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LockoutEnd))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LockoutEnabled))]
    [MapperIgnoreTarget(nameof(ApplicationUser.AccessFailedCount))]
    [MapperIgnoreTarget(nameof(ApplicationUser.IsSystemAdmin))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Depot))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Zone))]
    [MapperIgnoreTarget(nameof(ApplicationUser.CreatedAt))]
    [MapperIgnoreTarget(nameof(ApplicationUser.CreatedBy))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(ApplicationUser.IsActive))]
    [MapperIgnoreSource(nameof(CreateUserDto.Email))]
    [MapperIgnoreSource(nameof(CreateUserDto.Phone))]
    [MapperIgnoreSource(nameof(CreateUserDto.Role))]
    public static partial ApplicationUser ToEntity(this CreateUserDto dto);

    [MapperIgnoreTarget(nameof(ApplicationUser.Id))]
    [MapperIgnoreTarget(nameof(ApplicationUser.UserName))]
    [MapperIgnoreTarget(nameof(ApplicationUser.NormalizedUserName))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Email))]
    [MapperIgnoreTarget(nameof(ApplicationUser.NormalizedEmail))]
    [MapperIgnoreTarget(nameof(ApplicationUser.EmailConfirmed))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PasswordHash))]
    [MapperIgnoreTarget(nameof(ApplicationUser.SecurityStamp))]
    [MapperIgnoreTarget(nameof(ApplicationUser.ConcurrencyStamp))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PhoneNumber))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PhoneNumberConfirmed))]
    [MapperIgnoreTarget(nameof(ApplicationUser.TwoFactorEnabled))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LockoutEnd))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LockoutEnabled))]
    [MapperIgnoreTarget(nameof(ApplicationUser.AccessFailedCount))]
    [MapperIgnoreTarget(nameof(ApplicationUser.IsSystemAdmin))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Depot))]
    [MapperIgnoreTarget(nameof(ApplicationUser.Zone))]
    [MapperIgnoreTarget(nameof(ApplicationUser.CreatedAt))]
    [MapperIgnoreTarget(nameof(ApplicationUser.CreatedBy))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(ApplicationUser.LastModifiedBy))]
    [MapperIgnoreSource(nameof(UpdateUserDto.Email))]
    [MapperIgnoreSource(nameof(UpdateUserDto.Phone))]
    [MapperIgnoreSource(nameof(UpdateUserDto.Role))]
    public static partial void UpdateEntity(this UpdateUserDto dto, [MappingTarget] ApplicationUser user);
}
