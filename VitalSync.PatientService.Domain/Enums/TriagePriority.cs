using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitalSync.PatientService.Domain.Enums
{
    public enum TriagePriority
    {
        Routine = 1,
        Urgent = 2,
        Emergency = 3,
        Critical = 4
    }
}
