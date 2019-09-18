﻿using BridgeCare.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using BridgeCare.Models;
using BridgeCare.ApplicationLog;
using BridgeCare.EntityClasses;

namespace BridgeCare.Services
{
    public class CommittedProjects : ICommittedProjects
    {
        readonly ICommitted committed;
        private readonly ISections sections;

        public CommittedProjects(ICommitted committed, ISections sections)
        {
            this.committed = committed;
            this.sections = sections;
        }
        
        /// <summary>
        /// Save committed projects from the template files
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="db"></param>
        public void SaveCommittedProjectsFiles(HttpRequest httpRequest, BridgeCareContext db)
        {
            var selectedScenarioId = httpRequest.Form.Get("selectedScenarioId");
            var networkId = httpRequest.Form.Get("networkId");
            var noTreatment = httpRequest.Form.Get("applyNoTreatment") == "1" ? true : false;
            var files = httpRequest.Files;
            var committedProjectModels = new List<CommittedProjectModel>();

            for (int i = 0; i < files.Count; i++)
            {
                GetCommittedProjectModels(files[i], selectedScenarioId, networkId, noTreatment, committedProjectModels, db);
                committed.SaveCommittedProjects(committedProjectModels, db);
            }
        }

        /// <summary>
        /// Export committed projects for a simulation
        /// </summary>
        /// <param name="simulationId"></param>
        /// <param name="networkId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public byte[] ExportCommittedProjects(int simulationId, int networkId, BridgeCareContext db)
        {
            using (ExcelPackage excelPackage = new ExcelPackage(new System.IO.FileInfo("CommittedProjects.xlsx")))
            {
                // This method may stay here or if too long then move to helper class   Fill(worksheet, , db);
                var committedProjects = committed.GetCommittedProjects(simulationId, db);
                var worksheet = excelPackage.Workbook.Worksheets.Add("Committed Projects");
                if (committedProjects.Count != 0)
                {
                    AddHeaderCells(worksheet, committedProjects.FirstOrDefault().COMMIT_CONSEQUENCES.ToList());
                    AddDataCells(worksheet, committedProjects, networkId, db);
                }
                return excelPackage.GetAsByteArray();
            }
        }

        private void AddDataCells(ExcelWorksheet worksheet, List<CommittedEntity> committedProjects, int networkId, BridgeCareContext db)
        {
            var committedProjectsSectionIds = committedProjects.Select(cproj => cproj.SECTIONID).ToList();
            var sectionModels = sections.GetSections(networkId, db);
            // get all committed projects that have a matching section, if any, and add them to the excel file
            var row = 2;
            sectionModels?.Where(sec => committedProjectsSectionIds.Contains(sec.SectionId)).OrderBy(sec => sec.ReferenceKey).ToList().ForEach(model =>
            {
                committedProjects.Where(cproj => cproj.SECTIONID == model.SectionId).OrderByDescending(cproj => cproj.YEARS).ToList()
                    .ForEach(committedProject =>
                    {
                        var column = 1;
                        // BRKEY, BMSID
                        worksheet.Cells[row, column++].Value = Convert.ToInt32(model.ReferenceKey);
                        worksheet.Cells[row, column++].Value = model.ReferenceId;
                        // Committed_
                        worksheet.Cells[row, column++].Value = committedProject.TREATMENTNAME;
                        worksheet.Cells[row, column++].Value = committedProject.YEARS;
                        worksheet.Cells[row, column++].Value = committedProject.YEARANY;
                        worksheet.Cells[row, column++].Value = committedProject.YEARSAME;
                        worksheet.Cells[row, column++].Value = committedProject.BUDGET;
                        worksheet.Cells[row, column++].Value = committedProject.COST_;
                        worksheet.Cells[row, column++].Value = string.Empty; // AREA
                        // Consequences
                        committedProject.COMMIT_CONSEQUENCES.ToList().ForEach(commitConsequence =>
                        {
                            worksheet.Cells[row, column++].Value = commitConsequence.CHANGE_;
                        });
                        row++;
                    });
            });

            // get all the committed projects that didn't have a matching section, if any, and add them to the excel file noting that the section was not found
            var sectionIds = sectionModels != null ? sectionModels.Select(model => model.SectionId).ToList() : new List<int>();
            committedProjects.Where(cproj => !sectionIds.Contains(cproj.SECTIONID)).OrderByDescending(cproj => cproj.YEARS).ToList()
            .ForEach(committedProject =>
            {
                var column = 1;
                // note section not found here
                worksheet.Cells[row, column++].Value = "Section Not Found";
                worksheet.Cells[row, column++].Value = "Section Not Found";
                // Committed_
                worksheet.Cells[row, column++].Value = committedProject.TREATMENTNAME;
                worksheet.Cells[row, column++].Value = committedProject.YEARS;
                worksheet.Cells[row, column++].Value = committedProject.YEARANY;
                worksheet.Cells[row, column++].Value = committedProject.YEARSAME;
                worksheet.Cells[row, column++].Value = committedProject.BUDGET;
                worksheet.Cells[row, column++].Value = committedProject.COST_;
                worksheet.Cells[row, column++].Value = string.Empty; // AREA
                // Consequences
                committedProject.COMMIT_CONSEQUENCES.ToList().ForEach(commitConsequence =>
                {
                    worksheet.Cells[row, column++].Value = commitConsequence.CHANGE_;
                });
                row++;
            });
        }

