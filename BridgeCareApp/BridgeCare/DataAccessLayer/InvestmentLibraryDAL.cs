﻿using BridgeCare.EntityClasses;
using BridgeCare.Interfaces;
using BridgeCare.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace BridgeCare.DataAccessLayer
{
    public class InvestmentLibraryDAL : IInvestmentLibrary
    {
        /// <summary>
        /// Fetches a simulation's investment library data
        /// Throws a RowNotInTableException if no simulation is found
        /// </summary>
        /// <param name="id">Simulation identifier</param>
        /// <param name="db">BridgeCareContext</param>
        /// <returns>InvestmentLibraryModel</returns>
        public InvestmentLibraryModel GetSimulationInvestmentLibrary(int id, BridgeCareContext db)
        {
            if (!db.Simulations.Any(s => s.SIMULATIONID == id))
                    throw new RowNotInTableException($"No scenario found with id {id}");

             var simulation = db.Simulations
                .Include(s => s.INVESTMENTS)
                .Include(s => s.YEARLYINVESTMENTS)
                .Single(s => s.SIMULATIONID == id);
            return new InvestmentLibraryModel(simulation);
        }

        /// <summary>
        /// Executes an upsert/delete operation on a simulation's investment library data
        /// Throws a RowNotInTableException if no simulation is found
        /// </summary>
        /// <param name="model">InvestmentLibraryModel</param>
        /// <param name="db">BridgeCareContext</param>
        /// <returns>InvestmentLibraryModel</returns>
        public InvestmentLibraryModel SaveSimulationInvestmentLibrary(InvestmentLibraryModel model, BridgeCareContext db)
        {
            var id = int.Parse(model.Id);

            if (!db.Simulations.Any(s => s.SIMULATIONID == id))
                throw new RowNotInTableException($"No scenario found with id {id}");

            var simulation = db.Simulations
                .Include(s => s.INVESTMENTS)
                .Include(s => s.YEARLYINVESTMENTS)
                .Include(s => s.PRIORITIES).Include(s => s.PRIORITIES.Select(p => p.PRIORITYFUNDS))
                .Single(s => s.SIMULATIONID == id);

            if (simulation.INVESTMENTS != null)
                model.UpdateInvestment(simulation.INVESTMENTS);

            if (simulation.YEARLYINVESTMENTS.Any())
            {
                simulation.YEARLYINVESTMENTS.ToList().ForEach(yearlyInvestment =>
                {
                    var yearlyInvestmentModel =
                        model.BudgetYears.SingleOrDefault(m => m.Id == yearlyInvestment.YEARID.ToString());

                    if (yearlyInvestmentModel == null)
                        YearlyInvestmentEntity.DeleteEntry(yearlyInvestment, db);
                    else
                    {
                        yearlyInvestmentModel.matched = true;
                        yearlyInvestmentModel.UpdateYearlyInvestment(yearlyInvestment);
                    }
                });
                
            }

            simulation.PRIORITIES.ToList().ForEach(priorityEntity =>
            {
                var budgetsForNewFunds = new List<string>();

                if (priorityEntity.PRIORITYFUNDS.Any())
                    model.BudgetOrder.ForEach(budget =>
                    {
                      budgetsForNewFunds.Add(budget);
                    });

                if (budgetsForNewFunds.Any())
                {
                    priorityEntity.PRIORITYFUNDS
                        .ToList()
                        .ForEach(priorityFundEntity =>
                        {
                            PriorityFundEntity.DeleteEntry(priorityFundEntity, db);
                        });

                    db.PriorityFunds.AddRange(budgetsForNewFunds
                        .Select(budget => new PriorityFundEntity(priorityEntity.PRIORITYID, budget))
                    );
                }
            });

            if (simulation.INVESTMENTS == null)
                db.Investments.Add(new InvestmentsEntity(model));

            if (model.BudgetYears.Any(m => !m.matched))
                db.YearlyInvestments.AddRange(model.BudgetYears
                    .Where(yearlyInvestmentModel => !yearlyInvestmentModel.matched)
                    .Select(yearlyInvestmentModel => new YearlyInvestmentEntity(id, yearlyInvestmentModel))
                    .ToList()
                );

            db.SaveChanges();

            return new InvestmentLibraryModel(simulation);
        }
    }
}
