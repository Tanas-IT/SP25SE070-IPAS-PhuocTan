﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.PlantLotRequest
{
    public class UpdatePlantLotModel
    {
        public int PlantLotID { get; set; }
        public int? PartnerID { get; set; }
        public string? Name { get; set; }
        public int? InputQuantity { get; set; }
        public int? LastQuantity { get; set; }
        public int? UsedQuantity { get; set; }
        public string? Unit { get; set; }

        public string? Note { get; set; } = "";
        public string? Status { get; set; } = "";
        public int? MasterTypeId { get; set; }
        public bool? IsPass { get; set; } = false;
    }
}
