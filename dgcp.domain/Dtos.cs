namespace dgcp.service.Dtos;

public record PageDto(List<PageItemDto> Data, Pagination Pagination);
public record Pagination(string? Next, string? Prev, int Limit);
public record PageItemDto(string Url);
public record ReleaseRootDto(NameDto Publisher, DateTime PublishedDate, string PublicationPolicy, List<ReleaseDto> Releases);
public record ReleaseDto(string Ocid, string Id, DateTime Date, TenderDto Tender);
public record TenderDto(string Id, string Status, string Description, AmountDto Value, NameDto ProcuringEntity, PeriodDto TenderPeriod, List<TenderItemDto> Items, List<Document> Documents);
public record TenderItemDto(object Id, string Description, ClassificationDto Classification);
public record NameDto(string Name);
public record ClassificationDto(object Id);
public record AmountDto(decimal Amount, string Currency);
public record PeriodDto(DateTime StartDate, DateTime EndDate);
public record Document(string url);
