﻿using CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlanRequest;
using CapstoneProject_SP25_IPAS_BussinessObject.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.PlanModel
{
    public class UpdatePlanModel
    {
        public int PlanId { get; set; }

        public string? Status { get; set; }
        public string? PlanName { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }

        public string? Notes { get; set; }

        public string? PlanDetail { get; set; }

        public string? ResponsibleBy { get; set; }

        public string? Frequency { get; set; }

        public int? AssignorId { get; set; }

        public string? PesticideName { get; set; }

        public double? MaxVolume { get; set; }

        public double? MinVolume { get; set; }

        public int? ProcessId { get; set; }
        public int? SubProcessId { get; set; }

        public int? CropId { get; set; }
        public List<int>? ListLandPlotOfCrop { get; set; }

        public List<int>? GrowthStageId { get; set; }

        public int? PlantLotId { get; set; }

        public bool? IsDelete { get; set; }

        public int? MasterTypeId { get; set; }
        public List<int>? DayOfWeek { get; set; }
        public List<int>? DayOfMonth { get; set; }
        public List<DateTime>? CustomDates { get; set; }
        public List<EmployeeModel> ListEmployee { get; set; } = new List<EmployeeModel>();
        public List<PlanTargetModel>? PlanTargetModel { get; set; }
        [FlexibleTime]
        public string StartTime { get; set; }

        [FlexibleTime]
        public string EndTime { get; set; }

        // ✅ Constructor mặc định
        public UpdatePlanModel() { }

        // ✅ Constructor copy để tạo bản sao
        public UpdatePlanModel(UpdatePlanModel model)
        {
            PlanId = model.PlanId;
            Status = model.Status;
            PlanName = model.PlanName;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            IsActive = model.IsActive;
            Notes = model.Notes;
            PlanDetail = model.PlanDetail;
            ResponsibleBy = model.ResponsibleBy;
            Frequency = model.Frequency;
            AssignorId = model.AssignorId;
            PesticideName = model.PesticideName;
            MaxVolume = model.MaxVolume;
            MinVolume = model.MinVolume;
            ProcessId = model.ProcessId;
            CropId = model.CropId;
            GrowthStageId = model.GrowthStageId;
            PlantLotId = model.PlantLotId;
            IsDelete = model.IsDelete;
            MasterTypeId = model.MasterTypeId;
            DayOfWeek = model.DayOfWeek != null ? new List<int>(model.DayOfWeek) : null;
            DayOfMonth = model.DayOfMonth != null ? new List<int>(model.DayOfMonth) : null;
            CustomDates = model.CustomDates != null ? new List<DateTime>(model.CustomDates) : null;
            ListEmployee = model.ListEmployee != null ? new List<EmployeeModel>(model.ListEmployee) : new List<EmployeeModel>();
            StartTime = model.StartTime;
            EndTime = model.EndTime;
        }

    }
}