        private void AddHeaderCells(ExcelWorksheet worksheet, List<CommitConsequencesEntity> commitConsequences)
        {
            var fixColumnHeaders = new List<string>() { "BRKEY", "BMSID", "TREATMENT", "YEAR", "YEARANY", "YEARSAME", "BUDGET", "COST", "AREA" };
            int headerRow = 1;
            for (int column = 0; column < fixColumnHeaders.Count; column++)
            {
                worksheet.Cells[headerRow, column + 1].Value = fixColumnHeaders[column];
            }
            var currentColumn = fixColumnHeaders.Count;
            foreach (var commitConsequence in commitConsequences)
            {
                worksheet.Cells[headerRow, ++currentColumn].Value = commitConsequence.ATTRIBUTE_;
            }
        }

        private void GetCommittedProjectModels(HttpPostedFile postedFile, string selectedScenarioId, string networkId, bool applyNoTreatment, List<CommittedProjectModel> committedProjectModels, BridgeCareContext db)
        {
            try
            {
                var simulationId = Convert.ToInt32(selectedScenarioId);
                var package = new ExcelPackage(postedFile.InputStream);
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                var headers = worksheet.Cells.GroupBy(cell => cell.Start.Row).First();
                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;
                for (int row = start.Row + 1; row <= end.Row; row++)
                {
                    var column = start.Column + 2;
                    var brKey = Convert.ToInt32(GetCellValue(worksheet, row, 1));
                    var sectionId = sections.GetSectionId(Convert.ToInt32(networkId), brKey, db);

                    // BMSID till COST -> entry in COMMITTED_                    
                    var committedProjectModel = new CommittedProjectModel
                    {
                        SectionId = sectionId,
                        SimulationId = Convert.ToInt32(selectedScenarioId),
                        TreatmentName = GetCellValue(worksheet, row, column),
                        Years = Convert.ToInt32(GetCellValue(worksheet, row, ++column)),
                        YearAny = Convert.ToInt32(GetCellValue(worksheet, row, ++column)),
                        YearSame = Convert.ToInt32(GetCellValue(worksheet, row, ++column)),
                        Budget = GetCellValue(worksheet, row, ++column),
                        Cost = Convert.ToInt32(GetCellValue(worksheet, row, ++column))
                    };

                    var commitConsequences = new List<CommitConsequenceModel>();
                    // Ignore AREA column, from current column till end.Column -> attributes i.e. entry in COMMIT_CONSEQUENCES
                    for (var col = column + 2; col <= end.Column; col++)
                    {
                        commitConsequences.Add(new CommitConsequenceModel
                        {
                            Attribute_ = GetHeader(headers, col),
                            Change_ = GetCellValue(worksheet, row, col)
                        });
                    }
                    committedProjectModel.CommitConsequences = commitConsequences;
                    committedProjectModels.Add(committedProjectModel);

                    var simulation = db.Simulations.SingleOrDefault(s => s.SIMULATIONID == simulationId);
                    if (applyNoTreatment && simulation != null)
                    {
                        if (simulation.COMMITTED_START < committedProjectModel.Years)
                        {
                            var year = committedProjectModel.Years - 1;
                            while (year >= simulation.COMMITTED_START)
                            {
                                committedProjectModels.Add(new CommittedProjectModel()
                                {
                                    SectionId = committedProjectModel.SectionId,
                                    SimulationId = committedProjectModel.SimulationId,
                                    TreatmentName = "No Treatment",
                                    Years = year,
                                    YearAny = committedProjectModel.YearAny,
                                    YearSame = committedProjectModel.YearSame,
                                    Budget = committedProjectModel.Budget,
                                    Cost = 0,
                                    CommitConsequences = committedProjectModel.CommitConsequences
                                });
                                year--;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException.GeneralError(ex);
            }
        }

        private string GetHeader(IGrouping<int, ExcelRangeBase> headers, int col)
        {
            return (string)headers.Skip(col - 1).First().Value;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
        {
            return worksheet.Cells[row, col].Value.ToString().Trim();
        }       
    }
}