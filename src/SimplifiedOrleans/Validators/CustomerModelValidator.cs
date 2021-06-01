using FluentValidation;
using SimplifiedOrleans.Models;

namespace SimplifiedOrleans.Validators
{
	public class CustomerModelValidator : AbstractValidator<CustomerModel>
	{
		public CustomerModelValidator()
		{
			RuleFor(r => r.Name).NotEmpty();
		}
	}
}
