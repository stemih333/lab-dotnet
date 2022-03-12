namespace BlazorComponents.Services;

public class LoadingService : ILoadingService
{
    private int OngoingHttp { get; set; } = 0;


	public event Func<int, Task> OnHttpChanged;

	public Task FinishedHttpRequest()
	{
		if (OngoingHttp > 0)
		{
			--OngoingHttp;
			if (OnHttpChanged != null) 
				return OnHttpChanged(OngoingHttp);
		}

		return Task.CompletedTask;
	}

	public Task StartedHttpRequest()
	{
		++OngoingHttp;
		return OnHttpChanged(OngoingHttp);
	}

	//public void FinishedHttpRequest()
	//   {
	//       if (OngoingHttp > 0)
	//	{
	//           --OngoingHttp;
	//           OnHttpChanged(OngoingHttp);
	//       }
	//   }

	//   public void StartedHttpRequest()
	//{
	//       ++OngoingHttp;
	//       OnHttpChanged(OngoingHttp);
	//   }


}
