﻿using BridgeCare.Models;
using System.Linq;
using System.Web.Http;

namespace BridgeCare.Interfaces
{
    public interface IInvestmentLibrary
    {
        InvestmentLibraryModel GetSimulationInvestmentLibrary(int selectedScenarioId, BridgeCareContext db);

        InvestmentLibraryModel SaveSimulationInvestmentLibrary(InvestmentLibraryModel data, BridgeCareContext db);
    }
}
