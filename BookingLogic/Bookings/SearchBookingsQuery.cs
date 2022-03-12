using System.Linq.Dynamic.Core;

namespace AppLogic.Bookings;

public class SearchBookingsQuery : IRequest<SearchResultDto<SearchBookingDto>>
{
    public int? Page { get; set; } = 0;
    public int? PageSize { get; set; } = 20;
    public string SortColumn { get; set; } = "Id";
    public string SortOrder { get; set; } = "asc";
    public int? Id { get; set; }
    public BookingStatus? Status { get; set; }
    public DateTime? FromBookingDate { get; set; }
    public DateTime? ToBookingDate { get; set; }
    public DateTime? FromCreatedDate { get; set; }
    public DateTime? ToCreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? Approver { get; set; }

    public class SearchBookingsValidation : AbstractValidator<SearchBookingsQuery>
    {
        public SearchBookingsValidation()
        {
            RuleFor(_ => _).Custom((model, ctx) =>
            {
                if (model.FromBookingDate.HasValue && model.ToBookingDate.HasValue && model.FromBookingDate > model.ToBookingDate)
                    ctx.AddFailure("FromBookingDate", "From booking date cannot be greater than to booking date");

                if (model.FromCreatedDate.HasValue && model.ToCreatedDate.HasValue && model.FromCreatedDate > model.ToCreatedDate)
                    ctx.AddFailure("FromCreatedDate", "From booking date cannot be greater than to booking date");
            });
        }
    }

    public class SearchBookingsHandler : IRequestHandler<SearchBookingsQuery, SearchResultDto<SearchBookingDto>>
    {
        private readonly IMapper _mapper;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public SearchBookingsHandler(IMapper mapper, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _mapper = mapper;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<SearchResultDto<SearchBookingDto>> Handle(SearchBookingsQuery request, CancellationToken cancellationToken)
        {
            var isReader = await _auth.IsBookingReaderAsync();
            if (!isReader) throw new ForbiddenException();

            var query = _bookingUnitOfWork.Bookings
                .Include(_ => _.Rows)
                .Include(_ => _.Attachments)
                .Include(_ => _.Comments)
                .AsQueryable();

            if (request.Status.HasValue)
                query = query.Where(_ => _.BookingStatus == request.Status);

            if (request.FromBookingDate.HasValue)
                query = query.Where(_ => _.BookingDate >= request.FromBookingDate);

            if (request.ToBookingDate.HasValue)
                query = query.Where(_ => _.BookingDate <= request.ToBookingDate);

            if (request.FromCreatedDate.HasValue)
                query = query.Where(_ => _.Created >= request.FromBookingDate);

            if (request.ToCreatedDate.HasValue)
                query = query.Where(_ => _.Created <= request.ToCreatedDate);
            
            if (request.Id.HasValue)
                query = query.Where(_ => _.Id <= request.Id);

            if (!string.IsNullOrWhiteSpace(request.CreatedBy))
                query = query.Where(_ => _.CreatedBy == request.CreatedBy);

            if (!string.IsNullOrWhiteSpace(request.Approver))
                query = query.Where(_ => _.Approver == request.Approver);

            query = query.OrderBy($"{request.SortColumn} {request.SortOrder}");
                
            var totalNumberOfRows = query.Count();
            var results = query.Skip(request.PageSize!.Value * request.Page!.Value).Take(request.PageSize!.Value).Select(_ => _mapper.Map<SearchBookingDto>(_));

            return new SearchResultDto<SearchBookingDto>
            {
                Results = results,
                TotalNumberOfRows = totalNumberOfRows
            };
        }
    }
}
