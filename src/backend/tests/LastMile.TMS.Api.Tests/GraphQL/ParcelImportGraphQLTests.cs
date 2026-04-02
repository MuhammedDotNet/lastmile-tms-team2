using System.Net;
using System.Text.Json;
using FluentAssertions;
using LastMile.TMS.Persistence;

namespace LastMile.TMS.Api.Tests.GraphQL;

[Collection(ApiTestCollection.Name)]
public class ParcelImportGraphQLTests(CustomWebApplicationFactory factory)
    : GraphQLTestBase(factory), IAsyncLifetime
{
    [Fact]
    public async Task ParcelImports_AfterUpload_ReturnsHistoryAndDetail()
    {
        var token = await GetAdminAccessTokenAsync();
        var importId = await UploadCsvAsync(token);

        using var historyDocument = await PostGraphQLAsync(
            """
            query GetParcelImports {
              parcelImports {
                id
                fileName
                status
                totalRows
                processedRows
                importedRows
                rejectedRows
              }
            }
            """,
            accessToken: token);

        historyDocument.RootElement.TryGetProperty("errors", out var historyErrors)
            .Should().BeFalse("GraphQL should not return errors: {0}", historyErrors.ToString());

        var imports = historyDocument.RootElement
            .GetProperty("data")
            .GetProperty("parcelImports")
            .EnumerateArray()
            .ToList();

        imports.Should().ContainSingle();
        imports[0].GetProperty("id").GetGuid().Should().Be(importId);
        imports[0].GetProperty("status").GetString().Should().Be("CompletedWithErrors");
        imports[0].GetProperty("importedRows").GetInt32().Should().Be(1);
        imports[0].GetProperty("rejectedRows").GetInt32().Should().Be(1);

        using var detailDocument = await PostGraphQLAsync(
            """
            query GetParcelImport($id: UUID!) {
              parcelImport(id: $id) {
                id
                fileName
                status
                totalRows
                processedRows
                importedRows
                rejectedRows
                createdTrackingNumbers
                rowFailuresPreview {
                  rowNumber
                  errorMessage
                }
              }
            }
            """,
            variables: new
            {
                id = importId,
            },
            accessToken: token);

        detailDocument.RootElement.TryGetProperty("errors", out var detailErrors)
            .Should().BeFalse("GraphQL should not return errors: {0}", detailErrors.ToString());

        var parcelImport = detailDocument.RootElement
            .GetProperty("data")
            .GetProperty("parcelImport");

        parcelImport.GetProperty("status").GetString().Should().Be("CompletedWithErrors");
        parcelImport.GetProperty("totalRows").GetInt32().Should().Be(2);
        parcelImport.GetProperty("processedRows").GetInt32().Should().Be(2);
        parcelImport.GetProperty("importedRows").GetInt32().Should().Be(1);
        parcelImport.GetProperty("rejectedRows").GetInt32().Should().Be(1);
        parcelImport.GetProperty("createdTrackingNumbers").EnumerateArray().Should().ContainSingle();

        var failure = parcelImport
            .GetProperty("rowFailuresPreview")
            .EnumerateArray()
            .Single();
        failure.GetProperty("rowNumber").GetInt32().Should().Be(3);
        failure.GetProperty("errorMessage").GetString().Should().Contain("weight must be a valid number.");
    }

    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> UploadCsvAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/parcel-imports");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        request.Content = new MultipartFormDataContent
        {
            {
                new StringContent(DbSeeder.TestDepotAddressId.ToString()),
                "shipperAddressId"
            },
            {
                new ByteArrayContent(
                    System.Text.Encoding.UTF8.GetBytes(
                        """
                        recipient_street1,recipient_street2,recipient_city,recipient_state,recipient_postal_code,recipient_country_code,recipient_is_residential,recipient_contact_name,recipient_company_name,recipient_phone,recipient_email,description,parcel_type,service_type,weight,weight_unit,length,width,height,dimension_unit,declared_value,currency,estimated_delivery_date
                        15 George Street,,Sydney,NSW,2000,AU,true,Taylor Smith,Acme,+61000000000,taylor@example.com,Box,Package,STANDARD,2.5,KG,20,10,5,CM,100,AUD,2030-01-15
                        17 Pitt Street,,Sydney,NSW,2000,AU,true,Jordan Lee,Acme,+61000000001,jordan@example.com,Box,Package,STANDARD,abc,KG,20,10,5,CM,100,AUD,2030-01-15
                        """)),
                "file",
                "parcels.csv"
            }
        };

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("importId").GetGuid();
    }
}
