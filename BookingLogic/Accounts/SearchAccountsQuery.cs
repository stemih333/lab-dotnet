
namespace AppLogic.Accounts;
using AutoMapper.QueryableExtensions;
using Models.Dtos;

public class SearchAccountsQuery : IRequest<IEnumerable<AccountDto>>
{
    public int ReturnNumber { get; set; } = 20;
    public string SearchTerm { get; set; }

    public class SearchAccountsValidation : AbstractValidator<SearchAccountsQuery>
    {
        public SearchAccountsValidation()
        {
            RuleFor(_ => _.SearchTerm).NotEmpty().MinimumLength(2).MaximumLength(5);
            RuleFor(_ => _.ReturnNumber).NotEmpty().GreaterThan(0);
        }
    }

    public class SearchAccountsHandler : IRequestHandler<SearchAccountsQuery, IEnumerable<AccountDto>>
    {
        private readonly IMapper _mapper;
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IAuthService _auth;

        public SearchAccountsHandler(IMapper mapper, IBookingUnitOfWork bookingUnitOfWork, IAuthService auth)
        {
            _mapper = mapper;
            _bookingUnitOfWork = bookingUnitOfWork;
            _auth = auth;
        }

        public async Task<IEnumerable<AccountDto>> Handle(SearchAccountsQuery request, CancellationToken cancellationToken)
        {
            var isReader = await _auth.IsBookingReaderAsync();
            if (!isReader)
                throw new ForbiddenException();

            var accounts = _bookingUnitOfWork.Accounts
                .Where(_ => _.Number.StartsWith(request.SearchTerm))
                .ProjectTo<AccountDto>(_mapper.ConfigurationProvider)
                .OrderBy(_ => _.Id)
                .Take(request.ReturnNumber)
                .ToList();
            return await Task.FromResult(accounts);
        }
    }
}
