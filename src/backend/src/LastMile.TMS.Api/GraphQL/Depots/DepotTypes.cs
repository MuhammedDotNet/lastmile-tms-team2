using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Depots;

public sealed class AddressType : EntityObjectType<Address>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Address> descriptor)
    {
        descriptor.Name("Address");
        descriptor.Field(a => a.Street1);
        descriptor.Field(a => a.Street2);
        descriptor.Field(a => a.City);
        descriptor.Field(a => a.State);
        descriptor.Field(a => a.PostalCode);
        descriptor.Field(a => a.CountryCode);
        descriptor.Field(a => a.IsResidential);
        descriptor.Field(a => a.ContactName);
        descriptor.Field(a => a.CompanyName);
        descriptor.Field(a => a.Phone);
        descriptor.Field(a => a.Email);
    }
}

public sealed class OperatingHoursType : EntityObjectType<OperatingHours>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<OperatingHours> descriptor)
    {
        descriptor.Name("OperatingHours");
        descriptor.Field(o => o.DayOfWeek);
        descriptor.Field(o => o.OpenTime);
        descriptor.Field(o => o.ClosedTime);
        descriptor.Field(o => o.IsClosed);
    }
}

public sealed class DepotType : EntityObjectType<Depot>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Depot> descriptor)
    {
        descriptor.Name("Depot");
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.Name);
        descriptor.Field(d => d.Address).Type<AddressType>();
        descriptor.Field(d => d.OperatingHours).Type<ListType<NonNullType<OperatingHoursType>>>();
        descriptor.Field(d => d.IsActive);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }
}

public sealed class AddressFilterInputType : FilterInputType<Address>
{
    protected override void Configure(IFilterInputTypeDescriptor<Address> descriptor)
    {
        descriptor.Name("AddressFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(a => a.Street1);
        descriptor.Field(a => a.Street2);
        descriptor.Field(a => a.City);
        descriptor.Field(a => a.State);
        descriptor.Field(a => a.PostalCode);
        descriptor.Field(a => a.CountryCode);
        descriptor.Field(a => a.IsResidential);
        descriptor.Field(a => a.ContactName);
        descriptor.Field(a => a.CompanyName);
        descriptor.Field(a => a.Phone);
        descriptor.Field(a => a.Email);
    }
}

public sealed class AddressSortInputType : SortInputType<Address>
{
    protected override void Configure(ISortInputTypeDescriptor<Address> descriptor)
    {
        descriptor.Name("AddressSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(a => a.Street1);
        descriptor.Field(a => a.Street2);
        descriptor.Field(a => a.City);
        descriptor.Field(a => a.State);
        descriptor.Field(a => a.PostalCode);
        descriptor.Field(a => a.CountryCode);
        descriptor.Field(a => a.IsResidential);
        descriptor.Field(a => a.ContactName);
        descriptor.Field(a => a.CompanyName);
        descriptor.Field(a => a.Phone);
        descriptor.Field(a => a.Email);
    }
}

public sealed class OperatingHoursFilterInputType : FilterInputType<OperatingHours>
{
    protected override void Configure(IFilterInputTypeDescriptor<OperatingHours> descriptor)
    {
        descriptor.Name("OperatingHoursFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(o => o.DayOfWeek);
        descriptor.Field(o => o.OpenTime);
        descriptor.Field(o => o.ClosedTime);
        descriptor.Field(o => o.IsClosed);
    }
}

public sealed class OperatingHoursListFilterInputType
    : ListFilterInputType<OperatingHoursFilterInputType>
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        descriptor.Name("OperatingHoursListFilterInput");
        base.Configure(descriptor);
    }
}

public sealed class DepotFilterInputType : FilterInputType<Depot>
{
    protected override void Configure(IFilterInputTypeDescriptor<Depot> descriptor)
    {
        descriptor.Name("DepotFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.Name);
        descriptor.Field(d => d.Address).Type<AddressFilterInputType>();
        descriptor.Field(d => d.OperatingHours).Type<OperatingHoursListFilterInputType>();
        descriptor.Field(d => d.IsActive);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }
}

public sealed class DepotSortInputType : SortInputType<Depot>
{
    protected override void Configure(ISortInputTypeDescriptor<Depot> descriptor)
    {
        descriptor.Name("DepotSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.Name);
        descriptor.Field(d => d.Address).Type<AddressSortInputType>();
        descriptor.Field(d => d.IsActive);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }
}
