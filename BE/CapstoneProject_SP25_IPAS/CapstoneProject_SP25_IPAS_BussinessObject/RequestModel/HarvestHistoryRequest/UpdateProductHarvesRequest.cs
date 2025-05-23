﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.HarvestHistoryRequest
{
    public class UpdateProductHarvesRequest
    {
        [Required]
        public int ProductHarvestHistoryId { get; set; }
        //public int? MasterTypeId { get; set; }
        //public int? PlantId { get; set; }
        public string? Unit { get; set; }
        public double? SellPrice { get; set; }
        public double? CostPrice { get; set; }
        public int? Quantity { get; set; }
        //public int? HarvestHistoryId { get; set; }
        public int? UserId { get; set; }
    }
}
