﻿using CapstoneProject_SP25_IPAS_BussinessObject.BusinessModel.UserBsModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject_SP25_IPAS_BussinessObject.RequestModel.FarmRequest.UserFarmRequest
{
    public class UserFarmRequest
    {
        public int? FarmId { get; set; }
        [Required]
        public int UserId { get; set; }
        //[Required]
        public string? RoleName { get; set; }
        public bool? IsActive { get; set; }
        public List<SkillModel>? Skills { get; set; }
       
    }
}
