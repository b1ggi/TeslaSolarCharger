﻿using TeslaSolarCharger.SharedModel.Enums;

namespace TeslaSolarCharger.Model.BaseClasses;

public abstract class ResultConfigurationBase
{
    public int Id { get; set; }
    public decimal CorrectionFactor { get; set; }
    public ValueUsage UsedFor { get; set; }
    public ValueOperator Operator { get; set; }
}
