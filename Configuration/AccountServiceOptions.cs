namespace AccountService.Configuration
{

	public class AccountServiceOptions
	{
		public const string SectionName = "AccountService";

		public PaginationSettings Pagination { get; set; } = new();
		public CommissionSettings Commission { get; set; } = new();
		public ValidationSettings Validation { get; set; } = new();
		public MemorySettings Memory { get; set; } = new();
	}

	public class PaginationSettings
	{
		public int DefaultPage { get; set; } = 1;
		public int MinPageSize { get; set; } = 1;
		public int MaxPageSize { get; set; } = 100;
	}

	public class CommissionSettings
	{
		public decimal Rate { get; set; } = 0.005m;
		public decimal Min { get; set; } = 10;
		public decimal Max { get; set; } = 1000;
	}

	public class ValidationSettings
	{
		public int MaxDescriptionLength { get; set; } = 500;
	}

	public class MemorySettings
	{
		public int MaxInMemoryAccounts { get; set; } = 1000;
	}
}