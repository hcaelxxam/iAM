﻿using System;
using System.Collections.Generic;
using System.Linq;
using BridgeCare.Models;
using BridgeCare.Models.SummaryReport;

namespace BridgeCare.Interfaces.SummaryReport
{
    interface IWorkSummaryByBudget
    {
        IQueryable<WorkSummaryByBudgetModel> GetworkSummaryByBudgetsData(SimulationModel model, BridgeCareContext db);
    }
}
