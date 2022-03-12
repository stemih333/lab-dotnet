using Microsoft.AspNetCore.Components;

namespace BlazorComponents.Shared;

public class BaseLoadableComponent : ComponentBase, IDisposable
{
	protected bool HttpLoading { get; set; } = false;
	protected string DisabledClass { get; set; } = "";

	[Inject]
	protected ILoadingService LoadingService { get; set; }


	public async Task OnHttpChangedCallback(int httpNumber)
	{
		HttpLoading = httpNumber != 0;
		DisabledClass = HttpLoading ? "disabled" : "";
		await Task.Delay(1000);
		StateHasChanged();
	}

	protected override async Task OnInitializedAsync()
	{
		LoadingService.OnHttpChanged += OnHttpChangedCallback;
		await base.OnInitializedAsync();
	}

	public void Dispose()
	{
		LoadingService.OnHttpChanged -= OnHttpChangedCallback;
		GC.SuppressFinalize(this);

	}
}
