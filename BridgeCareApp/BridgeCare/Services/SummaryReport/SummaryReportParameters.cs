﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using BridgeCare.Interfaces;
using BridgeCare.Interfaces.CriteriaDrivenBudgets;
using BridgeCare.Models;
using BridgeCare.Models.CriteriaDrivenBudgets;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace BridgeCare.Services.SummaryReport
{
    public class SummaryReportParameters
    {
        private readonly BridgeCareContext db;
        private readonly ISimulationAnalysis analysisData;
        private readonly IInvestmentLibrary getInflationRate;
        private readonly ExcelHelper excelHelper;
        private readonly IPriority getPriorities;
        private readonly ICriteriaDrivenBudgets budgetCriteria;

        public SummaryReportParameters(ISimulationAnalysis simulationAnalysis, IInvestmentLibrary inflationRate,
            ExcelHelper excelHelper, IPriority priorities, ICriteriaDrivenBudgets budget,  BridgeCareContext db)
        {
            analysisData = simulationAnalysis ?? throw new ArgumentNullException(nameof(simulationAnalysis));
            getInflationRate = inflationRate ?? throw new ArgumentNullException(nameof(inflationRate));
            this.excelHelper = excelHelper;
            getPriorities = priorities ?? throw new ArgumentNullException(nameof(priorities));
            budgetCriteria = budget ?? throw new ArgumentNullException(nameof(budget));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        internal void Fill(ExcelWorksheet worksheet, SimulationModel simulationModel)
        {
            var simulationId = simulationModel.SimulationId;
            var investmentPeriod = analysisData.GetSimulationAnalysis(simulationId, db);
            var inflationAndInvestments = getInflationRate.GetSimulationInvestmentLibrary(simulationId, db);
            var priorities = getPriorities.GetSimulationPriorityLibrary(simulationId, db).Priorities;
            var criterias = budgetCriteria.GetCriteriaDrivenBudgets(simulationId, db);

            var currencyFormat = "_-$* #,##0.00_-;-$* #,##0.00_-;_-$* \"-\"??_-;_-@_-";

            // Simulation Name format
            excelHelper.MergeCells(worksheet, 1, 1, 1, 2);
            excelHelper.MergeCells(worksheet, 1, 3, 1, 10);

            worksheet.Cells["A1:B1"].Value = "Simulation Name";
            worksheet.Cells["C1:J1"].Value = simulationModel.SimulationName;
            excelHelper.ApplyBorder(worksheet.Cells[1, 1, 1, 10]);
            // End of Simulation Name format

            FillSimulationDetails(worksheet, investmentPeriod, inflationAndInvestments.InflationRate);
            FillAnalysisDetails(worksheet, investmentPeriod);
            FillJurisdictionCriteria(worksheet, investmentPeriod.Criteria);
            FillPriorities(worksheet, priorities);
            FillInvestmentAndBudgetCriteria(worksheet, inflationAndInvestments, criterias);
            worksheet.Cells.AutoFitColumns(50);
        }

        private void FillSimulationDetails(ExcelWorksheet worksheet, SimulationAnalysisModel investmentPeriod, double? inflationRate)
        {
            excelHelper.MergeCells(worksheet, 6, 6, 6, 8);
            excelHelper.MergeCells(worksheet, 8, 6, 8, 7);
            excelHelper.MergeCells(worksheet, 10, 6, 10, 7);
            excelHelper.MergeCells(worksheet, 12, 6, 12, 7);

            worksheet.Cells["F6:H6"].Value = "Investment:";
            worksheet.Cells["F8:G8"].Value = "Start Year:";
            worksheet.Cells["F10:G10"].Value = "Analysis Period:";
            worksheet.Cells["F12:G12"].Value = "Inflation Rate:";

            excelHelper.ApplyBorder(worksheet.Cells[6, 6, 12, 8]);

            worksheet.Cells["H8"].Value = investmentPeriod.StartYear;
            worksheet.Cells["H10"].Value = investmentPeriod.AnalysisPeriod;
            worksheet.Cells["H12"].Value = inflationRate;

            excelHelper.ApplyBorder(worksheet.Cells[8, 8, 12, 8]);
        }

        private void FillAnalysisDetails(ExcelWorksheet worksheet, SimulationAnalysisModel investmentPeriod)
        {
            excelHelper.MergeCells(worksheet, 6, 12, 6, 15);
            excelHelper.MergeCells(worksheet, 8, 12, 8, 13);
            excelHelper.MergeCells(worksheet, 10, 12, 10, 13);
            excelHelper.MergeCells(worksheet, 12, 12, 12, 13);
            excelHelper.MergeCells(worksheet, 14, 12, 14, 13);

            excelHelper.ApplyBorder(worksheet.Cells[6, 12, 14, 15]);

            excelHelper.MergeCells(worksheet, 8, 14, 8, 15);
            excelHelper.MergeCells(worksheet, 10, 14, 10, 15, false);
            excelHelper.MergeCells(worksheet, 12, 14, 12, 15, false);
            excelHelper.MergeCells(worksheet, 14, 14, 14, 15, false);

            excelHelper.ApplyBorder(worksheet.Cells[8, 14, 14, 15]);

            worksheet.Cells["L6:O6"].Value = "Analysis:";
            worksheet.Cells["L8:M8"].Value = "Optimization:";
            worksheet.Cells["L10:M10"].Value = "Budget:";
            worksheet.Cells["L12:M12"].Value = "Weighting:";
            worksheet.Cells["L14:M14"].Value = "Benefit:";
            var optimization = worksheet.DataValidations.AddListValidation("N8:O8");
            optimization.Formula.Values.Add("Incremental Benefit/Cost");
            optimization.Formula.Values.Add("Maximum Benefit");
            optimization.Formula.Values.Add("Remaining Life/Cost");
            optimization.Formula.Values.Add("Maximum Remaining Life");
            optimization.Formula.Values.Add("Multi-year Incremental Benefit/Cost");
            optimization.Formula.Values.Add("Multi-year Maximum Benefit");
            optimization.Formula.Values.Add("Multi-year Remaining Life/Cost");
            optimization.Formula.Values.Add("Multi-year Maximum Life");
            optimization.AllowBlank = false;
            worksheet.Cells["N8:O8"].Value = investmentPeriod.OptimizationType;

            var budgets = worksheet.DataValidations.AddListValidation("N10:O10");
            budgets.Formula.Values.Add("No Spending");
            budgets.Formula.Values.Add("As Budget Permits");
            budgets.Formula.Values.Add("Until Targets Met");
            budgets.Formula.Values.Add("Until Deficient Met");
            budgets.Formula.Values.Add("Targets/Deficient Met");
            budgets.Formula.Values.Add("Unlimited");
            worksheet.Cells["N10:O10"].Value = investmentPeriod.BudgetType;
            worksheet.Cells["N12:O12"].Value = investmentPeriod.WeightingAttribute;
            worksheet.Cells["N14:O14"].Value = investmentPeriod.BenefitAttribute;
        }

        private void FillPriorities(ExcelWorksheet worksheet, List<PriorityModel> priorities)
        {
            excelHelper.MergeCells(worksheet, 19, 12, 19, worksheet.Dimension.End.Column);
            excelHelper.MergeCells(worksheet, 20, 13, 20, worksheet.Dimension.End.Column);

            excelHelper.ApplyBorder(worksheet.Cells[20, 12, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column]);

            worksheet.Cells["L19:Z19"].Value = "Analysis Priorites:";

            worksheet.Cells["L20"].Value = "Number";
            worksheet.Cells["M20"].Value = "Criteria:";

            var startingRow = 21;
            foreach (var item in priorities)
            {
                excelHelper.MergeCells(worksheet, startingRow, 13, startingRow, worksheet.Dimension.End.Column, false);
                excelHelper.ApplyBorder(worksheet.Cells[21, 12, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column]);
                worksheet.Cells[startingRow, 12].Value = startingRow - 20;
                worksheet.Cells[startingRow, 13].Value = item.Criteria;
                startingRow++;
            }
        }

        private void FillJurisdictionCriteria(ExcelWorksheet worksheet, string criteria)
        {
            excelHelper.MergeCells(worksheet, 16, 12, 17, 13);
            excelHelper.MergeCells(worksheet, 16, 14, 17, 26, false);

            excelHelper.ApplyBorder(worksheet.Cells[16, 14, 17, 26]);

            worksheet.Cells["L16:M16"].Value = "Jurisdiction Criteria:";
            worksheet.Cells["N16:Z16"].Value = criteria;
        }

        private void FillInvestmentAndBudgetCriteria(ExcelWorksheet worksheet, InvestmentLibraryModel inflationAndInvestments, List<Models.CriteriaDrivenBudgets.CriteriaDrivenBudgetsModel> criterias)
        {
            worksheet.Cells[38, 1].Value = "Years";
            worksheet.Cells[38, 2].Value = "Total Funding";

            var startingRowInvestment = 40;
            var startingBudgetHeaderColumn = 2;
            var nextBudget = 0;
            var investmentGrid = new Dictionary<int, List<(string BudgetName, double? BudgetAmount)>>();
            foreach (var item in inflationAndInvestments.BudgetYears)
            {
                if (!investmentGrid.ContainsKey(item.Year))
                {
                    investmentGrid.Add(item.Year, new List<(string BudgetName, double? BudgetAmount)> { (item.BudgetName, item.BudgetAmount) });
                }
                else
                {
                    investmentGrid[item.Year].Add((item.BudgetName, item.BudgetAmount));
                }
            }

            var firstRow = true;
            foreach (var item in investmentGrid)
            {
                worksheet.Cells[startingRowInvestment, 1].Value = item.Key;
                foreach (var budget in item.Value)
                {
                    if (firstRow == true)
                    {
                        worksheet.Cells[39, startingBudgetHeaderColumn + nextBudget].Value = budget.BudgetName;
                        worksheet.Cells[startingRowInvestment, startingBudgetHeaderColumn + nextBudget].Value = budget.BudgetAmount.Value;
                        //worksheet.Cells[startingRowInvestment, startingBudgetHeaderColumn + nextBudget].Style.Numberformat.Format = currencyFormat;
                        nextBudget++;
                        continue;
                    }
                    for (var column = startingBudgetHeaderColumn; column <= item.Value.Count + 1; column++)
                    {
                        if (worksheet.Cells[39, column].Value.ToString() == budget.BudgetName)
                        {
                            //worksheet.Cells[startingRowInvestment, column].Style.Numberformat.Format = currencyFormat;
                            worksheet.Cells[startingRowInvestment, column].Value = budget.BudgetAmount.Value;
                            break;
                        }
                    }
                }
                startingRowInvestment++;
                firstRow = false;
                nextBudget = 0;
            }
            excelHelper.MergeCells(worksheet, 38, 1, 39, 1);
            excelHelper.MergeCells(worksheet, 38, 2, 38, inflationAndInvestments.BudgetOrder.Count + 2);
            excelHelper.ApplyBorder(worksheet.Cells[38, 1, startingRowInvestment - 1, inflationAndInvestments.BudgetOrder.Count + 3]);
            FillBudgetCriteria(worksheet, startingRowInvestment, criterias);
        }

        private void FillBudgetCriteria(ExcelWorksheet worksheet, int startingRowInvestment, List<CriteriaDrivenBudgetsModel> criterias)
        {
            var rowToApplyBorder = startingRowInvestment + 2;
            worksheet.Cells[startingRowInvestment + 2, 1].Value = "Budget Criteria";
            excelHelper.MergeCells(worksheet, startingRowInvestment + 2, 1, startingRowInvestment + 2, 5);

            worksheet.Cells[startingRowInvestment + 3, 1].Value = "Budget Name";
            worksheet.Cells[startingRowInvestment + 3, 2].Value = "Criteria";
            var cells = worksheet.Cells[startingRowInvestment + 3, 1, startingRowInvestment + 3, 2];
            excelHelper.ApplyStyle(cells);
            foreach (var item in criterias)
            {
                worksheet.Cells[startingRowInvestment + 4, 1].Value = item.BudgetName;
                worksheet.Cells[startingRowInvestment + 4, 2].Value = item.Criteria;
                excelHelper.MergeCells(worksheet, startingRowInvestment + 4, 2, startingRowInvestment + 4, 5, false);
                startingRowInvestment++;
            }
            excelHelper.ApplyBorder(worksheet.Cells[rowToApplyBorder, 1, startingRowInvestment + 3, 5]);
        }
    }
}
