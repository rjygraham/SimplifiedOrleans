using System;
using FluentValidation;
using SimplifiedOrleans.Models;

namespace SimplifiedOrleans.Validators
{
	public class SensorConfigurationModelValidator : AbstractValidator<SensorConfigurationModel>
	{
		public SensorConfigurationModelValidator()
		{
			RuleFor(r => r.CustomerId).NotNull().NotEqual(Guid.Empty);
			RuleFor(r => r.ReadingWindow).NotNull().NotEqual(TimeSpan.Zero);
		}
	}
}
