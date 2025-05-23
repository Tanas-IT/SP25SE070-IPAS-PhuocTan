﻿using System;
using System.Collections.Generic;

namespace CapstoneProject_SP25_IPAS_BussinessObject.Entities;

public partial class MasterType
{
    public int MasterTypeId { get; set; }

    public string? MasterTypeCode { get; set; }

    public string? MasterTypeName { get; set; }

    public string? MasterTypeDescription { get; set; }

    public string? Target { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsConflict { get; set; }
    public string? TypeName { get; set; }
    public int? MinTime { get; set; }

    public int? MaxTime { get; set; }
    public int? FarmID { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }


    public bool? IsDeleted { get; set; }

    public bool? IsDefault { get; set; }


    public string? BackgroundColor { get; set; }

    public string? TextColor { get; set; }

    public string? Characteristic { get; set; }

    public virtual Farm? Farm { get; set; }

    public virtual ICollection<Criteria> Criterias { get; set; } = new List<Criteria>();


    public virtual ICollection<ProductHarvestHistory> HarvestTypeHistories { get; set; } = new List<ProductHarvestHistory>();

    //public virtual ICollection<MasterTypeDetail> MasterTypeDetails { get; set; } = new List<MasterTypeDetail>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();
    public virtual ICollection<PlantLot> PlantLots { get; set; } = new List<PlantLot>();

    public virtual ICollection<SubProcess> SubProcesses { get; set; } = new List<SubProcess>();
    //public virtual ICollection<Type_Type> Type_Types_1 { get; set; } = new List<Type_Type>();
    //public virtual ICollection<Type_Type> Type_Types_2 { get; set; } = new List<Type_Type>();
    //public virtual ICollection<GrowthStageMasterType> GrowthStageMasterTypes { get; set; } = new List<GrowthStageMasterType>();

    public virtual ICollection<Type_Type> Products { get; set; } = new List<Type_Type>();
    public virtual ICollection<Type_Type> CriteriaSet { get; set; } = new List<Type_Type>();
    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
