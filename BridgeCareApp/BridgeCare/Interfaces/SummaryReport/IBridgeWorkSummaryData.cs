﻿using BridgeCare.Models;
using System.Collections.Generic;

namespace BridgeCare.Interfaces
{
    public interface IBridgeWorkSummaryData
    {
        List<InvestmentLibraryBudgetYearModel> GetYearlyBudgetModels(int simulationId, BridgeCareContext dbContext);
    }
}