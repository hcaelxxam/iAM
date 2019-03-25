﻿using BridgeCare.Interfaces;
using BridgeCare.Models;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;

namespace BridgeCare.Controllers
{
    public class PerformanceController : ApiController
    {
        private readonly BridgeCareContext db;
        private readonly IPerformance performance;

        public PerformanceController(IPerformance performanceInterface, BridgeCareContext context)
        {
            performance = performanceInterface ?? throw new ArgumentNullException(nameof(performanceInterface));
            db = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get: api/performance
        /// </summary>
        [ModelValidation("Given simulation data is not valid")]
        public IQueryable<PerformanceModel> Get(SimulationModel data) => performance.GetPerformance(data, db);
    }
}