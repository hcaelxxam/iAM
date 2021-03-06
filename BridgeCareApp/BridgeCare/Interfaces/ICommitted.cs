﻿using System.Collections.Generic;
using BridgeCare.EntityClasses;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface ICommitted
    {
        void SaveCommittedProjects(List<CommittedProjectModel> committedProjectModels, BridgeCareContext db);

        List<CommittedEntity> GetCommittedProjects(int simulationId, BridgeCareContext db);
    }
}
