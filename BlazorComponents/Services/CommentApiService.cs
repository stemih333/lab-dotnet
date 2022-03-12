namespace BlazorComponents.Services
{
	public class CommentApiService : BaseHttpService, ICommentApiService
	{
		private readonly string _apiName;
		private readonly IDownstreamWebApi _api;
		private readonly string _relativePath = "api/comment";

		public CommentApiService(IConfiguration configuration, IDownstreamWebApi api, ILoadingService loadingService) : base(loadingService)
		{
			_apiName = configuration.GetValue<string>("DownstreamApi:Name");
			_api = api;
		}

		public async Task<IdResultDto> AddNewComment(NewCommentCommand command)
		{
			return await RunRequest(async () =>
			{
				return await _api.CallWebApiForUserAsync<NewCommentCommand, IdResultDto>(_apiName, command, opts =>
				{
					opts.HttpMethod = HttpMethod.Post;
					opts.RelativePath = _relativePath;
				});
			});
		}
	}
}