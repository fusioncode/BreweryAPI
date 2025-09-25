namespace BreweryAPI.Models.DTOs.V2
{
    public class BreweryV2Dto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BreweryType { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Phone { get; set; }
        public string WebsiteUrl { get; set; }
        public string State { get; set; }
        public string Street { get; set; }
        
        // V2 Enhanced Properties
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public bool IsFavorite { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DistanceInfo Distance { get; set; }
    }

    public class DistanceInfo
    {
        public double? DistanceInKm { get; set; }
        public double? DistanceInMiles { get; set; }
        public string FormattedDistance { get; set; }
    }

    public class BrewerySearchV2RequestDto
    {
        public string Search { get; set; }
        public string SortBy { get; set; } = "city";
        public bool Descending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string BreweryType { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusInKm { get; set; }
        public double? MinRating { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class BrewerySearchV2ResponseDto
    {
        public List<BreweryV2Dto> Breweries { get; set; } = new List<BreweryV2Dto>();
        public PaginationInfo Pagination { get; set; }
        public SearchMetadata Metadata { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class SearchMetadata
    {
        public string SearchTerm { get; set; }
        public string SortBy { get; set; }
        public bool Descending { get; set; }
        public int ResultCount { get; set; }
        public double SearchTimeMs { get; set; }
        public List<string> AppliedFilters { get; set; } = new List<string>();
    }
}
