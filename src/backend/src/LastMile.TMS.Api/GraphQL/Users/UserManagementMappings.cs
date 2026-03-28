using Riok.Mapperly.Abstractions;

namespace LastMile.TMS.Api.GraphQL.Users;

[Mapper]
public static partial class UserManagementInputMapper
{
    public static partial LastMile.TMS.Application.Users.DTOs.CreateUserDto ToDto(this CreateUserInput input);

    [MapperIgnoreSource(nameof(UpdateUserInput.Id))]
    public static partial LastMile.TMS.Application.Users.DTOs.UpdateUserDto ToDto(this UpdateUserInput input);
}
