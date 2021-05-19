				binding.IsActive.Value = ProgressBindable.Value < Math.Max( ThresholdABindable.Value, ThresholdBBindable.Value );
			else
				binding.IsActive.Value = ProgressBindable.Value < Math.Min( ThresholdABindable.Value, ThresholdBBindable.Value );
