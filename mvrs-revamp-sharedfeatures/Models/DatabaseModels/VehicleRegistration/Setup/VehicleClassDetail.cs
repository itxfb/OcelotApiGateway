﻿using Models.DatabaseModels.Setup;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DatabaseModels.VehicleRegistration.Setup
{
    public class VehicleClassDetail : SetupBaseModel
    {
        [ForeignKey("VehicleClass")]
        public long VehicleClassId { get; set; }
        public virtual VehicleClass VehicleClass { get; set; }

        [ForeignKey("VehicleBodyType")]
        public long VehicleBodyTypeId { get; set; }
        public virtual VehicleBodyType VehicleBodyType { get; set; }

        [ForeignKey("VehicleCategory")]
        public long VehicleCategoryId { get; set; }
        public virtual VehicleCategory VehicleCategory { get; set; }

        [ForeignKey("VehicleClassification")]
        public long VehicleClassificationId { get; set; }
        public virtual VehicleClassification VehicleClassification { get; set; }

        [ForeignKey("VehicleRouteType")]
        public long? VehicleRouteTypeId { get; set; }
        public virtual VehicleRouteType VehicleRouteType { get; set; }

        [ForeignKey("VehicleType")]
        public long? VehicleTypeId { get; set; }
        public virtual VehicleType VehicleType { get; set; }

        [ForeignKey("VehicleUsage")]
        public long VehicleUsageId { get; set; }
        public virtual VehicleUsage VehicleUsage { get; set; }

        public bool IsActive { get; set; }

        public long EngineSizeMin { get; set; }
        public long EngineSizeMax { get; set; }

        public long SeatingCapacityMin { get; set; }
        public long SeatingCapacityMax { get; set; }

        public long LadenWeightMin { get; set; }
        public long LadenWeightMax { get; set; }

        public long UnladenWeightMin { get; set; }
        public long UnladenWeightMax { get; set; }

        public bool FitnessDocRequired { get; set; }
        public DateTime AppliedFrom { get; set; }
        public int TaxFrequency { get; set; }
    }
}
